using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections
{
    struct StackArrayQueue<T>
    {
        public struct Queue
        {
            int count;
            List<T> front;
            NullLinkedListStack<T>.Stack back;

            public bool IsEmpty { get { return count == 0 && back.IsEmpty; } }

            public Queue Enqueue(T x)
            {
                return new Queue { front = front, back = back.Push(x), count = count };
            }

            public Queue Dequeue(out T x)
            {
                if (count == 0)
                {
                    var newFront = new List<T>();
                    var newBack = back;
                    while (!newBack.IsEmpty)
                    {
                        T item;
                        newBack = newBack.Pop(out item);
                        newFront.Add(item);
                    }
                    var newCount = newFront.Count - 1;
                    x = newFront[newCount];
                    return new Queue { front = newFront, back = newBack, count = newCount };
                }
                else
                {
                    x = front[count - 1];
                    return new Queue { front = front, back = back, count = count - 1 };
                }
            }
        }

        public static Queue Empty = new Queue();
    }
}
