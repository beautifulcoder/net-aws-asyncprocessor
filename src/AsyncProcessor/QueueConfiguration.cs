namespace AsyncProcessor;

public class QueueConfiguration
{
  public required string Region { get; set; }
  public required string Account { get; set; }
  public required string Name { get; set; }

  public string SqsUrl => $"https://sqs.{Region}.amazonaws.com/{Account}/{Name}";
}
