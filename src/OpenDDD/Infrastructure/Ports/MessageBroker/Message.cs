namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public class Message : IMessage
    {
        private readonly string _content;
        
        public Message(string content)
        {
            _content = content;
        }

        public override string ToString()
        {
            return _content;
        }
    }
}
