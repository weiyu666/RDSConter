using Common;
using Common.Time;
using Quartz;
using Quartz.Impl;
using RegistrationFoundService.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using static RegistrationFoundService.SRC;

namespace RegistrationFoundService
{
    public class HeartService : IJob
    {
        IScheduler scheduler;

        public int Interval { get; set; }
        int Tasks =0 ;
        public HeartService()  
        {

        }

        public void Init() 
        {
            scheduler = new StdSchedulerFactory().GetScheduler().Result;

            var job = JobBuilder.Create<HeartService>()
              .WithIdentity("HeartService")
              .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity("Trigger", "Scheduler")
               .StartNow()
               .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMilliseconds(Interval))
               .RepeatForever()
               .RepeatForever()).ForJob(job)
               .Build();

            scheduler.ScheduleJob(job, trigger);
        }
       
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() => 
            {
                try
                {
                    foreach (var item in SRC.data)
                    {
                        if (Tasks >= Config.MaxTasks)
                        {
                            Thread.Sleep(100);
                        }

                        Tasks++;
                        var now = UnixTime.Seconds(DateTime.Now);
                        if ((now - item.service_utc_time) > 60)
                        {
                            item.service_status = service_status_enum.offLine.ToString();
                            if (Config.isLog)
                                Console.WriteLine("Loop-----------------------" + DateTime.Now);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
                finally { Tasks=0; }
            });
        }
       
        public void Start() { scheduler.Start(); }
        public void Stop() { scheduler.Shutdown(true); }
        public void Pause() { scheduler.PauseAll(); }
        public void Resume() { scheduler.ResumeAll(); }
    }
}
