using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using RegisterDiscoveryService.Model;

namespace RegisterDiscoveryService.DAO
{
    public class SRCDataAccess
    {
        private MySQLHelper mySqlHelp = new MySQLHelper();
        object locker = new object();

        //数据库的业务批量的去插入更新
        public void SRCQuery(Dictionary<string, List<Message>> data)//List<Message>
        {
            if (data == null || data.Count == 0) return;
            using (MySqlConnection conn = new MySqlConnection(Config.MySqlConnection))
            {
                conn.Open();//必须打开通道之后才能开始事务
                MySqlTransaction transaction = conn.BeginTransaction();//事务必须在try外面赋值不然catch里的transaction会报错:未赋值
                try
                {
                    foreach (List<Message> lines in data.Values)
                    {
                        if (lines == null || lines.Count == 0) return;
                        foreach (var line in lines)
                        {
                            var strSql = string.Format(@"REPLACE INTO mytable(ip_address,name,id,status,description,utc_time) 
                        VALUES('{0}','{1}','{2}','{3}','{4}',now());", line.ip_address, line.name, line.id, line.status, line.description);
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
        public Dictionary<string, List<Message>> SRCSelect()//getlist  List<Message>
        {
            var result = new Dictionary<string, List<Message>>();// List<Message>();
            var resList = new  List<Message>();// List<Message>();
            try
            {
                lock (this)
                {
                    var data = mySqlHelp.Query("SELECT * FROM mytable;");
                    if (data == null || data.Tables[0].Rows.Count == 0) return null;

                    foreach (DataRow item in data.Tables[0].Rows)
                    {
                        var message = new Message
                        {
                            ip_address = Util.To<string>(item["ip_address"]),
                            name = Util.To<string>(item["name"]),
                            id = Util.To<string>(item["id"]),
                            status = Util.To<string>(item["status"]),
                            description = Util.To<string>(item["description"]),
                            utc_time = Util.To<long>(item["utc_time"]),
                        };
                        //判断一下有没有重复key没有有直接new,否则就加value
                        if (!result.ContainsKey(message.name))
                        {
                            result[message.name] = new List<Message>();
                        }
                        //使用索引的方法加不然有同个key会报错
                        result[message.name].Add(message);
                    }
                }
            }
            catch (Exception e) { LogHelper.Error(e, Config.logName); }
            finally { Thread.Sleep(5); }
            return result;
        }
    }
}
