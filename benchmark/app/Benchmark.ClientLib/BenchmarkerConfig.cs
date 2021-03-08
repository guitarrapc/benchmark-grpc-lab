using Benchmark.ClientLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.ClientLib
{
    public class BenchmarkerConfig
    {
        // follow to ghz https://github.com/LesnyRumcajs/grpc_bench
        /// <summary>
        /// Number of requests to run. Default is 200.
        /// </summary>
        public int[] TotalRequests { get; init; } = new[] { 200 };
        /// <summary>
        /// Duration of application to send requests. When duration is reached, application stops and exits. If duration is specified, TotalRequest is ignored. Examples: -z 10s -z 3m.
        /// </summary>
        public string Duration { get; init; } = "0";
        public RequestPayload RequestPayload { get; init; } = RequestPayload.Byte100;
        public int ClientConnections { get; init; } = 5;
        public int ClientConcurrency { get; init; } = 50;

        public bool UseSelfCertEndpoint { get; set; } = false;

        public string GetRequestPayload() => Payload.FromPayload(RequestPayload);
    }
}
