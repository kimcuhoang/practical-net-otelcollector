using Microservices.WeatherService.Services;
using BuildingBlocks.Observability;
using BuildingBlocks.Observability.Middlewares;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

var webApplicationBuilder = WebApplication.CreateBuilder(args);

var environment = webApplicationBuilder.Environment;
var configuration = webApplicationBuilder.Configuration;

webApplicationBuilder.Configuration
                    .SetBasePath(environment.ContentRootPath)
                    .AddJsonFile("appsettings.json", false, true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);

webApplicationBuilder.AddObservability();

webApplicationBuilder.Services.AddGrpc();


webApplicationBuilder.WebHost
            .CaptureStartupErrors(true)
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 6001, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });


var app = webApplicationBuilder.Build();


app.UseTraceIdResponseHeader();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();

app.MapGet("/", () => "WeatherService");

app.Run();
