using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDis.Service.Entities
{
    public class AddressSettings
    {
        private string pmsName;
        private string urlAddress;
        private string timeLength;
        private string timePoint;
        private string status;
        private string remarks;
        private string counter;

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

        public string TIMELENGTH
        {
            get { return timeLength; }
            set { timeLength = value; }
        }

        public string TIMEPOINT
        {
            get { return timePoint; }
            set { timePoint = value; }
        }

        public string STATUS
        {
            get { return status; }
            set { status = value; }
        }

        public string REMARKS
        {
            get { return remarks; }
            set { remarks = value; }
        }

        public string COUNTER
        {
            get { return counter; }
            set { counter = value; }
        }
    }
}
