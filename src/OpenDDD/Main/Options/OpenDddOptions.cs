namespace OpenDDD.Main.Options
{
    public class OpenDddOptions
    {
        public string PersistenceProvider { get; set; } = "EfCore";
        public string StorageProvider { get; set; } = "SQLite";
        public string ConnectionString { get; set; } = "DataSource=Main/EfCore/YourApplicationName.db;Cache=Shared";
        public bool AutoRegisterDomainServices { get; set; } = true;
        public bool AutoRegisterRepositories { get; set; } = true;
        public bool AutoRegisterActions { get; set; } = true;
        public bool AutoRegisterConfigurations { get; set; } = true;
        public bool AutoRegisterInfrastructureServices { get; set; } = true;
        
        public OpenDddOptions UseEfCore()
        {
            PersistenceProvider = "EfCore";
            return this;
        }

        public OpenDddOptions UsePostgres(string? connectionString = null)
        {
            ValidatePersistenceProvider("EfCore");
            StorageProvider = "Postgres";
            ConnectionString = connectionString ?? string.Empty;
            return this;
        }
        
        public OpenDddOptions UseSqlServer(string? connectionString = null)
        {
            ValidatePersistenceProvider("EfCore");
            StorageProvider = "SqlServer";
            ConnectionString = connectionString ?? string.Empty;
            return this;
        }
        
        public OpenDddOptions UseSQLite(string? connectionString = null)
        {
            ValidatePersistenceProvider("EfCore");
            StorageProvider = "SQLite";
            ConnectionString = connectionString ?? string.Empty;
            return this;
        }

        public OpenDddOptions UseInMemory()
        {
            ValidatePersistenceProvider("EfCore");
            StorageProvider = "InMemory";
            ConnectionString = string.Empty;
            return this;
        }
        
        public OpenDddOptions EnableAutoRegisterConfigurations()
        {
            AutoRegisterConfigurations = true;
            return this;
        }

        public OpenDddOptions DisableAutoRegisterConfigurations()
        {
            AutoRegisterConfigurations = false;
            return this;
        }

        public OpenDddOptions EnableAutoRegisterDomainServices()
        {
            AutoRegisterDomainServices = true;
            return this;
        }

        public OpenDddOptions DisableAutoRegisterDomainServices()
        {
            AutoRegisterDomainServices = false;
            return this;
        }

        public OpenDddOptions EnableAutoRegisterRepositories()
        {
            AutoRegisterRepositories = true;
            return this;
        }

        public OpenDddOptions DisableAutoRegisterRepositories()
        {
            AutoRegisterRepositories = false;
            return this;
        }

        public OpenDddOptions EnableAutoRegisterActions()
        {
            AutoRegisterActions = true;
            return this;
        }

        public OpenDddOptions DisableAutoRegisterActions()
        {
            AutoRegisterActions = false;
            return this;
        }
        
        public OpenDddOptions EnableAutoRegisterInfrastructureServices()
        {
            AutoRegisterInfrastructureServices = true;
            return this;
        }

        public OpenDddOptions DisableAutoRegisterInfrastructureServices()
        {
            AutoRegisterInfrastructureServices = false;
            return this;
        }

        private void ValidatePersistenceProvider(params string[] allowedProviders)
        {
            if (!allowedProviders.Contains(PersistenceProvider))
            {
                throw new InvalidOperationException($"The current PersistenceProvider '{PersistenceProvider}' is not compatible with this StorageProvider '{StorageProvider}'.");
            }
        }
    }
}
