using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Runtime;
using Benchmark.Server.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using MessagePack;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class HubBenchmarkScenario : IBenchmarkHubReciever, IAsyncDisposable
    {
        private readonly BenchReporter _reporter;
        private readonly BenchmarkerConfig _config;
        private IBenchmarkHub[] _clients;

        public HubBenchmarkScenario(GrpcChannel[] channels, BenchReporter reporter, BenchmarkerConfig config)
        {
            _clients = channels.Select(x => StreamingHubClient.ConnectAsync<IBenchmarkHub, IBenchmarkHubReciever>(x, this).GetAwaiter().GetResult()).ToArray();
            _reporter = reporter;
            _config = config;
        }

        private IBenchmarkHub GetClient(int n) => _clients[n % _clients.Length];

        public async Task Run(int requestCount, CancellationToken ct)
        {
            using (var statistics = new Statistics(nameof(HubBenchmarkScenario) + requestCount))
            {
                await PlainTextAsync(requestCount, ct, (completeCount, errorCount) =>
                {
                    _reporter.AddBenchDetail(new BenchReportItem
                    {
                        ExecuteId = _reporter.ExecuteId,
                        ClientId = _reporter.ClientId,
                        TestName = nameof(HubBenchmarkScenario),
                        Begin = statistics.Begin,
                        End = DateTime.UtcNow,
                        Duration = statistics.Elapsed,
                        RequestCount = completeCount,
                        Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                        Errors = errorCount,
                    });
                    statistics.HasError(errorCount != 0);
                });
            }
        }

        /// <summary>
        /// Concurrent Run
        /// </summary>
        /// <param name="requestCount"></param>
        /// <param name="ct"></param>
        /// <param name="reportAction"></param>
        /// <returns></returns>
        private async Task PlainTextAsync(int requestCount, CancellationToken ct, Action<int, int> reportAction)
        {
            var data = new BenchmarkData
            {
                PlainText = _config.GetRequestPayload(),
            };
            void Run(TaskWorkerPool pool, BenchmarkData data) => pool.RunWorkers(id => GetClient(id).Process(data));

            var duration = _config.GetDuration();
            if (duration != TimeSpan.Zero)
            {
                // timeout base
                using var cts = new CancellationTokenSource(duration);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
                var linkedCt = linkedCts.Token;

                using var pool = new TaskWorkerPool(_config.ClientConcurrency, linkedCt);
                Run(pool, data);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                reportAction.Invoke(pool.CompleteCount, pool.Errors.Count);
            }
            else
            {
                // request base
                using var pool = new TaskWorkerPool(_config.ClientConcurrency, ct)
                {
                    CompleteCondition = x => x.completed >= requestCount,
                };
                Run(pool, data);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                reportAction.Invoke(pool.CompleteCount, pool.Errors.Count);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await Task.WhenAll(_clients.Select(x => x.DisposeAsync()));
        }

        void IBenchmarkHubReciever.OnProcess()
        {
            throw new NotImplementedException();
        }
    }
}
