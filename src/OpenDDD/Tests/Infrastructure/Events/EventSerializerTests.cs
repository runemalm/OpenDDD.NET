using Xunit;
using OpenDDD.Infrastructure.Events;

namespace OpenDDD.Tests.Infrastructure.Events
{
    public class EventSerializerTests
    {
        [Fact]
        public void Serialize_ShouldReturnJsonString_WhenEventIsValid()
        {
            // Arrange
            var testEvent = new TestEvent(
                Guid.NewGuid(),
                "TestEventOccurred",
                DateTime.UtcNow
            );

            // Act
            var json = EventSerializer.Serialize(testEvent);

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(json));
            Assert.Contains("EventId", json);
            Assert.Contains("EventName", json);
            Assert.Contains("OccurredAt", json);
        }

        [Fact]
        public void Serialize_ShouldThrowArgumentNullException_WhenEventIsNull()
        {
            // Arrange
            TestEvent? testEvent = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => EventSerializer.Serialize(testEvent!));
        }

        [Fact]
        public void Deserialize_ShouldReturnEvent_WhenJsonStringIsValid()
        {
            // Arrange
            var testEvent = new TestEvent(
                Guid.NewGuid(),
                "TestEventOccurred",
                DateTime.UtcNow
            );

            var json = EventSerializer.Serialize(testEvent);

            // Act
            var deserializedEvent = EventSerializer.Deserialize<TestEvent>(json);

            // Assert
            Assert.NotNull(deserializedEvent);
            Assert.Equal(testEvent.EventId, deserializedEvent.EventId);
            Assert.Equal(testEvent.EventName, deserializedEvent.EventName);
            Assert.Equal(testEvent.OccurredAt, deserializedEvent.OccurredAt);
        }

        [Fact]
        public void Deserialize_ShouldThrowArgumentNullException_WhenJsonStringIsNull()
        {
            // Arrange
            string? json = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => EventSerializer.Deserialize<TestEvent>(json!));
        }
    }
}
