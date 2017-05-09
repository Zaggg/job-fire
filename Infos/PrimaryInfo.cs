using SubDis.Service.Proxies;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SubDis.Service.Infos
{
    public class PrimaryInfo
    {
        //private static PrimaryInfo root = new PrimaryInfo();

        //public static PrimaryInfo GetInstance()
        //{
        //    object o = new object();
        //    if (root == null)
        //    {
        //        lock (o)
        //        {
        //            if (root == null)
        //            {
        //                root = new PrimaryInfo();
        //            }
        //        }
        //    }
        //    return root;

        //}
        
        private PrimaryInfo()
        {
            //USERNAME = ConfigurationManager.AppSettings["sysId"];
            TIMESTAMP = GetTimeStamp();
            PASSWORD = GetMd5Hash(GetKey(TIMESTAMP));
            
        }

        private PrimaryInfo(string value)
        {
            TIMESTAMP = GetTimeStamp();
            PASSWORD = GetMd5Hash(GetKey() + TIMESTAMP);
        }

        /// <summary>
        /// 生成实例
        /// </summary>
        /// <returns></returns>
        public static PrimaryInfo Create()
        {
            return new PrimaryInfo();
        }

        public static PrimaryInfo Create(string value)
        {
            return new PrimaryInfo(value);
        }

        public static Verify CreateRequest()
        {
            var infos = PrimaryInfo.Create();
            Verify request = new Verify();
            request.TimeStamp = infos.TIMESTAMP;
            request.EncryptedStr = infos.PASSWORD;
            return request;
        }

        public static TriggerDisRequest CreateJsonRequest(string pmsCode,string hotelGroupCode)
        {
            var infos = PrimaryInfo.Create("json");
            TriggerDisRequest request = new TriggerDisRequest();
            request.TimeStamp = infos.TIMESTAMP;
            request.Password = infos.PASSWORD;
            request.PMSCode = pmsCode;
            request.HotelGroupCode = hotelGroupCode;
            request.Invoker = "SubDis.Service";
            return request;
        }

        public string GetKey(string timeStamp)
        {
            return timeStamp + ConfigurationManager.AppSettings["psd"];
        }

        public string GetKey()
        {
            return ConfigurationManager.AppSettings["keyword"];
        }

        public string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString(); 
        }

        public string GetMd5Hash(string input)
        {
            var md5Hash = MD5.Create();

            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            StringBuilder sBuilder = new StringBuilder();
            
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            
            return sBuilder.ToString();
        }

        //private MD5 md5Hash;
       
        private string TimeStamp;
        private string UserName;
        private string Password;
        /// <summary>
        /// 全局变量 线程最大数
        /// </summary>
        public static string ThreadCount = ConfigurationManager.AppSettings["threadcount"]; 
        /// <summary>
        /// 全局变量 请求超时时间
        /// </summary>
        public static string RequestTimeOut = ConfigurationManager.AppSettings["requestTimeout"];

        /// <summary>
        /// 全局变量 服务执行频率
        /// </summary>
        public static string ServiceFrequency = ConfigurationManager.AppSettings["serviceFrequency"];

        /// <summary>
        /// 自动更新缓存时间段
        /// </summary>
        public static string AutoUpdateCacheTimeRange = ConfigurationManager.AppSettings["AutoUpdateCacheTimeSeconds"];

        /// <summary>
        /// 延时更新缓存时间段|仅用于获取缓存失败时
        /// </summary>
        public static string DelayUpdateCacheTimeDuration = ConfigurationManager.AppSettings["DelayUpdateCacheTimeDuration"];

        public string TIMESTAMP
        {
            get { return TimeStamp; }
            set { TimeStamp = value; }
        }
        
        public string USERNAME
        {
            get { return UserName; }
            set { UserName = value; }
        }

        public string PASSWORD
        {
            get { return Password; }
            set { Password = value; }
        }

        //public string THREADCOUNT
        //{
        //    get { return ThreadCount; }
        //    set { ThreadCount = value; }
        //}

        //public string REQUESTTIMEOUT
        //{
        //    get { return RequestTimeOut; }
        //    set { RequestTimeOut = value; }
        //}
    }
}
