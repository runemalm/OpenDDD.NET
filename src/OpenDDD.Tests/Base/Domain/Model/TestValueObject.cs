using OpenDDD.Domain.Model;

namespace OpenDDD.Tests.Base.Domain.Model
{
    public class TestValueObject : IValueObject
    {
        public int Number { get; private set; }
        public string Text { get; private set; }

        private TestValueObject() { }  // Needed if using EF Core..

        public TestValueObject(int number, string text)
        {
            Number = number;
            Text = text;
        }
    }
}
