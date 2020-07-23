using Quartz;
using Quartz.Impl;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace RegisterDiscoveryService
{
    public class DBTask : IJob
    {
        IScheduler scheduler;
        public int Interval { get; set; }
         int Tasks =0 ;

        public DBTask()
        {

        }

        public void Init() 
        {
            scheduler = new StdSchedulerFactory().GetScheduler().Result;
            var job = JobBuilder.Create<DBTask>()
              .WithIdentity("DBTask")
              .Build();

            var trigger = TriggerBuilder.Create()
               .WithIdentity("Trigger", "SchedulerCenter")
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
                    if (Tasks >= Config.MaxTasks)
                    {
                        Thread.Sleep(100);
                    }

                    if (Config.isReader)
                        RDS.TimerGetService();
                    else
                        RDS.TimerSaveMysql();

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
