using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benchmark.ClientLib.Reports
{
    public class HtmlBenchReporter
    {
        public HtmlBenchReport CreateReport(BenchReport[] reports, bool generateDetail)
        {
            if (reports == null)
                throw new ArgumentNullException(nameof(reports));
            if (!reports.Any())
                throw new ArgumentException($"{nameof(reports)} not contains any element.");

            var requests = reports.SelectMany(xs => xs.Items.Select(x => x.RequestCount)).Sum();
            var begin = reports.Select(x => x.Begin).OrderBy(x => x).First();
            var end = reports.Select(x => x.End).OrderByDescending(x => x).First();

            var client = new HtmlBenchReportClientInfo
            {
                Os = ToJoinedString(reports.Select(x => x.OS).Distinct()),
                Architecture = ToJoinedString(reports.Select(x => x.OS).Distinct()),
                Processors = reports.Select(x => x.CpuNumber).Distinct().OrderByDescending(x => x).First(),
                Memory = reports.Select(x => x.SystemMemory).Distinct().OrderByDescending(x => x).First(), // take biggest
                Framework = ToJoinedString(reports.Select(x => x.Framework).Distinct()),
                Version = ToJoinedString(reports.Select(x => x.Version).Distinct()),
            };
            var summary = new HtmlBenchReportSummary
            {
                ScenarioName = reports.Select(x => x.ScenarioName).First(),
                ReportId = reports.Select(x => x.ReportId).First(),
                Clients = reports.GroupBy(x => x.ClientId).Count(),
                Concurrency = reports.Select(x => x.Concurrency).First(),
                Connections = reports.Select(x => x.Connections).First(),
                Begin = begin,
                End = end,
                Duration = end - begin,
                Requests = requests,
                Rps = requests / reports.Select(x => x.Duration).Sum().TotalSeconds,
                Average = reports.SelectMany(xs => xs.Items.Select(x => x.Average)).Average(),
                Slowest = reports.SelectMany(xs => xs.Items.Select(x => x.Slowest)).Max(),
                Fastest = reports.SelectMany(xs => xs.Items.Select(x => x.Fastest)).Min(),
            };
            var config = new HtmlBenchConfigData
            {
                ClientConcurrency = reports.Select(x => x.Concurrency).First(),
                ClientConnections = reports.Select(x => x.Connections).First(),
            };
            var requestResults = reports.SelectMany(x => x.Items)
                .GroupBy(x => x.Type + x.RequestCount)
                .Select(unaryItems => new HtmlBenchReportRequestResult
                {
                    Key = reports.Select(x => x.ScenarioName).First(),
                    SummaryItems = GetRequestSummaryItems(unaryItems),
                    ClientDurationItems = Array.Empty<HtmlBenchReportRequestResultClientDurationItems>(),
                })
                .ToArray();

            return new HtmlBenchReport(
                client, 
                summary,
                config,
                requestResults);
        }

        private HtmlBenchReportRequestResultSummaryItem[] GetRequestSummaryItems(IEnumerable<BenchReportItem> sources)
        {
            // { connections int: duration TimeSpan}
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            return sources.GroupBy(x => x.RequestCount)
                .Select(xs =>
                {
                    var duration = xs.Select(x => x.Duration).Sum();
                    return new HtmlBenchReportRequestResultSummaryItem
                    {
                        RequestCount = xs.Key,
                        Duration = xs.Select(x => x.Duration).Average(),
                        Rps = xs.Sum(x => x.RequestCount) / duration.TotalSeconds,
                        Errors = xs.Sum(x => x.Errors),
                    };
                })
                .ToArray();
        }

        private static string ToJoinedString(IEnumerable<string> values, char separator = ',')
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (!values.Any())
                return "";
            return string.Join(separator, values);
        }
    }
}
