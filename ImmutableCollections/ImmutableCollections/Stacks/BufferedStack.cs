using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections
{
    struct BufferedStack<T>
    {
        public struct Stack
        {
            int count;
            T item;
            Node rest;

            public bool IsEmpty { get { return count == 0; } }

            public Stack Push(T x)
            {
                var self = this;
                self.MutPush(x);
                return self;
            }

            void MutPush(T x)
            {
                if ((count & 1) == 0) item = x;
                else rest = new Node(item, x, rest);
                count++;
            }

            public Stack Pop(out T x)
            {
                var self = this;
                self.MutPop(out x);
                return self;
            }

            void MutPop(out T x)
            {
                if ((count & 1) == 0)
                {
                    item = rest.item1;
                    x = rest.item2;
                    rest = rest.rest;
                }
                else
                {
                    x = item;
                }
                count--;
            }
        }

        sealed class Node
        {
            public T item1;
            public T item2;
            public Node rest;

            public Node(T x1, T x2, Node r)
            {
                item1 = x1;
                item2 = x2;
                rest = r;
            }
        }

        public static Stack Empty = new Stack();
    }
}
