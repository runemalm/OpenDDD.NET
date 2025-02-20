using FluentAssertions;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Tests.Base;
using Xunit;

namespace OpenDDD.Tests.Infrastructure.Events
{
    public class DomainPublisherTests : UnitTests
    {
        private class TestEvent : IDomainEvent { }

        [Fact]
        public async Task PublishAsync_ShouldStoreEvent_WhenValidEventIsPublished()
        {
            // Arrange
            var publisher = new DomainPublisher();
            var domainEvent = new TestEvent();

            // Act
            await publisher.PublishAsync(domainEvent, CancellationToken.None);

            // Assert
            publisher.GetPublishedEvents().Should().ContainSingle()
                .Which.Should().Be(domainEvent);
        }

        [Fact]
        public async Task PublishAsync_ShouldThrowArgumentNullException_WhenEventIsNull()
        {
            // Arrange
            var publisher = new DomainPublisher();

            // Act
            Func<Task> act = async () => await publisher.PublishAsync<TestEvent>(null!, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("domainEvent");
        }

        [Fact]
        public async Task GetPublishedEvents_ShouldReturnEmptyList_WhenNoEventsArePublished()
        {
            // Arrange
            var publisher = new DomainPublisher();

            // Act
            var events = publisher.GetPublishedEvents();

            // Assert
            events.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPublishedEvents_ShouldReturnAllPublishedEvents()
        {
            // Arrange
            var publisher = new DomainPublisher();
            var event1 = new TestEvent();
            var event2 = new TestEvent();

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
