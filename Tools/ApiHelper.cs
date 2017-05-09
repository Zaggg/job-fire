using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubDis.Service.Proxies;
//using SubDis.Service.Model;
using Newtonsoft.Json;
using SubDis.CacheAPI.Common.Model;

namespace SubDis.Service.Tools
{
    public class ApiHelper
    {
        private static ApiHelper root = new ApiHelper();

        private ApiHelper()
        {

        }
        public static ApiHelper GetInstance()
        {
            if (root == null)
            {
                object o = new object();
                lock (o)
                {
                    if (root == null)
                    {
                        root = new ApiHelper();
                    }
                }
            }
            return root;
        }

        public CacheReaderRS GetTasks()
        {
            try
            {
                SubDisCache _api = new SubDisCache();

                CacheReaderRQ request = GetDefaultRequest("Get task list cache");

                var requestString = JsonConvert.SerializeObject(request);

                var response = _api.CacheReadEntrance(requestString);

                return JsonConvert.DeserializeObject<CacheReaderRS>(response);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        public CacheReaderRQ GetDefaultRequest(string info)
        {
            CacheReaderRQ request = new CacheReaderRQ();
            request.CALLER = "SubDis.Service";
            request.REMARK = info;
            return request;
        }

        /// <summary>
        /// 更新缓存
        /// </summary>
        /// <returns></returns>
        public CacheUpdateRS UpdateTask()
        {
            try
            {
                SubDisCache _api = new SubDisCache();

                CacheReaderRQ request = GetDefaultRequest("Update task list cache");

                var requestString = JsonConvert.SerializeObject(request);

                var response = _api.CacheUpdateEntrance(requestString);

                return JsonConvert.DeserializeObject<CacheUpdateRS>(response);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
