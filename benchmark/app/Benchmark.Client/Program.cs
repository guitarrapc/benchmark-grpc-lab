using Benchmark.ClientLib;
using Benchmark.ClientLib.Utils;
using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using ZLogger;


ThreadPools.ModifyThreadPool(Environment.ProcessorCount * 8, Environment.ProcessorCount * 8);

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
    if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("BENCHCLIENT_DELAY")))
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
    }
    await builder.RunConsoleAppFrameworkAsync(args);
}

public class BenchmarkRunner : ConsoleAppBase
{
    private readonly string _path;
    public BenchmarkRunner()
    {
        _path = Environment.GetEnvironmentVariable("BENCHCLIENT_S3BUCKET") ?? throw new ArgumentNullException("Environment variable 'BENCHCLIENT_S3BUCKET' is not defined.");
    }

    private bool IsHttpsEndpoint(string endpoint) => endpoint.StartsWith("https://");

    /// <summary>
    /// Run Unary Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="iterations"></param>
    /// <param name="duration"></param>
    /// <param name="concurrency"></param>
    /// <param name="connections"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchUnary(string hostAddress = "http://localhost:5000", string iterations = "1", string duration = "30s", int concurrency = 50, int connections = 50, string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken)
        {
            Config = new BenchmarkerConfig
            {
                ClientConcurrency = concurrency,
                ClientConnections = connections,
                Duration = duration,
                TotalRequests = iter,
                UseSelfCertEndpoint = IsHttpsEndpoint(hostAddress),
            }
        };
        await benchmarker.BenchUnary(hostAddress, reportId);
    }

    /// <summary>
    /// Run Hub Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="iterations"></param>
    /// <param name="duration"></param>
    /// <param name="concurrency"></param>
    /// <param name="connections"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchHub(string hostAddress = "http://localhost:5000", string iterations = "1", string duration = "30s", int concurrency = 50, int connections = 50, string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken)
        {
            Config = new BenchmarkerConfig
            {
                ClientConcurrency = concurrency,
                ClientConnections = connections,
                Duration = duration,
                TotalRequests = iter,
                UseSelfCertEndpoint = IsHttpsEndpoint(hostAddress),
            }
        };
        await benchmarker.BenchHub(hostAddress, reportId);
    }

    /// <summary>
    /// Run Long running Hub Benchmark
    /// </summary>
    /// <param name="waitMilliseconds"></param>
    /// <param name="hostAddress"></param>
    /// <param name="iterations"></param>
    /// <param name="duration"></param>
    /// <param name="concurrency"></param>
    /// <param name="connections"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchLongRunHub(int waitMilliseconds, string hostAddress = "http://localhost:5000", string iterations = "1", string duration = "30s", int concurrency = 50, int connections = 50, string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken)
        {
            Config = new BenchmarkerConfig
            {
                ClientConcurrency = concurrency,
                ClientConnections = connections,
                Duration = duration,
                TotalRequests = iter,
                UseSelfCertEndpoint = IsHttpsEndpoint(hostAddress),
            }
        };
        await benchmarker.BenchLongRunHub(waitMilliseconds, hostAddress, reportId);
    }

    /// <summary>
    /// Run Grpc Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="iterations"></param>
    /// <param name="duration"></param>
    /// <param name="concurrency"></param>
    /// <param name="connections"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchGrpc(string hostAddress = "http://localhost:5000", string iterations = "1", string duration = "30s", int concurrency = 50, int connections = 50, string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken)
        {
            Config = new BenchmarkerConfig
            {
                ClientConcurrency = concurrency,
                ClientConnections = connections,
                Duration = duration,
                TotalRequests = iter,
                UseSelfCertEndpoint = IsHttpsEndpoint(hostAddress),
            }
        };
        await benchmarker.BenchGrpc(hostAddress, reportId);
    }

    /// <summary>
    /// Run REST Api Benchmark
    /// </summary>
    /// <param name="hostAddress"></param>
    /// <param name="iterations"></param>
    /// <param name="duration"></param>
    /// <param name="concurrency"></param>
    /// <param name="connections"></param>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task BenchApi(string hostAddress = "http://localhost:5000", string iterations = "1", string duration = "30s", int concurrency = 50, int connections = 50, string reportId = "")
    {
        var iter = iterations.Split(',').Select(x => int.Parse(x.Trim())).ToArray();
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken)
        {
            Config = new BenchmarkerConfig
            {
                ClientConcurrency = concurrency,
                ClientConnections = connections,
                Duration = duration,
                TotalRequests = iter,
                UseSelfCertEndpoint = IsHttpsEndpoint(hostAddress),
            }
        };
        await benchmarker.BenchApi(hostAddress, reportId);
    }

    /// <summary>
    /// List Reports
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task ListReports(string reportId)
    {
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken);
        await benchmarker.ListReports(reportId);
    }

    /// <summary>
    /// Get Report
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task GetReports(string reportId)
    {
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken);
        await benchmarker.GetReports(reportId);
    }

    /// <summary>
    /// Generate Report Html
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="htmlFileName"></param>
    /// <returns></returns>
    public async Task GenerateHtml(string reportId, bool generateDetail, string htmlFileName = "index.html")
    {
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken);
        await benchmarker.GenerateHtml(reportId, generateDetail, htmlFileName);
    }

    public async Task ListClients()
    {
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken);
        await benchmarker.ListClients();
    }

    public async Task RunAllClient(int processCount, string iterations = "256,1024,4096,16384", string benchCommand = "benchall", string hostAddress = "http://localhost:5000", string reportId = "")
    {
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken);
        await benchmarker.RunAllClient(processCount, iterations, benchCommand, hostAddress, reportId);
    }

    public async Task CancelCommands()
    {
        var benchmarker = new Benchmarker(_path, Context.Logger, Context.CancellationToken);
        await benchmarker.CancelCommands();
    }
}
