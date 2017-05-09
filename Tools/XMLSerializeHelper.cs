using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SubDis.Service.SerializeTools
{
    public class XMLSerializeHelper
    {
        private static XMLSerializeHelper root = new XMLSerializeHelper();

        public static XMLSerializeHelper GetInstance()
        {
            if (root == null)
            {
                object o = new object();
                lock (o)
                {
                    if (root == null)
                    {
                        root = new XMLSerializeHelper();
                    }
                }
            }
            return root;

        }

        public string SerializeObject(Object obj)
        {
            XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
            xmlns.Add("", "");
            var xml = "";

            XmlSerializer serializer = new XmlSerializer(obj.GetType());

            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                xml = writer.ToString();
            }

            //SoapFormatter _serializer = new SoapFormatter();
            

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    _serializer.Serialize(ms, xml);
            //    xml = Encoding.UTF8.GetString(ms.ToArray(), 0, ms.ToArray().Length);
            //}
            //using (StringWriter writer = new StringWriter())
            //{
            //    _serializer.Serialize(writer, obj);
            //    xml = writer.ToString();
            //}

            //XmlTypeMapping myTypeMapping = (new SoapReflectionImporter().ImportTypeMapping(obj.GetType()));
            //XmlSerializer _serializer = new XmlSerializer(myTypeMapping);
            //using (StringWriter writer = new StringWriter())
            //{
            //    _serializer.Serialize(writer, obj, xmlns);
            //    xml = writer.ToString();
            //}
            //MemoryStream ms = new MemoryStream() 
            //XmlTextWriter 

            return xml;
        }

        public object DeserializeObject(string xml,Type type)
        {
            XmlSerializer _serializer = new XmlSerializer(type);
            object obj;
            using (StringReader reader = new StringReader(xml))
            {
                using(XmlTextReader xmlReader = new XmlTextReader(reader))
                {
                    obj = _serializer.Deserialize(xmlReader);
                }
            }
            return obj;
        }
    }
}
