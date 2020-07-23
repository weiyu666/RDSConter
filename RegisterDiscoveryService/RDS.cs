using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using RegisterDiscoveryService.Model;
using RegisterDiscoveryService.DAO;

namespace RegisterDiscoveryService
{
    public class RDS
    {
       
        private static SRCDataAccess sqlOpertion = new SRCDataAccess();
        //service_name作为字典的key建立索引提高提高性能
        public static Dictionary<string, List<Message>> data = new Dictionary<string, List<Message>>();
        //写数据库的操作锁
        private static object lockWriteDB = new object();
        public static HeartTask heartService = new HeartTask();
        public static DBTask dBService = new DBTask();

        public static void Register(Message service)
        {
            //如果现阶段在data有数据的前提下新插入一条新数据为false,原来有就更新为true
            bool dataStatus = false;
            int index = 0;

            if (!data.ContainsKey(service.name))
            {
                data[service.name] = new List<Message>();
            }
            List<Message> services = data[service.name];
            //web是多线程的加锁
            lock (services)
            {
                service.status = Message.service_status.alive.ToString();

                //存入内存
                for (int item = 0; item < services.Count; item++)
                {

                    //如果现阶段内存中有该条数据就,删除原来的再插入新的更新!
                    if (service.id == services[item].id && service.name == services[item].name)
                    {
                        dataStatus = true;
                        index = item;
                        break;
                    }
                }

                if (dataStatus)
                {
                    //更新
                    service.utc_time = LinuxTime.Seconds(DateTime.Now);
                    services[index] = service;
                }
                else
                {
                    //如果现阶段内存中没有该条数据就添加插入
                    service.utc_time = LinuxTime.Seconds(DateTime.Now);
                    services.Add(service);
                }
                if (LogHelper.enable)
                    LogHelper.WriteLog(services.ToString());
            }
        }

        static internal List<Message> GetService(string serviceName)
        {
            //通过服务器的name来查找内存中的数据表当前符合条件数据
            List<Message> result = new List<Message>();
            if (data == null || serviceName == "")
            {
                return null;
            }

            if (data.ContainsKey(serviceName))
            {
                result = data[serviceName.ToLower()];
                return result;
            }
            else
            {
                return null;
            }
 
        }

        internal static void closeConn(Message service)
        {
            if (!data.ContainsKey(service.name))
            {
                data[service.name] = new List<Message>();
            }
            List<Message> services = data[service.name];
            lock (services)
            {
                if (services != null)
                {
                    foreach (var item in services)
                    {
                        if (item.id == service.id && item.name == service.name)
                        {
                            //服务状态重置为false
                            item.status = Util.To<string>(Message.service_status.offLine);
                            item.utc_time = LinuxTime.Seconds(DateTime.Now);
                        }
                    }
                }
            }
        }

        internal static void heartBeatPackage(Message service)
        {
            if (!data.ContainsKey(service.name))
            {
                data[service.name] = new List<Message>();
            }
            List<Message> services = data[service.name];
            lock (services)
            {
                if (services != null)
                {
                    List<Message> result = new List<Message>();
                    foreach (var item in services)
                    {
                        if (item.id == service.id && item.name == service.name)
                        {
                            item.utc_time = LinuxTime.Seconds(DateTime.Now);
                            item.status = Util.To<string>(Message.service_status.alive);
                        }
                    }
                }
            }
        }

        internal static void recvSync(Message service,string postMethod)
        {
            switch (postMethod)
            {
                case "Register":
                    Register(service);
                    break;
                case "heartBeatPackage":
                    heartBeatPackage(service);
                    break;
                case "closeConn":
                    closeConn(service);
                    break;
            }

        }

        public static void TimerSaveMysql()
        {
            //只有写服务操作,读数据,只能访问写接口
            lock (lockWriteDB)
            {
                sqlOpertion.SRCQuery(data);
            }
        }
        public static void TimerGetService()
        {
            //只有读服务操作,只能访问读接口
            data = sqlOpertion.SRCSelect();
        }
    }
}
