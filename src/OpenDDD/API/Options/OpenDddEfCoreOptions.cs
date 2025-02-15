namespace OpenDDD.API.Options
{
    public class OpenDddEfCoreOptions
    {
        public string Database { get; set; } = "SQLite";
        public string ConnectionString { get; set; } = "DataSource=Main/EfCore/YourProjectName.db;Cache=Shared";
    }
}
