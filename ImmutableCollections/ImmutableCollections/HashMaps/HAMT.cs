using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.HashMaps
{
    struct HAMT<K, V>
    {
        interface IHashMap
        {
            bool TryGetValue(K key, int hash, int depth, out V value);
            void Update(K key, int hash, int depth, V value);
            void Init();
        }

        struct TrieNode<Child> : IHashMap where Child : IHashMap, new()
        {
            int childrenbitmap;
            int entriesbitmap;

            Child[] children;
            KeyValuePair<K, V>[] entries;

            static Child[] emptychildren = new Child[0];
            static KeyValuePair<K, V>[] emptyentries = new KeyValuePair<K, V>[0];

            public void Init()
            {
                childrenbitmap = 0;
                entriesbitmap = 0;
                children = emptychildren;
                entries = emptyentries;
            }

            public bool TryGetValue(K key, int hash, int depth, out V value)
            {
                int bit = ComputeBit(hash, depth);
                int i;
                if (TryGetIndex(bit, childrenbitmap, out i))
                {
                    return children[i].TryGetValue(key, hash, depth + 1, out value);
                }
                else if (TryGetIndex(bit, entriesbitmap, out i))
                {
                    var kv = entries[i];
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
                int i;
                if (TryGetIndex(bit, childrenbitmap, out i))
                {
                    var newchildren = (Child[])children.Clone();
                    newchildren[i].Update(key, hash, depth + 1, value);
                    children = newchildren;
                }
                else if (TryGetIndex(bit, entriesbitmap, out i))
                {
                    var newchildren = new Child[children.Length + 1];
                    var newentries = new KeyValuePair<K, V>[entries.Length - 1];
                    var newchild = new Child();
                    newchild.Init();
                    newchild.Update(key, hash, depth + 1, value);
                    newchild.Update(entries[i].Key, entries[i].Key.GetHashCode(), depth + 1, entries[i].Value);

                    var j = ComputeIndex(bit, childrenbitmap);
                    Array.ConstrainedCopy(children, 0, newchildren, 0, j);
                    newchildren[j] = newchild;
                    Array.ConstrainedCopy(children, j, newchildren, j + 1, children.Length - j);

                    Array.ConstrainedCopy(entries, 0, newentries, 0, i);
                    Array.ConstrainedCopy(entries, i + 1, newentries, i, entries.Length - i - 1);

                    childrenbitmap = childrenbitmap | bit;
                    entriesbitmap = entriesbitmap & ~bit;
                    children = newchildren;
                    entries = newentries;
                }
                else
                {
                    i = ComputeIndex(bit, entriesbitmap);

                    var newentries = new KeyValuePair<K, V>[entries.Length + 1];
                    Array.ConstrainedCopy(entries, 0, newentries, 0, i);
                    newentries[i] = new KeyValuePair<K, V>(key, value);
                    Array.ConstrainedCopy(entries, i, newentries, i + 1, entries.Length - i);

                    entriesbitmap = entriesbitmap | bit;
                    entries = newentries;
                }
            }

            static int ComputeBit(int hash, int depth)
            {
                return 1 << ((hash >> (5 * depth)) & 0x01F);
            }

            static int ComputeIndex(int bit, int bitmap)
            {
                return BitCount(bitmap & (bit - 1));
            }

            static bool TryGetIndex(int bit, int bitmap, out int i)
            {
                if ((bitmap & bit) != 0)
                {
                    i = ComputeIndex(bit, bitmap);
                    return true;
                }
                else
                {
                    i = default(int);
                    return false;
                }
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

            public void Init()
            {
                entries = emptyentries;
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

            public static TrieMap Empty()
            {
                var hamt = new TrieMap();
                hamt.trie.Init();
                return hamt;
            }

            public bool TryGetValue(K key, out V value)
            {
                return trie.TryGetValue(key, key.GetHashCode(), 0, out value);
            }

            public TrieMap Update(K key, V value)
            {
                var self = this;
                self.trie.Update(key, key.GetHashCode(), 0, value);
                return self;
            }
        }

        public static TrieMap Empty = TrieMap.Empty();
    }

    public struct Test
    {
        public static void Run()
        {
            var h = HAMT<int, int>.Empty;

            for (int i = 0; i < 1000; i++) h = h.Update(i, i);

            for (int i = 0; i < 1000; i++)
            {
                int v;
                h.TryGetValue(i, out v);
                if (v != i) Console.WriteLine("Error: ", i, " returned ", v);
            }

            for (int i = 1001; i < 2000; i++)
            {
                int v;
                if (h.TryGetValue(i, out v)) Console.WriteLine("Error: ", i, " shouldn't be present");
            }

            Console.WriteLine("End");
        }
    }
}
