namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class DeleteByTest : TestBase
    {
        public DeleteByTest()
            : base(true)
        {
            AddTestRecord("1", string.Empty, "Mark", 25);
            AddTestRecord("2", "X", "Steve", 0);
            AddTestRecord("2", "Y", "Bob", 0);
            AddTestRecord("3", "X", "Alice", 0);
            AddTestRecord("4", "P", "Alice", 0);
        }

        [Fact]
        public void DeleteByPartitionKeyDeletesRecord()
        {
            int count = Db.SimpleTest.DeleteByPartitionKey(PartitionKey: "1");
            Assert.Equal(1, count);

            AssertDeleted("1", string.Empty);
        }

        [Fact]
        public void DeleteByPartitionKeyAndRowKeyDeletesRecord()
        {
            int count = Db.SimpleTest.DeleteByPartitionKeyAndRowKey(PartitionKey: "2", RowKey: "X");
            Assert.Equal(1, count);

            AssertDeleted("2", "X");
            AssertNotDeleted("2", "Y");
        }

        [Fact]
        public void DeleteByNonKeyFieldDeletesRecords()
        {
            int count = Db.SimpleTest.DeleteByName(Name: "Alice");
            Assert.Equal(2, count);

            AssertDeleted("3", "X");
            AssertDeleted("4", "P");
        }

        private void AssertDeleted(string partitionKey, string rowKey)
        {
            var record = Db.SimpleTest.Get(partitionKey, rowKey);
            Assert.Null(record);
        }

        private void AssertNotDeleted(string partitionKey, string rowKey)
        {
            var record = Db.SimpleTest.Get(partitionKey, rowKey);
            Assert.NotNull(record);
        }
    }
}