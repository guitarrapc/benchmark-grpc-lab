using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new CdkStack(app, "MagicOnionBenchmarkCdkStack", new StackProps
            {
                Tags = new Dictionary<string, string>()
                {
                    { "environment", "bench" },
                },
            });
            app.Synth();
        }
    }
}
