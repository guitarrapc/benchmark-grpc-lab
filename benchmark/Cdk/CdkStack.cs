using Amazon.CDK;
using Amazon.CDK.AWS.AutoScaling;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.S3.Deployment;
using System.Collections.Generic;
using System.IO;

namespace Cdk
{
    public class CdkStack : Stack
    {
        internal CdkStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            var logGroup = "MagicOnionBenchWorkerLogGroup";
            var vpc = new Vpc(this, "MagicOnionBenchVpc", new VpcProps { MaxAzs = 2 });
            var subnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE };
            var sg = new SecurityGroup(this, "MagicOnionBenchMasterSg", new SecurityGroupProps
            {
                AllowAllOutbound = true,
                Vpc = vpc,
            });
            foreach (var subnet in vpc.PrivateSubnets)
                sg.AddIngressRule(Peer.Ipv4(subnet.Ipv4CidrBlock), Port.AllTcp(), "VPC", true);

            var s3 = new Bucket(this, "MagicOnionBenchBucket", new BucketProps
            {
                AutoDeleteObjects = true,
                RemovalPolicy = RemovalPolicy.DESTROY,
                AccessControl = BucketAccessControl.PRIVATE,
                Versioned = true,
            });
            var masterDllDeployment = new BucketDeployment(this, "DeployMasterDll", new BucketDeploymentProps
            {
                DestinationBucket = s3,
                Sources = new[] { Source.Asset(Path.Combine(Directory.GetCurrentDirectory(), "out/linux/server/")) },
                DestinationKeyPrefix = "assembly/linux/server/"
            });
            var iamMasterRole = IamMaster(s3);
            var iamWorkerTaskExecuteRole = IamWorkerTaskExecute(logGroup);
            var iamWorkerTaskDefRole = IamWorkerTaskDef(s3);

            // master
            var asg = new AutoScalingGroup(this, "MagicOnionBenchMasterAsg", new AutoScalingGroupProps
            {
                SpotPrice = "0.01", // 0.0096 for spot price average
                Vpc = vpc,
                SecurityGroup = sg,
                VpcSubnets = subnets,
                InstanceType = InstanceType.Of(InstanceClass.STANDARD3, InstanceSize.MEDIUM),
                DesiredCapacity = 1,
                MaxCapacity = 1,
                AssociatePublicIpAddress = false,
                MachineImage = new AmazonLinuxImage(),
                AllowAllOutbound = true,
                GroupMetrics = new[] { GroupMetrics.All() },
                Role = iamMasterRole,
            });
            asg.AddUserData(@$"#!/bin/bash
# install .NET 5 Runtime
sudo rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
sudo yum install -y dotnet-sdk-5.0 aspnetcore-runtime-5.0
. /etc/profile.d/dotnet-cli-tools-bin-path.sh

mkdir -p /var/MagicOnion.Benchmark/server
aws s3 sync --exact-timestamps s3://{s3.BucketName}/assembly/linux/server/ ~/server
sudo chmod +x ~/server/Benchmark.Server
sudo cp -Rf ~/server/ /var/MagicOnion.Benchmark/.
sudo cp -f /var/MagicOnion.Benchmark/server/Benchmark.Server.service /etc/systemd/system/.
sudo systemctl enable Benchmark.Server
sudo systemctl restart Benchmark.Server
".Replace("\r\n", "\n"));
            asg.Node.AddDependency(masterDllDeployment);

            // worker
            var cluster = new Cluster(this, "MagicOnionBenchWorkerCluster", new ClusterProps
            {
                ClusterName = "MagicOnionBenchWorkerCluster",
                Vpc = vpc,
            });
            var taskDef = new FargateTaskDefinition(this, "MagicOnionBenchWorkerTaskDef", new FargateTaskDefinitionProps
            {
                ExecutionRole = iamWorkerTaskExecuteRole,
                TaskRole = iamWorkerTaskDefRole,
            });
            taskDef.AddContainer("worker", new ContainerDefinitionOptions
            {
                Image = ContainerImage.FromAsset(Path.Combine(Directory.GetCurrentDirectory(), "app"), new AssetImageProps
                {
                    File = "Benchmark.Client/Dockerfile",
                }),
                Environment = new Dictionary<string, string>
                {
                    { "BENCHCLIENT_RUNASWEB", "true" },
                    { "BENCHCLIENT_HOSTADDRESS", "http://0.0.0.0:80" },
                    { "BENCHCLIENT_S3BUCKET", s3.BucketName },
                },
                Logging = LogDriver.AwsLogs(new AwsLogDriverProps
                {
                    LogGroup = new LogGroup(this, "MagicOnionBenchWorkerLogGroup", new LogGroupProps
                    {
                        LogGroupName = logGroup,
                        RemovalPolicy = RemovalPolicy.DESTROY,
                        Retention = RetentionDays.TWO_WEEKS,
                    }),
                    StreamPrefix = logGroup,
                }),
            }).AddPortMappings(new PortMapping
            {
                ContainerPort = 80,
                HostPort = 80,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP,
            });
            var service = new ApplicationLoadBalancedFargateService(this, "MagicOnionBenchWorkerService", new ApplicationLoadBalancedFargateServiceProps
            {
                ServiceName = "MagicOnionBenchWorkerService",
                TaskSubnets = subnets,
                Cluster = cluster,
                TaskDefinition = taskDef,
                DesiredCount = 1,
                Cpu = 256,
                MemoryLimitMiB = 512,
                PublicLoadBalancer = true,
                PlatformVersion = FargatePlatformVersion.VERSION1_4,
            });

            new CfnOutput(this, "S3BucketName", new CfnOutputProps { Value = s3.BucketName });
            new CfnOutput(this, "EcsClusterName", new CfnOutputProps { Value = cluster.ClusterName });
            new CfnOutput(this, "EcsServiceName", new CfnOutputProps { Value = service.Service.ServiceName });
            new CfnOutput(this, "EcsTaskdefArn", new CfnOutputProps { Value = service.TaskDefinition.TaskDefinitionArn });
        }

        private Role IamMaster(Bucket s3)
        {
            var policy = new Policy(this, "MagicOnionBenchMasterPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListAllMyBuckets" },
                        Resources = new[] { "arn:aws:s3:::*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListBucket","s3:GetBucketLocation" },
                        Resources = new[] { s3.BucketArn },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:GetObject" },
                        Resources = new[] { $"{s3.BucketArn}/*" },
                    }),
                }
            });
            var role = new Role(this, "MagicOnionBenchMasterRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
            });
            role.AttachInlinePolicy(policy);
            return role;
        }

        private Role IamWorkerTaskExecute(string logGroup)
        {
            var policy = new Policy(this, "MagicOnionBenchWorkerTaskDefExecutionPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    // s3
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[]
                        {
                            "logs:CreateLogStream",
                            "logs:PutLogEvents"
                        },
                        Resources = new[] { $"arn:aws:logs:{this.Region}:{this.Account}:log-group:{logGroup}:*" },
                    }),
                }
            });
            var role = new Role(this, "MagicOnionBenchWorkerTaskDefExecutionRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });
            role.AttachInlinePolicy(policy);
            return role;
        }

        private Role IamWorkerTaskDef(Bucket s3)
        {
            var policy = new Policy(this, "MagicOnionBenchWorkerTaskDefTaskPolicy", new PolicyProps
            {
                Statements = new[]
                {
                    // s3
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListAllMyBuckets" },
                        Resources = new[] { "arn:aws:s3:::*" },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[] { "s3:ListBucket","s3:GetBucketLocation" },
                        Resources = new[] { s3.BucketArn },
                    }),
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Actions = new[]
                        {
                            "s3:PutObject",
                            "s3:PutObjectAcl",
                            "s3:GetObject",
                            "s3:GetObjectAcl",
                        },
                        Resources = new[] { $"{s3.BucketArn}/*" },
                    }),
                }
            });
            var role = new Role(this, "MagicOnionBenchWorkerTaskDefTaskRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com"),
            });
            role.AddManagedPolicy(ManagedPolicy.FromManagedPolicyArn(this, "ECSTaskExecutionRolePolicy", "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"));
            role.AttachInlinePolicy(policy);
            return role;
        }
    }
}
