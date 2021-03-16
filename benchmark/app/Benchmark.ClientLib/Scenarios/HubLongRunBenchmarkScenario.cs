using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Internal.Runtime;
using Benchmark.Server.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Benchmark.ClientLib.Scenarios
{
    public class HubLongRunBenchmarkScenario : ILongRunBenchmarkHubReciever, IAsyncDisposable
    {
        private readonly BenchmarkerConfig _config;
        private readonly BenchReporter _reporter;
        private ILongRunBenchmarkHub[] _clients;

        public HubLongRunBenchmarkScenario(GrpcChannel[] channels, BenchReporter reporter, BenchmarkerConfig config)
        {
            _clients = channels.Select(x => StreamingHubClient.ConnectAsync<ILongRunBenchmarkHub, ILongRunBenchmarkHubReciever>(x, this).GetAwaiter().GetResult()).ToArray();
            _reporter = reporter;
            _config = config;
        }

        private ILongRunBenchmarkHub GetClient(int n) => _clients[n % _clients.Length];

        public async Task Run(int requestCount, int waitMilliseonds, CancellationToken ct)
        {
            Statistics statistics = null;
            CallResult[] results = null;
            using (statistics = new Statistics(nameof(UnaryBenchmarkScenario) + requestCount))
            {
                results = await ProcessAsync(requestCount, waitMilliseonds, ct);
            }

            _reporter.AddDetail(new BenchReportItem
            {
                ExecuteId = _reporter.ExecuteId,
                ClientId = _reporter.ClientId,
                TestName = nameof(ProcessAsync),
                Begin = statistics.Begin,
                End = DateTime.UtcNow,
                Duration = statistics.Elapsed,
                RequestCount = results.Length,
                Type = nameof(MethodType.DuplexStreaming),
                Average = results.Select(x => x.Duration).Average(),
                Fastest = results.Min(x => x.Duration),
                Slowest = results.Max(x => x.Duration),
                Rps = results.Length / statistics.Elapsed.TotalSeconds,
                Errors = results.Where(x => x.Error != null).Count(),
                StatusCodeDistributions = StatusCodeDistribution.FromCallResults(results),
                ErrorCodeDistribution = ErrorCodeDistribution.FromCallResults(results),
                Latencies = LatencyDistribution.Calculate(results.Select(x => x.Duration).OrderBy(x => x).ToArray()),
            });
        }

        private async Task<CallResult[]> ProcessAsync(int requestCount, int waitMilliseonds, CancellationToken ct)
        {
            var data = new LongRunBenchmarkData
            {
                WaitMilliseconds = waitMilliseonds,
            };

            var duration = _config.GetDuration();
            if (duration != TimeSpan.Zero)
            {
                // timeout base
                using var cts = new CancellationTokenSource(duration);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
                var linkedCt = linkedCts.Token;

                using var pool = new TaskWorkerPool<LongRunBenchmarkData>(_config.ClientConcurrency, linkedCt);
                pool.RunWorkers((id, data, ct) => GetClient(id).Process(data), data, ct);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                return pool.GetResult();
            }
            else
            {
                // request base
                using var pool = new TaskWorkerPool<LongRunBenchmarkData>(_config.ClientConcurrency, ct)
                {
                    CompleteCondition = x => x.completed >= requestCount,
                };
                pool.RunWorkers((id, data, ct) => GetClient(id).Process(data), data, ct);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                return pool.GetResult();
            }
        }
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await Task.WhenAll(_clients.Select(x => x.DisposeAsync()));
        }

        void ILongRunBenchmarkHubReciever.OnProcess()
        {
            throw new NotImplementedException();
        }
    }
}
