namespace ClientApps.ClientSimulator.GrpcServices;

public class GrpcServiceOptions
{
    public GrpcServiceDependency WeatherService { get; set; }

    public class GrpcServiceDependency
    {
        public string Name { get; set; }
        public string Binding { get; set; } = "grpc";

        public void AsGrpcClient<TClient>(IServiceCollection services) where TClient: class
        {
            services.AddGrpcClient<TClient>((serviceProvider, client) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var uri = configuration.GetServiceUri(this.Name, binding: this.Binding);

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<TClient>();
                logger.LogInformation($"{this}: {uri}");

                client.Address = uri;
            });
        }

        public override string ToString() => $"{Name}-{Binding}";
    }
}
