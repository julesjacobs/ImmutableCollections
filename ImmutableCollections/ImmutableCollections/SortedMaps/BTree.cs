using System;
using System.Diagnostics;

namespace ImmutableCollections.SortedMaps
{
	struct BTree<K,V> where K : IComparable<K>
	{
		const int MinSize = 8;
		const int MaxSize = MinSize*2;

		interface ITree<Self> where Self : struct, ITree<Self>
		{
			bool TryGetValue(K key, out V value);
			V GetMin();
			bool Set(K key, V value, out K splitKey, out Self splitNode);
			void SetMin(V value);
		}

		struct Node<C> : ITree<Node<C>> where C : struct, ITree<C>
		{
			public K[] keys;
			public C[] children; // children.Length == keys.Length + 1

            // i manually inlined this further down because it's much faster that way
			bool Search(K key, out int i)
			{
				int cmp = 0;
				for (i = 0; i < keys.Length; i++) { // should try binary search
					cmp = key.CompareTo (keys [i]);
					if (cmp <= 0) break; // key <= keys[i]
				}
				return cmp == 0;
			}

            // inlined search
			public bool TryGetValue(K key, out V val)
			{
                int cmp = 0;
                int i;
                for (i = 0; i < keys.Length; i++)
                { // should try binary search
                    cmp = key.CompareTo(keys[i]);
                    if (cmp <= 0) break; // key <= keys[i]
                }
				if (cmp == 0) {
					val = children [i + 1].GetMin ();
					return true;
				}
				return children [i].TryGetValue (key, out val);
			}

			public V GetMin() { return children[0].GetMin(); }

			public bool Set(K key, V val, out K splitKey, out Node<C> splitNode)
			{
                int cmp = 0;
                int i;
                for (i = 0; i < keys.Length; i++)
                { // should try binary search
                    cmp = key.CompareTo(keys[i]);
                    if (cmp <= 0) break; // key <= keys[i]
                }
				if (cmp == 0) {
					children = CopyArray (children);
					children [i + 1].SetMin (val);
				} else {
					var c = children [i];
					K childSplitKey;
					C childSplitNode;

					if (c.Set (key, val, out childSplitKey, out childSplitNode)) {
						keys = ArrayInsert (keys, i, childSplitKey);
						children = ArrayInsert (children, i + 1, childSplitNode);
						children [i] = c;
						if (keys.Length > MaxSize) {
							// inefficient? this copies arrays that were just copied
							splitKey = keys [MinSize];
							var leftKeys = SubArray (keys, 0, MinSize);
							var rightKeys = SubArray (keys, MinSize + 1, MinSize);
							var leftChildren = SubArray (children, 0, MinSize + 1);
							var rightChildren = SubArray (children, MinSize + 1, MinSize + 1);
							keys = leftKeys;
							children = leftChildren;
							splitNode = new Node<C>{ keys = rightKeys, children = rightChildren };
							return true;

						}
					} else {
						children = CopyArray (children);
						children [i] = c;
					}
				}
				splitKey = default(K);
				splitNode = default(Node<C>);
				return false;
			}

			public void SetMin(V val)
			{
				children = CopyArray (children);
				children[0].SetMin (val);
			}			

			static T[] ArrayInsert<T>(T[] xs, int i, T x)
			{
				var tmp = new T[xs.Length + 1];
				Array.Copy (xs, 0, tmp, 0, i);
				tmp [i] = x;
				Array.Copy (xs, i, tmp, i + 1, xs.Length - i);
				return tmp;
			}

			static T[] SubArray<T>(T[] xs, int i, int len)
			{
				var tmp = new T[len];
				Array.Copy (xs, i, tmp, 0, len);
				return tmp;
			}

			static T[] CopyArray<T>(T[] xs){
				return SubArray (xs, 0, xs.Length);
			}
		}

		struct Leaf : ITree<Leaf>
		{
			public V value;

			public bool TryGetValue(K key, out V val)
			{
				val = default(V);
				return false;
			}

			public V GetMin() { return value; }

			public bool Set(K key, V val, out K splitKey, out Leaf splitNode)
			{
				splitKey = key;
				splitNode = new Leaf() { value = val };
				return true;
			}

			public void SetMin(V val) { value = val; }
		}

		public interface Tree
		{
			bool TryGetValue(K key, out V value);
			Tree Set(K key, V value);
		}

		class EmptyTree : Tree
		{
			public bool TryGetValue(K key, out V value)
			{
				value = default(V);
				return false;
			}

			public Tree Set(K key, V value)
			{
				return new Tree<Leaf> (){ min = key, self = new Leaf (){ value = value } };
			}
		}

		class Tree<N> : Tree where N : struct, ITree<N>
		{
			public K min;
			public N self;

			public bool TryGetValue(K key, out V value)
			{
				var cmp = key.CompareTo (min);
				if (cmp < 0) {
					value = default(V);
					return false;
				}
				if (cmp == 0) {
					value = self.GetMin ();
					return true;
				}
				return self.TryGetValue (key, out value);
			}

			public Tree Set(K key, V value)
			{
				var newself = self;
				var cmp = key.CompareTo (min);
				if (cmp == 0){
					newself.SetMin (value);
					return new Tree<N>{ min = min, self = newself };
				}
				var newmin = min;
				V val = value;
				if (cmp < 0) {
					val = self.GetMin ();
					newself.SetMin (value);
					newmin = key;
					key = min;
				}
				// now insert (key, val)
				K splitKey;
				N splitNode;
				if (newself.Set (key, val, out splitKey, out splitNode)) {
					// have to upgrade this tree with an extra level
					var children = new N[2];
					children [0] = newself;
					children [1] = splitNode;
					var keys = new K[]{ splitKey };
					return new Tree<Node<N>> (){ min = newmin, self = new Node<N> (){ keys = keys, children = children } };
				}
				return new Tree<N>{ min = newmin, self = newself };
			}
		}

		public static Tree Empty = new EmptyTree();
	}
}
