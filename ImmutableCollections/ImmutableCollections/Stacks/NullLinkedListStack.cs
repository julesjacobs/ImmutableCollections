using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections
{
    struct NullLinkedListStack<T>
    {
        public struct Stack
        {
            Node self;

            public bool IsEmpty { get { return self == null; } }

            public Stack Push(T x)
            {
                return new Stack { self = new Node(x, self) };
            }

            public Stack Pop(out T x)
            {
                x = self.item;
                return new Stack { self = self.rest };
            }
        }

        sealed class Node
        {
            public T item;
            public Node rest;

            public Node(T x, Node r)
            {
                item = x;
                rest = r;
            }
        }

        public static Stack Empty = new Stack();
    }
}
