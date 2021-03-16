using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Benchmark.ClientLib.Reports
{
    public class BenchReport
    {
        [JsonPropertyName("report_id")]
        public string ReportId { get; set; }
        [JsonPropertyName("execute_id")]
        public string ExecuteId { get; set; }
        /// <summary>
        /// Client Identifier for Same Machine Execution but treat as different client.
        /// </summary>
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        [JsonPropertyName("host_name")]
        public string HostName { get; set; }
        [JsonPropertyName("os")]
        public string OS { get; set; }
        [JsonPropertyName("os_architecture")]
        public string OsArchitecture { get; set; }
        [JsonPropertyName("process_architecture")]
        public string ProcessArchitecture { get; set; }
        [JsonPropertyName("cpu_number")]
        public int CpuNumber { get; set; }
        [JsonPropertyName("system_memory")]
        public long SystemMemory { get; set; }
        [JsonPropertyName("framework")]
        public string Framework { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("begin")]
        public DateTime Begin { get; set; }
        [JsonPropertyName("end")]
        public DateTime End { get; set; }
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }
        [JsonPropertyName("scenario_name")]
        public string ScenarioName { get; set; }
        [JsonPropertyName("concurrency")]
        public int Concurrency { get; set; }
        [JsonPropertyName("connections")]
        public int Connections { get; set; }
        [JsonPropertyName("items")]
        public BenchReportItem[] Items { get; set; }
    }

    public class BenchReportItem
    {
        [JsonPropertyName("execute_id")]
        public string ExecuteId { get; set; }
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("test_name")]
        public string TestName { get; set; }
        [JsonPropertyName("begin")]
        public DateTime Begin { get; set; }
        [JsonPropertyName("end")]
        public DateTime End { get; set; }
        [JsonPropertyName("duration")]
        public TimeSpan Duration { get; set; }
        [JsonPropertyName("request_count")]
        public int RequestCount { get; set; }
        [JsonPropertyName("slowest")]
        public TimeSpan Slowest { get; set; }
        [JsonPropertyName("fastest")]
        public TimeSpan Fastest { get; set; }
        [JsonPropertyName("Average")]
        public TimeSpan Average { get; set; }
        [JsonPropertyName("request_per_sec")]
        public Double Rps { get; set; }
        [JsonPropertyName("error_count")]
        public int Errors { get; set; }
        [JsonPropertyName("statuscode_distribution")]
        public StatusCodeDistribution[] StatusCodeDistributions { get; set; }
    }

    public struct StatusCodeDistribution
    {
        public string StatusCode { get; set; }
        public int Count { get; set; }
        public string Detail { get; set; }

        public static StatusCodeDistribution[] FromCallResults(IEnumerable<CallResult> callResults)
        {
            return callResults.Select(x => x.Status)
                .GroupBy(x => x.StatusCode)
                .Select(x => new StatusCodeDistribution
                {
                    Count = x.Count(),
                    StatusCode = x.Key.ToString(),
                    Detail = x.Select(x => x.Detail).FirstOrDefault()
                })
                .ToArray();
        }
    }

    public struct CallResult
    {
        public Exception Error { get; set; }
        public Status Status { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime TimeStamp { get; set; }
    }

}
