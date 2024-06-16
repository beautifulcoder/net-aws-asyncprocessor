using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AsyncProcessor.Tests;

public class WorkerProcessorTests
{
  private readonly Mock<ILogger<WorkerProcessor>> _logger = new();
  private readonly Mock<IAmazonSQS> _sqsClient = new();
  private readonly Mock<IWorkerHandler> _handler = new();
  private readonly Mock<IOptions<QueueConfiguration>> _config = new();

  private readonly WorkerProcessor _processor;

  public WorkerProcessorTests()
  {
    _config
      .Setup(x => x.Value)
      .Returns(new QueueConfiguration
      {
        Region = "us-east-1",
        Account = "123456789012",
        Name = "MyQueue"
      });

    _processor = new WorkerProcessor(
      _logger.Object,
      _sqsClient.Object,
      _handler.Object,
      _config.Object);
  }

  [Fact]
  public void ProcessesMessagesSuccessfully()
  {
    var message = new Message
    {
      MessageId = "1",
      Body = "Hello, world!"
    };

    var cts = new CancellationTokenSource(25);

    _sqsClient
      .Setup(x => x.ReceiveMessageAsync(
        It.IsAny<ReceiveMessageRequest>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ReceiveMessageResponse
      {
        Messages = [message]
      });

    _processor.StartAsync(cts.Token);

    _handler.Verify(x => x.ProcessMessage(
      message,
      It.IsAny<CancellationToken>()), Times.AtLeastOnce);
  }

  [Fact]
  public void HandlerThrowsException()
  {
    var message = new Message
    {
      MessageId = "1",
      Body = "Hello, world!"
    };

    var cts = new CancellationTokenSource(25);

    _sqsClient
      .Setup(x => x.ReceiveMessageAsync(
        It.IsAny<ReceiveMessageRequest>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new ReceiveMessageResponse
      {
        Messages = [message]
      });

    _handler
      .Setup(x => x.ProcessMessage(
        It.IsAny<Message>(),
        It.IsAny<CancellationToken>()))
      .Throws<Exception>();

    _processor.StartAsync(cts.Token);

    _logger.Verify(x => x.Log(
      LogLevel.Error,
      It.IsAny<EventId>(),
      It.IsAny<It.IsAnyType>(),
      It.IsAny<Exception>(),
      It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
  }
}
