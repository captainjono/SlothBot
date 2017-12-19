using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace SlothBot.Bridge
{
    /// <summary>
    /// Mirrors an in-memory dictionary with a peristant JSON file, so reads are fast, writes are slow.
    /// You will see performance degrade for exponentially as the dictionary grows.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class ReadOptimisedJsonFilePersistantDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TValue : new()
    {
        private readonly string _peristantStoreFullname;
        private readonly IDictionary<TKey, TValue> _inMemoryStore;

        public ReadOptimisedJsonFilePersistantDictionary(string peristantStoreFullname, Func<TValue, TKey> keyFromValue)
        {
            _peristantStoreFullname = peristantStoreFullname;
            _inMemoryStore = RetreiveFromFile(peristantStoreFullname, keyFromValue);
        }

        public static IDictionary<TKey, TValue> From(string filename, Func<TValue, TKey> keyFromValue)
        {
            return new ReadOptimisedJsonFilePersistantDictionary<TKey, TValue>(filename, keyFromValue);
        }
        
        private void SaveToFile()
        {
            File.WriteAllText(_peristantStoreFullname, _inMemoryStore.Values.ToJson());
        }

        public static IDictionary<TKey, TValue> RetreiveFromFile(string filename, Func<TValue, TKey> keyFromValue)
        {
            if (!File.Exists(filename)) File.Create(filename).Dispose();

            var contents = File.ReadAllText(filename);
            return (contents.FromJson<TValue[]>() ?? new TValue[] {})
                               .Select(v => new KeyValuePair<TKey, TValue>(keyFromValue(v), v))
                               .ToDictionary(k => k.Key, v => v.Value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _inMemoryStore.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _inMemoryStore.Add(item);
            SaveToFile();
        }

        public void Clear()
        {
            _inMemoryStore.Clear();
            SaveToFile();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _inMemoryStore.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _inMemoryStore.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var reuslt = _inMemoryStore.Remove(item);
            if (reuslt) SaveToFile();

            return reuslt;
        }

        public int Count => _inMemoryStore.Count;
        public bool IsReadOnly => _inMemoryStore.IsReadOnly;
        public void Add(TKey key, TValue value)
        {
            _inMemoryStore.Add(key, value);
            SaveToFile();
        }

        public bool ContainsKey(TKey key)
        {
            return _inMemoryStore.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            var result = _inMemoryStore.Remove(key);
            if (result) SaveToFile();
            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _inMemoryStore.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get { return _inMemoryStore[key]; }
            set
            {
                _inMemoryStore[key] = value;
                SaveToFile();
            }
        }

        public ICollection<TKey> Keys => _inMemoryStore.Keys;
        public ICollection<TValue> Values => _inMemoryStore.Values;
    }
}
