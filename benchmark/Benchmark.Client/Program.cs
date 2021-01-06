using Benchmark.Client;
using Benchmark.Client.Converters;
using Benchmark.Client.LoadTester;
using Benchmark.Client.Reports;
using Benchmark.Client.Scenarios;
using Benchmark.Client.Storage;
using Benchmark.Client.Utils;
using ConsoleAppFramework;
using Grpc.Net.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ZLogger;

var builder = Host.CreateDefaultBuilder()
    .ConfigureLogging((hostContext, logging) =>
    {
        logging.ClearProviders();
        logging.AddZLoggerConsole(configure => configure.EnableStructuredLogging = false);
        logging.SetMinimumLevel(LogLevel.Trace);
    });
if (Environment.GetEnvironmentVariable("BENCHCLIENT_RUNASWEB") == "true")
{
    var hostAddress = Environment.GetEnvironmentVariable("BENCHCLIENT_HOSTADDRESS") ?? "http://localhost:8080";
    await builder.RunConsoleAppFrameworkWebHostingAsync(hostAddress);
}
else
{
    await builder.RunConsoleAppFrameworkAsync(args);
}

public class BenchmarkRunner : ConsoleAppBase
{
    private readonly string _path;
    public BenchmarkRunner()
    {
        _path = Environment.GetEnvironmentVariable("BENCHCLIENT_S3BUCKET") ?? "bench-magiconion-s3-bucket-5c7e45b";
    }

    private string GetReportId() => DateTime.UtcNow.ToString("yyyyMMddHHmmss.fff") + "-" + Guid.NewGuid().ToString();

    /// <summary>
    /// Run Unary and Hub Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchAll(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = GetReportId();

        var executeId = Guid.NewGuid().ToString();
        Context.Logger.LogInformation($"reportId: {reportId}");
        Context.Logger.LogInformation($"executeId: {executeId}");

        var reporter = new BenchReporter(reportId, executeId, Dns.GetHostName());
        var iterationInts = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        reporter.Begin();
        {
            foreach (var iteration in iterationInts)
            {
                // Connect to the server using gRPC channel.
                var channel = GrpcChannel.ForAddress(hostAddress);

                // Unary
                Context.Logger.LogInformation($"Begin unary {iteration} requests.");
                var unary = new UnaryBenchmarkScenario(channel, reporter);
                await unary.Run(iteration);

                // StreamingHub
                Context.Logger.LogInformation($"Begin Streaming {iteration} requests.");
                await using var hub = new HubBenchmarkScenario(channel, reporter);
                await hub.Run(iteration);
            }
        }
        reporter.End();

        // output
        var benchJson = reporter.ToJson();

        // put json to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: Context.CancellationToken);
    }

    /// <summary>
    /// Run Unary Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchUnary(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = GetReportId();

        var executeId = Guid.NewGuid().ToString();
        Context.Logger.LogInformation($"reportId: {reportId}");
        Context.Logger.LogInformation($"executeId: {executeId}");

        var reporter = new BenchReporter(reportId, executeId, Dns.GetHostName());
        var iterationInts = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        reporter.Begin();
        {
            foreach (var iteration in iterationInts)
            {
                // Connect to the server using gRPC channel.
                var channel = GrpcChannel.ForAddress(hostAddress);

                // Unary
                Context.Logger.LogInformation($"Begin unary {iteration} requests.");
                var unary = new UnaryBenchmarkScenario(channel, reporter);
                await unary.Run(iteration);
            }
        }
        reporter.End();

        // output
        var benchJson = reporter.ToJson();

        // put json to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: Context.CancellationToken);
    }

    /// <summary>
    /// Run Hub Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchHub(string hostAddress = "http://localhost:5000", string iterations = "256,1024,4096,16384", string reportId = "")
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = GetReportId();

        var executeId = Guid.NewGuid().ToString();
        Context.Logger.LogInformation($"reportId: {reportId}");
        Context.Logger.LogInformation($"executeId: {executeId}");

        var reporter = new BenchReporter(reportId, executeId, Dns.GetHostName());
        var iterationInts = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        reporter.Begin();
        {
            foreach (var iteration in iterationInts)
            {
                // Connect to the server using gRPC channel.
                var channel = GrpcChannel.ForAddress(hostAddress);

                // StreamingHub
                Context.Logger.LogInformation($"Begin Streaming {iteration} requests.");
                await using var hub = new HubBenchmarkScenario(channel, reporter);
                await hub.Run(iteration);
            }
        }
        reporter.End();

        // output
        var benchJson = reporter.ToJson();

        // put json to s3
        var storage = StorageFactory.Create(Context.Logger);
        await storage.Save(_path, $"reports/{reporter.ReportId}", reporter.GetJsonFileName(), benchJson, ct: Context.CancellationToken);
    }

    /// <summary>
    /// List Reports
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task<string[]> ListReports(string reportId)
    {
        // access s3 and List json from reportId
        var storage = StorageFactory.Create(Context.Logger);
        var reports = await storage.List(_path, $"reports/{reportId}", Context.CancellationToken);
        foreach (var report in reports)
        {
            Context.Logger.LogInformation(report);
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
        var storage = StorageFactory.Create(Context.Logger);
        var reportJsons = await storage.Get(_path, $"reports/{reportId}", Context.CancellationToken);
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
    /// <param name="htmlFileName"></param>
    /// <returns></returns>
    public async Task GenerateHtml(string reportId, string htmlFileName = "index.html")
    {
        // access s3 and download json from reportId
        var reports = await GetReports(reportId);

        // generate html based on json data
        var htmlReporter = new HtmlBenchReporter();
        var htmlReport = htmlReporter.CreateReport(reports);
        var page = new BenchmarkReportPageTemplate()
        {
            Report = htmlReport,
        };
        var content = NormalizeNewLineLf(page.TransformText());

        // upload html report to s3
        var storage = StorageFactory.Create(Context.Logger);
        var outputUri = await storage.Save(_path, $"html/{reportId}", htmlFileName, content, overwrite: true, Context.CancellationToken);

        Context.Logger.LogInformation($"HtmlReport Uri: {outputUri}");
    }

    public async Task<ClientInfo[]> ListClients()
    {
        // call ssm to list up client instanceids
        var loadTester = LoadTesterFactory.Create(Context.Logger, this);
        var clients = await loadTester.ListClients();
        var json = JsonConvert.Serialize(clients);
        Context.Logger.LogInformation(json);
        return clients;
    }

    public async Task RunAllClient(int processCount, string iterations = "256,1024,4096,16384", string benchCommand = "benchall", string hostAddress = "http://localhost:5000", string reportId = "")
    {
        if (string.IsNullOrEmpty(reportId))
            reportId = GetReportId();

        Context.Logger.LogInformation($"reportId: {reportId}");

        // call ssm to execute Clients via CLI mode.
        var loadTester = LoadTesterFactory.Create(Context.Logger, this);
        try
        {
            await loadTester.Run(processCount, iterations, benchCommand, hostAddress, reportId, Context.CancellationToken);
        }
        catch (Exception ex)
        {
            Context.Logger.LogError(ex, "Run failed.");
        }

        // Generate Html Report
        await GenerateHtml(reportId);
    }

    public async Task CancelCommands(string status = "InProgress")
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
                        Value = status,
                    }
                },            
        }, Context.CancellationToken);

        foreach (var command in commands.CommandInvocations)
        {
            Context.Logger.LogInformation($"Cancelling {command.CommandId}");
            await client.CancelCommandAsync(new Amazon.SimpleSystemsManagement.Model.CancelCommandRequest
            {
                CommandId = command.CommandId,
            }, Context.CancellationToken);
        }
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
