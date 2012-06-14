using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Azure.Helpers;
using Xunit;

namespace Simple.Azure.Test
{
    public class TableServiceTest
    {
        [Fact]
        public void ListsTables()
        {
            var helper = new AzureHelper
                             {
                                 Account = "azuredecktest",
                                 SharedKey =
                                     "NXqAP07hSjgGiTlyCCcMoAYt4+NNd3qGT45HFgqOK2bqL4my1QFuGjVVa4NEQ8hXjLJEA0BERl8tNpPwEBZRng=="
                             };
            var tableService = new TableService(helper);
            var listTask = tableService.ListTablesAsync();
            listTask.Wait();
            Assert.False(listTask.IsFaulted);
            var list = listTask.Result.ToList();
            Assert.Equal(3, list.Count);
            Assert.Contains("foo", list);
            Assert.Contains("bar", list);
            Assert.Contains("quux", list);
        }
    }
}
