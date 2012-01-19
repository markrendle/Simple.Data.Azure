namespace Simple.Data.Azure.IntegrationTest
{
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public abstract class TestBase
    {
        protected TestBase(bool reset)
        {
            _tableClient = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
            _tableClient.DeleteTableIfExist("SimpleTest");
            _tableClient.CreateTable("SimpleTest");

        }

        protected void AddTestRecord(string partitionKey, string rowKey, string name, int age)
        {
            var mark = new SimpleTest {PartitionKey = partitionKey, RowKey = rowKey, Name = name, Age = age};
            var context = _tableClient.GetDataServiceContext();
            context.AddObject("SimpleTest", mark);
            context.SaveChanges();
        }

        protected readonly dynamic Db =
            Database.Opener.Open("Azure",
                                 new
                                     {
                                         Account = "devstoreaccount1",
                                         Url = "http://127.0.0.1:10002/devstoreaccount1/",
                                         Key = "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="
                                     });

        private CloudTableClient _tableClient;
    }

    public class SimpleTest : TableServiceEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}