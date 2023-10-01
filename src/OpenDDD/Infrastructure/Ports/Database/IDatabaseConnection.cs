using System;
using System.Threading.Tasks;
using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Ports.Database
{
    public interface IDatabaseConnection : IDocumentDatabaseConnection, ITransactionalDatabaseConnection, IStartable, IDisposable
    {
        IDatabaseConnectionSettings Settings { get; set; }
        
        void Open();
        Task OpenAsync();
        void TruncateDatabase();
        Task TruncateDatabaseAsync();
        void Close();
        Task CloseAsync();
    }
}
