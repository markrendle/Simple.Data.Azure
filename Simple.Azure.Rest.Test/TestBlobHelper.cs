using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Azure.Rest.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class TestBlobHelper
    {
        [Test]
        public void ListsContainers()
        {
            var blobHelper = new BlobHelper();

            var listTask = blobHelper.ListContainers2();
            listTask.Wait();

            Assert.False(listTask.IsFaulted);
            Assert.True(listTask.Result.Any(c => c.Name.Equals("public", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
