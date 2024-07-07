
using Microservices.WeatherService.GrpcServices.GreetingService;

namespace Microservices.WeatherService.GrpcServices;

public static class GrpcServicesRegistration
{
    public static WebApplicationBuilder AddGrpcServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddGrpc(options =>
        {
            options.EnableDetailedErrors = true;
        });

        return builder;
    }

    public static WebApplication MapGrpcServices(this WebApplication webApplication)
    {
        webApplication.MapGrpcService<GreetingGrpcService>();

        return webApplication;
    }
}
