namespace Simple.Azure.Rest
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Threading.Tasks;

    public class BlobHelper : RESTHelper
    {
        // Constructor.

        public BlobHelper(string storageAccount, string storageKey)
            : base("http://" + storageAccount + ".blob.core.windows.net/", storageAccount, storageKey)
        {
        }

        public BlobHelper()
            : base(
                "http://127.0.0.1:10000/devstoreaccount1/", "devstoreaccount1",
                "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==")
        {
        }

        // List containers.
        // Return true on success, false if not found, throw exception on error.

        public Task<IEnumerable<Container>> ListContainers()
        {
            return TaskExtensions.Retry(
                () => CreateRESTRequest("GET", "?restype=container&comp=list")
                          .ContinueWithResponse()
                          .ContinueWith(ParseContainers, HandleError(404).With(() => default(IEnumerable<Container>))));
        }

        private static IEnumerable<Container> ParseContainers(HttpWebResponse response)
        {
            string result;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                result = reader.ReadToEnd();
            }
            return XElement.Parse(result)
                .Element("Containers")
                .MaybeElements("Container")
                .Select(x => new Container {Name = x.Element("Name").MaybeValue(), Url = x.Element("URL").MaybeValue()})
                .ToList();
        }

        // Create a blob container. 
        // Return true on success, false if already exists, throw exception on error.

        public Task<bool> CreateContainer(string container)
        {
            return TaskExtensions.Retry(
                () => CreateRESTRequest("PUT", container + "?restype=container")
                          .ContinueWithResponse()
                          .ContinueWith(CloseAndReturnTrue, HandleError(409).With(() => false)));
        }

        private static bool CloseAndReturnTrue(WebResponse response)
        {
            response.Close();
            return true;
        }

        //// Get container properties.
        //// Return true on success, false if not found, throw exception on error.

        public Task<ContainerProperties> GetContainerProperties(string container)
        {
            return TaskExtensions.Retry(
                () => CreateRESTRequest("HEAD", container + "?restype=container")
                          .ContinueWithResponse()
                          .ContinueWith(ParseBlobContainerProperties,
                                        HandleError(409).WithDefault<ContainerProperties>()));
        }

        private static ContainerProperties ParseBlobContainerProperties(HttpWebResponse response)
        {
            response.Close();
            var headers = response.Headers;
            if (headers != null)
            {
                return new ContainerProperties
                           {
                               ETag = headers["ETag"],
                               LastModified = DateTime.Parse(headers["Last-Modified"])
                           };
            }
            return null;
        }

        //// Get container metadata.
        //// Return true on success, false if not found, throw exception on error.

        public Task<SortedList<string, string>> GetContainerMetadata(string container)
        {
            var metadataList = new SortedList<string, string>();
            return TaskExtensions.Retry(
                () =>
                CreateRESTRequest("HEAD", container + "?restype=container&comp=metadata", null, metadataList)
                    .ContinueWithResponse()
                    .ContinueWith(response => ParseMetadataList(metadataList, response),
                                  HandleError(404).WithDefault<SortedList<string, string>>()));
        }

        private static SortedList<string, string> ParseMetadataList(SortedList<string, string> metadataList,
                                                                    HttpWebResponse response)
        {
            response.Close();

            if ((int) response.StatusCode == 200)
            {
                if (response.Headers != null)
                {
                    for (int i = 0; i < response.Headers.Count; i++)
                    {
                        var key = response.Headers.AllKeys[i];
                        if (key.StartsWith("x-ms-meta-"))
                        {
                            metadataList.Add(key, response.Headers[key]);
                        }
                    }
                }
            }
            return metadataList;
        }


        //// Set container metadata.
        //// Return true on success, false if not found, throw exception on error.

        public Task<bool> SetContainerMetadata(string container, SortedList<string, string> metadataList)
        {
            var headers = new SortedList<string, string>();

            if (metadataList != null)
            {
                foreach (KeyValuePair<string, string> value in metadataList)
                {
                    headers.Add("x-ms-meta-" + value.Key, value.Value);
                }
            }

            return TaskExtensions.Retry(
                () => CreateRESTRequest("PUT", container + "?restype=container&comp=metadata", null,
                                        headers)
                          .ContinueWithResponse()
                          .ContinueWith(CloseAndReturnTrue, HandleError(404).With(() => false)));
        }


        // List blobs in a container.
        // Return true on success, false if not found, throw exception on error.

        public Task<IEnumerable<BlobListItem>> ListBlobs(string container)
        {
            var parser = new BlobListParser(container, string.Empty);
            return TaskExtensions.Retry(() =>
                                        CreateRESTRequest("GET",
                                                          container +
                                                          "?restype=container&comp=list&include=snapshots&include=metadata")
                                            .ContinueWithResponse()
                                            .ContinueWith(parser.ParseBlobList,
                                                          HandleError(404).With(Enumerable.Empty<BlobListItem>)));
        }

        public Task<IEnumerable<BlobListItem>> ListBlobs(string container, string prefix)
        {
            var parser = new BlobListParser(container, prefix);

            return TaskExtensions.Retry(() =>
                                        CreateRESTRequest("GET",
                                                          container +
                                                          "?restype=container&comp=list&include=snapshots&include=metadata&prefix=" + Uri.EscapeUriString(prefix))
                                            .ContinueWithResponse()
                                            .ContinueWith(parser.ParseBlobList,
                                                          HandleError(404).With(Enumerable.Empty<BlobListItem>)));
        }

        public Task<IEnumerable<string>> ListFolders(string container)
        {
            return TaskExtensions.Retry(() =>
                                        CreateRESTRequest("GET",
                                                          container +
                                                          "?restype=container&comp=list&include=snapshots&include=metadata")
                                            .ContinueWithResponse()
                                            .ContinueWith(ParseFolderList,
                                                          HandleError(404).With(Enumerable.Empty<string>)));
        }

        public Task<IEnumerable<string>> ListFolders(string container, string prefix)
        {
            return TaskExtensions.Retry(() =>
                                        CreateRESTRequest("GET",
                                                          container +
                                                          "?restype=container&comp=list&include=snapshots&include=metadata&prefix=" + prefix)
                                            .ContinueWithResponse()
                                            .ContinueWith(ParseFolderList,
                                                          HandleError(404).With(Enumerable.Empty<string>)));
        }

        public Task<Blob> GetBlob(Uri uri)
        {
            return TaskExtensions.Retry(() => 
                CreateRESTRequest("GET", uri.AbsolutePath)
                .ContinueWithResponse()
                .ContinueWith(ReadBlob, HandleError(404).With<Blob>(() => null))
                );
        }

        public Task<bool> PutBlob(string container, string blob, Stream content, string contentType = "application/octet-stream")
        {
            var headers = new SortedList<string, string> {{"x-ms-blob-type", "BlockBlob"}, {"Content-Type", contentType}};
            return TaskExtensions.Retry(() =>
                                                CreateRESTRequest("PUT", container + "/" + blob, content, headers)
                                                    .ContinueWithResponse()
                                                    .ContinueWith(t => true, HandleError(409).With(() => false)));
        }

        public Task<string> GetContainerACL(string container)
        {
            return TaskExtensions.Retry(() =>
                    CreateRESTRequest("GET", container + "?restype=container&comp=acl")
                        .ContinueWithResponse()
                        .ContinueWith(GetAccessLevelString, HandleError(404).WithDefault<string>(), TaskContinuationOptions.ExecuteSynchronously));
        }

        public Task<bool> CopyBlob(string sourceBlob, string destBlob)
        {
            var headers = new SortedList<string, string>
                              {{"x-ms-copy-source", "/" + StorageAccount + "/" + sourceBlob}};
            return TaskExtensions.Retry(() => CreateRESTRequest("PUT", destBlob, null, headers)
                                                  .ContinueWithResponse()
                                                  .ContinueWith(t => !t.IsFaulted));
        }

        private string GetAccessLevelString(HttpWebResponse response)
        {
            if (response.Headers != null)
            {
                string access = response.Headers["x-ms-blob-public-access"];
                if (access != null)
                {
                    switch (access)
                    {
                        case "container":
                        case "blob":
                            return access;
                        case "true":
                            return "container";
                        default:
                            return "private";
                    }
                }
            }
            return "private";
        }

        private Blob ReadBlob(HttpWebResponse response)
        {
            if (response.ContentLength <= 0) return null;

            var memoryStream = new MemoryStream((int)response.ContentLength);
            var responseStream = response.GetResponseStream();
            if (responseStream != null) responseStream.CopyTo(memoryStream);
            return new Blob {ContentType = response.ContentType, Stream = memoryStream};
        }

        //// Retrieve the content of a blob. 
        //// Return true on success, false if not found, throw exception on error.

        //public string GetBlob(string container, string blob)
        //{
        //    return Retry<string>(delegate()
        //    {
        //        HttpWebResponse response;

        //        string content = null;

        //        try
        //        {
        //            response = CreateRESTRequest("GET", container + "/" + blob).GetResponse() as HttpWebResponse;

        //            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //            {
        //                content = reader.ReadToEnd();
        //            }

        //            response.Close();
        //            return content;
        //        }
        //        catch (WebException ex)
        //        {
        //            if (ex.Status == WebExceptionStatus.ProtocolError &&
        //                ex.Response != null &&
        //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
        //                return null;

        //            throw;
        //        }
        //    });
        //}


        private class BlobListParser
        {
            private readonly string _container;
            private readonly string _prefix;

            public BlobListParser(string container, string prefix)
            {
                _container = container;
                _prefix = prefix;
            }

            public IEnumerable<BlobListItem> ParseBlobList(HttpWebResponse response)
            {
                var regex = new Regex("^" + Regex.Escape(_prefix), RegexOptions.IgnoreCase);
                string result;
                var responseStream = response.GetResponseStream();
                if (responseStream == null) return Enumerable.Empty<BlobListItem>();

                using (var reader = new StreamReader(responseStream))
                {
                    result = reader.ReadToEnd();
                }
                var blobs = XElement.Parse(result).Element("Blobs");

                if (blobs == null) return Enumerable.Empty<BlobListItem>();

                var names = blobs.Elements("Blob").Select(x => regex.Replace(x.Element("Name").MaybeValue(), string.Empty)).ToList();
                var folders = ParseFolders(names);

                var files = blobs
                    .Elements("Blob")
                    .Where(x => !regex.Replace(x.Element("Name").MaybeValue(), string.Empty).Contains("/"))
                    .Select(x => ParseBlobListItem(x, regex));

                return folders.Concat(files).ToList();
            }

            private static BlobListItem ParseBlobListItem(XElement x, Regex regex)
            {
                var item = new BlobListItem
                           {
                               Name = regex.Replace(x.Element("Name").MaybeValue(), string.Empty),
                               FullPath = x.Element("Url").MaybeValue()
                           };

                var properties = x.Element("Properties");
                if (properties != null)
                {
                    item.ContentType = properties.Element("Content-Type").MaybeValue();
                    item.Type = properties.Element("BlobType").MaybeValue().Replace("Blob", "");
                    item.LeaseStatus = properties.Element("LeaseStatus").MaybeValue();
                    item.Etag = properties.Element("Etag").MaybeValue();
                    var lastModified = properties.Element("Last-Modified").MaybeValue();
                    DateTime lastModifiedDate;
                    if (DateTime.TryParse(lastModified, out lastModifiedDate))
                    {
                        item.LastModified = lastModifiedDate.ToString("yyyy-MM-dd hh:mm");
                    }
                    else
                    {
                        item.LastModified = lastModified;
                    }
                    var size = properties.Element("Content-Length").MaybeValue();
                    if (!string.IsNullOrWhiteSpace(size))
                    {
                        item.Size = long.Parse(size);
                    }
                }

                return item;
            }

            private IEnumerable<BlobListItem> ParseFolders(IEnumerable<string> names)
            {
                var folders = names
                    .Where(n => n.Split('/').Length >= 2)
                    .Select(n => n.Split('/')[0])
                    .Distinct()
                    .Select(
                        n =>
                        new BlobListItem {Name = n, ContentType = "application/folder", FullPath = _container + '/' + _prefix + n, Type = "Folder"});
                return folders;
            }
        }

        private static IEnumerable<string> ParseFolderList(HttpWebResponse response)
        {
            string result;
            var responseStream = response.GetResponseStream();
            if (responseStream == null) return Enumerable.Empty<string>();

            using (var reader = new StreamReader(responseStream))
            {
                result = reader.ReadToEnd();
            }
            return XElement.Parse(result)
                .Element("Blobs")
                .MaybeElements("Blob")
                .Select(x => x.Element("Name").MaybeValue())
                .Where(n => n != null && n.Contains("/"))
                .Select(n => n.Substring(0, n.LastIndexOf('/')))
                .Distinct()
                .ToList();
        }




       
            //// Get container access control.
            //// Return true on success, false if not found, throw exception on error.
            //// accessLevel set to container|blob|private.

            //public string GetContainerACL(string container)
            //{
            //    return Retry<string>(delegate()
            //    {
            //        HttpWebResponse response;

            //        string accessLevel = String.Empty;

            //        try
            //        {
            //            response = CreateRESTRequest("GET", container + "?restype=container&comp=acl").GetResponse() as HttpWebResponse;
            //            response.Close();

            //            if ((int)response.StatusCode == 200)
            //            {
            //                if (response.Headers != null)
            //                {
            //                    string access = response.Headers["x-ms-blob-public-access"];
            //                    if (access != null)
            //                    {
            //                        switch (access)
            //                        {
            //                            case "container":
            //                            case "blob":
            //                                accessLevel = access;
            //                                break;
            //                            case "true":
            //                                accessLevel = "container";
            //                                break;
            //                            default:
            //                                accessLevel = "private";
            //                                break;
            //                        }
            //                    }
            //                    else
            //                    {
            //                        accessLevel = "private";
            //                    }
            //                }
            //            }

            //            return accessLevel;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Set container access control.
            //// Return true on success, false if not found, throw exception on error. 
            //// Set accessLevel to container|blob|private.

            //public bool SetContainerACL(string container, string accessLevel)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            switch (accessLevel)
            //            {
            //                case "container":
            //                case "blob":
            //                    headers.Add("x-ms-blob-public-access", accessLevel);
            //                    break;
            //                case "private":
            //                default:
            //                    break;
            //            }

            //            response = CreateRESTRequest("PUT", container + "?restype=container&comp=acl", string.Empty, headers).GetResponse() as HttpWebResponse;
            //            response.Close();

            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Get container access policy.
            //// Return true on success, false if not found, throw exception on error. 

            //public string GetContainerAccessPolicy(string container)
            //{
            //    return Retry<string>(delegate()
            //    {
            //        HttpWebResponse response;

            //        string accessPolicyXml = String.Empty;

            //        try
            //        {
            //            response = CreateRESTRequest("GET", container + "?restype=container&comp=acl").GetResponse() as HttpWebResponse;

            //            if ((int)response.StatusCode == 200)
            //            {
            //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //                {
            //                    accessPolicyXml = reader.ReadToEnd();
            //                }
            //            }

            //            response.Close();

            //            return accessPolicyXml;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Set container access policy (container|blob|private).
            //// Return true on success, false if not found, throw exception on error.

            //public bool SetContainerAccessPolicy(string container, string accessLevel, string accessPolicyXml)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            switch (accessLevel)
            //            {
            //                case "container":
            //                case "blob":
            //                    headers.Add("x-ms-blob-public-access", accessLevel);
            //                    break;
            //                case "private":
            //                default:
            //                    break;
            //            }

            //            response = CreateRESTRequest("PUT", container + "?restype=container&comp=acl", accessPolicyXml, headers).GetResponse() as HttpWebResponse;
            //            response.Close();

            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Delete a blob container. 
            //// Return true on success, false if not found, throw exception on error.

            //public bool DeleteContainer(string container)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            response = CreateRESTRequest("DELETE", container).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}




            

            //// Create or update a blob. 
            //// Return true on success, false if not found, throw exception on error.

            


            //// Create or update a page blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public bool PutBlob(string container, string blob, int pageBlobSize)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-blob-type", "PageBlob");
            //            headers.Add("x-ms-blob-content-length", pageBlobSize.ToString());

            //            response = CreateRESTRequest("PUT", container + "/" + blob, null, headers).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Create or update a blob condition based on an expected ETag value.
            //// Return true on success, false if not found, throw exception on error.

            //public bool PutBlobIfUnchanged(string container, string blob, string content, string expectedETagValue)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-blob-type", "BlockBlob");
            //            headers.Add("If-Match", expectedETagValue);

            //            response = CreateRESTRequest("PUT", container + "/" + blob, content, headers, expectedETagValue).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                ((int)(ex.Response as HttpWebResponse).StatusCode == 409 ||
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 412))
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Create or update a blob with an MD5 hash.
            //// Return true on success, false if not found, throw exception on error.

            //public bool PutBlobWithMD5(string container, string blob, string content)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            string md5 = Convert.ToBase64String(new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.Default.GetBytes(content)));

            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-blob-type", "BlockBlob");
            //            headers.Add("Content-MD5", md5);

            //            response = CreateRESTRequest("PUT", container + "/" + blob, content, headers, String.Empty, md5).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                ((int)(ex.Response as HttpWebResponse).StatusCode == 409 ||
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 400))
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Retrieve a page from a block blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public string GetPage(string container, string blob, int pageOffset, int pageSize)
            //{
            //    return Retry<string>(delegate()
            //    {
            //        HttpWebResponse response;

            //        string content = null;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-range", "bytes=" + pageOffset.ToString() + "-" + (pageOffset + pageSize - 1).ToString());
            //            response = CreateRESTRequest("GET", container + "/" + blob, null, headers).GetResponse() as HttpWebResponse;

            //            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //            {
            //                content = reader.ReadToEnd();
            //            }

            //            response.Close();
            //            return content;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Write a page to a page blob.
            //// Return true on success, false if not found, throw exception on error.

            //public bool PutPage(string container, string blob, string content, int pageOffset, int pageSize)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-page-write", "update");
            //            headers.Add("x-ms-range", "bytes=" + pageOffset.ToString() + "-" + (pageOffset + pageSize - 1).ToString());

            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=page ", content, headers).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Retrieve the list of regions in use for a page blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public string[] GetPageRegions(string container, string blob)
            //{
            //    return Retry<string[]>(delegate()
            //    {
            //        HttpWebResponse response;

            //        string[] regions = null;

            //        try
            //        {
            //            response = CreateRESTRequest("GET", container + "/" + blob + "?comp=pagelist").GetResponse() as HttpWebResponse;

            //            if ((int)response.StatusCode == 200)
            //            {
            //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //                {
            //                    List<string> regionList = new List<string>();
            //                    string result = reader.ReadToEnd();

            //                    XElement xml = XElement.Parse(result);

            //                    foreach (XElement range in xml.Elements("PageRange"))
            //                    {
            //                        regionList.Add(range.ToString());
            //                    }

            //                    regions = new string[regionList.Count];

            //                    int i = 0;
            //                    foreach (string region in regionList)
            //                    {
            //                        regions[i++] = region;
            //                    }
            //                }
            //            }

            //            response.Close();
            //            return regions;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Copy a blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public bool CopyBlob(string sourceContainer, string sourceBlob, string destContainer, string destBlob)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-copy-source", "/" + StorageAccount + "/" + sourceContainer + "/" + sourceBlob);

            //            response = CreateRESTRequest("PUT", destContainer + "/" + destBlob, null, headers).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Retrieve the list of uploaded blocks for a blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public string[] GetBlockList(string container, string blob)
            //{
            //    return Retry<string[]>(delegate()
            //    {
            //        HttpWebResponse response;

            //        string[] blockIds = null;

            //        try
            //        {
            //            response = CreateRESTRequest("GET", container + "/" + blob + "?comp=blocklist").GetResponse() as HttpWebResponse;

            //            if ((int)response.StatusCode == 200)
            //            {
            //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            //                {
            //                    List<string> blockIdList = new List<string>();
            //                    string result = reader.ReadToEnd();

            //                    XElement xml = XElement.Parse(result);

            //                    foreach (XElement blockGroup in xml.Elements())
            //                    {
            //                        foreach (XElement block in blockGroup.Elements("Block"))
            //                        {
            //                            blockIdList.Add(block.Element("Name").Value);
            //                        }
            //                    }

            //                    blockIds = new string[blockIdList.Count];

            //                    int i = 0;
            //                    foreach (string blockId in blockIdList)
            //                    {
            //                        blockIds[i++] = blockId;
            //                    }
            //                }
            //            }

            //            response.Close();
            //            return blockIds;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Put block - upload a block (portion) of a blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public bool PutBlock(string container, string blob, int blockId, string[] blockIds, string content)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();

            //            byte[] blockIdBytes = BitConverter.GetBytes(blockId);
            //            string blockIdBase64 = Convert.ToBase64String(blockIdBytes);

            //            blockIds[blockId] = blockIdBase64;

            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=block&blockid=" + blockIdBase64, content, headers).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Put block list - complete creation of blob based on uploaded content.
            //// Return true on success, false if not found, throw exception on error.

            //public bool PutBlockList(string container, string blob, string[] blockIds)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            StringBuilder content = new StringBuilder();
            //            content.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            //            content.Append("<BlockList>");
            //            for (int i = 0; i < blockIds.Length; i++)
            //            {
            //                content.Append("<Latest>" + blockIds[i] + "</Latest>");
            //            }
            //            content.Append("</BlockList>");

            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=blocklist", content.ToString(), null).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Create a snapshot of a blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public bool SnapshotBlob(string container, string blob)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=snapshot").GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Delete a blob. 
            //// Return true on success, false if not found, throw exception on error.

            //public bool DeleteBlob(string container, string blob)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            response = CreateRESTRequest("DELETE", container + "/" + blob).GetResponse() as HttpWebResponse;
            //            response.Close();
            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Lease a blob.
            //// Lease action: acquire|renew|break|release.
            //// Lease Id: returned on acquire action; must be specified for all other actions.
            //// Return true on success, false if not found, throw exception on error.

            //public string LeaseBlob(string container, string blob, string leaseAction, string leaseId)
            //{
            //    return Retry<string>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();
            //            headers.Add("x-ms-lease-action", leaseAction);
            //            if (!String.IsNullOrEmpty(leaseId))
            //            {
            //                headers.Add("x-ms-lease-id", leaseId);
            //            }

            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=lease", null, headers).GetResponse() as HttpWebResponse;
            //            response.Close();

            //            if (leaseAction == "acquire")
            //            {
            //                leaseId = response.Headers["x-ms-lease-id"];
            //            }

            //            return leaseId;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 409)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Retrieve a blob's properties.
            //// Return true on success, false if not found, throw exception on error.

            //public SortedList<string, string> GetBlobProperties(string container, string blob)
            //{
            //    return Retry<SortedList<string, string>>(delegate()
            //    {
            //        HttpWebResponse response;

            //        SortedList<string, string> propertiesList = new SortedList<string, string>();

            //        try
            //        {
            //            response = CreateRESTRequest("HEAD", container + "/" + blob).GetResponse() as HttpWebResponse;
            //            response.Close();

            //            if ((int)response.StatusCode == 200)
            //            {
            //                if (response.Headers != null)
            //                {
            //                    for (int i = 0; i < response.Headers.Count; i++)
            //                    {
            //                        propertiesList.Add(response.Headers.Keys[i], response.Headers[i]);
            //                    }
            //                }
            //            }

            //            return propertiesList;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Set blob properties.
            //// Return true on success, false if not found, throw exception on error.

            //public bool SetBlobProperties(string container, string blob, SortedList<string, string> propertyList)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();

            //            if (propertyList != null)
            //            {
            //                foreach (KeyValuePair<string, string> value in propertyList)
            //                {
            //                    headers.Add(value.Key, value.Value);
            //                }
            //            }

            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=properties", string.Empty, headers).GetResponse() as HttpWebResponse;
            //            response.Close();

            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}


            //// Retrieve a blob's metadata.
            //// Return true on success, false if not found, throw exception on error.

            //public SortedList<string, string> GetBlobMetadata(string container, string blob)
            //{
            //    return Retry<SortedList<string, string>>(delegate()
            //    {
            //        HttpWebResponse response;

            //        SortedList<string, string> metadata = new SortedList<string, string>();

            //        try
            //        {
            //            response = CreateRESTRequest("HEAD", container + "/" + blob + "?comp=metadata").GetResponse() as HttpWebResponse;
            //            response.Close();

            //            if ((int)response.StatusCode == 200)
            //            {
            //                if (response.Headers != null)
            //                {
            //                    for (int i = 0; i < response.Headers.Count; i++)
            //                    {
            //                        if (response.Headers.Keys[i].StartsWith("x-ms-meta-"))
            //                        {
            //                            metadata.Add(response.Headers.Keys[i], response.Headers[i]);
            //                        }
            //                    }
            //                }
            //            }

            //            return metadata;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return null;

            //            throw;
            //        }
            //    });
            //}


            //// Set blob metadata.
            //// Return true on success, false if not found, throw exception on error.

            //public bool SetBlobMetadata(string container, string blob, SortedList<string, string> metadataList)
            //{
            //    return Retry<bool>(delegate()
            //    {
            //        HttpWebResponse response;

            //        try
            //        {
            //            SortedList<string, string> headers = new SortedList<string, string>();

            //            if (metadataList != null)
            //            {
            //                foreach (KeyValuePair<string, string> value in metadataList)
            //                {
            //                    headers.Add("x-ms-meta-" + value.Key, value.Value);
            //                }
            //            }

            //            response = CreateRESTRequest("PUT", container + "/" + blob + "?comp=metadata", string.Empty, headers).GetResponse() as HttpWebResponse;
            //            response.Close();

            //            return true;
            //        }
            //        catch (WebException ex)
            //        {
            //            if (ex.Status == WebExceptionStatus.ProtocolError &&
            //                ex.Response != null &&
            //                (int)(ex.Response as HttpWebResponse).StatusCode == 404)
            //                return false;

            //            throw;
            //        }
            //    });
            //}

    }

    public class SortedList<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _internal = new Dictionary<TKey, TValue>();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_internal).GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)_internal).CopyTo(array, index);
        }

        public object SyncRoot
        {
            get { return ((ICollection)_internal).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return ((ICollection)_internal).IsSynchronized; }
        }

        public bool Contains(object key)
        {
            return ((IDictionary)_internal).Contains(key);
        }

        public void Add(object key, object value)
        {
            ((IDictionary)_internal).Add(key, value);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _internal.GetEnumerator();
        }

        public void Remove(object key)
        {
            ((IDictionary)_internal).Remove(key);
        }

        public object this[object key]
        {
            get { return _internal[(TKey)key]; }
            set { _internal[(TKey)key] = (TValue)value; }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get { return _internal.Keys; }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get { return _internal.Values; }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Add(TKey key, TValue value)
        {
            _internal.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_internal).Add(item);
        }

        public void Clear()
        {
            _internal.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_internal).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_internal).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_internal).Remove(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _internal.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _internal.ContainsValue(value);
        }

        public bool Remove(TKey key)
        {
            return _internal.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _internal.TryGetValue(key, out value);
        }

        public IEqualityComparer<TKey> Comparer
        {
            get { return _internal.Comparer; }
        }

        public int Count
        {
            get { return _internal.Count; }
        }

        public TValue this[TKey key]
        {
            get { return _internal[key]; }
            set { _internal[key] = value; }
        }

        public ICollection<TValue> Values
        {
            get { return _internal.Values; }
        }

        public ICollection<TKey> Keys
        {
            get { return _internal.Keys; }
        }
    }
}
