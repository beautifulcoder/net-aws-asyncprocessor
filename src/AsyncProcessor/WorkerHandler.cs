using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace AsyncProcessor;

public interface IWorkerHandler
{
  Task ProcessMessage(Message message, CancellationToken cancellationToken);
}

public class WorkerHandler(
  ILogger<WorkerHandler> _logger,
  IAmazonSQS _sqsClient,
  IOptions<QueueConfiguration> config)
  : IWorkerHandler
{
  private readonly string _sqsUrl = config.Value.SqsUrl;

  public async Task ProcessMessage(Message message, CancellationToken cancellationToken)
  {
    try
    {
      _logger.LogInformation("Processing message {MessageId}", message.MessageId);

      // Simulate processing time
      await Task.Delay(200, cancellationToken);

      _logger.LogInformation("Body: {Body}", message.Body);
      _logger.LogInformation("Message {MessageId} processed", message.MessageId);

      await _sqsClient.DeleteMessageAsync(_sqsUrl, message.ReceiptHandle, cancellationToken);
    }
    catch (TaskCanceledException)
    {
      throw;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, ex.Message);
    }
  }
}
