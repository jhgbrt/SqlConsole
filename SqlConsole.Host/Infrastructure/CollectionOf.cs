using System.Collections;
using System.Collections.Generic;

namespace SqlConsole.Host
{
    class CollectionOf<T> : IEnumerable<T>
    {
        protected readonly List<T> Items = new List<T>();

        public IEnumerator<T> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item) => Items.Add(item);
    }
}