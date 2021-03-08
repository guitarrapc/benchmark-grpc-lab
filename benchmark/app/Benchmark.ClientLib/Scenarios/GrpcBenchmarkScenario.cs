using Benchmark.ClientLib.Reports;
using Benchmark.Server;
using Grpc.Net.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class GrpcBenchmarkScenario
    {
        private readonly Greeter.GreeterClient _client;
        private readonly BenchReporter _reporter;
        private readonly BenchmarkerConfig _config;
        private ConcurrentDictionary<string, Exception> _errors = new ConcurrentDictionary<string, Exception>();

        public GrpcBenchmarkScenario(GrpcChannel channel, BenchReporter reporter, BenchmarkerConfig config)
        {
            _client = new Greeter.GreeterClient(channel);
            _reporter = reporter;
            _config = config;
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics(nameof(SayHelloAsync)))
            {
                await SayHelloAsync(requestCount);

                _reporter.AddBenchDetail(new BenchReportItem
                {
                    ExecuteId = _reporter.ExecuteId,
                    ClientId = _reporter.ClientId,
                    TestName = nameof(SayHelloAsync),
                    Begin = statistics.Begin,
                    End = DateTime.UtcNow,
                    Duration = statistics.Elapsed,
                    RequestCount = requestCount,
                    Type = nameof(Grpc.Core.MethodType.Unary),
                    Errors = _errors.Count,
                });
                statistics.HasError(_errors.Count);
            }
        }

        private async Task SayHelloAsync(int requestCount)
        {
            var data = new HelloRequest { Name = _config.GetRequestPayload() };
            for (var i = 0; i <= requestCount; i++)
            {
                try
                {
                    await _client.SayHelloAsync(data);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
        }

    }
}
