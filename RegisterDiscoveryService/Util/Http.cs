using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegisterDiscoveryService.Model;
using System;
using System.IO;
using System.Net;

namespace RegisterDiscoveryService
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class Http : Controller
    {
        public enum HttpStatus
        {
            /// <summary>
            /// 操作成功
            /// </summary>
            SUCCESS = 200,
            /// <summary>
            /// 操作失败
            /// </summary>
            ACTION_FAILED = 202,
            /// <summary>
            /// 未登录/登录过期
            /// </summary>
            NO_LOGIN = 401,
            /// <summary>
            /// 无权限
            /// </summary>
            NO_PERMISSIOIN = 403,
            /// <summary>
            /// 找不到资源
            /// </summary>
            NOT_FOUND = 404,
            /// <summary>
            /// 系统错误
            /// </summary>
            SYSTEM_ERROR = 500,

            /// <summary>
            /// 密码错误
            /// </summary>
            ERROR_PASSWORD = 412,
            /// <summary>
            /// 请求格式正确，但是由于含有语义错误，无法响应
            /// </summary>
            INVALIDE_PARAMETER = 422,
            /// <summary>
            /// t请求的资源的内容特性无法满足请求头中的条件，因而无法生成响应实体除非这是一个 HEAD 请求，否则该响应就应当406 Not Acceptable
            /// </summary>
            TOKEN_NOT_FOUND = 406,
        }

        //同步数据推送方法
        public static void Send(Message service, string syncMethod)
        {
            try
            {
                if (!Config.isReader)
                {
                    if (LogHelper.enable)
                        Console.WriteLine("主备份写服务同步转发post！" + Config.SyncURL);
                    Post(Config.SyncURL + "?postmethod=" + syncMethod, service);
                }
                if (LogHelper.enable)
                    Console.WriteLine("读服务不需要发同步post！" + Config.SyncURL);
            }
            catch (IOException e)
            {
                LogHelper.Error(e, Config.logName);
            }

        }

        /// <summary>
        /// 发送post请求
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="data">post:data</param>
        private static void Post(string url, Message service)
        {
            try
            {
                //序列化
                string str = JSON.Serialize(service);
                System.Net.HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "POST";
                req.ContentType = "application/json";

                byte[] reqData = System.Text.Encoding.UTF8.GetBytes(str);//把字符串转换为字节

                req.ContentLength = reqData.Length; //请求长度

                using (Stream reqStream = req.GetRequestStream()) //获取
                {
                    reqStream.Write(reqData, 0, reqData.Length);//向当前流中写入字节
                    reqStream.Close(); //关闭当前流
                }

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();

                using (Stream respStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(respStream))
                    {
                        var result = reader.ReadToEnd();
                    }
                }
            }
            catch (IOException e)
            {
                LogHelper.Error(e, Config.logName);
            }
        }

        #region Ajax处理

        /// <summary>
        /// 返回json
        /// </summary>
        /// <param name="status"></param>
        /// <param name="data"></param>
        /// <param name="pager">分页配置</param>
        /// <param name="isLog">是否写入日志</param>
        /// <returns></returns>
        protected static ActionResult ResponseAjax(RespMsg msg, bool isLog = false)
        {
            var content = JsonConvert.SerializeObject(msg.ToObject());
            return new ContentResult()
            {
                Content = content,
            };
        }



        public static ActionResult ResponseAjax(int status, int innerCode, string msg, string instance="", object data = null, bool isLog = false)
        {
            return ResponseAjax(new RespMsg
            {
                status = status,
                inner_code = innerCode,
                message = msg,
                data = data,
                instance = instance
            }, isLog);
        }
        #endregion


    }
}
