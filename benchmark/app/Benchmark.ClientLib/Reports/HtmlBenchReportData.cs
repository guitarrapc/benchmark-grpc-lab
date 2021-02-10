using System;
using System.Collections.Generic;

namespace Benchmark.ClientLib.Reports
{
    public record HtmlBenchReport(
        HtmlBenchReportClientInfo Client, 
        HtmlBenchReportSummary Summary,
        HtmlBenchReportClientResult UnaryClientResult,
        HtmlBenchReportRequestResult UnaryRequestResult, 
        // todo: add HubClientRequest
        HtmlBenchReportRequestResult HubRequestResult);
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
        public string ReportId { get; init; }
        public int Clients { get; init; }
        public long RequestTotal { get; init; }
        public DateTime Begin { get; init; }
        public DateTime End { get; init; }
        public double Rps { get; init; }
        public TimeSpan DurationTotal { get; init; }
        public TimeSpan DurationAvg { get; init; }
        public TimeSpan DurationMin { get; init; }
        public TimeSpan DurationMax { get; init; }
    }
    // client base
    public record HtmlBenchReportClientResult
    {
        public HtmlBenchReportClientResultSummaryItem[] SummaryItems { get; init; }
        public int Errors { get; init; }
        public (string Client, HtmlBenchReportClientResultClientDurationItem[] Items)[] ClientDurationItems { get; init; }
    }
    public record HtmlBenchReportClientResultSummaryItem
    {
        public int ClientCount { get; init; }
        public TimeSpan Duration { get; init; }
        public double Rps { get; init; }
    }
    public record HtmlBenchReportClientResultClientDurationItem
    {
        public int ClientCount { get; init; }
        public HtmlBenchReportClientResultSummaryItem[] SummaryItems { get; init; }
    }

    // request base
    public record HtmlBenchReportRequestResult
    {
        public HtmlBenchReportRequestResultSummaryItem[] SummaryItems { get; init; }
        public int Errors { get; init; }
        public (string Client, HtmlBenchReportRequestResultClientDurationItem[] Items)[] ClientDurationItems { get; init; }
    }
    public record HtmlBenchReportRequestResultSummaryItem
    {
        public int RequestCount { get; init; }
        public TimeSpan Duration { get; init; }
        public double Rps { get; init; }
    }
    public record HtmlBenchReportRequestResultClientDurationItem
    {
        public int RequestCount { get; init; }
        public HtmlBenchReportRequestResultSummaryItem[] SummaryItems { get; init; }
    }
}
