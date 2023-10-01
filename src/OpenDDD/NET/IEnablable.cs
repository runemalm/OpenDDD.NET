namespace OpenDDD.NET
{
    public interface IEnablable
    {
        bool IsEnabled { get; set; }
        
        void Enable();
        void Disable();
    }
}
