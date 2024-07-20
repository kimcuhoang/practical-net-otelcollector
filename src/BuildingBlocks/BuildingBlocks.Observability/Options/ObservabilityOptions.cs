namespace BuildingBlocks.Observability.Options;

public class ObservabilityOptions
{
    public string ServiceName { get; set; } = default!;
    public string CollectorUrl { get; set; } = @"http://localhost:4317";

    public bool EnabledTracing { get; set; } = false;
    public bool EnabledMetrics { get; set; } = false;

    public Uri CollectorUri => new(this.CollectorUrl);

    public string OtlpLogsCollectorUrl => $"{this.CollectorUrl}/v1/logs";
}
