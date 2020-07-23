using Newtonsoft.Json;
namespace RegisterDiscoveryService
{
    public sealed class JSON
    {
        public static T Deserialize<T>(string json) where T : class
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string Serialize(object obj)
        {
            if (obj == null) return "";
            return JsonConvert.SerializeObject(obj);
        }
    }
}