using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using ZLogger;

namespace Benchmark.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // expand thread pool
            //ThreadPools.ModifyThreadPool(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);

            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureEmbeddedConfiguration(args)
                .ConfigureLogging((hostContext, logging) =>
                {
                    logging.ClearProviders();
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.AddZLoggerConsole(configure => configure.EnableStructuredLogging = false);
                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureKestrel(serverOptions =>
                        {
                            Console.WriteLine($"MaxRequestBodySize: {serverOptions.Limits.MaxRequestBodySize}");
                            // default unlimit
                            // serverOptions.Limits.MaxConcurrentConnections = null;
                            // serverOptions.Limits.MaxConcurrentUpgradedConnections = null;

                            serverOptions.Limits.MaxRequestBodySize = null; // default 30000000

                            serverOptions.Limits.Http2.MaxStreamsPerConnection = 10000; // default 100
                            serverOptions.Limits.Http2.MaxFrameSize = 32768; // default 16384
                            serverOptions.Limits.Http2.InitialConnectionWindowSize = 655350; // default 131072
                            serverOptions.Limits.Http2.InitialStreamWindowSize = 655350; // default 98304
                        })
                        .UseKestrel(options =>
                        {
                            // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
                            options.ConfigureEndpointDefaults(endpointOptions =>
                            {
                                endpointOptions.Protocols = HttpProtocols.Http2;
                            });
                        })
                        .UseStartup<Startup>();
                });
    }
} 
