using Quartz;

namespace CommonLibrary.Common.Common
{
    public class JobState
    {
        public string job { get; set; }
        public TriggerState state { get; set; }
    }
}