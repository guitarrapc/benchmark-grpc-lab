# Benchmark

This is MagicOnion and gRPC Benchmark project.
You can run benchmark with `GitHub Actions`, `C# Benchmarker`, `grpc_bench`.
If you want run your benchmark on AWS EC2 VM, use AWS CDK.

* Benchmark apps are locate under `./app`.
* AWS CDK Benchmark Project is locate under `./Cdk`.

## Benchmark Servers

There are 3 servers to compare difference.

* dotnet_grpc_bench: gRPC server implemeted with both gRPC and MagicOnion.
* dotnet_grpc_https_bench: Self Cert gRPC server implemeted with both gRPC and MagicOnion.
* dotnet_api_bench: REST server implemented with ASP.NET Core API.
VM.

All Server implementations logic is same for each style, recieve message, deserialize and return response.
This identify each server performance changes.

## Run Benchmarker with GitHub Actions

Create Issue with title `RUN BENCHMARK`, body `30s`.
This trigger benchmark on GitHub Actions and report result when completed.

### Run C# Benchmarker

C# Benchmarker is minimum implementation with following what comminuty run benchmark do.
It benchmark to MagicOnion, gRPC and REST API implementaion through C# code sharing.

We use this benchmark to compare out MagicOnion and gRPC performance changes.

> see detail [README.md](app/README.md)

## Run grpc_bench Benchmarker

[grpc_bench](https://github.com/LesnyRumcajs/grpc_bench) is comminuty run benchmark of different gRPC server implemetaions.
It benchmark to gRPC implementation though Proto scheme.

We use this benchmark to compare out Benchmarker, this identify Server implementation misstake or Client Benchmarker misstake.

> see detail [README.md](app/README.md)

## Run benchmark on AWS EC2

We offer AWS CDK project to building you ec2 benchmark environment and prepare docker and binary on it.
Only you need is build binary and deploy CDK.

> see detail [README.CDK](README.CDK.md)
