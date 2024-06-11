using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ECS.Patterns;
using Amazon.CDK.AWS.Logs;

namespace AsyncProcessor.Cdk;

public class ServiceStack : Stack
{
  public ServiceStack(
    Construct scope,
    string id,
    IStackProps? props = null) : base(scope, id, props)
  {
    var vpc = new Vpc(this, "vpc");
    var cluster = new Cluster(this, "cluster", new ClusterProps
    {
      Vpc = vpc
    });

    _ = new QueueProcessingFargateService(
      this,
      "queue-processing-service",
      new QueueProcessingFargateServiceProps
      {
        Cluster = cluster,
        Image = ContainerImage.FromAsset("."),
        LogDriver = new AwsLogDriver(new AwsLogDriverProps
        {
          StreamPrefix = "async-processor-service",
          LogRetention = RetentionDays.ONE_DAY
        })
      });
  }
}
