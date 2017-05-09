using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SubDis.Service.Entities
{
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlRootAttribute("Result", IsNullable = false)]
    public class BasicRS
    {

        private string message;
        private bool flag;

        [XmlElement(ElementName = "mes")]
        public string MESSAGE
        {
            get { return message; }
            set { message = value; }
        }
        [XmlElement(ElementName = "ret")]
        public bool FLAG
        {
            get { return flag; }
            set { flag = value; }
        }

    }
}
