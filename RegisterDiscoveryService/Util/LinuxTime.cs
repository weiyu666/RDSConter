using System;

namespace RegisterDiscoveryService
{
    public class LinuxTime
    {
        public static long Seconds(DateTime time)
        {
            return (time.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
 
    }
}
