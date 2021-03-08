using Benchmark.ClientLib.Reports;
using Benchmark.Server.Shared;
using Grpc.Net.Client;
using MagicOnion.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Scenarios
{
    public class HubBenchmarkScenario : IBenchmarkHubReciever, IAsyncDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly BenchReporter _reporter;
        private IBenchmarkHub _client;
        private ConcurrentDictionary<string, Exception> _errors = new ConcurrentDictionary<string, Exception>();

        public HubBenchmarkScenario(GrpcChannel channel, BenchReporter reporter)
        {
            _channel = channel;
            _reporter = reporter;
        }

        public async Task Run(int requestCount)
        {
            using (var statistics = new Statistics(nameof(ConnectAsync)))
            {
                await ConnectAsync("console-client");

                _reporter.AddBenchDetail(new BenchReportItem
                {
                    ExecuteId = _reporter.ExecuteId,
                    ClientId = _reporter.ClientId,
                    TestName = nameof(ConnectAsync),
                    Begin = statistics.Begin,
                    End = DateTime.UtcNow,
                    Duration = statistics.Elapsed,
                    RequestCount = 0, // connect is setup, not count as request.
                    Errors =_errors.Count,
                    Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                });
                statistics.HasError(_errors.Count);
            }

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
                    Errors = _errors.Count,
                    Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                });
                statistics.HasError(_errors.Count);
            }

            using (var statistics = new Statistics(nameof(EndAsync)))
            {
                await EndAsync();
                _reporter.AddBenchDetail(new BenchReportItem
                {
                    ExecuteId = _reporter.ExecuteId,
                    ClientId = _reporter.ClientId,
                    TestName = nameof(EndAsync),
                    Begin = statistics.Begin,
                    End = DateTime.UtcNow,
                    Duration = statistics.Elapsed,
                    RequestCount = 0, // end is teardown, not count as request.
                    Errors = _errors.Count,
                    Type = nameof(Grpc.Core.MethodType.DuplexStreaming),
                });
                statistics.HasError(_errors.Count);
            }
        }

        private async Task ConnectAsync(string roomName)
        {
            try
            {
                _client = await StreamingHubClient.ConnectAsync<IBenchmarkHub, IBenchmarkHubReciever>(_channel, this);
                var name = Guid.NewGuid().ToString();
                await _client.Ready(roomName, name, "plaintext");
            }
            catch (Exception ex)
            {
                _errors.TryAdd(ex.GetType().FullName, ex);
            }
        }

        private async Task PlainTextAsync(int requestCount)
        {
            for (var i = 0; i < requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                try
                {
                    await _client.Process(data);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
        }

        private async Task PlainTextParallelAsync(int requestCount)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < requestCount; i++)
            {
                var data = new BenchmarkData
                {
                    PlainText = i.ToString(),
                };
                try
                {
                    var task = _client.Process(data);
                    tasks.Add(task);
                }
                catch (Exception ex)
                {
                    _errors.TryAdd(ex.GetType().FullName, ex);
                }
            }
            await Task.WhenAll(tasks);
        }

        private async Task EndAsync()
        {
            try
            {
                await _client.End();
            }
            catch (Exception ex)
            {
                _errors.TryAdd(ex.GetType().FullName, ex);
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            await _client.DisposeAsync();
        }

        void IBenchmarkHubReciever.OnStart(string requestType)
        {
            throw new NotImplementedException();
        }
        void IBenchmarkHubReciever.OnProcess()
        {
            throw new NotImplementedException();
        }
        void IBenchmarkHubReciever.OnEnd()
        {
            throw new NotImplementedException();
        }
    }
}
