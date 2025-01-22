namespace OpenDDD.Main.Interfaces
{
    public interface IStartable
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
