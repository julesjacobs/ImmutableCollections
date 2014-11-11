using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.Vectors
{
    struct MergeVector<T>
    {
        interface IVec
        {
            T Lookup(uint i);
            void Set(uint i, T x);
        }

        struct Node<C> : IVec where C : struct, IVec
        {
            public C[] children;

            public T Lookup(uint i)
            {
                return children[i >> 27].Lookup(i << 5);
            }

            public void Set(uint i, T x)
            {
                var tmp = new C[children.Length];
                Array.Copy(children, 0, tmp, 0, children.Length);
                children = tmp;
                children[i >> 27].Set(i << 5, x);
            }

        }

        struct Leaf : IVec
        {
            public T item;

            public T Lookup(uint i) { return item; }
            public void Set(uint i, T x) { item = x; }
        }

        interface IRoot<N> where N : struct, IVec
        {
            T Lookup(uint i);
            bool Add(T x, out N full); // returns whether the node is full, and if so, returns the full node
            void Set(uint i, T x);
            void Init();

            int Shift { get; }
        }

        struct Root<N,R> : IRoot<Node<N>>
            where N : struct, IVec 
            where R : struct, IRoot<N>
        {
            public N[] nodes;
            public R next;

            public T Lookup(uint i)
            {
                uint j = i >> 27;
                if (j < nodes.Length) return nodes[j].Lookup(i << 5);
                else if (j == nodes.Length) return next.Lookup(i << 5);
                else throw new IndexOutOfRangeException();
            }

            public bool Add(T x, out Node<N> newfull)
            {
                
                N full;
                if(next.Add(x, out full))
                {
                    nodes = nodes.Insert(nodes.Length, full);
                    if(nodes.Length == 32)
                    {
                        newfull = new Node<N> { children = nodes };
                        nodes = new N[0];
                        return true;
                    }
                }
                newfull = default(Node<N>);
                return false;
            }

            public void Set(uint i, T x)
            {
                uint j = i >> 27;
                if (j < nodes.Length) nodes[j].Set(i << 5, x);
                else if (j == nodes.Length) next.Set(i << 5, x);
                else throw new IndexOutOfRangeException();
            }

            public void Init()
            {
                nodes = new N[0];
                next.Init();
            }

            public int Shift { get { return (new R()).Shift - 5; } }
        }

        struct End : IRoot<Leaf>
        {
            public T Lookup(uint i) { throw new IndexOutOfRangeException(); }
            public bool Add(T x, out Leaf newfull)
            {
                newfull = new Leaf { item = x };
                return true;
            }
            public void Set(uint i, T x) { throw new IndexOutOfRangeException(); }
            public void Init() { }
            public int Shift { get { return 32; } }
        }

        public interface Vector
        {
            T Lookup(int i);
            Vector Add(T x);
            Vector Set(int i, T x);
        }

        class Vector<N, R> : Vector
            where R : struct, IRoot<N>
            where N : struct, IVec
        {
            public R self;

            static readonly int shift = (new R()).Shift;

            public T Lookup(int i) { return self.Lookup(unchecked((uint)i) << shift); }

            public Vector Add(T x)
            {
                var self2 = self;
                N newfull;
                if (self2.Add(x, out newfull)) // node is full
                {
                    var ret = new Vector<Node<N>, Root<N, R>>
                    {
                        self = new Root<N, R>()
                        {
                            nodes = new N[] { newfull },
                            next = new R()
                        }
                    };
                    ret.self.next.Init();
                    return ret;
                }
                return new Vector<N, R>() { self = self2 };
            }

            public Vector Set(int i, T x)
            {
                var self2 = self;
                self2.Set(unchecked((uint)i) << shift, x);
                return new Vector<N, R>() { self = self2 };
            }
        }

        class EmptyVector : Vector
        {
            public T Lookup(int i) { throw new IndexOutOfRangeException(); }

            public Vector Add(T x)
            {
                var v = new Vector<Node<Leaf>, Root<Leaf,End>>();
                Node<Leaf> dummy;
                v.self.Init();
                v.self.Add(x, out dummy);
                return v;
            }

            public Vector Set(int i, T x) { throw new IndexOutOfRangeException(); }
        }

        public static Vector Empty = new EmptyVector();
    }
}
