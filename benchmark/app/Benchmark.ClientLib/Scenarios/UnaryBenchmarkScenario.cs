using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Runtime;
using Benchmark.Server.Shared;
using Benchmark.Shared;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class UnaryBenchmarkScenario
    {
        private readonly IBenchmarkService _client;
        private readonly BenchReporter _reporter;
        private readonly BenchmarkerConfig _config;
        private ConcurrentDictionary<string, Exception> _errors = new ConcurrentDictionary<string, Exception>();

        public UnaryBenchmarkScenario(GrpcChannel channel, BenchReporter reporter, BenchmarkerConfig config)
        {
            _client = MagicOnionClient.Create<IBenchmarkService>(channel);
            _reporter = reporter;
            _config = config;
        }

        public async Task Run(int requestCount, CancellationToken ct)
        {
            using (var statistics = new Statistics(nameof(PlainTextAsync)))
            {
                await PlainTextAsync(requestCount, ct);

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
            var tasks = new List<UnaryResult<int>>();
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
            await ValueTaskUtils.WhenAll(tasks);
        }

        private async Task PlainTextAsync(int requestCount, CancellationToken ct)
        {
            var data = new BenchmarkData
            {
                PlainText = _config.GetRequestPayload(),
            };
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    await _client.PlainTextAsync(data);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
        }

        /// <summary>
        /// Run until timeout happen.
        /// </summary>
        /// <param name="requestCount"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task PlainTextConcurrentAsync(int requestCount, CancellationToken ct)
        {
            var data = new BenchmarkData
            {
                PlainText = _config.GetRequestPayload(),
            };

            using var pool = new UnaryResultPool<Nil>(_config.ClientConcurrency, ct);
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    pool.RegisterAsync(() => _client.PlainTextAsync(data)).FireAndForget();
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
            await pool.WaitForCompleteAsync();
            await Task.WhenAny(pool.Completed, pool.WaitForTimeout());
        }

        private async Task PlainTextAsyncParallel(int requestCount, CancellationToken ct)
        {
            var data = new BenchmarkData
            {
                PlainText = _config.GetRequestPayload(),
            };
            var tasks = new List<UnaryResult<Nil>>();
            for (var i = 0; i < requestCount; i++)
            {
                try
                {
                    var task = _client.PlainTextAsync(data);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
            await ValueTaskUtils.WhenAll(tasks);
        }
    }
}
