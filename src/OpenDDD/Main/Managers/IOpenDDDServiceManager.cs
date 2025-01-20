namespace OpenDDD.Main.Managers
{
    public interface IOpenDddServiceManager
    {
        Task StartServicesAsync(CancellationToken cancellationToken = default);
        Task StopServicesAsync(CancellationToken cancellationToken = default);
    }
}
