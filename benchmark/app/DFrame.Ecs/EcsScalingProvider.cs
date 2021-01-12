using System;
using System.Threading;
using System.Threading.Tasks;

namespace DFrame.Ecs
{
    public enum EcsScalingType
    {
        Fargate = 0,
        Ec2,
    }

    public class EcsEnvironment
    {
        /// <summary>
        /// Worker scaling type
        /// </summary>
        public EcsScalingType ScalingType { get; set; } = EcsScalingType.Fargate;
        /// <summary>
        /// Worker ECS cluster name.
        /// </summary>
        public string ClusterName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_CLUSTER_NAME") ?? "dframe-worker-cluster";
        /// <summary>
        /// Worker ECS service name.
        /// </summary>
        public string ServiceName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_SERVICE_NAME") ?? "dframe-worker-service";
        /// <summary>
        /// Worker ECS task name.
        /// </summary>
        public string TaskName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_TASK_NAME") ?? "dframe-worker-task";
        /// <summary>
        /// Image Tag for Worker Image.
        /// </summary>
        public string Image { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_IMAGE_NAME") ?? "";
        /// <summary>
        /// Image Tag for Worker Image.
        /// </summary>
        public string ImageTag { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_IMAGE_TAG") ?? "";
        /// <summary>
        /// Wait worker task creationg timeout seconds. default 120 sec.
        /// </summary>
        public int WorkerTaskCreationTimeout { get; set; } = int.Parse(Environment.GetEnvironmentVariable("DFRAME_WORKER_POD_CREATE_TIMEOUT") ?? "120");
        /// <summary>
        /// Preserve Worker ECS Service after execution. default false.
        /// </summary>
        /// <remarks>
        /// any value => true
        /// null => false
        /// </remarks>
        public bool PreserveWorker { get; set; } = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DFRAME_WORKER_PRESERVE"));
    }

    public class EcsScalingProvider : IScalingProvider
    {
        private readonly EcsEnvironment _env;
        IFailSignal _failSignal = default!;

        public EcsScalingProvider() : this(new EcsEnvironment())
        {
        }
        public EcsScalingProvider(EcsEnvironment env)
        {
            _env = env;
        }

        public Task StartWorkerAsync(DFrameOptions options, int processCount, IServiceProvider provider, IFailSignal failSignal, CancellationToken cancellationToken)
        {
            _failSignal = failSignal;

            Console.WriteLine($"scale out workers {_env.ScalingType}. {_env.TaskName}@{_env.ClusterName}/{_env.ServiceName} ({processCount} tasks)");
            // todo: confirm Cluster Exists or create
            // todo: confirm Service Exists or create
            // todo: create task for desired parameter
            // todo: create task on cluster/service
            // todo: check task creation is complete

            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
