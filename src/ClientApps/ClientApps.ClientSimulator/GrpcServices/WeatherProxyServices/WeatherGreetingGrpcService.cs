using Microservices.WeatherService;

namespace ClientApps.SimulateClientApp.GrpcServices.WeatherProxyServices;

public interface IWeatherGreetingGrpcService
{
    Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default);
}

public class WeatherGreetingGrpcService : IWeatherGreetingGrpcService
{
    private readonly Greeter.GreeterClient _client;

    public WeatherGreetingGrpcService(Greeter.GreeterClient client)
    {
        this._client = client;
    }

    public async Task<HelloReply> SayHello(HelloRequest request, CancellationToken cancellationToken = default)
    {
        return await this._client.SayHelloAsync(request, cancellationToken: cancellationToken);
    }
}
