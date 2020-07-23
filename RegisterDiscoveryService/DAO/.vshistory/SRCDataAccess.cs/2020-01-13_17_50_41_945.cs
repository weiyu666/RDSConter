using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

using static RegistrationFoundService.Base.AjaxBase;
using System.Threading;
using RegistrationFoundService.Model;
using static RegistrationFoundService.SRC;
using Common;
using System.Linq;

namespace RegistrationFoundService.Dal
{
    public class SRCDataAccess
    {
        private MySQLHelper mySqlHelp = new MySQLHelper();
        object locker = new object();

        //数据库的业务批量的去插入更新
        public void SRCQuery(Dictionary<string, List<SRCMessage>> data)//List<SRCMessage>
        {
            if (data == null || data.Count == 0) return;
            using (MySqlConnection conn = new MySqlConnection(Config.XSRCMySqlConnection))
            {
                conn.Open();//必须打开通道之后才能开始事务
                MySqlTransaction transaction = conn.BeginTransaction();//事务必须在try外面赋值不然catch里的transaction会报错:未赋值
                try
                {
                    foreach (List<SRCMessage> lines in data.Values)
                    {
                        if (lines == null || lines.Count == 0) return;
                        foreach (var line in lines)
                        {
                            var strSql = string.Format(@"REPLACE INTO xrtk_src(ip_address,service_name,service_id,service_status,description,service_utc_time) 
                        VALUES('{0}','{1}','{2}','{3}','{4}',now());", line.ip_address, line.service_name, line.service_id, line.service_status, line.description);
                            var cmd2 = new MySqlCommand(strSql, conn);
                            cmd2.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();//事务要么回滚要么提交，即Rollback()与Commit()只能执行一个
                }
                catch (MySqlException ex)
                {
                    LogHelper.Error(ex, Config.logName);
                    transaction.Rollback();//事务ExecuteNonQuery()执行失败报错
                }
                finally { conn.Close(); }
            }
        }

        //通过service_name去读去查询表
        public Dictionary<string, List<SRCMessage>> SRCSelect()//getlist  List<SRCMessage>
        {
            var result = new Dictionary<string, List<SRCMessage>>();// List<SRCMessage>();
            try
            {
                lock (this)
                {
                    var data = mySqlHelp.Query("SELECT * FROM xrtk_src;");
                    if (data == null || data.Tables[0].Rows.Count == 0) return null;

                    foreach (DataRow item in data.Tables[0].Rows)
                    {
                        result.Add(new SRCMessage
                        {
                            ip_address = Util.To<string>(item["ip_address"]),
                            service_name = Util.To<string>(item["service_name"]),
                            service_id = Util.To<string>(item["service_id"]),
                            service_status = Util.To<string>(item["service_status"]),
                            description = Util.To<string>(item["description"]),
                            service_utc_time = Util.To<long>(item["service_utc_time"]),
                        });
                    }
                }
            }
            catch (Exception e) { LogHelper.Error(e, Config.logName); }
            finally { Thread.Sleep(5); }
            return result;
        }
    }
}
