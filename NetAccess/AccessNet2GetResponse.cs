using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace SubDis.Service.NetAccess
{
    public class AccessNet2GetResponse
    {
        private static AccessNet2GetResponse root = new AccessNet2GetResponse();

        private AccessNet2GetResponse()
        {
            
        }
        public static AccessNet2GetResponse GetInstance()
        {
            if (root == null)
            {
                object o = new object();
                lock (o)
                {
                    if (root == null)
                    {
                        root = new AccessNet2GetResponse();
                    }
                }
            }
            return root;
        }


        private string SoapWebService(byte[] rqBytes, HttpWebRequest webRequest)
        {
            //byte[] rqBytes = Encoding.UTF8.GetBytes(jsonRQ);

            //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.Method = "POST";
            //webRequest.ContentType = "application/json";
            //webRequest.ContentLength = rqBytes.Length;

            string RSString = null;
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(rqBytes, 0, rqBytes.Length);
            }

            HttpWebResponse webRespond;
            try
            {
                webRespond = webRequest.GetResponse() as HttpWebResponse;

                if (webRespond.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (Stream rStream = webRespond.GetResponseStream())
                    {
                        using (StreamReader srr = new StreamReader(rStream, Encoding.UTF8))
                        {
                            RSString = srr.ReadToEnd();
                        }
                    }
                }
                webRespond.Close();
                return RSString;
            }
            catch (WebException ex)
            {
                webRespond = (HttpWebResponse)ex.Response;
                using (Stream rStream = webRespond.GetResponseStream())
                {
                    using (StreamReader srr = new StreamReader(rStream, Encoding.UTF8))
                    {
                        RSString = srr.ReadToEnd();
                        webRespond.Close();
                        throw new Exception(RSString);
                    }
                }
                
            }
        }

        private string WebService(byte[] rqBytes, HttpWebRequest webRequest)
        {
            //byte[] rqBytes = Encoding.UTF8.GetBytes(jsonRQ);

            //HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            //webRequest.Method = "POST";
            //webRequest.ContentType = "application/json";
            //webRequest.ContentLength = rqBytes.Length;

            string RSString = null;
            using (Stream requestStream = webRequest.GetRequestStream())
            {
                requestStream.Write(rqBytes, 0, rqBytes.Length);
            }

            HttpWebResponse webRespond;
            try
            {
                webRespond = webRequest.GetResponse() as HttpWebResponse;

                if (webRespond.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    using (Stream rStream = webRespond.GetResponseStream())
                    {
                        using (StreamReader srr = new StreamReader(rStream, Encoding.UTF8))
                        {
                            RSString = srr.ReadToEnd();
                        }
                    }
                }
                webRespond.Close();
                return RSString;
            }
            catch (WebException ex)
            {
                webRespond = (HttpWebResponse)ex.Response;
                using (Stream rStream = webRespond.GetResponseStream())
                {
                    using (StreamReader srr = new StreamReader(rStream, Encoding.UTF8))
                    {
                        RSString = srr.ReadToEnd();
                        webRespond.Close();
                        throw new Exception(RSString);
                    }
                }

            }
        }

        public string JsonWebService(string jsonRQ, string url)
        {
            byte[] rqBytes = Encoding.UTF8.GetBytes(jsonRQ);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = rqBytes.Length;

            return WebService(rqBytes, webRequest);
        }

        public string XmlWebService(string xml, string url)
        {
            byte[] rqBytes = Encoding.UTF8.GetBytes(xml);

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "text/xml;charset=utf-8";
            webRequest.ContentLength = rqBytes.Length;
            webRequest.Timeout = 1200000;

            return SoapWebService(rqBytes, webRequest);
        }
    }
}
