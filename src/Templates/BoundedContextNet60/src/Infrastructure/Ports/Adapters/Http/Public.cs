using DDD.Infrastructure.Ports.Adapters.Http;

namespace Infrastructure.Ports.Adapters.Http
{
    /// <summary>
	/// An attribute to denote the endpoint should be added
	/// to the 'Public' swagger definition.
	/// </summary>
    public class Public : DocsDefinitionAttribute
    {
        public Public() : base()
        {
            
        }
    }
}
