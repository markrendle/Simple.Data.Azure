namespace Simple.Azure.Rest
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public static class XElementEx
    {
        public static string MaybeValue(this XElement element)
        {
            return element == null ? string.Empty : element.Value;
        }

        public static IEnumerable<XElement> MaybeElements(this XElement element)
        {
            return element == null ? Enumerable.Empty<XElement>() : element.Elements();
        }

        public static XElement MaybeElement(this XElement element, XName name)
        {
            return element == null ? null : element.Element(name);
        }

        public static IEnumerable<XElement> MaybeElements(this XElement element, XName name)
        {
            return element == null ? Enumerable.Empty<XElement>() : element.Elements(name);
        }
    }
}