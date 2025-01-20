namespace OpenDDD.Main.Interfaces
{
    public interface IStoppable
    {
        Task StopAsync(CancellationToken cancellationToken);
    }
}
