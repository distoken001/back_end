using Quartz;

namespace deMarketService.Common.Model.HttpApiModel
{
    public class JobState
    {
        public string job { get; set; }
        public TriggerState state { get; set; }
    }
}
