using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Runtime
{
    /// <summary>
    /// TaskPool to run within pool size. This enable you to run only selected concurrent tasks at once.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskPool<T> : IDisposable
    {
        public Task Completed => _completeTask.Task;

        private readonly ConcurrentQueue<(TaskCompletionSource tcs, Task<T> task)> _pool = new ConcurrentQueue<(TaskCompletionSource, Task<T>)>();
        private readonly int _poolSize;
        private readonly AutoResetEvent _autoResetEvent;
        private readonly CancellationToken _ct;
        private object _lock = new object();
        private TaskCompletionSource _timeoutTcs;
        private readonly TaskCompletionSource _completeTask;

        public TaskPool(int poolSize, CancellationToken ct)
        {
            _poolSize = poolSize;
            _autoResetEvent = new AutoResetEvent(false);
            _ct = ct;
            _timeoutTcs = new TaskCompletionSource();

            RunEngine().FireAndForget();
        }

        /// <summary>
        ///  Register your method to run on worker thread. Only allow you to register whithin pool size.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Task RegisterAsync(Func<Task<T>> item)
        {
            lock (_lock)
            {
                if (_pool.Count >= _poolSize)
                    _autoResetEvent.WaitOne();

                var tcs = new TaskCompletionSource();
                _pool.Enqueue((tcs, item.Invoke()));
                return tcs.Task;
            }
        }

        /// <summary>
        /// Wait for Pool complete
        /// </summary>
        /// <returns></returns>
        public async Task WaitForCompleteAsync()
        {
            while (_pool.Count != 0)
            {
                if (_ct.IsCancellationRequested)
                {
                    _completeTask.SetCanceled();
                    return;
                }

                await Task.Delay(1, _ct);
            }
            _completeTask.SetResult();
        }

        /// <summary>
        /// Wait for Pool end by timeout
        /// </summary>
        /// <returns></returns>
        public Task WaitForTimeout()
        {
            return _timeoutTcs.Task;
        }

        public void Dispose()
        {
            _autoResetEvent?.Dispose();
            _pool.Clear();
        }

        /// <summary>
        /// Main execution
        /// </summary>
        /// <returns></returns>
        private async Task RunEngine()
        {
            while (true)
            {
                if (_ct.IsCancellationRequested)
                {
                    _timeoutTcs.TrySetResult();
                    return;
                }

                if (!_pool.TryPeek(out var item))
                {
                    // todo: 1ms wait.... too match...?
                    await Task.Delay(1);
                    continue;
                }

                try
                {
                    var result = await item.task.ConfigureAwait(false);
                    //Console.WriteLine($"complete queue {_pool.Count}");
                    item.tcs.TrySetResult();
                }
                catch (OperationCanceledException oex)
                {
                    Console.WriteLine("canceled");
                    item.tcs.TrySetCanceled();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"exception {ex.Message} {ex.GetType().FullName} {ex.StackTrace}");
                    item.tcs.TrySetException(ex);
                }
                finally
                {
                    _pool.TryDequeue(out var _);
                    _autoResetEvent.Set();
                }
            }
        }
    }
}
