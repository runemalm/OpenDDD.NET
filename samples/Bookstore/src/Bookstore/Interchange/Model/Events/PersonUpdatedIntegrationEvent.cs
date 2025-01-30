using OpenDDD.Domain.Model;

namespace Bookstore.Interchange.Model.Events
{
    public class PersonUpdatedIntegrationEvent : IIntegrationEvent
    {
        public string Email { get; set; }
        public string FullName { get; set; }

        public PersonUpdatedIntegrationEvent() { }

        public PersonUpdatedIntegrationEvent(string email, string fullName)
        {
            Email = email;
            FullName = fullName;
        }

        public override string ToString()
        {
            return $"PersonUpdatedIntegrationEvent: Email={Email}, FullName={FullName}";
        }
    }
}
