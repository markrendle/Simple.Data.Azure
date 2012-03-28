namespace Simple.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Helpers;
#if(SILVERLIGHT)
    using TraceOrDebug = System.Diagnostics.Debug;
#else
    using TraceOrDebug = System.Diagnostics.Trace;
#endif

    public class TableService
    {
        private readonly AzureHelper _azureHelper;

        public TableService(AzureHelper azureHelper)
        {
            _azureHelper = azureHelper;
        }
#if(!SILVERLIGHT)
        public IEnumerable<string> ListTables()
        {
            var request = _azureHelper.CreateTableRequest("tables", RestVerbs.GET);

            IEnumerable<string> list;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Trace.WriteLine(response.StatusCode, "HttpResponse");
                list = TableHelper.ReadTableList(response.GetResponseStream()).ToList();
            }

            return list;
        }
#endif

        public Task<IEnumerable<string>> ListTablesAsync()
        {
            var request = _azureHelper.CreateTableRequest("tables", RestVerbs.GET);

            Func<IAsyncResult, IEnumerable<string>> endMethod = result =>
                                 {
                                     var response = (HttpWebResponse) request.EndGetResponse(result);
                                     TraceOrDebug.WriteLine(response.StatusCode);
                                     return TableHelper.ReadTableList(response.GetResponseStream());
                                 };

            return Task.Factory.FromAsync(request.BeginGetResponse, endMethod, null);
        }

        public void CreateTable(string tableName)
        {
            var dict = new Dictionary<string, object> { { "TableName", tableName } };
            var data = DataServicesHelper.CreateDataElement(dict);

            DoRequest(data, "tables", RestVerbs.POST);
        }

#if(SILVERLIGHT)
        private void DoRequest(XElement element, string command, string method)
        {
            var request = _azureHelper.CreateTableRequest(command, method, element.ToString());

            request.BeginGetResponse(ar =>
                                         {
                                             var response = (HttpWebResponse)request.EndGetResponse(ar);
                                             Debug.WriteLine(response.StatusCode);
                                         }, null);
        }
#else
        private void DoRequest(XElement element, string command, string method)
        {
            var request = _azureHelper.CreateTableRequest(command, method, element.ToString());

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                Trace.WriteLine(response.StatusCode);
            }
        }
#endif
    }
}
