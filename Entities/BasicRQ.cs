using SubDis.Service.Infos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SubDis.Service.Entities
{
    [Serializable]
    [XmlRootAttribute("Verify", IsNullable = false)]
    public class BasicRQ
    {
        private string TimeStamp;
        private string md5;

        [XmlElement(ElementName = "TimeStamp")]
        public string TIMESTAMP
        {
            get { return TimeStamp; }
            set { TimeStamp = value; }
        }

        [XmlElement(ElementName = "EncryptedStr")]
        public string MD5
        {
            get { return md5; }
            set { md5 = value; }
        }
    
        public BasicRQ()
        {
            var infos = PrimaryInfo.Create();
            TIMESTAMP = infos.TIMESTAMP;
            MD5 = infos.PASSWORD;
        }

    }
}
