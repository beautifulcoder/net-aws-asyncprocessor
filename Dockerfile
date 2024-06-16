FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/AsyncProcessor/AsyncProcessor.csproj", "src/AsyncProcessor/AsyncProcessor.csproj"]
RUN dotnet restore "src/AsyncProcessor/AsyncProcessor.csproj"
COPY ["src/AsyncProcessor", "src/AsyncProcessor"]
RUN dotnet publish "src/AsyncProcessor/AsyncProcessor.csproj" -c Release -o /src/publish --runtime linux-x64 --self-contained false

FROM base as final
WORKDIR /app
COPY --from=build /src/publish .
ENTRYPOINT ["dotnet", "AsyncProcessor.dll"]
