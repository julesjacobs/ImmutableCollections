using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.HashMaps
{
    interface IHashMap<K, V, Self>
    {
        bool TryGetValue(K key, int hash, int depth, out V value);
        Self Update(K key, int hash, int depth, V value);
        void Init();
    }

    struct TrieNode<K, V, Child> : IHashMap<K, V, TrieNode<K, V, Child>> where Child : IHashMap<K, V, Child>, new()
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

        public TrieNode<K, V, Child> Update(K key, int hash, int depth, V value)
        {
            int bit = ComputeBit(hash, depth);
            int i;
            if (TryGetIndex(bit, childrenbitmap, out i))
            {
                var newchildren = (Child[])children.Clone();
                newchildren[i] = children[i].Update(key, hash, depth + 1, value);
                return new TrieNode<K, V, Child>
                {
                    childrenbitmap = childrenbitmap,
                    entriesbitmap = entriesbitmap,
                    children = newchildren,
                    entries = entries
                };
            }
            else if (TryGetIndex(bit, entriesbitmap, out i))
            {
                var newchildren = new Child[children.Length + 1];
                var newentries = new KeyValuePair<K, V>[entries.Length - 1];
                var newchild = new Child();
                newchild.Init();
                newchild = newchild.Update(key, hash, depth + 1, value);
                newchild = newchild.Update(entries[i].Key, entries[i].Key.GetHashCode(), depth + 1, entries[i].Value);

                var j = ComputeIndex(bit, childrenbitmap);
                Array.ConstrainedCopy(children, 0, newchildren, 0, j);
                newchildren[j] = newchild;
                Array.ConstrainedCopy(children, j, newchildren, j + 1, children.Length - j);

                Array.ConstrainedCopy(entries, 0, newentries, 0, i);
                Array.ConstrainedCopy(entries, i + 1, newentries, i, entries.Length - i - 1);

                return new TrieNode<K, V, Child>
                {
                    childrenbitmap = childrenbitmap | bit,
                    entriesbitmap = entriesbitmap & ~bit,
                    children = newchildren,
                    entries = newentries
                };
            }
            else
            {
                i = ComputeIndex(bit, entriesbitmap);

                var newentries = new KeyValuePair<K, V>[entries.Length + 1];
                Array.ConstrainedCopy(entries, 0, newentries, 0, i);
                newentries[i] = new KeyValuePair<K, V>(key, value);
                Array.ConstrainedCopy(entries, i, newentries, i + 1, entries.Length - i);

                return new TrieNode<K, V, Child>
                {
                    childrenbitmap = childrenbitmap,
                    entriesbitmap = entriesbitmap | bit,
                    children = children,
                    entries = newentries
                };
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

    struct Bucket<K, V> : IHashMap<K, V, Bucket<K, V>>
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

        public Bucket<K, V> Update(K key, int hash, int depth, V value)
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
            return new Bucket<K, V> { entries = list.ToArray() };
        }

        static Func<K, K, bool> eq = EqualityComparer<K>.Default.Equals;
    }

    struct TrieMap<K, V>
    {
        TrieNode<K, V, TrieNode<K, V, TrieNode<K, V, TrieNode<K, V, TrieNode<K, V, TrieNode<K, V, TrieNode<K, V, Bucket<K, V>>>>>>>> trie;

        public static TrieMap<K, V> Empty()
        {
            var hamt = new TrieMap<K, V>();
            hamt.trie.Init();
            return hamt;
        }

        public bool TryGetValue(K key, out V value)
        {
            return trie.TryGetValue(key, key.GetHashCode(), 0, out value);
        }

        public TrieMap<K, V> Update(K key, V value)
        {
            return new TrieMap<K, V> { trie = trie.Update(key, key.GetHashCode(), 0, value) };
        }
    }

}
