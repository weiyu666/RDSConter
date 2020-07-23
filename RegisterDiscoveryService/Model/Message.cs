using System;

namespace RegisterDiscoveryService.Model
{
    #region 传递一条数据与表路由表的数据结构
    public class Message
    {
        public string status { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string ip_address { get; set; }
        public string description { get; set; }
        public Int64 utc_time { get; set; }
        /// <summary>
        /// 服务器的状态
        /// </summary>
        public enum service_status
        {
            alive,
            offLine,
            stop
        }
    }
    #endregion
}
