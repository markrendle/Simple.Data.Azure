namespace Simple.Data.Azure
{
    static class ObjectEx
    {
        public static string ToStringOrEmpty(this object obj)
        {
            return ReferenceEquals(obj, null) ? string.Empty : obj.ToString();
        }

        public static string ToStringOrNull(this object obj)
        {
            return ReferenceEquals(obj, null) ? null : obj.ToString();
        }
    }
}