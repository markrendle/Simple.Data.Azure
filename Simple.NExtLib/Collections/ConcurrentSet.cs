using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.NExtLib.Collections
{
    using System.Collections;
    using System.Collections.Concurrent;

    public class ConcurrentSet<T> : IEnumerable<T>
    {
        private readonly ConcurrentDictionary<T, int> _internal;

        public ConcurrentSet()
        {
            _internal = new ConcurrentDictionary<T, int>();
        }

        public ConcurrentSet(IEnumerable<T> items)
        {
            _internal = new ConcurrentDictionary<T, int>(items.Select(x => new KeyValuePair<T, int>(x, 0)));
        }

        public bool Add(T item)
        {
            return _internal.TryAdd(item, 0);
        }

        public bool Remove(T item)
        {
            int _;
            return _internal.TryRemove(item, out _);
        }

        public int Count
        {
            get { return _internal.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _internal.Keys.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
