namespace OpenDDD.API.Options
{
    public class OpenDddSqliteOptions
    {
        public string ConnectionString { get; set; } = "DataSource=Infrastructure/Persistence/EfCore/YourProjectName.db;Cache=Shared";
    }
}
