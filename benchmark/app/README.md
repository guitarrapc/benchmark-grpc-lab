# Benchmarker

There are 3 server implementaion projects.

* dotnet_grpc_bench: gRPC server implemeted for both gRPC and MagicOnion.
* dotnet_grpc_https_bench: Self Cert gRPC server implemeted for both gRPC and MagicOnion.
* dotnet_api_bench: REST server implemented with ASP.NET Core API.

You can benchmark these implementaion with `C# Benchmarker` or `grpc_bench`.

## C# Benchmarker

C# implemented benchmarker for all server implementations `dotnet_grpc_bench`, `dotnet_grpc_https_bench` and `dotnet_api_bench`.

### Prerequisites

Linux or Windows or MacOS or All platform with Docker. Keep in mind that the results on Docker may not be that reliable, Docker for Mac/Windows runs on a VM.

### Running benchmark on Docker

* To build the benchmarks images use following. You need them to run the benchmarks.
  * linux: `./build.sh [BENCH1] [BENCH2]`
  * windows: `./build.ps1 [BENCH1] [BENCH2]`

To run the benchmarks use following. They will be run sequentially.
  * linux: `./bench.sh [BENCH1] [BENCH2]`
  * windows: `./bench.ps1 [BENCH1] [BENCH2]`

To clean-up the benchmark images use following.
  * linux: `./clean.sh [BENCH1] [BENCH2]`.
  * windows: not supported.

> TIPS: to change benchclient command, write command to `bench_command` file.

### Running benchmark on Host

You need .NET 5 SDK to build and run benchmark on host.

* To build the benchmarks binary use following.
  * linux: `./publish.sh [BENCH1] [BENCH2]`
  * windows: `./publish.ps1 [BENCH1] [BENCH2]`

To run the benchmarks use following. They will be run sequentially.
  * linux: `./run.sh [BENCH1] [BENCH2]`
  * windows: `./run.ps1 [BENCH1] [BENCH2]`

## grpc_bench

[grpc_bench](https://github.com/LesnyRumcajs/grpc_bench) is comminuty run benchmark of different gRPC server implemetaions.
It benchmark to gRPC implementation though Proto scheme.

We use this benchmark to compare out Benchmarker, this identify Server implementation misstake or Client Benchmarker misstake.

### Prerequisites

Windows or Linux or MacOS with Docker. Keep in mind that the results on MacOS may not be that reliable, Docker for Mac runs on a VM.

### Running benchmark

* To build the benchmarks images use following. You need them to run the benchmarks.
  * linux: `./ghz_build.sh [BENCH1] [BENCH2]`
  * windows: `./ghz_build.ps1 [BENCH1] [BENCH2]`

To run the benchmarks use following. They will be run sequentially.
  * linux: `./ghz_bench.sh [BENCH1] [BENCH2]`
  * windows: `./ghz_bench.ps1 [BENCH1] [BENCH2]`

To clean-up the benchmark images use following.
  * linux: `./ghz_clean.sh [BENCH1] [BENCH2]`.
  * windows: not supported.
