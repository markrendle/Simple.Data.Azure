using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Azure.Helpers
{
    internal static class Atom
    {
        private const string Xml =
            @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<entry xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns=""http://www.w3.org/2005/Atom"">
  <title />
  <updated/>
  <author>
    <name />
  </author>
  <id />
  <content type=""application/xml"">
    <m:properties/>
  </content>
</entry>";

        public static string DataServicesAtomEntryXml
        {
            get { return Xml; }
        }
    }
}