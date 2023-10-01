// using System.Threading.Tasks;
// using OpenDDD.Domain.Model;
// using OpenDDD.Main;
// using OpenDDD.NET;
// using ThMap.PubSub;
//
// namespace OpenDDD.Infrastructure.Ports.Adapters.Events
// {
//     public class ListenerBase : SubscriberBase, IListener
//     {
//         private readonly string _eventName;
//         private readonly DomainModelVersionBase _domainModelVersion;
//         private readonly ContextType _contextType;
//         private readonly IConnection _connection;
//         private readonly IDateTimeProvider _dateTimeProvider;
//         
//         public ListenerBase(
//             string eventName,
//             DomainModelVersionBase domainModelVersion,
//             ContextType contextType,
//             IConnection connection,
//             IDateTimeProvider dateTimeProvider)
//         {
//             _eventName = eventName;
//             _domainModelVersion = domainModelVersion;
//             _contextType = contextType;
//             _connection = connection;
//             _dateTimeProvider = dateTimeProvider;
//         }
//
//         public override Task SubscribeAsync(string topic)
//         {
//             throw new System.NotImplementedException();
//         }
//
//         public override Task UnsubscribeAsync(string topic)
//         {
//             throw new System.NotImplementedException();
//         }
//     }
// }
