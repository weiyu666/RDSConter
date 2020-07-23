using Quartz;
using Quartz.Impl;
using RegisterDiscoveryService.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static RegisterDiscoveryService.RDS;

namespace RegisterDiscoveryService
{
    public class HeartTask : IJob
    {
        IScheduler scheduler;

        public int Interval { get; set; }
        int Tasks = 0;
        public HeartTask()
        {

        }

        public void Init()
        {
            scheduler = new StdSchedulerFactory().GetScheduler().Result;

            var job = JobBuilder.Create<HeartTask>()
              .WithIdentity("HeartTask")
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

                    foreach (List<Message> item in RDS.data.Values)
                    {
                        if (item == null || item.Count == 0) return;
                        foreach (var msg in item)
                        {
                            if (Tasks >= Config.MaxTasks)
                            {
                                Thread.Sleep(100);
                            }

                            Tasks++;
                            var now = LinuxTime.Seconds(DateTime.Now);
                            if ((now - msg.utc_time) > 60)
                            {
                                msg.status = Message.service_status.offLine.ToString();
                                if (LogHelper.enable)
                                    Console.WriteLine("Loop-----------------------" + DateTime.Now);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message.ToString());
                }
                finally { Tasks = 0; }
            });
        }

        public void Start() { scheduler.Start(); }
        public void Stop() { scheduler.Shutdown(true); }
        public void Pause() { scheduler.PauseAll(); }
        public void Resume() { scheduler.ResumeAll(); }
    }
}
