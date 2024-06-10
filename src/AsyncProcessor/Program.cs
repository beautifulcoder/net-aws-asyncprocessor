using AsyncProcessor;

Host.CreateDefaultBuilder(args)
  .ConfigureServices((_, services) =>
  {
    services.AddHostedService<WorkerProcessor>();
    services.AddAsyncProcessor();
  })
  .Build()
  .Run();
