namespace Simple.Data.Azure.IntegrationTest
{
    using System.Collections.Generic;
    using Xunit;

    public class InsertTest : TestBase
    {
        public InsertTest() : base(true)
        {
        }

        [Fact]
        public void SimpleInsertTest()
        {
            var alice = new Dictionary<string, object>
                            {
                                {"PartitionKey", "1234"},
                                {"RowKey", string.Empty},
                                {"Name", "Alice"},
                                {"Age", 32}
                            };

            var actual = Db.SimpleTest.Insert(alice);

            Assert.NotNull(actual);
            Assert.Equal("Alice", actual.Name);
            Assert.Equal(32, actual.Age);
        }
    }
}