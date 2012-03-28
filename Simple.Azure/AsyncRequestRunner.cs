namespace Simple.Azure
{
    using System.Net;
    using System.Threading.Tasks;
    using NExtLib.IO;

    public class AsyncRequestRunner
    {
        public Task<string> Request(HttpWebRequest request)
        {
            using (var response = TryRequest(request))
            {
                return TryGetResponseBody(response);
            }
        }

        public Task<HttpWebResponse> TryRequest(HttpWebRequest request)
        {
            return Task.Factory.FromAsync(request.BeginGetResponse,
                                          result => (HttpWebResponse) request.EndGetResponse(result), null);
        }

        private static Task<string> TryGetResponseBody(Task<HttpWebResponse> response)
        {
            if (response != null)
            {
                return response.ContinueWith(t =>
                                                 {
                                                     if (t.Result != null)
                                                     {
                                                         var stream = t.Result.GetResponseStream();
                                                         if (stream != null)
                                                         {
                                                             return QuickIO.StreamToString(stream);
                                                         }
                                                     }
                                                     return string.Empty;
                                                 });
            }

            return new Task<string>(() => string.Empty);
        }
    }
}