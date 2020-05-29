using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Producer
{
    class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();

                    services.Configure<EventHubOptions>(hostContext.Configuration.GetSection(EventHubOptions.EventHub));

                    services.AddHostedService<Worker>();
                });
    }
}
