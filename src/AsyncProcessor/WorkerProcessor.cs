using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace AsyncProcessor;

public class WorkerProcessor(
  ILogger<WorkerProcessor> _logger,
  IAmazonSQS _sqsClient,
  IWorkerHandler _handler,
  IOptions<QueueConfiguration> config)
  : BackgroundService
{
  private readonly string _sqsUrl = config.Value.SqsUrl;

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation("WorkerProcessor is starting.");

    while (!stoppingToken.IsCancellationRequested)
    {
      try
      {
        var receiveMessageRequest = new ReceiveMessageRequest
        {
          QueueUrl = _sqsUrl,
          MaxNumberOfMessages = 10,
          WaitTimeSeconds = 20
        };

        var response = await _sqsClient.ReceiveMessageAsync(
          receiveMessageRequest,
          stoppingToken);

        foreach (var message in response.Messages)
        {
          await _handler.ProcessMessage(message, stoppingToken);
        }
      }
      catch (TaskCanceledException)
      {
        _logger.LogInformation("WorkerProcessor is stopping.");
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, ex.Message);
      }
    }
  }
}
