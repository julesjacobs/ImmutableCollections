using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.Vectors
{
    // Sandro Maggi's vector: http://sourceforge.net/p/sasa/code/ci/default/tree/Bench/Fvec.cs
    public struct Fvec<T>
    {
        Node root;
        uint info; // [offset:29 bits][tree height:3 bits]

        int Height
        {
            get { return unchecked((int)(info & 0x7)); }
        }

        static uint BaseIndexBlock(int height)
        {
            return (uint)32 << 5 * (height - 1);
        }

        uint Offset
        {
            get { return info >> 3; }
        }

        uint Length
        {
            get { return BaseIndexBlock(Height) + Offset; }
        }

        public long Count
        {
            get { return Length; }
        }

        const int MASK = 0x1F;

        public T this[int index]
        {
            get
            {
                var i = unchecked((uint)index);
                //if (i >= Length) throw new IndexOutOfRangeException();
                T[] a0;
                if (Height == 0)
                {
                    a0 = root.items;
                }
                else
                {
                    var children = root.children;
                    for (var j = 5 * Height; j > 5; j -= 5)
                    {
                        children = children[(i >> j) & MASK].children;
                        if (children == null) return default(T);
                    }
                    a0 = children[(i >> 5) & MASK].items;
                }
                return a0 == null ? default(T) : a0[i & MASK];
            }
        }

        public Fvec<T> Set(int index, T value)
        {
            return Set(unchecked((uint)index), value);
        }

        public Fvec<T> Set(uint i, T value)
        {
            T[] a0;
            Node[] a1 = null, a2 = null, a3 = null, a4 = null, a5 = null, a6 = null;
            switch (Height)
            {
                case 0:
                    a0 = i > (uint)32 - 1 ? Build(i, root.items, ref a1, ref a2, ref a3, ref a4, ref a5, ref a6) :
                                            Dup(root.items);
                    goto LOAD0;
                case 1:
                    a1 = i > ((uint)32 << 5) - 1 ? Build(i, root.children, ref a2, ref a3, ref a4, ref a5, ref a6) :
                                                   Dup(root.children);
                    goto LOAD1;
                case 2:
                    a2 = i > ((uint)32 << 10) - 1 ? Build(i, root.children, ref a3, ref a4, ref a5, ref a6) :
                                                    Dup(root.children);
                    goto LOAD2;
                case 3:
                    a3 = i > ((uint)32 << 15) - 1 ? Build(i, root.children, ref a4, ref a5, ref a6) :
                                                    Dup(root.children);
                    goto LOAD3;
                case 4:
                    a4 = i > ((uint)32 << 20) - 1 ? Build(i, root.children, ref a5, ref a6) :
                                                    Dup(root.children);
                    goto LOAD4;
                case 5:
                    a5 = i > ((uint)32 << 25) - 1 ? Build(i, root.children, ref a6) :
                                                    Dup(root.children);
                    goto LOAD5;
                case 6:
                    a6 = Dup6(root.children);
                    goto LOAD6;
                default:
                    throw new NotSupportedException("IMPOSSIBLE!");
            }
        LOAD6:
            a5 = Dup(a6[i >> 30].children);
        LOAD5:
            a4 = Dup(a5[(i >> 25) & MASK].children);
        LOAD4:
            a3 = Dup(a4[(i >> 20) & MASK].children);
        LOAD3:
            a2 = Dup(a3[(i >> 15) & MASK].children);
        LOAD2:
            a1 = Dup(a2[(i >> 10) & MASK].children);
        LOAD1:
            a0 = Dup(a1[(i >> 5) & MASK].items);
        LOAD0:
            a0[i & MASK] = value;
            //FIXME: call-return might be faster since we don't need to check for a1-a6 nullity, ie. it's implicit in call stack
            if (a1 == null) return new Fvec<T> { root = a0, info = Math.Max(Length, i + 1) << 3 };
            a1[(i >> 5) & MASK] = a0;
            if (a2 == null) return new Fvec<T> { root = a1, info = 1 | (Math.Max(Length, i + 1) - (uint)32) << 3 };
            a2[(i >> 10) & MASK] = a1;
            if (a3 == null) return new Fvec<T> { root = a2, info = 2 | (Math.Max(Length, i + 1) - ((uint)32 << 5)) << 3 };
            a3[(i >> 15) & MASK].children = a2;
            if (a4 == null) return new Fvec<T> { root = a3, info = 3 | (Math.Max(Length, i + 1) - ((uint)32 << 10)) << 3 };
            a4[(i >> 20) & MASK].children = a3;
            if (a5 == null) return new Fvec<T> { root = a4, info = 4 | (Math.Max(Length, i + 1) - ((uint)32 << 15)) << 3 };
            a5[(i >> 5) & MASK].children = a4;
            if (a6 == null) return new Fvec<T> { root = a5, info = 5 | (Math.Max(Length, i + 1) - ((uint)32 << 20)) << 3 };
            a6[i >> 30] = a5;
            return new Fvec<T> { root = a6, info = 6 | (Math.Max(Length, i + 1) - ((uint)32 << 25)) << 3 };
        }

        public Fvec<T> Add(T value)
        {
            return Set(Length, value);
        }

        static T[] Build(uint i, T[] a0, ref Node[] a1, ref Node[] a2, ref Node[] a3, ref Node[] a4, ref Node[] a5, ref Node[] a6)
        {
            // if index is larger, build and initialize whole tree above a0, adding existing arrays, if any
            a1 = new Node[32];
            a1[0].items = a0;
            if (i > (32 << 5) - 1 && a2 == null) a2 = Init(0, a1);
            return new T[32];
        }

        static Node[] Build(uint i, Node[] a1, ref Node[] a2, ref Node[] a3, ref Node[] a4, ref Node[] a5, ref Node[] a6)
        {
            // if index is larger, build and initialize whole tree above a0, adding existing arrays, if any
            a2 = Init(0, a1);
            if (i > (32 << 10) - 1) a2 = Build(i, a2, ref a3, ref a4, ref a5, ref a6);
            return new Node[32];
        }

        static Node[] Build(uint i, Node[] a2, ref Node[] a3, ref Node[] a4, ref Node[] a5, ref Node[] a6)
        {
            // if index is larger, build and initialize whole tree above a0, adding existing arrays, if any
            a3 = Init(0, a2);
            if (i > (32 << 15) - 1) a3 = Build(i, a3, ref a4, ref a5, ref a6);
            return new Node[32];
        }

        static Node[] Build(uint i, Node[] a3, ref Node[] a4, ref Node[] a5, ref Node[] a6)
        {
            // if index is larger, build and initialize whole tree above a0, adding existing arrays, if any
            a4 = Init(0, a3);
            if (i > (32 << 20) - 1) a4 = Build(i, a4, ref a5, ref a6);
            return new Node[32];
        }

        static Node[] Build(uint i, Node[] a4, ref Node[] a5, ref Node[] a6)
        {
            // if index is larger, build and initialize whole tree above a0, adding existing arrays, if any
            a5 = Init(0, a4);
            if (i > (32 << 25) - 1) a5 = Build(i, a5, ref a6);
            return new Node[32];
        }

        static Node[] Build(uint i, Node[] a5, ref Node[] a6)
        {
            // if index is larger, build and initialize whole tree above a0, adding existing arrays, if any
            a6 = new Node[4];
            a6[0].children = a5;
            return new Node[32];
        }

        static Node[] Dup6(Node[] items)
        {
            var x = new Node[4];
            if (items != null) Array.Copy(items, 0, x, 0, items.Length);
            return x;
        }

        static Node[] Dup(Node[] items)
        {
            var x = new Node[32];
            if (items != null) Array.Copy(items, 0, x, 0, items.Length);
            return x;
        }

        static T[] Dup(T[] items)
        {
            var x = new T[32];
            if (items != null) Array.Copy(items, 0, x, 0, items.Length);
            return x;
        }

        static Node[] Init(uint index, Node[] value)
        {
            var x = new Node[32];
            x[index].children = value;
            return x;
        }

        public override string ToString()
        {
            return root.ToString();
        }

        struct Node
        {
            internal Node[] children;
            internal T[] items;

            public static implicit operator Node(Node[] x)
            {
                return new Node { children = x };
            }
            public static implicit operator Node(T[] x)
            {
                return new Node { items = x };
            }
        }
    }
}
