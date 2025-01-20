namespace OpenDDD.Main.Options
{
    public class OpenDddOptions
    {
        public string ConnectionString { get; set; } = string.Empty;
        public bool AutoRegisterActions { get; set; } = true;
        public bool AutoRegisterRepositories { get; set; } = true;
    }
}
