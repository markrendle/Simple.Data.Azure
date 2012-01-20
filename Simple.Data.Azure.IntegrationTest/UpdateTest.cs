namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class UpdateTest : TestBase
    {
        public UpdateTest()
            : base(true)
        {
            AddTestRecord("1", string.Empty, "Mark", 25);
        }

        [Fact]
        public void UpdateUpdatesRecord()
        {
            var record = Db.SimpleTest.Get("1");
            record.Age = 38;
            int count = Db.SimpleTest.Update(record);
            Assert.Equal(1, count);

            AssertRecord("1", string.Empty, "Mark", 38);
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