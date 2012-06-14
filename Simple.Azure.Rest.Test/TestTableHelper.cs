namespace Simple.Azure.Rest.Test
{
    using Xunit;

    public class TestTableHelper
    {
        [Fact]
        public void ListsTables()
        {
            var helper = new TableHelper();
            var task = helper.ListTables();
            task.Wait();
            System.Diagnostics.Debug.WriteLine("AAAGH");
            Assert.False(task.IsFaulted);
            Assert.Equal(9, task.Result.Count);
        }
    }
}