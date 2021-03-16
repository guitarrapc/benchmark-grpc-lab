using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Runtime;
using Benchmark.Server.Shared;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class ApiBenchmarkScenario
    {
        private readonly ApiClient[] _clients;
        private readonly BenchReporter _reporter;
        private readonly BenchmarkerConfig _config;

        public ApiBenchmarkScenario(ApiClient[] clients, BenchReporter reporter, BenchmarkerConfig config)
        {
            _clients = clients;
            _reporter = reporter;
            _config = config;
        }

        private ApiClient GetClient(int n) => _clients[n % _clients.Length];

        public async Task Run(int requestCount, CancellationToken ct)
        {
            using (var statistics = new Statistics(nameof(PlainTextAsync)))
            {
                await PlainTextAsync(requestCount, ct, (completeCount, errorCount) =>
                {
                    _reporter.AddBenchDetail(new BenchReportItem
                    {
                        ExecuteId = _reporter.ExecuteId,
                        ClientId = _reporter.ClientId,
                        TestName = nameof(ApiBenchmarkScenario),
                        Begin = statistics.Begin,
                        End = DateTime.UtcNow,
                        Duration = statistics.Elapsed,
                        RequestCount = completeCount,
                        Type = "REST",
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
            var json = JsonSerializer.Serialize<BenchmarkData>(data);
            void Run(TaskWorkerPool pool, string json) => pool.RunWorkers(id => GetClient(id).PlainTextAsync(json));

            var duration = _config.GetDuration();
            if (duration != TimeSpan.Zero)
            {
                // timeout base
                using var cts = new CancellationTokenSource(duration);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, ct);
                var linkedCt = linkedCts.Token;

                using var pool = new TaskWorkerPool(_config.ClientConcurrency, linkedCt);
                Run(pool, json);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                Console.WriteLine($"duration/completed/timeouted: {duration}/{pool.Completed}/{pool.Timeouted}");
                reportAction.Invoke(pool.CompleteCount, pool.Errors.Count);
            }
            else
            {
                // request base
                using var pool = new TaskWorkerPool(_config.ClientConcurrency, ct)
                {
                    CompleteCondition = x => x.completed >= requestCount,
                };
                Run(pool, json);
                await Task.WhenAny(pool.WaitForCompleteAsync(), pool.WaitForTimeout());
                reportAction.Invoke(pool.CompleteCount, pool.Errors.Count);
            }
        }

        /// <summary>
        /// ThreadSafe client
        /// </summary>
        public class ApiClient
        {
            private readonly HttpClient _client;
            private readonly string _endpointPlainText;

            public ApiClient(string endpoint)
            {
                _client = new HttpClient();
                _endpointPlainText = endpoint + "/plaintext";
           }

            public async Task PlainTextAsync(string json)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync(_endpointPlainText, content);
                res.EnsureSuccessStatusCode();
            }
        }
    }
}
