﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version: 16.0.0.0
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
namespace Benchmark.ClientLib
{
    using System.Linq;
    using System.Text;
    using System.Collections.Generic;
    using Benchmark.ClientLib;
    using System;
    
    /// <summary>
    /// Class to produce the template output
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public partial class BenchmarkReportPageTemplate : BenchmarkReportPageTemplateBase
    {
        /// <summary>
        /// Create the template output
        /// </summary>
        public virtual string TransformText()
        {
            this.Write("\r\n");

    var client = Report.Client;
    var config = Report.Config;
    var summary = Report.Summary;
    var requests = Report.Requests;

    var lineColor = GetLineColor();
    var barColor = GetColor();

            this.Write(@"
<!-- auto-generated -->
<!DOCTYPE html>
<html lang=""ja"">

<head>
    <meta charset=""utf-8"">
    <title>MagicOnion Benchmark</title>

    <link rel=""stylesheet"" href=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css""
        integrity=""sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk"" crossorigin=""anonymous""/>
    <!-- MDB -->
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.1/css/all.min.css""/>
    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css?family=Roboto:300,400,500,700&display=swap""/>
    <link rel=""stylesheet"" href=""https://cdnjs.cloudflare.com/ajax/libs/mdb-ui-kit/3.0.0/mdb.min.css""/>

    <script src=""https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.7.2/Chart.bundle.js""></script>
</head>

<body>
    <header>
        <nav class=""navbar navbar-dark bg-dark box-shadow"">
            <div class=""container d-flex"">
                <a class=""navbar-brand"" href=""#"">MagicOnion Benchmark</a>
                <div class=""collapse navbar-collapse"" id=""navbarText"">
                    <ul class=""navbar-nav me-auto mb-2 mb-lg-0"">
                    </ul>
                </div>
                <span class=""navbar-text"">");
            this.Write(this.ToStringHelper.ToStringWithCulture(client.Framework));
            this.Write(" v");
            this.Write(this.ToStringHelper.ToStringWithCulture(client.Version));
            this.Write(@"</span>
            </div>
        </nav>
    </header>

    <main>
        <div class=""bg-light"">
            <div class=""container"" style=""padding-top: 3rem;"">

                <h1 class=""text-muted"">Test Result</h1>
                <p class=""text-muted"">ReportId: ");
            this.Write(this.ToStringHelper.ToStringWithCulture(summary.ReportId));
            this.Write("</p>\r\n                <p class=\"text-muted\">Period: ");
            this.Write(this.ToStringHelper.ToStringWithCulture(summary.Begin));
            this.Write(" - ");
            this.Write(this.ToStringHelper.ToStringWithCulture(summary.End));
            this.Write(@"</p>

                <div class=""table-responsive"">
                    <h2 class=""text-muted"">Client</h2>
                    <table class=""table table-striped table-sm"">
                        <thead>
                            <tr>
                            <th scope=""col"">OS</th>
                            <th scope=""col""># Process</th>
                            <th scope=""col"">Memory</th>
                            <th scope=""col""># Concurrency</th>
                            <th scope=""col""># Connections</th>
                            </tr>
                        </thead>
                        <tr>
                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(client.Os));
            this.Write(" (");
            this.Write(this.ToStringHelper.ToStringWithCulture(client.Architecture));
            this.Write(")</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(client.Processors));
            this.Write("</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(client.Memory));
            this.Write("GiB</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(config.ClientConcurrency));
            this.Write("</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(config.ClientConnections));
            this.Write(@"</td>
                        </tr>
                        </tbody>
                    </table>
                </div>

                <div class=""table-responsive"">
                    <h2 class=""text-muted"">Summary</h2>
                    <table class=""table table-striped table-sm"">
                        <thead>
                            <th scope=""col""># Clients</th>
                            <th scope=""col""># Requests Ttl</th>
                            <th scope=""col"">Duration</th>
                            <th scope=""col"">Slowest</th>
                            <th scope=""col"">Fastest</th>
                            <th scope=""col"">Average</th>
                            <th scope=""col"">Requests/sec</th>
                        </thead>
                        <tr>
                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(summary.Clients));
            this.Write("</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(summary.Requests));
            this.Write("</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f2}", summary.Duration.TotalSeconds)));
            this.Write(" sec</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f3}", summary.Slowest.TotalMilliseconds)));
            this.Write(" ms</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f3}", summary.Fastest.TotalMilliseconds)));
            this.Write(" ms</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f3}", summary.Average.TotalMilliseconds)));
            this.Write(" ms</td>\r\n                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f2}", summary.Rps)));
            this.Write(" rps</td>\r\n                        </tr>\r\n                        </tbody>\r\n     " +
                    "               </table>\r\n                </div>\r\n\r\n                ");
 if (requests.Any()) {
            this.Write("                ");
 foreach(var request in requests) {
            this.Write("                <div class=\"table-responsive\">\r\n                    <h2 class=\"te" +
                    "xt-muted\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write(" Summary</h2>\r\n                    <table class=\"table table-striped table-sm\">\r\n" +
                    "                        <thead>\r\n                            <th scope=\"col\">Req" +
                    "uests</th>\r\n                        ");
 foreach(var item in request.Summaries) { 
            this.Write("                            <th scope=\"col\">");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.RequestCount));
            this.Write(" req</th>\r\n                        ");
 } 
            this.Write("                        </thead>\r\n                        <tr>\r\n                 " +
                    "           <td>Duration (Avg)</td>\r\n                        ");
 foreach(var item in request.Summaries) { 
            this.Write("                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f2}", item.Duration.TotalMilliseconds)));
            this.Write(" ms</td>\r\n                        ");
 } 
            this.Write("                        </tr>\r\n                        <tr>\r\n                    " +
                    "        <td>Rps</td>\r\n                        ");
 foreach(var item in request.Summaries) { 
            this.Write("                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(string.Format("{0:f2}", item.Rps)));
            this.Write(" rps</td>\r\n                        ");
 } 
            this.Write("                        </tr>\r\n                        <tr>\r\n                    " +
                    "        <td>Errors</td>\r\n                        ");
 foreach(var item in request.Summaries) { 
            this.Write("                            <td>");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Errors));
            this.Write("</td>\r\n                        ");
 } 
            this.Write("                        </tr>\r\n                        </tbody>\r\n                " +
                    "    </table>\r\n\r\n                    <canvas id=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write("LatencyDistribution\"></canvas>\r\n                    <canvas id=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write("RequestAvg\"></canvas>\r\n                    <canvas id=\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write("RequestStackBar\"></canvas>\r\n\r\n                    <script>\r\n                     " +
                    "   var ctx = document.getElementById(\"");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write("LatencyDistribution\");\r\n                        var myChart = new Chart(ctx, {\r\n " +
                    "                           type: \'horizontalBar\',\r\n                            d" +
                    "ata: {\r\n                                labels: [\r\n                             " +
                    "   ");
 foreach(var item in request.Latencies) { 
            this.Write("                                    \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Percentile));
            this.Write("\",\r\n                                ");
 } 
            this.Write("                                ],\r\n                                datasets: [\r\n" +
                    "                                {\r\n                                    label: \"L" +
                    "atency(ms)\",\r\n                                    backgroundColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(barColor));
            this.Write("\",\r\n                                    borderColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(barColor));
            this.Write("\",\r\n                                    data: [\r\n                                " +
                    "    ");
 foreach(var item in request.Latencies) { 
            this.Write("                                        ");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Duration.TotalMilliseconds));
            this.Write(",\r\n                                    ");
 } 
            this.Write(@"                                    ]
                                },
                                ]
                            },
                            options: {
                                title: {
                                    display: true,
                                    text: 'Latency Distribution',
                                    padding: 3
                                },
                                legend: {
                                    labels: {
                                        boxWidth: 30,
                                        padding: 20
                                    },
                                    display: true
                                },
                                tooltips: {
                                    mode: 'label' // data colum for tooltip
                                },
                                responsive: true
                            }
                        });
                    </script>
                    <script>
                        var ctx = document.getElementById(""");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write("RequestAvg\");\r\n                        var myChart = new Chart(ctx, {\r\n          " +
                    "                  type: \'bar\',\r\n                            data: {\r\n           " +
                    "                     labels: [\r\n                                ");
 foreach(var item in request.Summaries) { 
            this.Write("                                    \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.RequestCount));
            this.Write("\",\r\n                                ");
 } 
            this.Write(@"                                ],
                                datasets: [
                                {
                                    type: 'bar',
                                    label: ""Duration"",
                                    backgroundColor: """);
            this.Write(this.ToStringHelper.ToStringWithCulture(barColor));
            this.Write("\",\r\n                                    borderColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(barColor));
            this.Write("\",\r\n                                    data: [\r\n                                " +
                    "    ");
 foreach(var item in request.Summaries) { 
            this.Write("                                        ");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Duration.TotalMilliseconds));
            this.Write(",\r\n                                    ");
 } 
            this.Write(@"                                    ]
                                },
                                {
                                    type: 'line',
                                    label: ""Rps"",
                                    pointBackgroundColor: """);
            this.Write(this.ToStringHelper.ToStringWithCulture(lineColor));
            this.Write("\",\r\n                                    backgroundColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(lineColor));
            this.Write("\",\r\n                                    borderColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(lineColor));
            this.Write("\",\r\n                                    fill: false,\r\n                           " +
                    "         data: [\r\n                                    ");
 foreach(var item in request.Summaries) { 
            this.Write("                                        ");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Rps));
            this.Write(",\r\n                                    ");
 } 
            this.Write(@"                                    ]
                                },
                                ]
                            },
                            options: {
                                title: {
                                    display: true,
                                    text: 'Average Duration & Rps (Unary)',
                                    padding: 3
                                },
                                legend: {
                                    labels: {
                                        boxWidth: 30,
                                        padding: 20
                                    },
                                    display: true
                                },
                                tooltips: {
                                    mode: 'label' // data colum for tooltip
                                },
                                responsive: true
                            }
                        });
                    </script>
                    <script>
                        var ctx = document.getElementById(""");
            this.Write(this.ToStringHelper.ToStringWithCulture(request.Key));
            this.Write("RequestStackBar\");\r\n                        var myChart = new Chart(ctx, {\r\n     " +
                    "                       type: \'bar\',\r\n                            data: {\r\n      " +
                    "                          labels: [\r\n                                ");
 foreach(var item in request.Summaries) { 
            this.Write("                                    \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.RequestCount));
            this.Write("\",\r\n                                ");
 } 
            this.Write("                                ],\r\n                                datasets: [\r\n" +
                    "                                ");
 for(var i = 0; i < request.Durations.Length; i++) { 
            this.Write("                                {\r\n                                    ");

                                        var current = request.Durations[i];
                                        var items = current.Items;
                                        var color = GetColor(i);
                                    
            this.Write("                                    type: \'bar\',\r\n                               " +
                    "     label: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(current.Client));
            this.Write("\",\r\n                                    borderWidth: 1,\r\n                        " +
                    "            backgroundColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(color));
            this.Write("\",\r\n                                    borderColor: \"");
            this.Write(this.ToStringHelper.ToStringWithCulture(color));
            this.Write("\",\r\n                                    data: [\r\n                                " +
                    "    ");
 foreach(var summaries in items) { 
            this.Write("                                        ");
 foreach(var item in summaries.Summaries) { 
            this.Write("                                            ");
            this.Write(this.ToStringHelper.ToStringWithCulture(item.Duration.TotalMilliseconds));
            this.Write(",\r\n                                        ");
 } 
            this.Write("                                    ");
 } 
            this.Write("                                    ]\r\n                                },\r\n      " +
                    "                          ");
 } 
            this.Write("                                ]\r\n                            },\r\n              " +
                    "              options: {\r\n                                title: {\r\n            " +
                    "                        display: true,\r\n                                    text" +
                    ": \'Stacked Duration per Client (Unary)\',\r\n                                    pa" +
                    "dding: 3\r\n                                },\r\n                                sc" +
                    "ales: {\r\n                                    xAxes: [{\r\n                        " +
                    "                stacked: true, // use stacked bar chart\r\n                       " +
                    "                 categoryPercentage: 0.4 // width of bar\r\n                      " +
                    "              }],\r\n                                    yAxes: [{\r\n              " +
                    "                          stacked: true // use stacked bar chart\r\n              " +
                    "                      }]\r\n                                },\r\n                  " +
                    "              legend: {\r\n                                    labels: {\r\n        " +
                    "                                boxWidth: 30,\r\n                                 " +
                    "       padding: 20\r\n                                    },\r\n                    " +
                    "                display: false\r\n                                },\r\n            " +
                    "                    tooltips: {\r\n                                    mode: \'labe" +
                    "l\' // data colum for tooltip\r\n                                },\r\n              " +
                    "                  responsive: true\r\n                            }\r\n             " +
                    "           });\r\n                    </script>\r\n                </div>\r\n         " +
                    "       ");
}
            this.Write("                ");
}
            this.Write(@"
            </div>
        </div>
    </main>

    <footer class=""text-muted"" style=""padding-top: 3rem;padding-bottom: 3rem;"">
        <div class=container>
            <a href=""#"" class=""btn btn-outline-info float-right"" role=""button"">
                <i class=""fa fa-angle-up""></i>
            </a>
            <p class=""text-center"">
                <a class=""text-dark"" href=""https://github.com/cysharp/MagicOnion/"">Visit the GitHub</a>
            /
            © 2020 Copyright:
                <a class=""text-dark"" href=""https://cysharp.co.jp/"">Cysharp, Inc.</a>
            </p>
        </div>
    </footer>

    <script src=""https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js""
        integrity=""sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI""
        crossorigin=""anonymous""></script>
    <!-- MDB -->
    <script type=""text/javascript"" src=""https://cdnjs.cloudflare.com/ajax/libs/mdb-ui-kit/3.0.0/mdb.min.js""></script>
</body>

</html>");
            return this.GenerationEnvironment.ToString();
        }
    }
    #region Base class
    /// <summary>
    /// Base class for this transformation
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TextTemplating", "16.0.0.0")]
    public class BenchmarkReportPageTemplateBase
    {
        #region Fields
        private global::System.Text.StringBuilder generationEnvironmentField;
        private global::System.CodeDom.Compiler.CompilerErrorCollection errorsField;
        private global::System.Collections.Generic.List<int> indentLengthsField;
        private string currentIndentField = "";
        private bool endsWithNewline;
        private global::System.Collections.Generic.IDictionary<string, object> sessionField;
        #endregion
        #region Properties
        /// <summary>
        /// The string builder that generation-time code is using to assemble generated output
        /// </summary>
        protected System.Text.StringBuilder GenerationEnvironment
        {
            get
            {
                if ((this.generationEnvironmentField == null))
                {
                    this.generationEnvironmentField = new global::System.Text.StringBuilder();
                }
                return this.generationEnvironmentField;
            }
            set
            {
                this.generationEnvironmentField = value;
            }
        }
        /// <summary>
        /// The error collection for the generation process
        /// </summary>
        public System.CodeDom.Compiler.CompilerErrorCollection Errors
        {
            get
            {
                if ((this.errorsField == null))
                {
                    this.errorsField = new global::System.CodeDom.Compiler.CompilerErrorCollection();
                }
                return this.errorsField;
            }
        }
        /// <summary>
        /// A list of the lengths of each indent that was added with PushIndent
        /// </summary>
        private System.Collections.Generic.List<int> indentLengths
        {
            get
            {
                if ((this.indentLengthsField == null))
                {
                    this.indentLengthsField = new global::System.Collections.Generic.List<int>();
                }
                return this.indentLengthsField;
            }
        }
        /// <summary>
        /// Gets the current indent we use when adding lines to the output
        /// </summary>
        public string CurrentIndent
        {
            get
            {
                return this.currentIndentField;
            }
        }
        /// <summary>
        /// Current transformation session
        /// </summary>
        public virtual global::System.Collections.Generic.IDictionary<string, object> Session
        {
            get
            {
                return this.sessionField;
            }
            set
            {
                this.sessionField = value;
            }
        }
        #endregion
        #region Transform-time helpers
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void Write(string textToAppend)
        {
            if (string.IsNullOrEmpty(textToAppend))
            {
                return;
            }
            // If we're starting off, or if the previous text ended with a newline,
            // we have to append the current indent first.
            if (((this.GenerationEnvironment.Length == 0) 
                        || this.endsWithNewline))
            {
                this.GenerationEnvironment.Append(this.currentIndentField);
                this.endsWithNewline = false;
            }
            // Check if the current text ends with a newline
            if (textToAppend.EndsWith(global::System.Environment.NewLine, global::System.StringComparison.CurrentCulture))
            {
                this.endsWithNewline = true;
            }
            // This is an optimization. If the current indent is "", then we don't have to do any
            // of the more complex stuff further down.
            if ((this.currentIndentField.Length == 0))
            {
                this.GenerationEnvironment.Append(textToAppend);
                return;
            }
            // Everywhere there is a newline in the text, add an indent after it
            textToAppend = textToAppend.Replace(global::System.Environment.NewLine, (global::System.Environment.NewLine + this.currentIndentField));
            // If the text ends with a newline, then we should strip off the indent added at the very end
            // because the appropriate indent will be added when the next time Write() is called
            if (this.endsWithNewline)
            {
                this.GenerationEnvironment.Append(textToAppend, 0, (textToAppend.Length - this.currentIndentField.Length));
            }
            else
            {
                this.GenerationEnvironment.Append(textToAppend);
            }
        }
        /// <summary>
        /// Write text directly into the generated output
        /// </summary>
        public void WriteLine(string textToAppend)
        {
            this.Write(textToAppend);
            this.GenerationEnvironment.AppendLine();
            this.endsWithNewline = true;
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void Write(string format, params object[] args)
        {
            this.Write(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Write formatted text directly into the generated output
        /// </summary>
        public void WriteLine(string format, params object[] args)
        {
            this.WriteLine(string.Format(global::System.Globalization.CultureInfo.CurrentCulture, format, args));
        }
        /// <summary>
        /// Raise an error
        /// </summary>
        public void Error(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Raise a warning
        /// </summary>
        public void Warning(string message)
        {
            System.CodeDom.Compiler.CompilerError error = new global::System.CodeDom.Compiler.CompilerError();
            error.ErrorText = message;
            error.IsWarning = true;
            this.Errors.Add(error);
        }
        /// <summary>
        /// Increase the indent
        /// </summary>
        public void PushIndent(string indent)
        {
            if ((indent == null))
            {
                throw new global::System.ArgumentNullException("indent");
            }
            this.currentIndentField = (this.currentIndentField + indent);
            this.indentLengths.Add(indent.Length);
        }
        /// <summary>
        /// Remove the last indent that was added with PushIndent
        /// </summary>
        public string PopIndent()
        {
            string returnValue = "";
            if ((this.indentLengths.Count > 0))
            {
                int indentLength = this.indentLengths[(this.indentLengths.Count - 1)];
                this.indentLengths.RemoveAt((this.indentLengths.Count - 1));
                if ((indentLength > 0))
                {
                    returnValue = this.currentIndentField.Substring((this.currentIndentField.Length - indentLength));
                    this.currentIndentField = this.currentIndentField.Remove((this.currentIndentField.Length - indentLength));
                }
            }
            return returnValue;
        }
        /// <summary>
        /// Remove any indentation
        /// </summary>
        public void ClearIndent()
        {
            this.indentLengths.Clear();
            this.currentIndentField = "";
        }
        #endregion
        #region ToString Helpers
        /// <summary>
        /// Utility class to produce culture-oriented representation of an object as a string.
        /// </summary>
        public class ToStringInstanceHelper
        {
            private System.IFormatProvider formatProviderField  = global::System.Globalization.CultureInfo.InvariantCulture;
            /// <summary>
            /// Gets or sets format provider to be used by ToStringWithCulture method.
            /// </summary>
            public System.IFormatProvider FormatProvider
            {
                get
                {
                    return this.formatProviderField ;
                }
                set
                {
                    if ((value != null))
                    {
                        this.formatProviderField  = value;
                    }
                }
            }
            /// <summary>
            /// This is called from the compile/run appdomain to convert objects within an expression block to a string
            /// </summary>
            public string ToStringWithCulture(object objectToConvert)
            {
                if ((objectToConvert == null))
                {
                    throw new global::System.ArgumentNullException("objectToConvert");
                }
                System.Type t = objectToConvert.GetType();
                System.Reflection.MethodInfo method = t.GetMethod("ToString", new System.Type[] {
                            typeof(System.IFormatProvider)});
                if ((method == null))
                {
                    return objectToConvert.ToString();
                }
                else
                {
                    return ((string)(method.Invoke(objectToConvert, new object[] {
                                this.formatProviderField })));
                }
            }
        }
        private ToStringInstanceHelper toStringHelperField = new ToStringInstanceHelper();
        /// <summary>
        /// Helper to produce culture-oriented representation of an object as a string
        /// </summary>
        public ToStringInstanceHelper ToStringHelper
        {
            get
            {
                return this.toStringHelperField;
            }
        }
        #endregion
    }
    #endregion
}
