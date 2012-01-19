namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class UpdateByTest : TestBase
    {
        public UpdateByTest() : base(true)
        {
            AddTestRecord("1", string.Empty, "Mark", 25);
            AddTestRecord("2", "X", "Steve", 0);
            AddTestRecord("2", "Y", "Bob", 0);
            AddTestRecord("3", "X", "Alice", 0);
            AddTestRecord("4", "P", "Alice", 0);
        }

        [Fact]
        public void UpdateByPartitionKeyUpdatesRecord()
        {
            int count = Db.SimpleTest.UpdateByPartitionKey(PartitionKey: "1", Age: 38);
            Assert.Equal(1, count);

            AssertRecord("1", string.Empty, "Mark", 38);
        }
        
        [Fact]
        public void UpdateByPartitionKeyAndRowKeyUpdatesRecord()
        {
            int count = Db.SimpleTest.UpdateByPartitionKeyAndRowKey(PartitionKey: "2", RowKey: "X", Age: 4);
            Assert.Equal(1, count);

            AssertRecord("2", "X", "Steve", 4);
            AssertRecord("2", "Y", "Bob", 0);
        }

        [Fact]
        public void UpdateByNonKeyFieldUpdatesRecords()
        {
            int count = Db.SimpleTest.UpdateByName(Name: "Alice", Age: 21);
            Assert.Equal(2, count);

            AssertRecord("3", "X", "Alice", 21);
            AssertRecord("4", "P", "Alice", 21);
        }

        private void AssertRecord(string partitionKey, string rowKey, string expectedName, int expectedAge)
        {
            var steve = Db.SimpleTest.Get(partitionKey, rowKey);
            Assert.NotNull(steve);
            Assert.Equal(expectedName, steve.Name);
            Assert.Equal(expectedAge, steve.Age);
        }
    }
}