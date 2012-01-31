namespace Simple.Azure.Extensions
{
    using System.Net;
    using System.Xml.Linq;
    using System.IO;
    using System.Xml;

    public static class WebResponseExtensions
    {
        public static XElement GetXmlContent(this WebResponse response)
        {
            XElement xml;

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                xml = XElement.Load(sr);
            }

            return xml;
        }

        public static XmlReader GetResponseReader(this WebResponse response)
        {
            return XmlReader.Create(response.GetResponseStream());
        }
    }
}
