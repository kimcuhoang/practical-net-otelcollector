namespace ClientApps.ClientSimulator.GrpcServices;

public class GrpcServiceOptions
{
    public GrpcServiceDependency WeatherService { get; set; }

    public class GrpcServiceDependency
    {
        public string Name { get; set; }
        public string ServiceUrl { get; set; }
        public string ServiceGrpcUrl { get; set; }

        public void AsGrpcClient<TClient>(IServiceCollection services) where TClient: class
        {
            services.AddGrpcClient<TClient>((serviceProvider, client) =>
            {
                var uri = new Uri(this.ServiceGrpcUrl);

                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<TClient>();
                logger.LogInformation($"{this}: {uri}");

                client.Address = uri;
            });
        }

        public override string ToString() => $"{this.Name}-{this.ServiceGrpcUrl}";
    }
}
