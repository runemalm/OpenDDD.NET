namespace OpenDDD.API.Options
{
    public class OpenDddSqlLiteOptions
    {
        public string ConnectionString { get; set; } = "DataSource=Infrastructure/Persistence/EfCore/YourProjectName.db;Cache=Shared";
    }
}
