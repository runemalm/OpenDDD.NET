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
        public bool Seeders { get; set; } = true;

        public void EnableAll()
        {
            Actions = true;
            DomainServices = true;
            Repositories = true;
            InfrastructureServices = true;
            EventListeners = true;
            EfCoreConfigurations = true;
            Seeders = true;
        }

        public void DisableAll()
        {
            Actions = false;
            DomainServices = false;
            Repositories = false;
            InfrastructureServices = false;
            EventListeners = false;
            EfCoreConfigurations = false;
            Seeders = false;
        }
    }
}
