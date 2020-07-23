using Microsoft.Extensions.Configuration;
using System.IO;

namespace RegisterDiscoveryService
{
    public class Config
    {
        public static dynamic MySqlConnection => GetCustomSettings<string>("MySqlConnection");
        public static dynamic SyncURL => GetCustomSettings<string>("SyncURL");
        public static dynamic isReader => GetCustomSettings<bool>("isReader");
        public static dynamic Interval => GetCustomSettings<int>("Interval");
        public static dynamic HeaartInterval => GetCustomSettings<int>("HeaartInterval");
        public static dynamic MaxTasks => GetCustomSettings<int>("MaxTasks");

        public static dynamic Instance => GetCustomSettings<string>("Instance");
        public static dynamic isLog => GetCustomSettings<bool>("isLog");
        public static dynamic logPath => GetCustomSettings<string>("logPath");
        public static dynamic logName => GetCustomSettings<string>("logName");

        private static T GetCustomSettings<T>(string key)
        {
            var config = new ConfigurationBuilder()
                         .AddInMemoryCollection()
                         .SetBasePath(Directory.GetCurrentDirectory())
                         .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                         .Build();

            return Util.To<T>(config.GetSection("AppSettings").GetValue(key, string.Empty));
        }
    }
}
