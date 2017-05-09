using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW.PMS.SubDis.Service.Model.Obsoleted
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheCommonRQ
    {
        private string Remark;
        private string Caller;

        [JsonProperty(PropertyName = "Remark")]
        public string REMARK
        {
            get { return Remark; }
            set { Remark = value; }
        }

        [JsonProperty(PropertyName = "Caller")]
        public string CALLER
        {
            get { return Caller; }
            set { Caller = value; }
        }
    }
}
