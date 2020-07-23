using Common;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RegistrationFoundService.Base;
using RegistrationFoundService.Dal;
using RegistrationFoundService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Timers;

namespace RegistrationFoundService.Controllers
{

    [ApiController]
    [Route("v1/[action]")]
    public class SRCController : AjaxBase, IRouterServiceTable
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
                    return Base.AjaxBase.ResponseAjax(400, 400, "The data is incomplete,Fail plaese again", Config.Instance, null);
                }
                
                //2、接收请求数据并转化成为自己所需要的数据格式struct
                var service = JSON.Deserialize<SRCMessage>(json);
                if (service.service_id == "" || service.service_name == "")
                {
                    return Base.AjaxBase.ResponseAjax(400, 400, "service_id or service_name not is not allowed!", Config.Instance);
                }
                else
                {
                    SRC.Register(service);
                    SRC.Send(service, "Register");
                    //这里data:不需要返回
                    return Base.AjaxBase.ResponseAjax(200, 200, "register service successful", Config.Instance, null);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Base.AjaxBase.ResponseAjax(400, 400, e.Message.ToString(), Config.Instance, null);
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
            // var name = Request.Query["service_name"].ToString();
            try
            {
                return ResponseAjax(200, 200, "getService successful", Config.Instance, SRC.GetService(service_name));
            }
            catch(Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return ResponseAjax(500, 500, "Server exception! Please contact the administrator", Config.Instance);
            }
        }


        [ActionName("syncSRCWrite")]
        [HttpPost]
        public async Task<IActionResult> recvSyncWriteService(string postmethod)
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
                var service = JSON.Deserialize<SRCMessage>(json);
                if (service.service_id == "" || service.service_name == "")
                {
                    return Base.AjaxBase.ResponseAjax(400, 400, "service_id or service_name not is not allowed!", Config.Instance);
                }
                else
                {
                    SRC.recvSync(service , postmethod);
                    return Base.AjaxBase.ResponseAjax(200, 200, "Sync Write Service successful!", Config.Instance);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Base.AjaxBase.ResponseAjax(500, 500, e.Message.ToString(), Config.Instance);
            }
        }



        [ActionName("quit")]
        [HttpPost]
        public async Task<IActionResult> quitService()
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
                var service = JSON.Deserialize<SRCMessage>(json);
                if (service.service_id == "" || service.service_name == "")
                {
                    return Base.AjaxBase.ResponseAjax(400, 400, "service_id or service_name not is not allowed!", Config.Instance);
                }
                else
                {
                    SRC.Quit(service);
                    SRC.Send(service, "Quit");
                    return Base.AjaxBase.ResponseAjax(200, 200, "Quit Service successful", Config.Instance);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Base.AjaxBase.ResponseAjax(500, 500, e.Message.ToString(), Config.Instance);
            }
        }

        [ActionName("keep-alive")]
        [HttpPost]
        public async Task<IActionResult> keepAliveService()
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
                var service = JSON.Deserialize<SRCMessage>(json);
                if (service.service_id == "" || service.service_name == "")
                {
                    return Base.AjaxBase.ResponseAjax(400, 400, "service_id or service_name not is not allowed!", Config.Instance);
                }
                else
                {
                    SRC.KeepAlive(service);
                    SRC.Send(service, "KeepAlive");
                    return Base.AjaxBase.ResponseAjax(200, 200, "The service is alive", Config.Instance);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e, Config.logName);
                return Base.AjaxBase.ResponseAjax(500, 500, e.Message.ToString(), Config.Instance);
            }
        }
    }
}
