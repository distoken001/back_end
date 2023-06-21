using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
using Quartz;
using System;

namespace deMarketService
{
    public class IOCJobFactory : IJobFactory
    {
        private readonly ServiceProvider _serviceProvider;
        public IOCJobFactory(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {

            IJobDetail jobDetail = bundle.JobDetail;
            var data = bundle.JobDetail.JobDataMap;
            var scope = _serviceProvider.CreateScope();
            data["service_scope"] = scope;
            Type jobType = jobDetail.JobType;
            try
            {
                //if (log.IsDebugEnabled())
                //{
                //	log.Debug($"Producing instance of Job '{jobDetail.Key}', class={jobType.FullName}");
                //}
                var job = scope.ServiceProvider.GetService(bundle.JobDetail.JobType) as IJob;
                //var job = ObjectUtils.InstantiateType<IJob>(jobType);
                return job;
            }
            catch (Exception e)
            {
                SchedulerException se = new SchedulerException($"Problem instantiating class '{jobDetail.JobType.FullName}'", e);
                throw se;
            }

        }

        public void ReturnJob(IJob job)
        {

            var disposable = job as IDisposable;
            disposable?.Dispose();

        }
    }
}
