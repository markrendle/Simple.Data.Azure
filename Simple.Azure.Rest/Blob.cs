namespace Simple.Azure.Rest
{
    using System.IO;

    public class Blob
    {
        public string ContentType { get; set; }
        public Stream Stream { get; set; }
    }
}