namespace OpenDDD.API.Options
{
    public class OpenDddAutoRegisterOptions
    {
        public bool Actions { get; set; } = true;
        public bool DomainServices { get; set; } = true;
        public bool Repositories { get; set; } = true;
        public bool InfrastructureServices { get; set; } = true;
        public bool EventListeners { get; set; } = true;
        public bool EfCoreConfigurations { get; set; } = true;
    }
}
