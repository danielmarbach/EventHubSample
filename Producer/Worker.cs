using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Producer
{
    class Worker : BackgroundService
    {
        private ILogger<Worker> logger;
        private IOptions<EventHubOptions> options;

        public Worker(ILogger<Worker> logger, IOptions<EventHubOptions> options)
        {
            this.options = options;
            this.logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            long counter = 0;
            await using var producerClient = new EventHubProducerClient(options.Value.ConnectionString, options.Value.Name);

            while (!stoppingToken.IsCancellationRequested)
            {
                using EventDataBatch eventBatch = await producerClient.CreateBatchAsync(stoppingToken);

                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(counter++.ToString())));
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(counter++.ToString())));
                eventBatch.TryAdd(new EventData(Encoding.UTF8.GetBytes(counter++.ToString())));

                await producerClient.SendAsync(eventBatch, stoppingToken);

                logger.Log(LogLevel.Debug,".");

                await Task.Delay(5, stoppingToken);
            }
        }
    }
}