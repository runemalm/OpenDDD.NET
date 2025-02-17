using OpenDDD.Domain.Model;

namespace OpenDDD.Tests.Domain.Model
{
    public class TestValueObject : IValueObject
    {
        public int Number { get; private set; }
        public string Text { get; private set; }

        private TestValueObject() { }

        public TestValueObject(int number, string text)
        {
            Number = number;
            Text = text;
        }
    }
}
