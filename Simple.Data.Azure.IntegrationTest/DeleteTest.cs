namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class DeleteTest : TestBase
    {
        public DeleteTest()
            : base(true)
        {
            AddTestRecord("1", string.Empty, "Mark", 25);
            AddTestRecord("2", "X", "Steve", 0);
            AddTestRecord("2", "Y", "Bob", 0);
            AddTestRecord("3", "X", "Alice", 0);
            AddTestRecord("4", "P", "Alice", 0);
        }

        [Fact]
        public void DeleteRecordDeletesRecord()
        {
            var record = Db.SimpleTest.Get("1");
            int count = Db.SimpleTest.Delete(record);
            Assert.Equal(1, count);

            AssertDeleted("1", string.Empty);
        }

        [Fact]
        public void DeleteRecordDoesNotDeleteOtherRecord()
        {
            var record = Db.SimpleTest.Get("2", "X");
            int count = Db.SimpleTest.Delete(record);
            Assert.Equal(1, count);

            AssertDeleted("2", "X");
            AssertNotDeleted("2", "Y");
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