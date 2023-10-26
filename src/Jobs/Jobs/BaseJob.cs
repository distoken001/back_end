using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;
using System.Threading.Tasks;

namespace Jobs.Jobs
{
    public abstract class BaseJob : IDisposable, IJob
    {
        public IJobExecutionContext jobExecutionContext;
        public IServiceScope scope;

        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"{context.Trigger.JobKey.Name} Begin At Time : {DateTime.Now.ToString()}");
            jobExecutionContext = context;
            scope = context.JobDetail.JobDataMap["service_scope"] as IServiceScope;
            return ExecuteMethod(context);
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetService<T>()
        {
            return this.scope.ServiceProvider.GetService<T>();
        }

        public abstract Task ExecuteMethod(IJobExecutionContext context);

        public void Dispose()
        {
            if (jobExecutionContext != null)
            {
                var detail = jobExecutionContext.JobDetail;
                var map = detail.JobDataMap;
                var service_scope = map["service_scope"];
                if (service_scope != null)
                {
                    var scope_entity = service_scope as IServiceScope;
                    if (scope_entity != null)
                    {
                        scope_entity.Dispose();
                    }
                }
            }
        }
    }
}
