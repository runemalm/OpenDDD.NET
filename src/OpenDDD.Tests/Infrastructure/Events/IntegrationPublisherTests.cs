using FluentAssertions;
using Xunit;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Infrastructure.Events
{
    public class IntegrationPublisherTests : UnitTests
    {
        private class TestIntegrationEvent : IIntegrationEvent { }

        [Fact]
        public async Task PublishAsync_ShouldStoreEvent_WhenValidEventIsPublished()
        {
            // Arrange
            var publisher = new IntegrationPublisher();
            var integrationEvent = new TestIntegrationEvent();

            // Act
            await publisher.PublishAsync(integrationEvent, CancellationToken.None);

            // Assert
            publisher.GetPublishedEvents().Should().ContainSingle()
                .Which.Should().Be(integrationEvent);
        }

        [Fact]
        public async Task PublishAsync_ShouldThrowArgumentNullException_WhenEventIsNull()
        {
            // Arrange
            var publisher = new IntegrationPublisher();

            // Act
            Func<Task> act = async () => await publisher.PublishAsync<TestIntegrationEvent>(null!, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("integrationEvent");
        }

        [Fact]
        public async Task GetPublishedEvents_ShouldReturnEmptyList_WhenNoEventsArePublished()
        {
            // Arrange
            var publisher = new IntegrationPublisher();

            // Act
            var events = publisher.GetPublishedEvents();

            // Assert
            events.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPublishedEvents_ShouldReturnAllPublishedEvents()
        {
            // Arrange
            var publisher = new IntegrationPublisher();
            var event1 = new TestIntegrationEvent();
            var event2 = new TestIntegrationEvent();

            // Act
            await publisher.PublishAsync(event1, CancellationToken.None);
            await publisher.PublishAsync(event2, CancellationToken.None);
            var publishedEvents = publisher.GetPublishedEvents();

            // Assert
            publishedEvents.Should().HaveCount(2);
            publishedEvents.Should().ContainInOrder(event1, event2);
        }
    }
}
