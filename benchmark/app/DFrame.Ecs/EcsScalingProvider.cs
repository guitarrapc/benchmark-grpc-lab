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
        public string Image { get; set; } = Environment.GetEnvironmentVariable("DFRAME_WORKER_IMAGE") ?? "";
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
        private readonly EcsService _ecs;

        public EcsScalingProvider() : this(new EcsEnvironment())
        {
        }
        public EcsScalingProvider(EcsEnvironment env)
        {
            _env = env;
            _ecs = new EcsService(_env.ClusterName, _env.ServiceName, _env.TaskDefinitionName, _env.ContainerName);
        }

        public async Task StartWorkerAsync(DFrameOptions options, int processCount, IServiceProvider provider, IFailSignal failSignal, CancellationToken cancellationToken)
        {
            _failSignal = failSignal;

            Console.WriteLine($"scale out {processCount} workers. Cluster: {_ecs.ClusterName}, Service: {_ecs.ServiceName}, TaskDef: {_ecs.TaskDefinitionName}");

            Console.WriteLine($"checking ECS is ready.");
            if (!await _ecs.ExistsClusterAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Cluster not found. Please confirm provided name {nameof(_ecs.ClusterName)} is valid."));
                return;
            }
            if (!await _ecs.ExistsServiceAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS Service not found in ECS Cluster {_ecs.ClusterName}. Please confirm provided name {nameof(_ecs.ServiceName)} is valid."));
                return;
            }
            if (!await _ecs.ExistsTaskDefinitionAsync())
            {
                _failSignal.TrySetException(new EcsException($"ECS TaskDefinition not found. Please confirm provided name {nameof(_ecs.TaskDefinitionName)} is valid."));
                return;
            }

            using (var cts = new CancellationTokenSource(_env.WorkerTaskCreationTimeout * 1000))
            {
                // create task for desired parameter
                var updatedTaskDefinition = await _ecs.UpdateTaskDefinitionImageAsync(_env.Image);

                // update service and deploy new task
                await _ecs.UpdateServiceDeploymentAsync(updatedTaskDefinition.TaskRevision, processCount);
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine($"scale in workers for {_ecs.TaskDefinitionName}@{_ecs.ClusterName}/{_ecs.ServiceName}");
            if (!_env.PreserveWorker)
            {
                using var cts = new CancellationTokenSource(120 * 1000);
                await _ecs.ScaleServiceAsync(0, cts.Token);
            }
            else
            {
                Console.WriteLine($"detected preserve worker, scale in action skipped.");
            }
        }
    }
}
