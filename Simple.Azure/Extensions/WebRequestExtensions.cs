namespace Simple.Azure.Extensions
{
    using System.Net;

    internal static class WebRequestExtensions
    {
#if(!SILVERLIGHT)
        internal static void SetContent(this WebRequest request, string content)
        {
            using (var writer = new System.IO.StreamWriter(request.GetRequestStream()))
            {
                writer.Write(content);
            }
        }
#endif
        internal static void SetContentAsync(this WebRequest request, string content)
        {
            request.BeginGetRequestStream
                (ar =>
                     {
                         using (var writer = new System.IO.StreamWriter(request.EndGetRequestStream(ar)))
                         {
                             writer.Write(content);
                         }

                     }, null);
        }
    }
}
