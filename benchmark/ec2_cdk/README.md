# Welcome to your CDK C# project!

This is a CDK project to create EC2 instance to run benchmark.

The `cdk.json` file tells the CDK Toolkit how to execute your app.

It uses the [.NET Core CLI](https://docs.microsoft.com/dotnet/articles/core/) to compile and execute your project.

## Step to deploy

install cdk cli.

```shell
npm install -g aws-cdk
npm update -g aws-cdk
```

deploy cdk.

```shell
cdk synth
cdk bootstrap # only on initial execution
cdk deploy
```

## After cdk deployed

login to ec2 via Session Manager.

make sure you can run docker.

```shell
docker run --rm hello-world
```

run command to bench.

```sh
git clone https://github.com/guitarrapc/benchmar-lab.git
cd benchmark/app
./build.sh
./bench.sh
cat ./results/**/*.report
```
