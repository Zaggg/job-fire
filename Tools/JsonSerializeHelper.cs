//using HW.CacheHandler.Model.RouteGroup.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDis.Service.SerializeTools
{
    public class JsonSerializeHelper
    {
        public static JsonSerializeHelper jsonTools = new JsonSerializeHelper();

        private JsonSerializeHelper()
        {

        }

        public static JsonSerializeHelper GetInstance()
        {
            if (jsonTools == null)
            {
                object o = new object();
                lock (o)
                {
                    if (jsonTools == null)
                    {
                        jsonTools = new JsonSerializeHelper();
                    }
                }
            }
            return jsonTools;
      
        }

        public string Convert(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public T ReverseInfo<T>(string request)
        {
            return JsonConvert.DeserializeObject<T>(request);
        }
    }
}
