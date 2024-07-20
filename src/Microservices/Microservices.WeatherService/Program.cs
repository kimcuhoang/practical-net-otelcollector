using BuildingBlocks.Observability;
using BuildingBlocks.Observability.Middlewares;
using Microservices.WeatherService.GrpcServices;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Serilog;

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
                options.ListenAnyIP(6001, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http1;
                });
                options.ListenAnyIP(16001, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http2;
                });
            });


using var app = webApplicationBuilder.Build();

app.UseSerilogRequestLogging();

app
    .UseTraceIdResponseHeader()
    .MapGrpcServices();

app.MapGet("/", () => "WeatherService");

await app.RunAsync();
