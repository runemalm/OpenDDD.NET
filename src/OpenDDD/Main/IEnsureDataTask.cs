using System.Threading;
using OpenDDD.Application;

namespace OpenDDD.Main
{
	public interface IEnsureDataTask
	{
		void Execute(ActionId actionId, CancellationToken ct);
	}
}
