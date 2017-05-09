using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HW.PMS.SubDis.Service.Model.Obsoleted
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CacheUpdateRS
    {
        private string code;
        private string messages;

        [JsonProperty(PropertyName = "Code")]
        public string CODE
        {
            get { return code; }
            set { code = value; }
        }

        [JsonProperty(PropertyName = "Messages")]
        public string MESSAGES
        {
            get { return messages; }
            set { messages = value; }
        }
    }
}
