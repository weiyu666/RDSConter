using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RegisterDiscoveryService.Model;
using System;
using System.IO;
using System.Threading.Tasks;


namespace RegisterDiscoveryService.Controllers
{

    [ApiController]
    [Route("version10/[action]")]
    public class SRCController : Http, IRouter
    {

        public SRCController()
        {

        }

        [ActionName("register")]
        [HttpPost]
        public async Task<IActionResult> Register()
        {
            string json = null;
            try
            {
                Request.EnableBuffering();
                using (var stream = Request.Body)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        json = await reader.ReadToEndAsync();
                    }
                }

                //1、数据验证
                if (string.IsNullOrEmpty(json))
                {
                    return Http.ResponseAjax(400, 400, "The data is incomplete,Fail plaese again", Config.Instance, null);
                }

                //2、接收请求数据并转化成为自己所需要的数据格式struct              
                var service = JSON.Deserialize<Message>(json);
                if (service.id == "" || service.name == "")
                {
                    return Http.ResponseAjax(400, 400, "id or name not is not allowed!", Config.Instance);
                }
                else
                {
                    RDS.Register(service);
                    Http.Send(service, "Register");
                    //这里data:不需要返回
                    return Http.ResponseAjax(200, 200, "register service successful", Config.Instance, null);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Http.ResponseAjax(400, 400, e.Message.ToString(), Config.Instance, null);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="service_name">通过service_name去读去查询服务器列表</param>
        /// <returns></returns>
        [ActionName("getService")]
        public IActionResult getService(string service_name )
        {
            //get 获取name参数方法二
            // var name = Request.Query["name"].ToString();
            try
            {
                return ResponseAjax(200, 200, "getService successful", Config.Instance, RDS.GetService(service_name));
            }
            catch(Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return ResponseAjax(500, 500, e.Message.ToString()+"Server exception! Please contact the administrator", Config.Instance);
            }
        }


        [ActionName("syncSRCWrite")]
        [HttpPost]
        public async Task<IActionResult> syncMachineService(string postmethod)
        {
            string json = null;
            try
            {
                Request.EnableBuffering();
                using (var stream = Request.Body)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        json = await reader.ReadToEndAsync();
                    }
                }
                var service = JSON.Deserialize<Message>(json);
                if (service.id == "" || service.name == "")
                {
                    return Http.ResponseAjax(400, 400, "id or name not is not allowed!", Config.Instance);
                }
                else
                {
                    RDS.recvSync(service , postmethod);
                    return Http.ResponseAjax(200, 200, "Sync Write Service successful!", Config.Instance);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Http.ResponseAjax(500, 500, e.Message.ToString(), Config.Instance);
            }
        }



        [ActionName("closeConnectionService")]
        [HttpPost]
        public async Task<IActionResult> closeConnectionService()
        {
            string json = null;
            try
            {
                Request.EnableBuffering();
                using (var stream = Request.Body)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        json = await reader.ReadToEndAsync();
                    }
                }
                var service = JSON.Deserialize<Message>(json);
                if (service.id == "" || service.name == "")
                {
                    return Http.ResponseAjax(400, 400, "id or name not is not allowed!", Config.Instance);
                }
                else
                {
                    RDS.closeConn(service);
                    Http.Send(service, "closeConn");
                    return Http.ResponseAjax(200, 200, "closeConn Service successful", Config.Instance);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Http.ResponseAjax(500, 500, e.Message.ToString(), Config.Instance);
            }
        }

        [ActionName("heartService")]
        [HttpPost]
        public async Task<IActionResult> heartService()
        {
            string json = null;
            try
            {
                Request.EnableBuffering();
                using (var stream = Request.Body)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        json = await reader.ReadToEndAsync();
                    }
                }
                var service = JSON.Deserialize<Message>(json);
                if (service.id == "" || service.name == "")
                {
                    return Http.ResponseAjax(400, 400, "id or name not is not allowed!", Config.Instance);
                }
                else
                {
                    RDS.heartBeatPackage(service);
                    Http.Send(service, "heartBeatPackage");
                    return Http.ResponseAjax(200, 200, "The service is alive", Config.Instance);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Http.ResponseAjax(500, 500, e.Message.ToString(), Config.Instance);
            }
        }
    }
}
