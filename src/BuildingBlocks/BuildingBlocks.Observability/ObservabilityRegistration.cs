using BuildingBlocks.Observability.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Settings.Configuration;

namespace BuildingBlocks.Observability;

public static class ObservabilityRegistration
{
    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        ObservabilityOptions observabilityOptions = new();

        configuration
            .GetRequiredSection(nameof(ObservabilityOptions))
            .Bind(observabilityOptions);

        builder.AddSerilog(observabilityOptions);

        return builder;
    }

    private static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder, ObservabilityOptions observabilityOptions)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddSerilog((sp, serilog) =>
        {
            serilog
                .ReadFrom.Configuration(configuration, new ConfigurationReaderOptions
                {
                    SectionName = $"{nameof(ObservabilityOptions)}:{nameof(Serilog)}"
                })
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ApplicationName", observabilityOptions.ServiceName)
                .WriteTo.Console();
        });

        return builder;
    }
}
