using BuildingBlocks.Observability.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Configuration;
using Serilog.Settings.Configuration;
using Serilog.Sinks.OpenTelemetry;

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
        builder.Services
            .AddOpenTelemetry()
            .AddTracing(observabilityOptions);

        return builder;
    }
    private static OpenTelemetryBuilder AddTracing(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {
        if (!observabilityOptions.EnabledTracing) return builder;

        builder.WithTracing(tracing =>
        {
            tracing
                .AddSource(observabilityOptions.ServiceName)
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(observabilityOptions.ServiceName))
                .SetErrorStatusOnException()
                .SetSampler(new AlwaysOnSampler())
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                });

            tracing
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = observabilityOptions.CollectorUri;
                    options.ExportProcessorType = ExportProcessorType.Batch;
                    options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                });
        });

        return builder;
    }

    private static OpenTelemetryBuilder AddMetrics(this OpenTelemetryBuilder builder, ObservabilityOptions observabilityOptions)
    {
        //builder.WithMetrics(metrics =>
        //{
        //    var meter = new Meter(observabilityOptions.ServiceName);

        //    metrics
        //        .AddMeter(meter.Name)
        //        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(meter.Name))
        //        .AddAspNetCoreInstrumentation();

        //    metrics
        //        .AddOtlpExporter(options =>
        //        {
        //            options.Endpoint = observabilityOptions.CollectorUri;
        //            options.ExportProcessorType = ExportProcessorType.Batch;
        //            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        //        });
        //});

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

            serilog
                .WriteTo.OpenTelemetry(c =>
                {
                    c.Endpoint = $"{observabilityOptions.CollectorUrl}/v1/logs";
                    c.IncludedData = IncludedData.TraceIdField | IncludedData.SpanIdField | IncludedData.SourceContextAttribute;
                    c.ResourceAttributes = new Dictionary<string, object>
                                                    {
                                                        {"service.name", observabilityOptions.ServiceName},
                                                        {"index", 10},
                                                        {"flag", true},
                                                        {"value", 3.14}
                                                    };
                });
        });

        return builder;
    }
}
