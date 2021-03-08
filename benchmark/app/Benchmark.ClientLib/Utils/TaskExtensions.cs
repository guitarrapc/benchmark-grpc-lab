using System;
using System.Threading.Tasks;

namespace Benchmark.ClientLib
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            task.ContinueWith(x =>
            {
                Console.WriteLine("TaskUnhandled", x.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
