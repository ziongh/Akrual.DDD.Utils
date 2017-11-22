using System.Collections;
using System.Collections.Generic;
using Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer;

namespace Akrual.DDD.Utils.Domain.Utils.Collections
{
    /// <summary>
    ///     A list with equality based on its content and not on the list's reference 
    ///     (i.e.: 2 different instances containing the same items in the same order will be equals).
    /// </summary>
    /// <remarks>This type is not thread-safe (for hashcode updates).</remarks>
    /// <typeparam name="T">Type of the listed items.</typeparam>
    public class ListByValue<T> : EquatableByValue<ListByValue<T>>, IList<T>
    {
        private readonly IList<T> list;

        public ListByValue() : this(new List<T>())
        {
        }

        public ListByValue(IList<T> list)
        {
            this.list = list;
        }

        public int Count => this.list.Count;

        public bool IsReadOnly => ((ICollection<T>)this.list).IsReadOnly;

        public T this[int index]
        {
            get { return this.list[index]; }
            set
            {
                this.ResetHashCode();
                this.list[index] = value;
            }
        }

        public void Add(T item)
        {
            this.ResetHashCode();
            this.list.Add(item);
        }

        public void Clear()
        {
            this.ResetHashCode();
            this.list.Clear();
        }

        public bool Contains(T item)
        {
            return this.list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return this.list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            this.ResetHashCode();
            this.list.Insert(index, item);
        }

        public bool Remove(T item)
        {
            this.ResetHashCode();
            return this.list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.ResetHashCode();
            this.list.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.list).GetEnumerator();
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return (IEnumerable<object>)this.list;
        }
    }
}
