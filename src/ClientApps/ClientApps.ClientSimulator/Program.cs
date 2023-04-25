using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using BuildingBlocks.Observability;
using BuildingBlocks.Observability.Middlewares;
using ClientApps.SimulateClientApp.GrpcServices;
using ClientApps.SimulateClientApp.GrpcServices.WeatherProxyServices;

var webApplicationBuilder = WebApplication.CreateBuilder(args);

var environment = webApplicationBuilder.Environment;
var configuration = webApplicationBuilder.Configuration;

webApplicationBuilder.Configuration
                    .SetBasePath(environment.ContentRootPath)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);

webApplicationBuilder
    .AddObservability()
    .AddGrpcServices();


webApplicationBuilder.WebHost
            .CaptureStartupErrors(true)
            .UseKestrel(options =>
            {
                options.AddServerHeader = false;

                // We have to separate into different ports because we don't use TLS 
                options.Listen(IPAddress.Any, 6002, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http1;
                });
                options.Listen(IPAddress.Any, 16002, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http2;
                });
            });


var app = webApplicationBuilder.Build();


app
    .UseTraceIdResponseHeader()
    .MapGrpcServices();

app.MapGet("/", () => "SimulateClientApp");

app.MapGet("/hello", async (IWeatherGreetingGrpcService weatherGreetingService, CancellationToken cancellationToken) =>
{
    var result = await weatherGreetingService.SayHello(new Microservices.WeatherService.HelloRequest
    {
        Name = "SimulateClientApp"
    }, cancellationToken);

    return result.Message;
});


app.Run();