using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Akrual.DDD.Utils.Domain.Utils.Collections.EquallityComparer;

namespace Akrual.DDD.Utils.Domain.Utils.Collections
{
    /// <summary>
    ///     A dictionary with equality based on its content and not on the dictionary's reference 
    ///     (i.e.: 2 different instances containing the same entries will be equals).
    /// </summary>
    /// <remarks>This type is not thread-safe (for hashcode updates).</remarks>
    /// <typeparam name="K">The type of keys in the dictionary.</typeparam>
    /// <typeparam name="V">The type of values in the dictionary.</typeparam>
    public class DictionaryByValue<K, V> : EquatableByValueWithoutOrder<DictionaryByValue<K, V>>, IDictionary<K, V>
    {
        private readonly IDictionary<K, V> dictionary;

        public DictionaryByValue(IDictionary<K, V> dictionary)
        {
            this.dictionary = dictionary;
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            foreach (var kv in dictionary)
            {
                yield return kv;
            }
        }

        protected override bool EqualsWithoutOrderImpl(EquatableByValueWithoutOrder<DictionaryByValue<K, V>> obj)
        {
            var other = obj as DictionaryByValue<K, V>;
            if (other == null)
            {
                return false;
            }

            return !this.dictionary.Except(other).Any();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        public void Add(KeyValuePair<K, V> item)
        {
            base.ResetHashCode();
            dictionary.Add(item);
        }

        public void Clear()
        {
            base.ResetHashCode();
            dictionary.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return dictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            dictionary.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            base.ResetHashCode();
            return dictionary.Remove(item);
        }

        public int Count => dictionary.Count;

        public bool IsReadOnly => dictionary.IsReadOnly;

        public bool ContainsKey(K key)
        {
            return dictionary.ContainsKey(key);
        }

        public void Add(K key, V value)
        {
            base.ResetHashCode();
            dictionary.Add(key, value);
        }

        public bool Remove(K key)
        {
            base.ResetHashCode();
            return dictionary.Remove(key);
        }

        public bool TryGetValue(K key, out V value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public V this[K key]
        {
            get => dictionary[key];
            set
            {
                base.ResetHashCode();
                dictionary[key] = value;
            }
        }

        public ICollection<K> Keys => dictionary.Keys;

        public ICollection<V> Values => dictionary.Values;
    }
}
