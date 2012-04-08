namespace Simple.Azure.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Net;
    using System.Xml.Linq;

    public class QueueHelper : RESTHelper
    {
        // Constructor.

        public QueueHelper(string storageAccount, string storageKey) : base("http://" + storageAccount + ".queue.core.windows.net/", storageAccount, storageKey)
        {
        }

        public QueueHelper()
            : base("http://127.0.0.1:10001/devstoreaccount1/", "devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==")
        {
        }

        // List queues.
        // Return true on success, false if not found, throw exception on error.

        //public List<string> ListQueues()
        //{
        //    return Retry<List<string>>(delegate()
        //    {
        //        HttpWebResponse response;

        //        List<string> queues = new List<string>();

        //        try
        //        {
        //            response = CreateRESTRequest("GET", "?comp=list").GetResponse() as HttpWebResponse;

        //            if ((int)response.StatusCode == 200)
        //            {
        //                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //                {
        //                    string result = reader.ReadToEnd();

        //                    XElement x = XElement.Parse(result);
        //                    foreach (XElement queue in x.Element("Queues").Elements("Queue"))
        //                    {
        //                        queues.Add(queue.Element("Name").Value);
        //                    }
        //                }
        //            }

        //            response.Close();

        //            return queues;
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


        //// Create a queue. 
        //// Return true on success, false if already exists, throw exception on error.

        //public bool CreateQueue(string queue)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            response = CreateRESTRequest("PUT", queue).GetResponse() as HttpWebResponse;
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


        //// Delete a queue. 
        //// Return true on success, false if not found, throw exception on error.

        //public bool DeleteQueue(string queue)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            response = CreateRESTRequest("DELETE", queue).GetResponse() as HttpWebResponse;
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


        //// Get queue metadata.
        //// Return true on success, false if not found, throw exception on error.

        //public SortedList<string, string> GetQueueMetadata(string queue)
        //{
        //    return Retry<SortedList<string, string>>(delegate()
        //    {
        //        HttpWebResponse response;

        //        SortedList<string, string> metadataList = new SortedList<string, string>();

        //        try
        //        {
        //            response = CreateRESTRequest("HEAD", queue + "?comp=metadata", string.Empty, metadataList).GetResponse() as HttpWebResponse;
        //            response.Close();

        //            if ((int)response.StatusCode == 200)
        //            {
        //                if (response.Headers != null)
        //                {
        //                    for (int i = 0; i < response.Headers.Count; i++)
        //                    {
        //                        if (response.Headers.Keys[i].StartsWith("x-ms-meta-"))
        //                        {
        //                            metadataList.Add(response.Headers.Keys[i], response.Headers[i]);
        //                        }
        //                    }
        //                }
        //            }

        //            return metadataList;
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


        //// Set queue metadata.
        //// Return true on success, false if not found, throw exception on error.

        //public SortedList<string, string> SetQueueMetadata(string queue)
        //{
        //    return Retry<SortedList<string, string>>(delegate()
        //    {
        //        HttpWebResponse response;

        //        SortedList<string, string> metadataList = new SortedList<string,string>();

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

        //            response = CreateRESTRequest("PUT", queue + "?comp=metadata", string.Empty, headers).GetResponse() as HttpWebResponse;
        //            response.Close();

        //            return metadataList;
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


        //// Peek the next message from a queue. 
        //// Return true on success, false if not found, throw exception on error.

        //public string PeekMessage(string queue)
        //{
        //    return Retry<string>(delegate()
        //    {
        //        HttpWebResponse response;

        //        string message = null;

        //        try
        //        {
        //            response = CreateRESTRequest("GET", queue + "/messages?peekonly=true").GetResponse() as HttpWebResponse;

        //            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //            {
        //                message = reader.ReadToEnd();
        //            }

        //            response.Close();
        //            return message;
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


        //// Retrieve the next message from a queue. 
        //// Return true on success (message available), false if no message or no queue, throw exception on error.
        //// TODO: modify for retries.

        //public bool GetMessage(string queue, out string messageBody, out string messageId, out string popReceipt)
        //{
        //    HttpWebResponse response;
        //    string result;

        //    messageBody = null;
        //    messageId = null;
        //    popReceipt = null;

        //    try
        //    {
        //        response = CreateRESTRequest("GET", queue + "/messages").GetResponse() as HttpWebResponse;

        //        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //        {
        //            result = reader.ReadToEnd();
        //            response.Close();
        //        }

        //        XElement x = XElement.Parse(result);
        //        if (x.Elements("QueueMessage").Count() > 0)
        //        {
        //            XElement queueMessageElement = x.Element("QueueMessage");
                    
        //            messageId = queueMessageElement.Element("MessageId").Value;
                    
        //            popReceipt = queueMessageElement.Element("PopReceipt").Value;

        //            string messageBody64 = queueMessageElement.Element("MessageText").Value;
        //            byte[] messsageBodyBytes = Convert.FromBase64String(messageBody64);
        //            messageBody = new UTF8Encoding().GetString(messsageBodyBytes);

        //            return true;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        if (ex.Status == WebExceptionStatus.ProtocolError &&
        //            ex.Response != null &&
        //            (int)(ex.Response as HttpWebResponse).StatusCode == 409)
        //            return false;

        //        throw;
        //    }
        //}


        //// Create or update a blob. 
        //// Return true on success, false if already exists, throw exception on error.

        //public bool PutMessage(string queue, string messageBody)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            byte[] messageBodyBytes = new UTF8Encoding().GetBytes(messageBody);
        //            string messageBodyBase64 = Convert.ToBase64String(messageBodyBytes);
        //            string message = "<QueueMessage><MessageText>" + messageBodyBase64 + "</MessageText></QueueMessage>";
        //            response = CreateRESTRequest("POST", queue + "/messages", message, null).GetResponse() as HttpWebResponse;
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


        //// Clear all messages from a queue. 
        //// Return true on success, false if already exists, throw exception on error.

        //public bool ClearMessages(string queue)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            response = CreateRESTRequest("DELETE", queue + "/messages").GetResponse() as HttpWebResponse;
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


        //// Delete a previously read message. 
        //// Return true on success, false if already exists, throw exception on error.

        //public bool DeleteMessage(string queue, string messageId, string popReceipt)
        //{
        //    return Retry<bool>(delegate()
        //    {
        //        HttpWebResponse response;

        //        try
        //        {
        //            response = CreateRESTRequest("DELETE", queue + "/messages/" + messageId + "?popreceipt=" + Uri.EscapeDataString(popReceipt)).GetResponse() as HttpWebResponse;
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
