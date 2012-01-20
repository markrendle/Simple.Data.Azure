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
        public void InsertWorksWithADictionary()
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

        [Fact]
        public void InsertWorksWithAnObject()
        {
            var bob = new SimpleTest
                          {
                              PartitionKey = "1235",
                              RowKey = string.Empty,
                              Name = "Bob",
                              Age = 42
                          };

            var actual = Db.SimpleTest.Insert(bob);

            Assert.NotNull(actual);
            Assert.Equal(bob.PartitionKey, actual.PartitionKey);
            Assert.Equal(bob.RowKey, actual.RowKey);
            Assert.Equal(bob.Name, actual.Name);
            Assert.Equal(bob.Age, actual.Age);
        }

        [Fact]
        public void InsertWorksWithNamedParameters()
        {
            const string partitionKey = "1236";
            const string rowKey = "";
            const string name = "Chuck";
            const int age = 20;

            var actual = Db.SimpleTest.Insert(PartitionKey: partitionKey, RowKey: rowKey, Name: name, Age: age);

            Assert.NotNull(actual);
            Assert.Equal(partitionKey, actual.PartitionKey);
            Assert.Equal(rowKey, actual.RowKey);
            Assert.Equal(name, actual.Name);
            Assert.Equal(age, actual.Age);
        }
    }
}