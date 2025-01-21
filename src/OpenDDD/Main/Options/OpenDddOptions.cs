namespace OpenDDD.Main.Options
{
    public class OpenDddOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool AutoRegisterDomainServices { get; set; } = true;
        public bool AutoRegisterRepositories { get; set; } = true;
        public bool AutoRegisterActions { get; set; } = true;
    }
}
