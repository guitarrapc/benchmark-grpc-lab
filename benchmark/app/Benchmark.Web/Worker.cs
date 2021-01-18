using Benchmark.ClientLib;
using DFrame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmark.Web
{
    public class UnaryWorker : Worker
    {
        private CancellationTokenSource _cts;
        private string _hostAddress;
        private string _iterations;
        private string _reportId;
        private Benchmarker _benchmarker;

        public override async Task SetupAsync(WorkerContext context)
        {
            _cts = new CancellationTokenSource();
            _hostAddress = Environment.GetEnvironmentVariable("BENCH_SERVER_HOST") ?? "http://localhost:5000";
            _iterations = "254,1024,4096,16384";
            _reportId = Environment.GetEnvironmentVariable("BENCH_REPORTID") ?? "testtest";
            //var path = Environment.GetEnvironmentVariable("BENCH_S3BUCKET") ?? throw new ArgumentNullException("Environment variable 'BENCHCLIENT_S3BUCKET' is not defined.");
            var path = "bin/hogemogefugafuga";

            _benchmarker = new Benchmarker(path, null, _cts.Token);
        }
        public override async Task ExecuteAsync(WorkerContext context)
        {
            await _benchmarker.BenchUnary(_hostAddress, _iterations, _reportId);
        }
        public override async Task TeardownAsync(WorkerContext context)
        {
            await _benchmarker.GenerateHtml(_reportId);
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public class SampleWorker : Worker
    {
        IDistributedQueue<int> queue;

        public override async Task SetupAsync(WorkerContext context)
        {
            queue = context.CreateDistributedQueue<int>("sampleworker-testq");
        }

        public override async Task ExecuteAsync(WorkerContext context)
        {
            var randI = (int)new Random().Next(1, 3999);
            //Console.WriteLine($"Enqueue from {Environment.MachineName} {context.WorkerId}: {randI}");

            await queue.EnqueueAsync(randI);
        }

        public override async Task TeardownAsync(WorkerContext context)
        {
            while (true)
            {
                var v = await queue.TryDequeueAsync();
                if (v.HasValue)
                {
                    //Console.WriteLine($"Dequeue all from {Environment.MachineName} {context.WorkerId}: {v.Value}");
                }
                else
                {
                    return;
                }
            }
        }
    }
}
