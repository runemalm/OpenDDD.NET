namespace OpenDDD.Main.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StopPriorityAttribute : Attribute
    {
        public int StopPriority { get; }

        public StopPriorityAttribute(int stopPriority)
        {
            StopPriority = stopPriority;
        }
    }
}
