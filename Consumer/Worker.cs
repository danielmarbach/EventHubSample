using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
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

            // ideally we would do snapshotting
            var storageClient = new BlobContainerClient(options.Value.BlobConnectionString, options.Value.BlobContainerName);
            var processorClient = new EventProcessorClient(storageClient, options.Value.ConsumerGroup, options.Value.ConnectionString, options.Value.Name);

            async Task processEventHandler(ProcessEventArgs eventArgs)
            {
                if (eventArgs.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                try
                {

                    await Console.Out.WriteLineAsync($"Event Received: { Encoding.UTF8.GetString(eventArgs.Data.Body.ToArray()) }");
                }
                catch (Exception ex)
                {
                    // For real-world scenarios, you should take action appropriate to your application.  For our example, we'll just log
                    // the exception to the console.

                    Console.WriteLine();
                    Console.WriteLine($"An error was observed while processing events.  Message: { ex.Message }");
                    Console.WriteLine();
                }
            };

            async Task processErrorHandler(ProcessErrorEventArgs eventArgs)
            {
                if (eventArgs.CancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await Console.Out.WriteLineAsync();
                await Console.Out.WriteLineAsync("===============================");
                await Console.Out.WriteLineAsync($"The error handler was invoked during the operation: { eventArgs.Operation ?? "Unknown" }, for Exception: { eventArgs.Exception.Message }");
                await Console.Out.WriteLineAsync("===============================");
                await Console.Out.WriteLineAsync();
            }

            processorClient.ProcessEventAsync += processEventHandler;
            processorClient.ProcessErrorAsync += processErrorHandler;

            try
            {

                await processorClient.StartProcessingAsync(stoppingToken);

                using var cancellationSource = new CancellationTokenSource();
                cancellationSource.CancelAfter(TimeSpan.FromSeconds(60));

                while ((!cancellationSource.IsCancellationRequested))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(250), CancellationToken.None);
                }

                await processorClient.StopProcessingAsync(CancellationToken.None);
            }
            finally
            {
                processorClient.ProcessEventAsync -= processEventHandler;
                processorClient.ProcessErrorAsync -= processErrorHandler;
            }
        }
    }
}