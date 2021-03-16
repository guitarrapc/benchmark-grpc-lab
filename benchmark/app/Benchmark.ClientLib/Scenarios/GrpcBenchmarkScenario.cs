using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Runtime;
using Benchmark.Server;
using Grpc.Net.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class GrpcBenchmarkScenario
    {
        private readonly Greeter.GreeterClient[] _clients;
        private readonly BenchReporter _reporter;
        private readonly BenchmarkerConfig _config;

        public GrpcBenchmarkScenario(GrpcChannel[] channels, BenchReporter reporter, BenchmarkerConfig config)
        {
            _clients = channels.Select(x => new Greeter.GreeterClient(x)).ToArray();
            _reporter = reporter;
            _config = config;
        }

        private Greeter.GreeterClient GetClient(int n) => _clients[n % _clients.Length];

        public async Task Run(int requestCount, CancellationToken ct)
        {
            using (var statistics = new Statistics(nameof(GrpcBenchmarkScenario) + requestCount))
            {
                await SayHelloAsync(requestCount, ct, (completeCount, errorCount) =>
                {
                    _reporter.AddBenchDetail(new BenchReportItem
                    {
                        ExecuteId = _reporter.ExecuteId,
                        ClientId = _reporter.ClientId,
                        TestName = nameof(GrpcBenchmarkScenario),
                        Begin = statistics.Begin,
                        End = DateTime.UtcNow,
                        Duration = statistics.Elapsed,
                        RequestCount = completeCount, // concurrent will run single request
                        Type = nameof(Grpc.Core.MethodType.Unary),
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
        private async Task SayHelloAsync(int requestCount, CancellationToken ct, Action<int, int> reportAction)
        {
            var data = new HelloRequest { Name = _config.GetRequestPayload() };
            void Run(AsyncUnaryCallWorkerPool<HelloReply> pool, HelloRequest data, CancellationToken ct) => pool.RunWorkers(id => GetClient(id).SayHelloAsync(data, cancellationToken: ct));

            var duration = _config.GetDuration();
            if (duration != TimeSpan.Zero)
            {
                // duration base
                using var cts = new CancellationTokenSource(duration);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
                var linkedCt = linkedCts.Token;

                using var pool = new AsyncUnaryCallWorkerPool<HelloReply>(_config.ClientConcurrency, linkedCt);
                Run(pool, data, linkedCt);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                reportAction(pool.CompleteCount, pool.Errors.Count);
            }
            else
            {
                // request base
                using var pool = new AsyncUnaryCallWorkerPool<HelloReply>(_config.ClientConcurrency, ct)
                {
                    CompleteCondition = x => x.completed >= requestCount,
                };
                Run(pool, data, ct);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                reportAction(pool.CompleteCount, pool.Errors.Count);
            }
        }
    }
}
