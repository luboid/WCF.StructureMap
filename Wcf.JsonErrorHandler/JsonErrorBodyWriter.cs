using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wcf.JsonErrorHandler
{
    public class JsonErrorBodyWriter : BodyWriter
    {
        Exception exception;

        public JsonErrorBodyWriter(Exception exception):base(true)
        {
            this.exception = exception;
        }

        protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
        {
            if (exception is FaultException a)
            {
                writer.WriteStartElement("root");
                writer.WriteAttributeString("type", "object");//fake

                writer.WriteStartElement("Fault");
                writer.WriteAttributeString("type", "object");//fake


                if (null != a.Code)
                {
                    //a.Code.SubCode.Name
                    writer.WriteStartElement("Code");
                    writer.WriteAttributeString("type", "object");//fake
                    writer.WriteElementString("Value", a.Code.Name);
                    if (a.Code.SubCode != null)
                    {
                        writer.WriteStartElement("SubCode");
                        writer.WriteAttributeString("type", "object");//fake
                        writer.WriteElementString("Value", a.Code.SubCode.Name);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                writer.WriteStartElement("Reason");
                writer.WriteAttributeString("type", "object");//fake
                writer.WriteElementString("Text", a.Message);
                writer.WriteEndElement();

                if (!string.IsNullOrWhiteSpace(a.Action))
                {
                    writer.WriteElementString("Action", a.Action);
                }

                var pi = a.GetType().GetProperty("Detail");
                if (pi != null)
                {
                    var details = pi.GetValue(a); var dt = details.GetType();

                    writer.WriteStartElement("Detail");
                    writer.WriteAttributeString("type", "object");//fake

                    foreach (var dpi in dt.GetProperties())
                    {
                        writer.WriteElementString(dpi.Name, Convert.ToString(dpi.GetValue(details)));
                    }

                    foreach (var dpi in dt.GetFields())
                    {
                        writer.WriteElementString(dpi.Name, Convert.ToString(dpi.GetValue(details)));
                    }

                    writer.WriteEndElement();
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
            else
            {
                writer.WriteStartElement("root");
                writer.WriteAttributeString("type", "object");

                writer.WriteStartElement("Fault");
                writer.WriteAttributeString("type", "object");//fake

                writer.WriteStartElement("Reason");
                writer.WriteAttributeString("type", "object");
                writer.WriteElementString("Text", exception.Message);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }
    }
}
