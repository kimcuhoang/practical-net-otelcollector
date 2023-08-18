using Microsoft.AspNetCore.Server.Kestrel.Core;
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

webApplicationBuilder.Services
    // https://stackoverflow.com/a/71933535
    // TLDR: AddEndpointsApiExplorer for minimal API
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(swagger =>
    {
        // https://stackoverflow.com/a/71202135
        // https://stackoverflow.com/a/53521371
        swagger.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Practical .NET with OTEL Collector",
            Version = "v1",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Email = "kim.cuhoang@gmail.com",
                Name = "Kim CH"
            }
        });
    });


webApplicationBuilder.WebHost
            .CaptureStartupErrors(true)
            .UseKestrel(options =>
            {
                options.AddServerHeader = false;

                // We have to separate into different ports because we don't use TLS 
                options.ListenAnyIP(6002, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http1;
                });
                options.ListenAnyIP(16002, listeningOptions =>
                {
                    listeningOptions.Protocols = HttpProtocols.Http2;
                });
            });


var app = webApplicationBuilder.Build();

//https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?source=recommendations&view=aspnetcore-7.0&tabs=visual-studio

app
.UseSwagger()
.UseSwaggerUI(swagger =>
{
    //swagger.RoutePrefix = string.Empty;
    swagger.SwaggerEndpoint("/swagger/v1/swagger.json", ".NET OTEL v1");
});

app
    .UseTraceIdResponseHeader()
    .MapGrpcServices();

app
    .MapGet("/", () => TypedResults.Ok("SimulateClientApp"))
    .WithName("Test-01")
    .WithOpenApi();

app
    .MapGet("/hello", async (IWeatherGreetingGrpcService weatherGreetingService, CancellationToken cancellationToken) =>
    {
        var result = await weatherGreetingService.SayHello(new Microservices.WeatherService.HelloRequest
        {
            Name = "SimulateClientApp"
        }, cancellationToken);

        return TypedResults.Ok(result);
    })
    .WithName("Test-02")
    .WithOpenApi(); ;


await app.RunAsync();