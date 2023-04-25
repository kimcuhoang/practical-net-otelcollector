using Grpc.Core;

namespace Microservices.WeatherService.GrpcServices.GreetingService;

public class GreetingGrpcService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    => Task.FromResult(new HelloReply
    {
        Message = $"WeatherService response to {request.Name}"
    });
}
