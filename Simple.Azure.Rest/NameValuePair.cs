namespace Simple.Azure.Rest
{
    public class NameValuePair
    {
        public NameValuePair()
        {
            
        }
        public NameValuePair(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}