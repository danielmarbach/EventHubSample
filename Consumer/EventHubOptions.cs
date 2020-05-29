namespace Producer
{
    public class EventHubOptions
    {
        public const string EventHub = "EventHub";

        public string ConnectionString { get; set; }
        public string BlobConnectionString { get; set; }
        public string BlobContainerName { get; set; }
        public string Name { get; set; }
        public string ConsumerGroup { get; set; }
    }
}