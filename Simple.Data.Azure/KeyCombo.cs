namespace Simple.Data.Azure
{
    using System;
    using System.Collections.Generic;

    struct KeyCombo : IEquatable<KeyCombo>
    {
        private readonly string _partitionKey;
        private readonly string _rowKey;
        private static KeyCombo _empty = default(KeyCombo);

        public static KeyCombo Empty
        {
            get { return _empty; }
        }

        public static KeyCombo FromDictionary(IDictionary<string,object> source)
        {
            return new KeyCombo(source["PartitionKey"].ToStringOrNull(), source["RowKey"].ToStringOrNull());
        }

        public KeyCombo(string partitionKey) : this(partitionKey, partitionKey == null ? null : string.Empty)
        {
        }

        public KeyCombo(string partitionKey, string rowKey) : this()
        {
            _partitionKey = partitionKey;
            _rowKey = rowKey;
        }

        public string RowKey
        {
            get { return _rowKey; }
        }

        public string PartitionKey
        {
            get { return _partitionKey; }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(KeyCombo other)
        {
            return Equals(other._partitionKey, _partitionKey) && Equals(other._rowKey, _rowKey);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        /// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (KeyCombo)) return false;
            return Equals((KeyCombo) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_partitionKey != null ? _partitionKey.GetHashCode() : 0)*397) ^ (_rowKey != null ? _rowKey.GetHashCode() : 0);
            }
        }

        public static bool operator ==(KeyCombo left, KeyCombo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(KeyCombo left, KeyCombo right)
        {
            return !left.Equals(right);
        }
    }
}