namespace OpenDDD.Main.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class StartPriorityAttribute : Attribute
    {
        public int StartPriority { get; }

        public StartPriorityAttribute(int startPriority)
        {
            StartPriority = startPriority;
        }
    }
}
