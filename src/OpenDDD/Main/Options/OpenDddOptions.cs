namespace OpenDDD.Main.Options
{
    public class OpenDddOptions
    {
        public string PersistenceProvider { get; set; } = "EfCore";
        public string MessagingProvider { get; set; } = "InMemory";
        public OpenDddEfCoreOptions EfCore { get; set; } = new();
        public OpenDddAutoRegisterOptions AutoRegister { get; set; } = new();
        public OpenDddEventsOptions Events { get; set; } = new();
        public OpenDddAzureServiceBusOptions AzureServiceBus { get; set; } = new();
    }
}
