using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Mock
{
	public interface IMock
	{
		void Reset();
		Task ResetAsync();
	}
}
