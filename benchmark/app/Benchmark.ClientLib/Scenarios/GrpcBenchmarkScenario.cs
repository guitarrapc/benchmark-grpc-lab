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
        private readonly HelloRequest _simpleRequest;
        private readonly BenchReporter _reporter;
        private ConcurrentDictionary<string, Exception> _errors = new ConcurrentDictionary<string, Exception>();

        public GrpcBenchmarkScenario(GrpcChannel channel, BenchReporter reporter)
        {
            _client = new Greeter.GreeterClient(channel);
            _reporter = reporter;

            _simpleRequest = new HelloRequest {  Name = "100" };
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
            for (var i = 0; i <= requestCount; i++)
            {
                try
                {
                    await _client.SayHelloAsync(_simpleRequest);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
        }

    }
}
