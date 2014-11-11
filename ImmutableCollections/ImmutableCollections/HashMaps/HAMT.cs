using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.HashMaps
{
    struct HAMT<K, V>
    {
        // TODO:
        //   optimize copying
        //   remove depth parameter
        //   try inlining bit calculations
        interface IHashMap
        {
            bool TryGetValue(K key, int hash, int depth, out V value);
            void Update(K key, int hash, int depth, V value);
            void Init2(K key1, V val1, int hash1, K key2, V val2, int hash2, int depth);
        }

        struct TrieNode<Child> : IHashMap where Child : IHashMap, new()
        {
            int childrenbitmap;
            int entriesbitmap;

            Child[] children;
            KeyValuePair<K, V>[] entries;

            public bool TryGetValue(K key, int hash, int depth, out V value)
            {
                int bit = ComputeBit(hash, depth);
                if ((bit & childrenbitmap) != 0)
                {
                    return children[ComputeIndex(bit, childrenbitmap)].TryGetValue(key, hash, depth + 5, out value);
                }
                else if ((bit & entriesbitmap) != 0)
                {
                    var kv = entries[ComputeIndex(bit, entriesbitmap)];
                    if (eq(kv.Key, key))
                    {
                        value = kv.Value;
                        return true;
                    }
                }
                value = default(V);
                return false;
            }

            public void Update(K key, int hash, int depth, V value)
            {
                int bit = ComputeBit(hash, depth);
                if ((bit & childrenbitmap) != 0)
                {
                    var tmp = new Child[children.Length];
                    Array.Copy(children, tmp, children.Length);
                    children = tmp;
                    children[ComputeIndex(bit, childrenbitmap)].Update(key, hash, depth + 5, value);
                }
                else if ((bit & entriesbitmap) != 0)
                {
                    int i = ComputeIndex(bit, entriesbitmap);
                    if (eq(entries[i].Key,key)) entries[i] = new KeyValuePair<K,V>(key,value);
                    else
                    {
                        int j = 0;
                        if (children == null) children = new Child[1];
                        else
                        {
                            j = ComputeIndex(bit, childrenbitmap);
                            children = children.Insert(j, new Child());
                        }
                        children[j].Init2(key, value, hash, entries[i].Key, entries[i].Value, entries[i].Key.GetHashCode(), depth + 5);
                        entries = entries.Remove(i);
                        childrenbitmap = childrenbitmap | bit;
                        entriesbitmap = entriesbitmap & ~bit;
                    }
                }
                else
                {
                    if (entries == null) entries = new[] { new KeyValuePair<K, V>(key, value) };
                    else entries = entries.Insert(ComputeIndex(bit, entriesbitmap), new KeyValuePair<K, V>(key, value));
                    entriesbitmap = entriesbitmap | bit;
                }
            }

            public void Init2(K key1, V val1, int hash1, K key2, V val2, int hash2, int depth)
            {
                var bit1 = ComputeBit(hash1, depth);
                var bit2 = ComputeBit(hash2, depth);
                if(bit1 == bit2)
                {
                    childrenbitmap = bit1;
                    children = new Child[1];
                    children[0].Init2(key1, val2, hash1, key2, val2, hash2, depth + 5);
                }else
                {
                    entriesbitmap = bit1 | bit2;
                    if (bit1 < bit2) entries = new[] { new KeyValuePair<K, V>(key1, val1), new KeyValuePair<K, V>(key2, val2) };
                    else entries = new[] { new KeyValuePair<K, V>(key2, val2), new KeyValuePair<K, V>(key1, val1) };
                }
            }

            static int ComputeBit(int hash, int depth)
            {
                return 1 << ((hash >> depth) & 0x01F);
            }

            static int ComputeIndex(int bit, int bitmap)
            {
                return BitCount(bitmap & (bit - 1));
            }

            // blatantly stolen
            static int BitCount(int value)
            {
                value = value - ((value >> 1) & 0x55555555);                    // reuse input as temporary
                value = (value & 0x33333333) + ((value >> 2) & 0x33333333);     // temp
                value = ((value + (value >> 4) & 0xF0F0F0F) * 0x1010101) >> 24; // count
                return value;
            }

            static Func<K, K, bool> eq = EqualityComparer<K>.Default.Equals;
        }

        struct Bucket : IHashMap
        {
            KeyValuePair<K, V>[] entries;

            static KeyValuePair<K, V>[] emptyentries = new KeyValuePair<K, V>[0];

            public void Init2(K key1, V val1, int hash1, K key2, V val2, int hash2, int depth)
            {
                entries = new[] { new KeyValuePair<K, V>(key1, val1), new KeyValuePair<K, V>(key2, val2) };
            }

            public bool TryGetValue(K key, int hash, int depth, out V value)
            {
                foreach (var kv in entries)
                {
                    if (eq(kv.Key, key))
                    {
                        value = kv.Value;
                        return true;
                    }
                }
                value = default(V);
                return false;
            }

            public void Update(K key, int hash, int depth, V value)
            {
                // inefficient: makes double copy
                var list = new List<KeyValuePair<K, V>>();
                foreach (var kv in entries)
                {
                    if (eq(kv.Key, key))
                    {
                        list.Add(new KeyValuePair<K, V>(key, value));
                    }
                    else
                    {
                        list.Add(kv);
                    }
                }
                entries = list.ToArray();
            }

            static Func<K, K, bool> eq = EqualityComparer<K>.Default.Equals;
        }

        public struct TrieMap
        {
            TrieNode<TrieNode<TrieNode<TrieNode<TrieNode<TrieNode<TrieNode<Bucket>>>>>>> trie;

            public bool TryGetValue(K key, out V value)
            {
                return trie.TryGetValue(key, key.GetHashCode(), 0, out value);
            }

            public TrieMap Set(K key, V value)
            {
                var self = this;
                self.trie.Update(key, key.GetHashCode(), 0, value);
                return self;
            }
        }

        public static TrieMap Empty = new TrieMap();
    }

    public struct Test
    {
        public static void Run()
        {
            var h = HAMT<int, int>.Empty;

            for (int i = 0; i < 1000000; i++) h = h.Set(i, i);

            for (int i = 0; i < 500000; i++)
            {
                h = h.Set(i, -i);
            }

            for (int i = 0; i < 1000000; i++)
            {
                int v;
                h.TryGetValue(i, out v);
                if ((i < 500000 && v != -i) || (i >= 500000 && v != i)) Console.WriteLine("Error: {0} returned {1}", i, v);
            }

            for (int i = 1000001; i < 2000000; i++)
            {
                int v;
                if (h.TryGetValue(i, out v)) Console.WriteLine("Error: {0} shouldn't be present", i);
            }

            Console.WriteLine("End");
        }
    }
}
