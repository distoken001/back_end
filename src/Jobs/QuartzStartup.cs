using CommonLibrary.Common.Common;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System.Collections.Specialized;
using System.Text;

namespace Jobs
{
    public class QuartzStartup
    {
        public static IScheduler scheduler = null;

        public static async Task Run()
        {
            try
            {
                var properties = new NameValueCollection
                {
                    ["quartz.plugin.triggHistory.type"] = "Quartz.Plugin.History.LoggingJobHistoryPlugin, Quartz.Plugins",
                    ["quartz.plugin.jobInitializer.type"] = "Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin, Quartz.Plugins",
                    ["quartz.plugin.jobInitializer.fileNames"] = "Jobs.xml",
                    ["quartz.plugin.jobInitializer.failOnFileNotFound"] = "true",
                    ["quartz.plugin.jobInitializer.scanInterval"] = "120",
                    ["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz",
                    ["quartz.threadPool.threadCount"] = "15",
                    ["quartz.threadPool.threadPriority"] = "Normal",
                };

                ISchedulerFactory sf = new StdSchedulerFactory(properties);
                scheduler = await sf.GetScheduler();
                scheduler.JobFactory = new IOCJobFactory(Startup.privider);
                Console.WriteLine("start the schedule");
                await scheduler.Start();
                Console.WriteLine("end");
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
            catch (Exception ex)
            {
                Console.Write($"err={ex.ToString()}");
            }
        }

        /// <summary>
        /// 暂停或者恢复调度器
        /// </summary>
        /// <param name="scheduler"></param>
        public static async Task PauseOrResumeSchedulerAsync(string command)
        {
            if (command == "P")
            {
                await scheduler.PauseAll();
            }
            else if (command == "R")
            {
                await scheduler.ResumeAll();
            }
        }

        /// <summary>
        /// 获取trigger状态
        /// </summary>
        /// <param name="scheduler"></param>
        public static async Task<List<JobState>> GetTriggerState()
        {
            var jobGroupNames = await scheduler.GetJobGroupNames();
            //var dict = new Dictionary<string, TriggerState> { };
            var result = new List<JobState>();
            foreach (var jobGroupName in jobGroupNames)
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(jobGroupName));
                foreach (var jobKey in jobKeys)
                {
                    string jobName = jobKey.Name;
                    string jobGroup = jobKey.Group;
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    ITrigger trigger = null;
                    foreach (var trig in triggers)
                    {
                        trigger = trig;
                        break;
                    }
                    var triggerState = await scheduler.GetTriggerState(trigger.Key);
                    //dict.Add($"{jobGroup}   {jobName}", triggerState);
                    result.Add(new JobState { job = $"{jobName}", state = triggerState });
                }
            }
            return result;
        }

        /// <summary>
        /// 获取正在执行的job
        /// </summary>
        /// <returns></returns>
        public static async Task<List<string>> GetCurrentlyExecutingJobsAsync()
        {
            var result = new List<string>();
            var currentlyExecutingJobs = await scheduler.GetCurrentlyExecutingJobs();
            currentlyExecutingJobs.ToList().ForEach(e =>
            {
                var jobName = e.Trigger.JobKey.Name;
                result.Add(jobName);
            });
            return result;
        }

        /// <summary>
        /// 翻译trigger状态
        /// </summary>
        /// <param name="triggerState"></param>
        /// <returns></returns>
        public static string TranslateTriggerState(TriggerState triggerState)
        {
            switch (triggerState)
            {
                case TriggerState.Blocked:
                    return "阻塞";

                case TriggerState.Complete:
                    return "执行完成，不会重新触发";

                case TriggerState.Error:
                    return "错误，不会触发";

                case TriggerState.None:
                    return "触发器不存在";

                case TriggerState.Normal:
                    return "正常";

                case TriggerState.Paused:
                    return "暂停";

                default:
                    return "未知";
            }
        }

        /// <summary>
        /// 获取html 正在运行job
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetBuiltyCurrentlyExecutingJobsAsync()
        {
            var result = (await QuartzStartup.GetCurrentlyExecutingJobsAsync()).OrderBy(e => e).ToList();
            var str = new StringBuilder();
            result.ForEach(e =>
            {
                str.Append($"<p>{e}：<span style='background-color:red;'>正在运行</span></p>");
            });
            return str.ToString();
        }

        /// <summary>
        /// 获取html trigger状态
        /// </summary>
        /// <returns></returns>
        public static async System.Threading.Tasks.Task<string> GetBuiltyTriggerStateAsync()
        {
            var result = (await QuartzStartup.GetTriggerState()).OrderBy(e => e.job).ToList();
            var str = new StringBuilder();
            result.ForEach(e =>
            {
                if (e.state == Quartz.TriggerState.Blocked)
                {
                    str.Append($"<p>{e.job} :  <span style='background-color:red;'>{QuartzStartup.TranslateTriggerState(e.state)}</span></p>");
                }
                else if (e.state == Quartz.TriggerState.Paused)
                {
                    str.Append($"<p>{e.job} :  <span  style='background-color:green;'>{QuartzStartup.TranslateTriggerState(e.state)}</span></p>");
                }
                else
                {
                    str.Append($"<p>{e.job} : <span style='background-color:yellow;'>{QuartzStartup.TranslateTriggerState(e.state)}</span></p>");
                }
            });
            return str.ToString();
        }
    }
}