using ClientApps.SimulateClientApp.GrpcServices.WeatherProxyServices;
using Microservices.WeatherService;

namespace ClientApps.SimulateClientApp.GrpcServices;

public static class GrpcServicesRegistration
{
    public static WebApplicationBuilder AddGrpcServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc();

        builder.Services
            .AddGrpcClient<Greeter.GreeterClient>(client =>
            {
                client.Address = new Uri(@"http://localhost:16001");
            });

        builder.Services
            .AddScoped<IWeatherGreetingGrpcService, WeatherGreetingGrpcService>();

        return builder;
    }

    public static WebApplication MapGrpcServices(this WebApplication webApplication)
    {
        return webApplication;
    }
}
