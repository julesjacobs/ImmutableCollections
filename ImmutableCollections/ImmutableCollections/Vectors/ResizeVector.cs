using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.Vectors
{
    struct ResizeVector<T>
    {
        static int BitsPerLevel = 5; // branching factor = 2^BitsPerLevel
        static int MaxNodeSize = 1 << BitsPerLevel;

        interface IVec
        {
            T Lookup(uint i);
            bool Add(T x); // returns whether adding was succesful: it's not succesful if the node is full
            void Init(T x); // initialize a new node with 1 element
            void Set(uint i, T x);
            int Shift { get; }
        }

        struct Node<C> : IVec where C : struct, IVec
        {
            public C[] children;

            public T Lookup(uint i)
            {
                return children[i >> (32 - BitsPerLevel)].Lookup(i << BitsPerLevel);
            }

            public void Set(uint i, T x)
            {
                var tmp = new C[children.Length];
                Array.Copy(children, 0, tmp, 0, children.Length);
                children = tmp;
                children[i >> (32 - BitsPerLevel)].Set(i << BitsPerLevel, x);
            }

            public void Init(T x)
            {
                children = new C[1];
                children[0].Init(x);
            }

            public bool Add(T x) 
            {
                var n = children.Length;
                var last = children[n - 1];
                if(last.Add(x))
                {
                    var tmp = new C[n];
                    Array.Copy(children, 0, tmp, 0, n - 1);
                    tmp[n - 1] = last;
                    children = tmp;
                    return true;
                }
                if (n == MaxNodeSize) return false; // can't go larger, add failed
                var tmp2 = new C[n + 1];
                Array.Copy(children, 0, tmp2, 0, n);
                tmp2[n].Init(x);
                children = tmp2;
                return true;
            }

            public int Shift { get { return (new C()).Shift - BitsPerLevel; } }
        }

        struct Leaf : IVec
        {
            public T item;

            public T Lookup(uint i) { return item; }
            public void Init(T x) { item = x; }
            public bool Add(T x) { return false; }
            public void Set(uint i, T x) { item = x; }

            public int Shift { get { return 32; } }
        }

        public interface Vector 
        {
            T Lookup(int i);
            Vector Add(T x);
            Vector Set(int i, T x);
        }

        class Vector<V> : Vector where V : struct, IVec
        {
            public V self;

            static readonly int shift = (new V()).Shift;

            public T Lookup(int i) { return self.Lookup(unchecked((uint)i) << shift); }

            public Vector Add(T x)
            {
                var self2 = self;
                if (self2.Add(x)) return new Vector<V>() { self = self2 };
                // node was full, upgrade to next level
                var children = new V[2];
                children[0] = self;
                children[1].Init(x);
                return new Vector<Node<V>>() { self = new Node<V>() { children = children } };
            }

            public Vector Set(int i, T x)
            {
                var self2 = self;
                self2.Set(unchecked((uint)i) << shift, x);
                return new Vector<V>() { self = self2 };
            }
        }

        class EmptyVector : Vector
        {
            public T Lookup(int i) { throw new IndexOutOfRangeException(); }

            public Vector Add(T x)
            {
                var v = new Vector<Leaf>();
                v.self.Init(x);
                return v;
            }

            public Vector Set(int i, T x) { throw new IndexOutOfRangeException(); }
        }

        public static Vector Empty = new EmptyVector();
    }
}
