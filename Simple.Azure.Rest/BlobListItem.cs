namespace Simple.Azure.Rest
{
    public class BlobListItem
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string FullPath { get; set; }
        public string LastModified { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
        public string LeaseStatus { get; set; }
        public string Etag { get; set; }
    }
}