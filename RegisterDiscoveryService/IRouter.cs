using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace RegisterDiscoveryService
{

    interface IRouter
    {
        /// <summary>
        /// 读服务:只使用该接口其他不需要!,提供服务列表.
        /// </summary>
        /// <returns>返回服务集群列表</returns>
        /// <param name="service_name">通过service_name去读去查询服务器列表</param
        public IActionResult getService(string service_name);


        /// <summary>
        /// 写服务:只写服务不使用getService()接口,改接口是注册服务器,批量的去Mysql里去写每隔一秒钟写一次,客户端向服务器发送注册的post请求的json格式，收到后就注册
        /// </summary>
        public Task<IActionResult> Register();

        /// <summary>
        ///  写服务:断开服务器,客户端向服务器发送要断开连接的post请求,解析boby,收到该消息180s后断开
        /// </summary>
        public Task<IActionResult> closeConnectionService();


        /// <summary>
        /// 发送接口，用于主服务器与备份服务器之间的同步
        /// </summary>
        public Task<IActionResult> heartService();

        /// <summary>
        /// 接收接口，用于主服务器与备份服务器之间的同步
        /// </summary>
        public Task<IActionResult> syncMachineService(string post_method);

    }

}