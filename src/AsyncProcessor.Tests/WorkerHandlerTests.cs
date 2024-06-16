using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AsyncProcessor.Tests;

public class WorkerHandlerTests
{
  private readonly Mock<ILogger<WorkerHandler>> _logger = new();
  private readonly Mock<IAmazonSQS> _sqsClient = new();
  private readonly Mock<IOptions<QueueConfiguration>> _config = new();

  private readonly IWorkerHandler _handler;

  public WorkerHandlerTests()
  {
    _config
      .Setup(x => x.Value)
      .Returns(new QueueConfiguration
      {
        Region = "us-east-1",
        Account = "123456789012",
        Name = "MyQueue"
      });

    _handler = new WorkerHandler(
      _logger.Object,
      _sqsClient.Object,
      _config.Object);
  }

  [Fact]
  public async Task ProcessesMessageSuccessfully()
  {
    var message = new Message
    {
      MessageId = "1",
      Body = "Hello, world!"
    };

    await _handler.ProcessMessage(message, default);

    _sqsClient.Verify(x => x.DeleteMessageAsync(
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task LogsErrorWhenProcessingFails()
  {
    var message = new Message
    {
      MessageId = "1",
      Body = "Hello, world!"
    };

    _sqsClient
      .Setup(x => x.DeleteMessageAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("DeleteMessageAsync failed"));

    await _handler.ProcessMessage(message, default);

    _logger.Verify(x => x.Log(
      LogLevel.Error,
      It.IsAny<EventId>(),
      It.IsAny<It.IsAnyType>(),
      It.IsAny<Exception>(),
      It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
  }

  [Fact]
  public async Task RethrowsTaskCanceledException()
  {
    var message = new Message
    {
      MessageId = "1",
      Body = "Hello, world!"
    };

    _sqsClient
      .Setup(x => x.DeleteMessageAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()))
      .ThrowsAsync(new TaskCanceledException("DeleteMessageAsync was canceled"));

    await Assert.ThrowsAsync<TaskCanceledException>(
      () => _handler.ProcessMessage(message, default));
  }
}
