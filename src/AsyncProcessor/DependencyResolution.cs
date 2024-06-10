using Amazon.SQS;

namespace AsyncProcessor;

public static class DependencyResolution
{
  public static IServiceCollection AddAsyncProcessor(this IServiceCollection services)
  {
    services.Configure<QueueConfiguration>(queue =>
    {
      queue.Region = ApplicationConstants.Region;
      queue.Account = ApplicationConstants.Account;
      queue.Name = Environment.GetEnvironmentVariable("QUEUE_NAME")
        ?? throw new ArgumentNullException(nameof(queue.Name));
    });

    services.AddHostedService<WorkerProcessor>();
    services.AddSingleton<IWorkerHandler, WorkerHandler>();
    services.AddSingleton<IAmazonSQS, AmazonSQSClient>();

    return services;
  }
}
