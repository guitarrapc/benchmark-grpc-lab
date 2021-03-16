using System;
using System.Collections.Generic;

namespace Benchmark.ClientLib.Reports
{
    public record HtmlBenchReport(
        HtmlBenchReportClientInfo Client, 
        HtmlBenchReportSummary Summary,
        HtmlBenchConfigData Config,
        HtmlBenchReportRequestResult[] RequestResults);
    public record HtmlBenchReportClientInfo
    {
        public string Os { get; init; }
        public string Architecture { get; init; }
        public int Processors { get; init; }
        public long Memory { get; init; }
        public string Framework { get; init; }
        public string Version { get; init; }
    }
    public record HtmlBenchReportSummary
    {
        public string ScenarioName { get; init; }
        public string ReportId { get; init; }
        public int Clients { get; init; }
        public int Concurrency { get; init; }
        public int Connections { get; init; }
        public DateTime Begin { get; init; }
        public DateTime End { get; init; }
        public TimeSpan Duration { get; init; }
        public long Requests { get; init; }
        public double Rps { get; init; }
        public TimeSpan Average { get; init; }
        public TimeSpan Fastest { get; init; }
        public TimeSpan Slowest { get; init; }
    }
    public record HtmlBenchConfigData
    {
        public int ClientConcurrency { get; init; }
        public int ClientConnections { get; init; }
    }
    // request base
    public record HtmlBenchReportRequestResult
    {
        public string Key { get; init; }
        public HtmlBenchReportRequestResultSummaryItem[] SummaryItems { get; init; }
        public HtmlBenchReportRequestResultClientDurationItems[] ClientDurationItems { get; init; }
    }
    public record HtmlBenchReportRequestResultSummaryItem
    {
        public int RequestCount { get; init; }
        public TimeSpan Duration { get; init; }
        public double Rps { get; init; }
        public int Errors { get; init; }
    }

    public record HtmlBenchReportRequestResultClientDurationItems(
        string Client,
        HtmlBenchReportRequestResultClientDurationItem[] Items);
    
    public record HtmlBenchReportRequestResultClientDurationItem
    {
        public int RequestCount { get; init; }
        public HtmlBenchReportRequestResultSummaryItem[] SummaryItems { get; init; }
    }
}
