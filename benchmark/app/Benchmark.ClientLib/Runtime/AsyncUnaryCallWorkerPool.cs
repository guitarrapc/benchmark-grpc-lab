using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Runtime
{
    public class AsyncUnaryCallWorkerPool<T> : IDisposable
    {
        private readonly int _workerCount;
        private readonly CancellationToken _ct;
        private readonly TaskCompletionSource _timeoutTcs = new TaskCompletionSource();
        private readonly TaskCompletionSource _completeTask = new TaskCompletionSource();
        private int _completeCount;
        private ConcurrentDictionary<string, Exception> _errors = new ConcurrentDictionary<string, Exception>();
        private readonly Channel<Func<int, AsyncUnaryCall<T>>> _channel;
        private readonly ChannelWriter<Func<int, AsyncUnaryCall<T>>> _writer;
        private readonly ChannelReader<Func<int, AsyncUnaryCall<T>>> _reader;

        public Func<(int current, int completed), bool> CompleteCondition { get; init; } = (x) => false;
        public int CompleteCount => _completeCount;
        public bool Timeouted => _timeoutTcs.Task.IsCompleted;
        public bool Completed => _completeTask.Task.IsCompleted;
        public ConcurrentDictionary<string, Exception> Errors => _errors;

        public AsyncUnaryCallWorkerPool(int workerCount, CancellationToken ct) : this(workerCount, 1000, ct)
        {
        }

        public AsyncUnaryCallWorkerPool(int workerCount, int channelSize, CancellationToken ct)
        {
            _workerCount = workerCount;
            _ct = ct;
            _ct.Register(() => _timeoutTcs.TrySetResult());
            _channel = System.Threading.Channels.Channel.CreateBounded<Func<int, AsyncUnaryCall<T>>>(new BoundedChannelOptions(channelSize)
            {
                SingleReader = false,
                SingleWriter = true,
                FullMode = BoundedChannelFullMode.Wait,
            });
            _writer = _channel.Writer;
            _reader = _channel.Reader;
        }

        /// <summary>
        /// Wait for Pool complete
        /// </summary>
        /// <returns></returns>
        public Task WaitForCompleteAsync() => _completeTask.Task;

        /// <summary>
        /// Wait for Pool end by timeout
        /// </summary>
        /// <returns></returns>
        public Task WaitForTimeout() => _timeoutTcs.Task;

        public void RunWorkers(Func<int, AsyncUnaryCall<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            RunCore(action);
            WatchComplete();
        }

        public void Dispose() => _writer.TryComplete();

        /// <summary>
        /// Main execution
        /// </summary>
        /// <returns></returns>
        private void RunCore(Func<int, AsyncUnaryCall<T>> action)
        {
            // write
            Task.Run(async () =>
            {
                while (await _writer.WaitToWriteAsync(_ct).ConfigureAwait(false))
                {
                    try
                    {
                        if (_ct.IsCancellationRequested)
                            return;

                        await _writer.WriteAsync(action, _ct).ConfigureAwait(false);
                    }
                    catch (ChannelClosedException)
                    {
                        // already closed.
                    }
                    catch (OperationCanceledException)
                    {
                        // canceled
                    }
                }
            }, _ct);

            // read
            var workerId = 0;
            for (var i = 0; i < _workerCount; i++)
            {
                var id = workerId++;
                Task.Run(async () =>
                {
                    while (await _reader.WaitToReadAsync(_ct))
                    {
                        try
                        {
                            var item = await _reader.ReadAsync(_ct);

                            if (_ct.IsCancellationRequested)
                                return;

                            await item.Invoke(id);
                            //Console.WriteLine($"done {_completeCount} ({_reader.Count}, id {id})");
                        }
                        catch (ChannelClosedException)
                        {
                            // already closed.
                        }
                        catch (OperationCanceledException)
                        {
                            // canceled
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"exception {ex.Message} {ex.GetType().FullName} {ex.StackTrace}");
                            _errors.TryAdd(ex.GetType().FullName, ex);
                        }
                        finally
                        {
                            Interlocked.Increment(ref _completeCount);
                        }
                    }
                }, _ct);
            }
        }

        private void WatchComplete()
        {
            // complete
            Task.Run(async () =>
            {
                while (!CompleteCondition((_reader.Count, _completeCount)))
                {
                    if (_ct.IsCancellationRequested)
                    {
                        _completeTask.SetCanceled();
                        return;
                    }

                    await Task.Delay(100);
                }
                _writer.TryComplete();
                _completeTask.SetResult();
            }, _ct);
        }
    }
}
