namespace Simple.Azure
{
    public class Entity
    {
        public string[] Names { get; set; }
        public object[] Values { get; set; }
        public string[] Types { get; set; }
    }

    internal class EntityValue
    {
        private readonly string _name;
        private readonly object _value;
        private readonly string _type;

        public EntityValue(string name, object value, string type)
        {
            _name = name;
            _value = value;
            _type = type;
        }

        public string Type
        {
            get { return _type; }
        }

        public object Value
        {
            get { return _value; }
        }

        public string Name
        {
            get { return _name; }
        }
    }
}