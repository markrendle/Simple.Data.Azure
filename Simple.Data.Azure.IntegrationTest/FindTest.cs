using System;
using System.Linq;
using System.Text;

namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class FindTest : TestBase
    {
        public FindTest() : base(true)
        {
            AddTestRecord("1", string.Empty, "Mark", 25);
        }

        [Fact]
        public void SimpleFindTest()
        {
            var mark = Db.SimpleTest.FindByPartitionKey("1");
            Assert.NotNull(mark);
            Assert.Equal("Mark", mark.Name);
            Assert.Equal(25, mark.Age);
        }
    }
}
