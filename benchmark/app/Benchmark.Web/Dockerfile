#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["app/Benchmark.Web/Benchmark.Web.csproj", "app/Benchmark.Web/"]
COPY ["app/DFrame.ComponentLibrary/DFrame.ComponentLibrary.csproj", "app/DFrame.ComponentLibrary/"]
COPY ["app/DFrame.Hosting/DFrame.Hosting.csproj", "app/DFrame.Hosting/"]
COPY ["app/DFrame.Profiler/DFrame.Profiler.csproj", "app/DFrame.Profiler/"]
COPY ["app/Benchmark.ClientLib/Benchmark.ClientLib.csproj", "app/Benchmark.ClientLib/"]
RUN dotnet restore "app/Benchmark.Web/Benchmark.Web.csproj"
COPY . .
WORKDIR "/src/app/Benchmark.Web"
RUN dotnet build "Benchmark.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Benchmark.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Benchmark.Web.dll"]