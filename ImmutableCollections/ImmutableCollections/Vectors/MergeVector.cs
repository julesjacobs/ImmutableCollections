using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.Vectors
{
    public struct MergeVector<T>
    {
        const int MASK = 0x1F;

        interface INode
        {
            int Shift { get; }
            T Lookup(uint i);
            void Set(uint i, T x);
        }

        struct Node<Child> : INode where Child : struct, INode
        {
            static int shift = (new Child()).Shift + 5;

            public Child[] children;

            public T Lookup(uint i)
            {
                return children[(i >> shift) & MASK].Lookup(i);
            }

            public void Set(uint i, T x)
            {
                var tmp = new Child[children.Length];
                Array.Copy(children, 0, tmp, 0, children.Length);
                children = tmp;
                children[(i >> shift) & MASK].Set(i, x);
            }

            // this method adds the full Child `c` to the children and returns true if this node is full after adding
            public bool AddAndIsFull(Child c)
            {
                if (children == null) children = new[] { c };
                else
                {
                    var tmp = new Child[children.Length + 1];
                    Array.Copy(children, 0, tmp, 0, children.Length);
                    tmp[children.Length] = c;
                    children = tmp;
                }
                return children.Length == 32;
            }

            public int Shift { get { return shift; } }
        }

        struct Leaf : INode
        {
            public T item;
            public T Lookup(uint i) { return item; }
            public void Set(uint i, T x) { item = x; }
            public int Shift { get { return -5; } }
        }

        public class Vec
        {
            uint Length;
            // logically the elements in this vector are the concatenation of the elements of level6 + level5 + ... + level0
            // this is done to support Add in amortized O(1) it also happens to work out nicely for a vec where not all levels are in use
            // the upper levels are simply null in that case; there is no virtual dispatch at all.
            Node<Node<Node<Node<Node<Node<Node<Leaf>>>>>>> level6;
            Node<Node<Node<Node<Node<Node<Leaf>>>>>> level5;
            Node<Node<Node<Node<Node<Leaf>>>>> level4;
            Node<Node<Node<Node<Leaf>>>> level3;
            Node<Node<Node<Leaf>>> level2;
            Node<Node<Leaf>> level1;
            Node<Leaf> level0;

            public Vec() { }

            public Vec(Vec v)
            {
                Length = v.Length;
                level0 = v.level0;
                level1 = v.level1;
                level2 = v.level2;
                level3 = v.level3;
                level4 = v.level4;
                level5 = v.level5;
                level6 = v.level6;
            }

            public T this[int index]
            {
                get { return Lookup(index); }
            }

            public T Lookup(int index)
            {
                var i = unchecked((uint)index);
                if (i >= Length) throw new IndexOutOfRangeException();
                var same = i ^ Length;
                if ((same >> 5) == 0) return level0.Lookup(i);
                if ((same >> 10) == 0) return level1.Lookup(i);
                if ((same >> 15) == 0) return level2.Lookup(i);
                if ((same >> 20) == 0) return level3.Lookup(i);
                if ((same >> 25) == 0) return level4.Lookup(i);
                if ((same >> 30) == 0) return level5.Lookup(i);
                return level6.Lookup(i);
            }

            public Vec Set(int index, T x)
            {
                var i = unchecked((uint)index);
                if (i > Length) throw new IndexOutOfRangeException();
                if (i == Length) return Add(x);
                var tmp = new Vec(this);
                var same = i ^ Length;
                if ((same >> 5) == 0) tmp.level0.Set(i, x);
                else if ((same >> 10) == 0) tmp.level1.Set(i, x);
                else if ((same >> 15) == 0) tmp.level2.Set(i, x);
                else if ((same >> 20) == 0) tmp.level3.Set(i, x);
                else if ((same >> 25) == 0) tmp.level4.Set(i, x);
                else if ((same >> 30) == 0) tmp.level5.Set(i, x);
                else tmp.level6.Set(i, x);
                return tmp;
            }

            static bool AddAndClear<Q>(ref Node<Node<Q>> parent, ref Node<Q> child) where Q : struct, INode
            {
                var ret = parent.AddAndIsFull(child);
                child.children = null;
                return ret;
            }

            public Vec Add(T x)
            {
                var tmp = new Vec(this);
                tmp.Length += 1;
                if (tmp.level0.AddAndIsFull(new Leaf { item = x }))
                    if (AddAndClear(ref tmp.level1, ref tmp.level0))
                        if (AddAndClear(ref tmp.level2, ref tmp.level1))
                            if (AddAndClear(ref tmp.level3, ref tmp.level2))
                                if (AddAndClear(ref tmp.level4, ref tmp.level3))
                                    if (AddAndClear(ref tmp.level5, ref tmp.level4))
                                        AddAndClear(ref tmp.level6, ref tmp.level5);
                return tmp;
            }
        }

        public static Vec Empty = new Vec();
    }
}
