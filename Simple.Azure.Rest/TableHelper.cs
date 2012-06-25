namespace Simple.Azure.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class TableHelper : RESTHelper
    {
        // Constructor.

        public TableHelper(string storageAccount, string storageKey) : base("http://" + storageAccount + ".table.core.windows.net/", storageAccount, storageKey)
        {
            IsTableStorage = true;
        }

        public TableHelper()
            : base("http://127.0.0.1:10002/devstoreaccount1/", "devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==")
        {
            IsTableStorage = true;
        }


        // List tables.
        // Return true on success, false if not found, throw exception on error.

        public async Task<List<string>> ListTables()
        {
            var tables = new List<string>();

            var request = CreateRequest(HttpMethod.Get, "Tables");
            var client = new HttpClient();
            var response = await client.SendAsync(request);

            if ((int)response.StatusCode == 200)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                using (var reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();

                    XNamespace ns = "http://www.w3.org/2005/Atom";
                    XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

                    XElement x = XElement.Parse(result, LoadOptions.SetBaseUri);

                    tables.AddRange(x.Descendants(d + "TableName").Select(tableName => tableName.Value));
                }
            }
            else
            {
                Debug.WriteLine(response.ReasonPhrase);
            }

            return tables;
        }

        //public List<string> ListTables()
        //{
        //    return Retry(delegate()
        //    {
        //        HttpWebResponse response;
        //        List<string> tables = new List<string>();

        //        tables = new List<string>();

        //        try
        //        {
        //            var request = CreateRESTRequest("GET", "Tables");
        //            response = request.GetResponse() as HttpWebResponse;

        //            if ((int)response.StatusCode == 200)
        //            {
        //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //                {
        //                    string result = reader.ReadToEnd();

        //                    XNamespace ns = "http://www.w3.org/2005/Atom";
        //                    XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        //                    XElement x = XElement.Parse(result, LoadOptions.SetBaseUri);

        //                    foreach (XElement tableName in x.Descendants(d + "TableName"))
        //                    {
        //                        tables.Add(tableName.Value);
        //                    }
        //                }
        //            }

        //            response.Close();

        //            return tables;
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


        //// Create a table.
        //// Return true on success, false if already exists / unable to create, throw exception on error.

        //public bool CreateTable(string tableName)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            string now = DateTime.UtcNow.ToString("o");

        //            string requestBody = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
        //                                  "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
        //                                  "       xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
        //                                  "       xmlns=\"http://www.w3.org/2005/Atom\"> " +
        //                                  "  <title /> " +
        //                                  "  <updated>" + now + "</updated> " +
        //                                  "  <author>" +
        //                                  "    <name/> " +
        //                                  "  </author> " +
        //                                  "  <id/> " +
        //                                  "  <content type=\"application/xml\">" +
        //                                  "    <m:properties>" +
        //                                  "      <d:TableName>{0}</d:TableName>" +
        //                                  "    </m:properties>" +
        //                                  "  </content> " +
        //                                  "</entry>",
        //                                  tableName);

        //            response = CreateRESTRequest("POST", "Tables", requestBody).GetResponse() as HttpWebResponse;
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


        //// Delete table.
        //// Return true on success, false if not found, throw exception on error.

        //public bool DeleteTable(string tableName)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            response = CreateRESTRequest("DELETE", "Tables('" + tableName + "')").GetResponse() as HttpWebResponse;
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


        //// Insert entity.
        //// Return true on success, false if not found, throw exception on error.

        //public bool InsertEntity(string tableName, string partitionKey, string rowKey, object obj)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            // Create properties list. Use reflection to retrieve properties from the object.

        //            StringBuilder properties = new StringBuilder();
        //            properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", "PartitionKey", partitionKey));
        //            properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", "RowKey", rowKey));

        //            Type t = obj.GetType();
        //            PropertyInfo[] pi = t.GetProperties();
        //            MethodInfo mi;
        //            foreach (PropertyInfo p in pi)
        //            {
        //                try
        //                {
        //                    mi = p.GetGetMethod();
        //                    properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", p.Name, mi.Invoke(obj, null).ToString()));
        //                }
        //                catch (NullReferenceException)
        //                {
        //                }
        //            }

        //            string requestBody = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
        //                                  "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
        //                                  "       xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
        //                                  "       xmlns=\"http://www.w3.org/2005/Atom\"> " +
        //                                  "  <title /> " +
        //                                  "  <updated>2009-03-18T11:48:34.9840639-07:00</updated> " +
        //                                  "  <author>" +
        //                                  "    <name/> " +
        //                                  "  </author> " +
        //                                  "  <id/> " +
        //                                  "  <content type=\"application/xml\">" +
        //                                  "  <m:properties>" +
        //                                  "{1}" +
        //                                  "  </m:properties>" +
        //                                  "  </content> " +
        //                                  "</entry>",
        //                                  tableName,
        //                                  properties);

        //            response = CreateRESTRequest("POST", tableName, requestBody).GetResponse() as HttpWebResponse;
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


        //// Retrieve an entity. Returns entity XML.
        //// Return true on success, false if not found, throw exception on error.

        //public string GetEntity(string tableName, string partitionKey, string rowKey)
        //{
        //    return Retry<string>(delegate()
        //    {
        //        HttpWebRequest request;
        //        HttpWebResponse response;

        //        string entityXml = null;

        //        try
        //        {
        //            string resource = String.Format(tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);

        //            SortedList<string, string> headers = new SortedList<string, string>();
        //            headers.Add("If-Match", "*");

        //            request = CreateRESTRequest("GET", resource, null, headers);

        //            request.Accept = "application/atom+xml";

        //            response = request.GetResponse() as HttpWebResponse;

        //            if ((int)response.StatusCode == 200)
        //            {
        //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //                {
        //                    string result = reader.ReadToEnd();

        //                    XNamespace ns = "http://www.w3.org/2005/Atom";
        //                    XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        //                    XElement entry = XElement.Parse(result);

        //                    entityXml = entry.ToString();

        //                }
        //            }

        //            response.Close();

        //            return entityXml;
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


        //// Query entities. Returned entity list XML matching query filter.
        //// Return true on success, false if not found, throw exception on error.

        //public string QueryEntities(string tableName, string filter)
        //{
        //    return Retry<string>(delegate()
        //    {
        //        HttpWebRequest request;
        //        HttpWebResponse response;

        //        string entityXml = null;

        //        try
        //        {
        //            string resource = String.Format(tableName + "()?$filter=" + Uri.EscapeDataString(filter));

        //            request = CreateRESTRequest("GET", resource, null, null);
        //            request.Accept = "application/atom+xml,application/xml";

        //            response = request.GetResponse() as HttpWebResponse;

        //            if ((int)response.StatusCode == 200)
        //            {
        //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //                {
        //                    string result = reader.ReadToEnd();

        //                    XNamespace ns = "http://www.w3.org/2005/Atom";
        //                    XNamespace d = "http://schemas.microsoft.com/ado/2007/08/dataservices";

        //                    XElement entry = XElement.Parse(result);

        //                    entityXml = entry.ToString();
        //                }
        //            }

        //            response.Close();

        //            return entityXml;
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


        //// Replace Update entity. Completely replace previous entity with new entity.
        //// Return true on success, false if not found, throw exception on error

        //public bool ReplaceUpdateEntity(string tableName, string partitionKey, string rowKey, object obj)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebRequest request;
        //        HttpWebResponse response;

        //        try
        //        {
        //            string now = DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);

        //            // Create properties list. Use reflection to retrieve properties from the object.

        //            StringBuilder properties = new StringBuilder();
        //            properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", "PartitionKey", partitionKey));
        //            properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", "RowKey", rowKey));
        //            properties.Append(string.Format("<d:{0} m:type=\"Edm.DateTime\">{1}</d:{0}>\n", "Timestamp", now));

        //            Type t = obj.GetType();
        //            PropertyInfo[] pi = t.GetProperties();
        //            MethodInfo mi;
        //            foreach (PropertyInfo p in pi)
        //            {
        //                try
        //                {
        //                    mi = p.GetGetMethod();
        //                    properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", p.Name, mi.Invoke(obj, null).ToString()));
        //                }
        //                catch (NullReferenceException)
        //                {
        //                }
        //            }

        //            string id = String.Format("http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", StorageAccount, tableName, partitionKey, rowKey);

        //            string requestBody = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
        //                                  "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
        //                                  "       xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
        //                                  "       xmlns=\"http://www.w3.org/2005/Atom\"> " +
        //                                  "  <title /> " +
        //                                  "  <updated>{0}</updated> " +
        //                                  "  <author>" +
        //                                  "    <name/> " +
        //                                  "  </author> " +
        //                                  "  <id>{1}</id> " +
        //                                  "  <content type=\"application/xml\">" +
        //                                  "  <m:properties>" +
        //                                  "{2}" +
        //                                  "  </m:properties>" +
        //                                  "  </content> " +
        //                                  "</entry>",
        //                                  now,
        //                                  id,
        //                                  properties);

        //            string resource = String.Format(tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);

        //            SortedList<string, string> headers = new SortedList<string, string>();
        //            headers.Add("If-Match", "*");

        //            request = CreateRESTRequest("PUT", resource, requestBody, headers);

        //            request.Accept = "application/atom+xml";

        //            response = request.GetResponse() as HttpWebResponse;
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


        //// Merge update an entity (preserve previous properties not overwritten).
        //// Return true on success, false if not found, throw exception on error.

        //public bool MergeUpdateEntity(string tableName, string partitionKey, string rowKey, object obj)
        //{
        //    return Retry<bool>(delegate() 
        //    {
        //        HttpWebRequest request;
        //        HttpWebResponse response;

        //        try
        //        {
        //            string now = DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture);

        //            // Create properties list. Use reflection to retrieve properties from the object.

        //            StringBuilder properties = new StringBuilder();
        //            properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", "PartitionKey", partitionKey));
        //            properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", "RowKey", rowKey));
        //            properties.Append(string.Format("<d:{0} m:type=\"Edm.DateTime\">{1}</d:{0}>\n", "Timestamp", now));

        //            Type t = obj.GetType();
        //            PropertyInfo[] pi = t.GetProperties();
        //            MethodInfo mi;
        //            foreach (PropertyInfo p in pi)
        //            {
        //                try
        //                {
        //                    mi = p.GetGetMethod();
        //                    properties.Append(string.Format("<d:{0}>{1}</d:{0}>\n", p.Name, mi.Invoke(obj, null).ToString()));
        //                }
        //                catch (NullReferenceException)
        //                {
        //                }
        //            }

        //            string id = String.Format("http://{0}.table.core.windows.net/{1}(PartitionKey='{2}',RowKey='{3}')", StorageAccount, tableName, partitionKey, rowKey);

        //            string requestBody = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>" +
        //                                  "<entry xmlns:d=\"http://schemas.microsoft.com/ado/2007/08/dataservices\"" +
        //                                  "       xmlns:m=\"http://schemas.microsoft.com/ado/2007/08/dataservices/metadata\"" +
        //                                  "       xmlns=\"http://www.w3.org/2005/Atom\"> " +
        //                                  "  <title /> " +
        //                                  "  <updated>{0}</updated> " +
        //                                  "  <author>" +
        //                                  "    <name/> " +
        //                                  "  </author> " +
        //                                  "  <id>{1}</id> " +
        //                                  "  <content type=\"application/xml\">" +
        //                                  "  <m:properties>" +
        //                                  "{2}" +
        //                                  "  </m:properties>" +
        //                                  "  </content> " +
        //                                  "</entry>",
        //                                  now,
        //                                  id,
        //                                  properties);

        //            string resource = String.Format(tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);

        //            SortedList<string, string> headers = new SortedList<string, string>();
        //            headers.Add("If-Match", "*");

        //            request = CreateRESTRequest("MERGE", resource, requestBody, headers);

        //            request.Accept = "application/atom+xml";

        //            response = request.GetResponse() as HttpWebResponse;
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


        //// Delete entity.
        //// Return true on success, false if not found, throw exception on error.

        //public bool DeleteEntity(string tableName, string partitionKey, string rowKey)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebRequest request;
        //        HttpWebResponse response;

        //        try
        //        {
        //            string resource = String.Format(tableName + "(PartitionKey='{0}',RowKey='{1}')", partitionKey, rowKey);

        //            SortedList<string, string> headers = new SortedList<string, string>();
        //            headers.Add("If-Match", "*");

        //            request = CreateRESTRequest("DELETE", resource, null, headers);

        //            response = request.GetResponse() as HttpWebResponse;
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

    }
}
