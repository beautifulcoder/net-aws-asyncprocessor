using Amazon.CDK;
using Amazon.CDK.Assertions;
using AsyncProcessor.Cdk;

namespace AsyncProcessor.Tests;

public class ServiceStackTests
{
  [Fact]
  public void ServiceStackContainsResources()
  {
    var app = new App();
    var stack = new ServiceStack(app, "test-stack");

    var template = Template.FromStack(stack);

    Assert.NotNull(template);

    template.ResourceCountIs("AWS::EC2::VPC", 1);
    template.ResourceCountIs("AWS::ECS::Cluster", 1);
    template.ResourceCountIs("AWS::ECS::Service", 1);
    template.ResourceCountIs("AWS::Logs::LogGroup", 1);
    template.ResourceCountIs("AWS::ECS::TaskDefinition", 1);
  }
}
