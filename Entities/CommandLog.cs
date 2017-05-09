using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDis.Service.Entities
{
    public class CommandLog
    {
        private string pmsName;
        private string urlAddress;
        private DateTime createTime;
        private int status;
        private string remarks;
        
        public string PMSNAME
        {
            get { return pmsName; }
            set { pmsName = value; }
        }

        public string URLADDRESS
        {
            get { return urlAddress; }
            set { urlAddress = value; }
        }

        public DateTime CREATETIME
        {
            get { return createTime; }
            set { createTime = value; }
        }

        public int STATUS
        {
            get { return status; }
            set { status = value; }
        }

        public string REMARKS
        {
            get { return remarks; }
            set { remarks = value; }
        }
    }
}
