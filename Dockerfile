FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN apt-get update &&  \
    apt-get install -y --no-install-recommends gcc curl zip xz-utils && \
    curl -Lo /opt/node-v20.14.0-linux-x64.tar.xz https://nodejs.org/dist/v20.14.0/node-v20.14.0-linux-x64.tar.xz && \
    tar -xvf /opt/node-v20.14.0-linux-x64.tar.xz -C /opt && rm -rf /opt/node-v20.14.0-linux-x64.tar.xz

ENV PATH=/opt/node-v20.14.0-linux-x64/bin:$PATH

RUN npm install -g aws-cdk

WORKDIR /src
COPY ["src/AsyncProcessor/AsyncProcessor.csproj", "src/AsyncProcessor/AsyncProcessor.csproj"]
RUN dotnet restore "src/AsyncProcessor/AsyncProcessor.csproj"
COPY ["src/AsyncProcessor.Tests/AsyncProcessor.Tests.csproj", "src/AsyncProcessor.Tests/AsyncProcessor.Tests.csproj"]
RUN dotnet restore "src/AsyncProcessor.Tests/AsyncProcessor.Tests.csproj"
COPY ["src/AsyncProcessor.Cdk/AsyncProcessor.Cdk.csproj", "src/AsyncProcessor.Cdk/AsyncProcessor.Cdk.csproj"]
RUN dotnet restore "src/AsyncProcessor.Cdk/AsyncProcessor.Cdk.csproj"

COPY ["src/AsyncProcessor", "src/AsyncProcessor"]
COPY ["src/AsyncProcessor.Tests", "src/AsyncProcessor.Tests"]
COPY ["src/AsyncProcessor.Cdk", "src/AsyncProcessor.Cdk"]

RUN dotnet test "src/AsyncProcessor.Tests/AsyncProcessor.Tests.csproj" --logger "trx;LogFileName=testresults.trx" --results-directory /testresults
RUN dotnet publish "src/AsyncProcessor/AsyncProcessor.csproj" -c Release -o /src/publish --runtime linux-x64 --self-contained false

FROM base as final
WORKDIR /app
COPY --from=build /src/publish .
ENTRYPOINT ["dotnet", "AsyncProcessor.dll"]
