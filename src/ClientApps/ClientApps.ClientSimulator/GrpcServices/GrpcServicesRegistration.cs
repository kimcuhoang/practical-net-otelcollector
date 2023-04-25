using ClientApps.ClientSimulator.GrpcServices;
using ClientApps.SimulateClientApp.GrpcServices.WeatherProxyServices;
using Microservices.WeatherService;

namespace ClientApps.SimulateClientApp.GrpcServices;

public static class GrpcServicesRegistration
{
    public static WebApplicationBuilder AddGrpcServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();

        var configuration = builder.Configuration;
        var grpcServiceOptions = new GrpcServiceOptions();

        configuration
            .GetRequiredSection(nameof(GrpcServiceOptions))
            .Bind(grpcServiceOptions);

        grpcServiceOptions
            .WeatherService.AsGrpcClient<Greeter.GreeterClient>(builder.Services);

        builder.Services
            .AddScoped<IWeatherGreetingGrpcService, WeatherGreetingGrpcService>();

        return builder;
    }

    public static WebApplication MapGrpcServices(this WebApplication webApplication)
    {
        return webApplication;
    }
}
