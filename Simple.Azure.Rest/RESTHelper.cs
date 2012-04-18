namespace Simple.Azure.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;

    public class RESTHelper
    {
#if SILVERLIGHT
        private static readonly IWebRequestCreate WebRequestCreator = System.Net.Browser.WebRequestCreator.ClientHttp;
#else
        private static readonly IWebRequestCreate WebRequestCreator = new HttpWebRequestCreator();
#endif

        public RESTHelper(string endpoint, string storageAccount, string storageKey)
        {
            Endpoint = endpoint;
            StorageAccount = storageAccount;
            StorageKey = storageKey;
        }

        #region REST HTTP Request Helper Methods

        // Construct and issue a REST request and return the response.

        public Task<HttpWebRequest> CreateRESTRequest(string method, string resource, Stream requestBody = null,
                                                      IDictionary<string, string> headers = null,
                                                      string ifMatch = "", string md5 = "")
        {
            DateTime now = DateTime.UtcNow;
            string uri = Endpoint.TrimEnd('/') + '/' + resource.TrimStart('/');


            var request = (HttpWebRequest)WebRequestCreator.Create(new Uri(uri));
            request.Method = method;
            request.ContentLength = 0;
            request.Headers["x-ms-date"] = now.ToString("R", CultureInfo.InvariantCulture);
            request.Headers["x-ms-version"] = "2009-09-19";

            if (IsTableStorage)
            {
                request.ContentType = "application/atom+xml";

                request.Headers["DataServiceVersion"] = "1.0;NetFx";
                request.Headers["MaxDataServiceVersion"] = "1.0;NetFx";
            }

            if (headers != null)
            {
                foreach (var header in headers.OrderBy(kvp => kvp.Key))
                {
                    if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        request.ContentType = header.Value;
                    }
                    else
                    {
                        try
                        {
                            request.Headers[header.Key] = header.Value;
                        }
                        catch (ArgumentException)
                        {
                            throw;
                        }
                    }
                }
            }

            if (requestBody != null)
            {
                request.Headers["Accept-Charset"] = "UTF-8";
                request.ContentLength = requestBody.Length;
            }

            request.Headers["Authorization"] = AuthorizationHeader(method, now, request.Headers, request.RequestUri,
                                                                   request.ContentLength, ifMatch, md5);

            if (requestBody != null)
            {
                return Task.Factory.FromAsync<Stream>(request.BeginGetRequestStream, request.EndGetRequestStream, null)
                    .ContinueWith(t =>
                    {
                        requestBody.CopyTo(t.Result);
                        t.Result.Close();
                        return request;
                    });
            }

            var tcs = new TaskCompletionSource<HttpWebRequest>();
            tcs.SetResult(request);
            return tcs.Task;
        }

        // Generate an authorization header.

        public string AuthorizationHeader(string method, DateTime now, WebHeaderCollection headers, Uri uri,
                                          long contentLength, string ifMatch = "", string md5 = "")
        {
            string messageSignature;

            if (IsTableStorage)
            {
                messageSignature = String.Format("{0}\n\n{1}\n{2}\n{3}",
                                                 method,
                                                 "application/atom+xml",
                                                 now.ToString("R", CultureInfo.InvariantCulture),
                                                 GetCanonicalizedResource(uri, StorageAccount)
                    );
            }
            else
            {
                messageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                                                 method,
                                                 (method == "GET" || method == "HEAD")
                                                     ? String.Empty
                                                     : contentLength.ToString(CultureInfo.InvariantCulture),
                                                 ifMatch,
                                                 GetCanonicalizedHeaders(headers),
                                                 GetCanonicalizedResource(uri, StorageAccount),
                                                 md5
                    );
            }
            byte[] signatureBytes = Encoding.UTF8.GetBytes(messageSignature);
            var sha256 = new HMACSHA256(Convert.FromBase64String(StorageKey));
            return "SharedKey " + StorageAccount + ":" + Convert.ToBase64String(sha256.ComputeHash(signatureBytes));
        }

        // Get canonicalized headers.

        public string GetCanonicalizedHeaders(WebHeaderCollection headers)
        {
            var sb = new StringBuilder();
            List<string> headerNameList =
                headers.AllKeys.Where(hn => hn.StartsWith("x-ms-", StringComparison.OrdinalIgnoreCase)).OrderBy(s => s).
                    ToList();
            foreach (string headerName in headerNameList)
            {
                var builder = new StringBuilder(headerName);
                string separator = ":";
                foreach (string headerValue in GetHeaderValues(headers, headerName))
                {
                    string trimmedValue = headerValue.Replace("\r\n", String.Empty);
                    builder.Append(separator);
                    builder.Append(trimmedValue);
                    separator = ",";
                }
                sb.Append(builder.ToString());
                sb.Append("\n");
            }
            return sb.ToString();
        }

        // Get header values.

        public IList<string> GetHeaderValues(WebHeaderCollection headers, string headerName)
        {
            return new[] { headers[headerName] };
        }

        // Get canonicalized resource.

        public string GetCanonicalizedResource(Uri address, string accountName)
        {
            var str = new StringBuilder();
            var builder = new StringBuilder("/");
            builder.Append(accountName);
            builder.Append(address.AbsolutePath);
            str.Append(builder.ToString());
            IDictionary<string, string> values2 = new Dictionary<string, string>();
            if (!IsTableStorage)
            {
                IDictionary<string, string> values = ParseQueryString(address.Query);
                foreach (string str2 in values.Keys)
                {
                    List<string> list = values[str2].Split(',').ToList();
                    list.Sort();
                    var builder2 = new StringBuilder();
                    foreach (var obj2 in list)
                    {
                        if (builder2.Length > 0)
                        {
                            builder2.Append(",");
                        }
                        builder2.Append(obj2);
                    }
                    values2.Add((str2 == null) ? null : str2.ToLowerInvariant(), builder2.ToString());
                }
            }
            List<string> list2 = values2.Keys.ToList();
            list2.Sort();
            foreach (string str3 in list2)
            {
                var builder3 = new StringBuilder(string.Empty);
                builder3.Append(str3);
                builder3.Append(":");
                builder3.Append(values2[str3]);
                str.Append("\n");
                str.Append(builder3.ToString());
            }
            return str.ToString();
        }

        private IDictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>();
            foreach (var pair in query.Split(new[] { '?', '&' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('=')))
            {
                var value = pair.Length == 2 ? pair[1] : string.Empty;
                if (result.ContainsKey(pair[0]))
                {
                    result[pair[0]] += "," + value;
                }
                else
                {
                    result.Add(pair[0], value);
                }
            }
            return result;
        }

        #endregion

        protected bool IsTableStorage { get; set; }

        public string Endpoint { get; internal set; }

        public string StorageAccount { get; internal set; }

        public string StorageKey { get; internal set; }


        protected static WebExceptionHandler<object> HandleError(params int[] statusCodes)
        {
            return new WebExceptionHandler<object>(statusCodes);
        }
    }

    internal class HttpWebRequestCreator : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            return WebRequest.CreateDefault(uri);
        }
    }
}