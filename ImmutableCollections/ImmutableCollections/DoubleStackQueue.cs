using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections
{
    struct DoubleStackQueue<T>
    {
        public struct Queue
        {
            NullLinkedListStack<T>.Stack front;
            NullLinkedListStack<T>.Stack back;

            public bool IsEmpty { get { return front.IsEmpty && back.IsEmpty;  } }

            public Queue Enqueue(T x)
            {
                return new Queue { front = front, back = back.Push(x) };
            }

            public Queue Dequeue(out T x)
            {
                if(front.IsEmpty) 
                {
                    while(!back.IsEmpty){
                        T item;
                        back = back.Pop(out item);
                        front = front.Push(item);
                    }
                }
                return new Queue { front = front.Pop(out x), back = back };
            }
        }

        public static Queue Empty = new Queue();
    }
}
