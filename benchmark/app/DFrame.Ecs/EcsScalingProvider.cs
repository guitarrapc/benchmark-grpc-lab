using System;
using System.Threading;
using System.Threading.Tasks;

namespace DFrame.Ecs
{
    public class EcsEnvironment
    {
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
        public string TaskDefinitionName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_TASK_NAME") ?? "dframe-worker-task";
        /// <summary>
        /// Worker ECS task container name.
        /// </summary>
        public string ContainerName { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_CONTAINER_NAME") ?? "worker";
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
        IFailSignal _failSignal = default!;

        private readonly EcsEnvironment _env;
        private readonly EcsService _ecsService;

        public EcsScalingProvider() : this(new EcsEnvironment())
        {
        }
        public EcsScalingProvider(EcsEnvironment env)
        {
            _env = env;
            _ecsService = new EcsService(_env.ClusterName, _env.ServiceName, _env.TaskDefinitionName, _env.ContainerName);
        }

        public async Task StartWorkerAsync(DFrameOptions options, int processCount, IServiceProvider provider, IFailSignal failSignal, CancellationToken cancellationToken)
        {
            _failSignal = failSignal;

            Console.WriteLine($"scale out {processCount} workers for {_ecsService.TaskDefinitionName}@{_ecsService.ClusterName}/{_ecsService.ServiceName}");

            Console.WriteLine($"checking ECS Cluster is ready.");
            if (!await _ecsService.ExistsClusterAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Cluster not found. Please confirm provided name {nameof(_ecsService.ClusterName)} is valid."));
                return;
            }
            if (!await _ecsService.ExistsServiceAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Service not found in ECS Cluster {_ecsService.ClusterName}. Please confirm provided name {nameof(_ecsService.ServiceName)} is valid."));
                return;
            }
            if (!await _ecsService.ExistsTaskDefinitionAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS TaskDefinition not found. Please confirm provided name {nameof(_ecsService.TaskDefinitionName)} is valid."));
                return;
            }

            using (var cts = new CancellationTokenSource(_env.WorkerTaskCreationTimeout * 1000))
            {
                // create task for desired parameter
                var updatedTaskDefinition = await _ecsService.UpdateTaskDefinitionImageAsync(_env.Image, _env.ImageTag);

                // deploy new task
                await _ecsService.RollingServiceDeploymentAsync(updatedTaskDefinition.TaskRevision, processCount);
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine($"scale in workers for {_ecsService.TaskDefinitionName}@{_ecsService.ClusterName}/{_ecsService.ServiceName}");
            if (!_env.PreserveWorker)
            {
                using var cts = new CancellationTokenSource(120 * 1000);
                await _ecsService.ScaleServiceAsync(0, cts.Token);
            }
            else
            {
                Console.WriteLine($"detected preserve worker, scale in action skipped.");
            }
        }
    }
}
