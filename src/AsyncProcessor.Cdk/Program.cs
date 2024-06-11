using AsyncProcessor.Cdk;

var app = new App();

_ = new ServiceStack(app, "async-processor-stack", new StackProps
{
  Env = new Amazon.CDK.Environment
  {
    Account = "123456789012",
    Region = "us-east-1"
  }
});

app.Synth();
