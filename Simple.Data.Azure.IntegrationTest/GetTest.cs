namespace Simple.Data.Azure.IntegrationTest
{
    using Xunit;

    public class GetTest : TestBase
    {
        public GetTest() : base(true)
        {
        }

        [Fact]
        public void SimpleGetTest()
        {
            var mark = Db.SimpleTest.Get("1");
            Assert.NotNull(mark);
            Assert.Equal("Mark", mark.Name);
            Assert.Equal(25, mark.Age);
        }
    }
}