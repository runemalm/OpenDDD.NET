using OpenDDD.Infrastructure.Persistence.OpenDdd.Serializers;
using OpenDDD.Tests.Base;
using Xunit;

namespace OpenDDD.Tests.Infrastructure.Persistence.OpenDdd.Serializers
{
    public class OpenDddSerializerTests : UnitTests
    {
        private readonly OpenDddSerializer _serializer;

        public OpenDddSerializerTests()
        {
            _serializer = new OpenDddSerializer();
        }

        [Fact]
        public void Should_Serialize_Object_To_Json_With_CamelCase_Properties()
        {
            var testObject = new TestClass(42, "Hello World", 99.99m);
            var json = _serializer.Serialize(testObject);

            Assert.Contains("\"number\":42", json);
            Assert.Contains("\"text\":\"Hello World\"", json);
            Assert.Contains("\"price\":99.99", json);
        }

        [Fact]
        public void Should_Deserialize_Json_Back_To_Object_Correctly()
        {
            var json = "{\"number\":99,\"text\":\"Test Value\",\"price\":199.95}";
            var obj = _serializer.Deserialize<TestClass>(json);

            Assert.Equal(99, obj.Number);
            Assert.Equal("Test Value", obj.Text);
            Assert.Equal(199.95m, obj.Price);
        }

        [Fact]
        public void Should_Preserve_Decimal_Precision_During_Serialization_And_Deserialization()
        {
            var original = new TestClass(1, "Precision Test", 123.456789m);
            var json = _serializer.Serialize(original);
            var deserialized = _serializer.Deserialize<TestClass>(json);

            Assert.Equal(original.Price, deserialized.Price);
        }

        private class TestClass
        {
            public int Number { get; init; }
            public string Text { get; init; }
            public decimal Price { get; init; }

            public TestClass(int number, string text, decimal price)
            {
                Number = number;
                Text = text;
                Price = price;
            }
        }
    }
}
