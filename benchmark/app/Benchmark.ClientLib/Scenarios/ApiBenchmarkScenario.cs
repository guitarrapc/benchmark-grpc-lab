using Benchmark.ClientLib.Reports;
using Benchmark.Server.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class ApiBenchmarkScenario
    {
        private readonly ApiClient _client;
        private readonly BenchReporter _reporter;
        private readonly BenchmarkerConfig _config;
        private ConcurrentDictionary<string, Exception> _errors = new ConcurrentDictionary<string, Exception>();

        public ApiBenchmarkScenario(ApiClient client, BenchReporter reporter, BenchmarkerConfig config)
        {
            _client = client;
            _reporter = reporter;
            _config = config;
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics(nameof(PlainTextAsync)))
            {
                await PlainTextAsync(requestCount);

                _reporter.AddBenchDetail(new BenchReportItem
                {
                    ExecuteId = _reporter.ExecuteId,
                    ClientId = _reporter.ClientId,
                    TestName = nameof(PlainTextAsync),
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

        private async Task SumAsync(int requestCount)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    // Call the server-side method using the proxy.
                    var task = _client.SumAsync(i, i);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
            await Task.WhenAll(tasks);
        }

        private async Task PlainTextAsync(int requestCount)
        {
            var data = new BenchmarkData
            {
                PlainText = _config.GetRequestPayload(),
            };
            for (var i = 0; i < requestCount; i++)
            {
                var json = JsonSerializer.Serialize<BenchmarkData>(data);
                try
                {
                    await _client.PlainTextAsync(json);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
        }

        private async Task PlainTextAsyncParallel(int requestCount)
        {
            var data = new BenchmarkData
            {
                PlainText = _config.GetRequestPayload(),
            };
            var tasks = new List<Task>();
            for (var i = 0; i < requestCount; i++)
            {
                var json = JsonSerializer.Serialize<BenchmarkData>(data);
                try
                {
                    var task = _client.PlainTextAsync(json);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// ThreadSafe client
        /// </summary>
        public class ApiClient
        {
            private readonly HttpClient _client;
            private readonly string _endpointPlainText;
            private readonly string _endpointSum;

            public ApiClient(string endpoint)
            {
                _client = new HttpClient();
                _endpointPlainText = endpoint + "/plaintext";
                _endpointSum = endpoint + "/sum";
            }

            public async Task PlainTextAsync(string json)
            {
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var res = await _client.PostAsync(_endpointPlainText, content);
                res.EnsureSuccessStatusCode();
            }

            public async Task SumAsync(int x, int y)
            {
                var res = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Post, _endpointSum + $"?x={x}&y={y}"));
                res.EnsureSuccessStatusCode();
            }
        }
    }
}
