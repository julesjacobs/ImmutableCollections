using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections
{
    struct LinkedListStack<T>
    {
        public abstract class Stack
        {
            public abstract bool IsEmpty { get; }

            public Stack Push(T x)
            {
                return new Node(x, this);
            }

            public abstract Stack Pop(out T x);
        }

        sealed class Node : Stack
        {
            T item;
            Stack rest;

            public Node(T x, Stack r)
            {
                item = x;
                rest = r;
            }

            public override bool IsEmpty { get { return false; } }

            public override Stack Pop(out T x)
            {
                x = this.item;
                return rest;
            }
        }

        sealed class End : Stack
        {
            public override bool IsEmpty { get { return true; } }

            public override Stack Pop(out T x)
            {
                throw new InvalidOperationException();
            }
        }

        public static Stack Empty = new End();
    }
}
