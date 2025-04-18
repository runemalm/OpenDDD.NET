using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Npgsql;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Persistence.OpenDdd.UoW.Postgres;
using OpenDDD.Infrastructure.TransactionalOutbox;
using OpenDDD.Tests.Base;
using Xunit.Abstractions;

namespace OpenDDD.Tests.Integration.Infrastructure.Persistence.OpenDdd.UoW
{
    [Collection("PostgresTests")]
    public class PostgresUoWTests : IntegrationTests, IAsyncLifetime
    {
        private readonly string _connectionString;
        private Mock<IDomainPublisher> _domainPublisherMock = default!;
        private Mock<IIntegrationPublisher> _integrationPublisherMock = default!;
        private Mock<IOutboxRepository> _outboxRepoMock = default!;

        public PostgresUoWTests(ITestOutputHelper output)
            : base(output, enableLogging: true)
        {
            _connectionString = Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION_STRING")
                ?? "Host=localhost;Port=5432;Database=testdb;Username=testuser;Password=testpassword";
        }

        public async Task InitializeAsync()
        {
            _domainPublisherMock = new Mock<IDomainPublisher>();
            _domainPublisherMock.Setup(x => x.GetPublishedEvents()).Returns(Array.Empty<IDomainEvent>());

            _integrationPublisherMock = new Mock<IIntegrationPublisher>();
            _integrationPublisherMock.Setup(x => x.GetPublishedEvents()).Returns(Array.Empty<IIntegrationEvent>());

            _outboxRepoMock = new Mock<IOutboxRepository>();
            _outboxRepoMock
                .Setup(x => x.SaveEventAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        public async Task DisposeAsync()
        {
            
        }

        [Fact]
        public async Task UnitOfWork_Should_Not_LeakConnections_When_UsedMultipleTimes()
        {
            var exceptions = new List<Exception>();

            for (int i = 0; i < 300; i++)
            {
                var uow = await CreateScopedUnitOfWork();

                try
                {
                    await uow.StartAsync(default);
                    await uow.CommitAsync(default);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
                finally
                {
                    uow.Dispose();
                }
            }

            exceptions.Should().BeEmpty();
        }
        
        private async Task<PostgresOpenDddUnitOfWork> CreateScopedUnitOfWork()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            var session = new PostgresDatabaseSession(connection);

            var uow = new PostgresOpenDddUnitOfWork(
                session,
                _domainPublisherMock.Object,
                _integrationPublisherMock.Object,
                _outboxRepoMock.Object,
                LoggerFactory.CreateLogger<PostgresOpenDddUnitOfWork>());

            return uow;
        }
    }
}
