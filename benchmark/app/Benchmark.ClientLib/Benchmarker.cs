using Benchmark.ClientLib.Converters;
using Benchmark.ClientLib.Internal;
using Benchmark.ClientLib.LoadTester;
using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Scenarios;
using Benchmark.ClientLib.Storage;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.ClientLib
{
    public class Benchmarker
    {
        private readonly string _path;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly string _clientId = Guid.NewGuid().ToString();
        private readonly ConcurrentDictionary<string, GrpcChannel> _grpcChannelCache = new ConcurrentDictionary<string, GrpcChannel>();
        private readonly ConcurrentDictionary<string, Channel> _ccoreChannelCache = new ConcurrentDictionary<string, Channel>();

        public BenchmarkerConfig Config { get; init; } = new BenchmarkerConfig();

        public Benchmarker(string path, ILogger logger, CancellationToken cancellationToken)
        {
            _path = path;
            _logger = logger;
            _cancellationToken = cancellationToken;
            ThreadPools.ModifyThreadPool(Environment.ProcessorCount * 8, Environment.ProcessorCount * 8, logger);
        }

        private static string NewReportId() => DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();

        /// <summary>
        /// Run Unary Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchUnary(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");

            var channels = Enumerable.Range(0, Config.ClientConnections).Select(x => CreateGrpcChannel(hostAddress)).ToArray();

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.MagicOnion, nameof(UnaryBenchmarkScenario), Config);
            reporter.Begin();
            {
                foreach (var request in Config.TotalRequests)
                {
                    _logger?.LogInformation($"Begin unary benchmark. requests {request}, concurrency {Config.ClientConcurrency}, connections {Config.ClientConnections}");
                    var scenario = new UnaryBenchmarkScenario(channels, reporter, Config);
                    await scenario.Run(request, _cancellationToken);
                }
            }
            reporter.End();

            await OutputReportAsync(reporter, Config.GenerateHtmlReportAfterBench);
        }

        /// <summary>
        /// Run Hub Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchHub(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");

            var channels = Enumerable.Range(0, Config.ClientConnections).Select(x => CreateGrpcChannel(hostAddress)).ToArray();

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.MagicOnion, nameof(HubBenchmarkScenario), Config);
            reporter.Begin();
            {
                foreach (var request in Config.TotalRequests)
                {
                    _logger?.LogInformation($"Begin Streaming benchmark. requests {request}, concurrency {Config.ClientConcurrency}, connections {Config.ClientConnections}");
                    await using var scenario = new HubBenchmarkScenario(channels, reporter, Config);
                    await scenario.Run(request, _cancellationToken);
                }
            }
            reporter.End();

            await OutputReportAsync(reporter, Config.GenerateHtmlReportAfterBench);
        }

        /// <summary>
        /// Run Hub Benchmark for LongRun Serverside wait
        /// </summary>
        /// <param name="waitMilliseconds"></param>
        /// <param name="parallel"></param>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchLongRunHub(int waitMilliseconds, string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");

            var channels = Enumerable.Range(0, Config.ClientConnections).Select(x => CreateGrpcChannel(hostAddress)).ToArray();

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.MagicOnion, nameof(HubLongRunBenchmarkScenario), Config);
            reporter.Begin();
            {
                foreach (var request in Config.TotalRequests)
                {
                    _logger?.LogInformation($"Begin LongRunHub Streaming benchmark. requests {request}, concurrency {Config.ClientConcurrency}, connections {Config.ClientConnections}");
                    await using var scenario = new HubLongRunBenchmarkScenario(channels, reporter, Config);
                    await scenario.Run(request, waitMilliseconds, _cancellationToken);
                }
            }
            reporter.End();

            await OutputReportAsync(reporter, Config.GenerateHtmlReportAfterBench);
        }

        /// <summary>
        /// Run Hub Benchmark for LongRun Serverside wait
        /// </summary>
        /// <param name="waitMilliseconds"></param>
        /// <param name="insecure"></param>
        /// <param name="parallel"></param>
        /// <param name="hostAddress">IP:Port Style address.</param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchCCoreLongRunHub(int waitMilliseconds, bool insecure = true, string hostAddress = "localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");

            var credentials = !insecure
                ? Config.UseSelfCertEndpoint
                    ? new SslCredentials(File.ReadAllText("server.local.crt"))
                    : new SslCredentials()
                : ChannelCredentials.Insecure;
            var channels = Enumerable.Range(0, Config.ClientConnections).Select(x => CreateCCoreChannel(hostAddress, credentials)).ToArray();

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.GrpcCcore, nameof(CCoreHubLongRunBenchmarkScenario), Config);
            reporter.Begin();
            {
                foreach (int request in Config.TotalRequests)
                {
                    _logger?.LogInformation($"Begin LongRun C-CoreStreaming benchmark. requests {request}, concurrency {Config.ClientConcurrency}, connections {Config.ClientConnections}");
                    await using var scenario = new CCoreHubLongRunBenchmarkScenario(channels, reporter, Config);
                    await scenario.Run(request, waitMilliseconds, _cancellationToken);
                }
            }
            reporter.End();

            await OutputReportAsync(reporter, Config.GenerateHtmlReportAfterBench);
        }

        /// <summary>
        /// Run Grpc Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchGrpc(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            Config.Validate();
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");

            var channels = Enumerable.Range(0, Config.ClientConnections).Select(x => CreateGrpcChannel(hostAddress)).ToArray();

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.GrpcDotnet, nameof(GrpcBenchmarkScenario), Config);
            reporter.Begin();
            {
                foreach (int request in Config.TotalRequests)
                {
                    _logger?.LogInformation($"Begin grpc benchmark. requests {request}, concurrency {Config.ClientConcurrency}, connections {Config.ClientConnections}");
                    var scenario = new GrpcBenchmarkScenario(channels, reporter, Config);
                    await scenario.Run(request, _cancellationToken);
                }
            }
            reporter.End();

            await OutputReportAsync(reporter, Config.GenerateHtmlReportAfterBench);
        }

        /// <summary>
        /// Run REST Api Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchApi(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");

            // single thread-safe client
            var apiClients = Enumerable.Range(0, Config.ClientConnections).Select(x => new ApiBenchmarkScenario.ApiClient(hostAddress)).ToArray();

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.AspnetCore, nameof(ApiBenchmarkScenario), Config);
            reporter.Begin();
            {
                foreach (var request in Config.TotalRequests)
                {
                    _logger?.LogInformation($"Begin api benchmark. requests {request}, concurrency {Config.ClientConcurrency}, connections {Config.ClientConnections}");
                    var scenario = new ApiBenchmarkScenario(apiClients, reporter, Config);
                    await scenario.Run(request, _cancellationToken);
                }
            }
            reporter.End();

            await OutputReportAsync(reporter, Config.GenerateHtmlReportAfterBench);
        }

        private async Task OutputReportAsync(BenchReporter reporter, bool generateHtmlReport)
        {
            // output
            var benchJson = reporter.ToJson();

            // save json
            var storage = StorageFactory.Create(_logger);
            var jsonOutput = await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: _cancellationToken);
            _logger?.LogInformation($"JsonReport Uri: {jsonOutput}");

            // generate html report
            if (generateHtmlReport)
            {
                await GenerateHtmlAsync(reporter.ReportId);
            }

            ConsoleOutput(reporter.Report);
        }

        /// <summary>
        /// List Reports
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task<string[]> ListReports(string reportId)
        {
            // access s3 and List json from reportId
            var storage = StorageFactory.Create(_logger);
            var reports = await storage.List(_path, $"reports/{reportId}", _cancellationToken);
            _logger.LogInformation($"Total {reports.Length} reports");
            foreach (var report in reports)
            {
                _logger?.LogInformation(report);
            }
            return reports;
        }

        /// <summary>
        /// Get Report
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task<BenchReport[]> GetReports(string reportId)
        {
            // access s3 and get jsons from reportId
            var storage = StorageFactory.Create(_logger);
            var reportJsons = await storage.Get(_path, $"reports/{reportId}", _cancellationToken);
            var reports = new List<BenchReport>();
            foreach (var json in reportJsons)
            {
                var report = JsonConvert.Deserialize<BenchReport>(json);
                reports.Add(report);
            }
            return reports.ToArray();
        }

        /// <summary>
        /// Generate Report Html
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="generateDetail"></param>
        /// <param name="htmlFileName"></param>
        /// <returns></returns>
        public async Task GenerateHtmlAsync(string reportId, string htmlFileName = "index.html")
        {
            // access s3 and download json from reportId
            var reports = await GetReports(reportId);
            if (!reports.Any())
                return;

            // generate html based on json data
            var htmlReporter = new HtmlReporter();
            var htmlReport = htmlReporter.CreateReport(reports);
            var page = new BenchmarkReportPageTemplate()
            {
                Report = htmlReport,
            };
            var content = NormalizeNewLineLf(page.TransformText());

            // upload html report to s3
            var storage = StorageFactory.Create(_logger);
            var outputUri = await storage.Save(_path, $"html/{reportId}", htmlFileName, content, overwrite: true, _cancellationToken);

            _logger?.LogInformation($"HtmlReport Uri: {outputUri}");
        }

        public async Task<ClientInfo[]> ListClients()
        {
            // call ssm to list up client instanceids
            var loadTester = LoadTesterFactory.Create(_logger, this);
            var clients = await loadTester.ListClients();
            var json = JsonConvert.Serialize(clients);
            _logger?.LogInformation(json);
            return clients;
        }

        public async Task RunAllClient(int processCount, string iterations = "256,1024,4096,16384", string benchCommand = "benchall", string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            _logger?.LogInformation($"reportId: {reportId}");

            // call ssm to execute Clients via CLI mode.
            var loadTester = LoadTesterFactory.Create(_logger, this);
            try
            {
                await loadTester.Run(processCount, iterations, benchCommand, hostAddress, reportId, _cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Run failed.");
            }

            // Generate Html Report
            await GenerateHtmlAsync(reportId);
        }

        public async Task CancelCommands()
        {
            var config = AmazonEnvironment.IsAmazonEc2()
                ? new Amazon.SimpleSystemsManagement.AmazonSimpleSystemsManagementConfig
                {
                    RegionEndpoint = Amazon.Util.EC2InstanceMetadata.Region,
                }
                : new Amazon.SimpleSystemsManagement.AmazonSimpleSystemsManagementConfig
                {
                    RegionEndpoint = Amazon.RegionEndpoint.APNortheast1,
                };
            var client = new Amazon.SimpleSystemsManagement.AmazonSimpleSystemsManagementClient(config);
            var commands = await client.ListCommandInvocationsAsync(new Amazon.SimpleSystemsManagement.Model.ListCommandInvocationsRequest
            {
                Filters = new List<Amazon.SimpleSystemsManagement.Model.CommandFilter>
                {
                    new Amazon.SimpleSystemsManagement.Model.CommandFilter
                    {
                        Key = "Status",
                        Value = "InProgress",
                    }
                },
            }, _cancellationToken);

            foreach (var command in commands.CommandInvocations)
            {
                _logger?.LogInformation($"Cancelling {command.CommandId}");
                await client.CancelCommandAsync(new Amazon.SimpleSystemsManagement.Model.CancelCommandRequest
                {
                    CommandId = command.CommandId,
                }, _cancellationToken);
            }
        }

        /// <summary>
        /// Create GrpcChannel
        /// </summary>
        /// <param name="hostAddress">http style address. e.g. http://localhost:5000</param>
        /// <returns></returns>
        private GrpcChannel CreateGrpcChannel(string hostAddress)
        {
            var handler = new SocketsHttpHandler
            {
                // default HTTP/2 MutipleConnections = 100, true enable additional HTTP/2 connection via channel.
                // memo: create Channel Pool and random get pool for each connection to avoid too match channel connection.
                EnableMultipleHttp2Connections = false,
                // Enable KeepAlive to keep HTTP/2 while non-active status.
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                ConnectTimeout = TimeSpan.FromSeconds(10),
            };
            if (Config.UseSelfCertEndpoint)
            {
                // allow non trusted certificate
                RemoteCertificateValidationCallback validationHandler = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true;
                handler.SslOptions = new SslClientAuthenticationOptions
                {
                    RemoteCertificateValidationCallback = validationHandler,
                };
            }

            return GrpcChannel.ForAddress(hostAddress, new GrpcChannelOptions
            {
                HttpHandler = handler,
                MaxReceiveMessageSize = int.MaxValue,
                MaxSendMessageSize = int.MaxValue,
            });
        }

        /// <summary>
        /// Create CCore Channel
        /// </summary>
        /// <param name="hostAddress">IP:Port style address. e.g. localhost:5000</param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private Channel CreateCCoreChannel(string hostAddress, ChannelCredentials credentials)
        {
            return new Channel(hostAddress, credentials, new ChannelOption[]
            {
                new ChannelOption("grpc.keepalive_time_ms", 60_000),
                new ChannelOption("grpc.keepalive_timeout_ms", 30_000),
                new ChannelOption("grpc.max_receive_message_length", int.MaxValue),
                new ChannelOption("grpc.max_send_message_length", int.MaxValue),

            });
        }

        /// <summary>
        /// Console Output Report
        /// </summary>
        /// <param name="report"></param>
        private void ConsoleOutput(BenchReport report)
        {
            var template = new BenchmarkConsoleOutputTemplate(report);
            var content = NormalizeNewLineLf(template.TransformText());
            _logger?.LogInformation(content);
        }

        private static string NormalizeNewLine(string content)
        {
            return content
                .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase)
                .Replace("\n", Environment.NewLine, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeNewLineLf(string content)
        {
            return content
                .Replace("\r\n", "\n", StringComparison.OrdinalIgnoreCase);
        }
    }
}
