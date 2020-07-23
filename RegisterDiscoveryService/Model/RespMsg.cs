using Newtonsoft.Json;

namespace RegisterDiscoveryService.Model
{
    public class RespMsg
    {
        public int status;
        public int inner_code;
        public string message;
        public object data;
        public string instance;

        public Http.HttpStatus HttpStatus
        {
            set
            {
                status = (int)value;
                message = value.ToString();
            }
        }
        public object ToObject()
        {
            return new
            {
                status = status,
                message = message,
                instance = instance,
                data = data
            };
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(ToObject());
        }
    }

   
}
