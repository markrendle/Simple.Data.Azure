namespace Simple.Data.Azure.IntegrationTest
{
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;

    public class TestBase
    {
        protected TestBase(bool reset)
        {
            var tableClient = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient();
            tableClient.DeleteTableIfExist("SimpleTest");
            tableClient.CreateTable("SimpleTest");

            var mark = new SimpleTest {PartitionKey = "1", RowKey = string.Empty, Name = "Mark", Age = 25};
            var context = tableClient.GetDataServiceContext();
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
    }

    public class SimpleTest : TableServiceEntity
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}