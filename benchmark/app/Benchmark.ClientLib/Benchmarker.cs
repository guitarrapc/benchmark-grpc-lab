using Benchmark.ClientLib.Converters;
using Benchmark.ClientLib.LoadTester;
using Benchmark.ClientLib.Reports;
using Benchmark.ClientLib.Scenarios;
using Benchmark.ClientLib.Storage;
using Benchmark.ClientLib.Utils;
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
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                //// single channel
                //// Connect to the server using gRPC channel.
                //var channel = GetOrCreateChannel(hostAddress);
                //var scenario = new UnaryBenchmarkScenario(channel, reporter);

                foreach (var iteration in Config.TotalRequests)
                {
                    // separate channel
                    // Connect to the server using gRPC channel.
                    var channel = CreateGrpcChannel(hostAddress);
                    var scenario = new UnaryBenchmarkScenario(channel, reporter, Config);

                    _logger?.LogInformation($"Begin unary {iteration} requests.");
                    await scenario.Run(iteration, _cancellationToken);
                }
            }
            reporter.End();

            await OutputAsync(reporter);
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
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                // single channel
                //// Connect to the server using gRPC channel.
                //var channel = GetOrCreateChannel(hostAddress);
                //await using var scenario = new HubBenchmarkScenario(channel, reporter);

                foreach (var iteration in Config.TotalRequests)
                {
                    // separate channel
                    var channel = CreateGrpcChannel(hostAddress);
                    await using var scenario = new HubBenchmarkScenario(channel, reporter, Config);

                    _logger?.LogInformation($"Begin Streaming {iteration} requests.");
                    await scenario.Run(iteration);
                }
            }
            reporter.End();

            await OutputAsync(reporter);
        }

        /// <summary>
        /// Run Hub Benchmark for LongRun Serverside wait
        /// </summary>
        /// <param name="waitMilliseconds"></param>
        /// <param name="parallel"></param>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchLongRunHub(int waitMilliseconds, bool parallel = false, string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                //// single chnannel
                //var channel = GetOrCreateChannel(hostAddress);
                //await using var scenario = new HubLongRunBenchmarkScenario(channel, reporter);

                foreach (var iteration in Config.TotalRequests)
                {
                    // separate channel
                    // Connect to the server using gRPC channel.
                    var channel = CreateGrpcChannel(hostAddress);
                    await using var scenario = new HubLongRunBenchmarkScenario(channel, reporter);

                    _logger?.LogInformation($"Begin LongRun Streaming {iteration} requests.");
                    await scenario.Run(iteration, waitMilliseconds, parallel);
                }
            }
            reporter.End();

            await OutputAsync(reporter);
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
        public async Task BenchCCoreLongRunHub(int waitMilliseconds, bool insecure = true, bool parallel = false, string hostAddress = "localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId);
            reporter.Begin();
            {
                var credentials = !insecure
                    ? Config.UseSelfCertEndpoint
                        ? new SslCredentials(File.ReadAllText("server.local.crt"))
                        : new SslCredentials()
                    : ChannelCredentials.Insecure;

                //// single chnannel
                //var channel = GetOrCreateCCoreChannel(hostAddress, credentials);
                //await using var scenario = new HubLongRunBenchmarkScenario(channel, reporter);

                foreach (var iteration in Config.TotalRequests)
                {
                    // separate channel
                    // Connect to the server using gRPC channel.
                    var channel = CreateCCoreChannel(hostAddress, credentials);
                    await using var scenario = new CCoreHubLongRunBenchmarkScenario(channel, reporter);

                    _logger?.LogInformation($"Begin Ccore LongRun Streaming {iteration} requests.");
                    await scenario.Run(iteration, waitMilliseconds, parallel);
                }
            }
            reporter.End();

            await OutputAsync(reporter);
        }

        /// <summary>
        /// Run Grpc Benchmark
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public async Task BenchGrpc(string hostAddress = "http://localhost:5000", string reportId = "")
        {
            if (string.IsNullOrEmpty(reportId))
                reportId = NewReportId();

            var executeId = Guid.NewGuid().ToString();
            _logger?.LogInformation($"reportId: {reportId}");
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.GrpcDotnet);
            reporter.Begin();
            {
                // single channel
                // Connect to the server using gRPC channel.
                //var channel = GetOrCreateChannel(hostAddress);
                //var scenario = new GrpcBenchmarkScenario(channel, reporter);

                foreach (var iteration in Config.TotalRequests)
                {
                    // separate channel
                    // Connect to the server using gRPC channel.
                    var channel = CreateGrpcChannel(hostAddress);
                    var scenario = new GrpcBenchmarkScenario(channel, reporter, Config);

                    _logger?.LogInformation($"Begin grpc {iteration} requests.");
                    await scenario.Run(iteration);
                }
            }
            reporter.End();

            await OutputAsync(reporter);
        }

        /// <summary>
        /// Run Grpc Benchmark
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
            _logger?.LogInformation($"executeId: {executeId}");

            var reporter = new BenchReporter(reportId, _clientId, executeId, Framework.AspnetCore);
            reporter.Begin();
            {
                // single thread-safe client
                var apiClient = new ApiBenchmarkScenario.ApiClient(hostAddress);
                foreach (var iteration in Config.TotalRequests)
                {
                    var scenario = new ApiBenchmarkScenario(apiClient, reporter, Config);

                    _logger?.LogInformation($"Begin api {iteration} requests.");
                    await scenario.Run(iteration);
                }
            }
            reporter.End();

            await OutputAsync(reporter);
        }

        private async Task OutputAsync(BenchReporter reporter)
        {
            // output
            var benchJson = reporter.ToJson();

            // put json to s3
            var storage = StorageFactory.Create(_logger);
            await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: _cancellationToken);
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
        public async Task GenerateHtml(string reportId, bool generateDetail = true, string htmlFileName = "index.html")
        {
            // access s3 and download json from reportId
            var reports = await GetReports(reportId);
            if (!reports.Any())
                return;

            // generate html based on json data
            var htmlReporter = new HtmlBenchReporter();
            var htmlReport = htmlReporter.CreateReport(reports, generateDetail);
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
            await GenerateHtml(reportId);
        }

        public async Task CancelCommands()
        {
            var config = AmazonUtils.IsAmazonEc2()
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
        /// Get GrpcChannel from cache or create GrpcChannel if not exists.
        /// </summary>
        /// <param name="hostAddress">http style address. e.g. http://localhost:5000</param>
        /// <returns></returns>
        private GrpcChannel GetOrGrpcCreateChannel(string hostAddress)
        {
            return _grpcChannelCache.GetOrAdd(hostAddress, CreateGrpcChannel(hostAddress));
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
                EnableMultipleHttp2Connections = true,
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
            });
        }

        /// <summary>
        /// Get CCore Channel from cache or create Channel if not exists.
        /// </summary>
        /// <param name="hostAddress">IP:Port style address. e.g. localhost:5000</param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private Channel GetOrCreateCCoreChannel(string hostAddress, ChannelCredentials credentials)
        {
            return _ccoreChannelCache.GetOrAdd(hostAddress, CreateCCoreChannel(hostAddress, credentials));
        }
        /// <summary>
        /// Create CCore Channel
        /// </summary>
        /// <param name="hostAddress">IP:Port style address. e.g. localhost:5000</param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private Channel CreateCCoreChannel(string hostAddress, ChannelCredentials credentials)
        {
            return new Channel(hostAddress, credentials);
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
