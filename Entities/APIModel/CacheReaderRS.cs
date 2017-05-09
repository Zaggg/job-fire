using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW.PMS.SubDis.Service.Model.Obsoleted
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheReader
    {
        private string pms_supplier_name;
        private string url_address;
        private string Time_Length;
        private string Time_Point;
        private string status;
        private string remark;
        private string Counter;
        private string Id;
        private string pmsCode;
        private string hotelGroupCode;

        [JsonProperty(PropertyName = "Pms_Supplier_Name")]
        public string PMS_SUPPLIER_NAME
        {
            get { return pms_supplier_name; }
            set { pms_supplier_name = value; }
        }

        [JsonProperty(PropertyName = "Url_Address")]
        public string URL_ADDRESS
        {
            get { return url_address; }
            set { url_address = value; }
        }

        [JsonProperty(PropertyName = "Time_Length")]
        public string TIME_LENGTH
        {
            get { return Time_Length; }
            set { Time_Length = value; }
        }

        [JsonProperty(PropertyName = "Time_Point")]
        public string TIME_POINT
        {
            get { return Time_Point; }
            set { Time_Point = value; }
        }
        [JsonProperty(PropertyName = "Status")]
        public string STATUS
        {
            get { return status; }
            set { status = value; }
        }

        [JsonProperty(PropertyName = "Remark")]
        public string REMARK
        {
            get { return remark; }
            set { remark = value; }
        }

        [JsonProperty(PropertyName = "Counter")]
        public string COUNTER
        {
            get { return Counter; }
            set { Counter = value; }
        }

        [JsonProperty(PropertyName = "ID")]
        public string ID
        {
            get { return Id; }
            set { Id = value; }
        }

        [JsonProperty(PropertyName = "PmsCode")]
        public string PmsCode
        {
            get { return this.pmsCode; }
            set { this.pmsCode = value; }
        }

        [JsonProperty(PropertyName = "HotelGroupCode")]
        public string HotelGroupCode
        {
            get { return this.hotelGroupCode; }
            set { this.hotelGroupCode = value; }
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheReaderRS
    {
        private string code;
        private CacheReader[] cacheReader;

        /// <summary>
        /// 0成功 1失败
        /// </summary>
        [JsonProperty(PropertyName = "Code")]
        public string CODE
        {
            get { return code; }
            set { code = value; }
        }

        [JsonProperty(PropertyName = "Infos")]
        public CacheReader[] CACHEREADER
        {
            get { return cacheReader; }
            set { cacheReader = value; }
        }
    }

}
