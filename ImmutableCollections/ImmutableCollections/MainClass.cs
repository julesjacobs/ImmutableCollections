using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ImmutableCollections
{
    public class MainClass
    {
        static void Benchmark(string name, Action f)
        {
            f();
            GC.GetTotalMemory(true);
            var watch = Stopwatch.StartNew();
            for(int i=0; i<10; i++)
            {
                f();
            }
            watch.Stop();
            Console.WriteLine("{0}: {1} ms", name, watch.ElapsedMilliseconds);
        }

        static void Memory(string name, Action f)
        {
            var initial = GC.GetTotalMemory(true);
            f();
            var final = GC.GetTotalMemory(true);
            Console.WriteLine("{0}: {1} bytes", name, final - initial);
        }

        static void StackBenchmark()
        {
            var n = 25;

            Benchmark("Collections.ImmutableStack", () =>
            {
                var s = ImmutableStack<int>.Empty;
                s = s.Push(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Pop(out k);
                    for (int i = 0; i < k; i++) s = s.Push(i);
                }
            });

            Benchmark("LinkedListStack", () =>
            {
                var s = LinkedListStack<int>.Empty;
                s = s.Push(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Pop(out k);
                    for (int i = 0; i < k; i++) s = s.Push(i);
                }
            });

            Benchmark("NullLinkedListStack", () =>
            {
                var s = NullLinkedListStack<int>.Empty;
                s = s.Push(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Pop(out k);
                    for (int i = 0; i < k; i++) s = s.Push(i);
                }
            });

            Benchmark("BufferedStack", () =>
            {
                var s = BufferedStack<int>.Empty;
                s = s.Push(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Pop(out k);
                    for (int i = 0; i < k; i++) s = s.Push(i);
                }
            });

            int size = 1000000;

            var s1 = ImmutableStack<int>.Empty;
            Memory("Collections.ImmutableStack", () =>
            {
                for (int i = 0; i < size; i++) s1 = s1.Push(i);
            });

            var s2 = LinkedListStack<int>.Empty;
            Memory("LinkedListStack", () =>
            {
                for (int i = 0; i < size; i++) s2 = s2.Push(i);
            });

            var s3 = NullLinkedListStack<int>.Empty;
            Memory("NullLinkedListStack", () =>
            {
                for (int i = 0; i < size; i++) s3 = s3.Push(i);
            });

            var s4 = BufferedStack<int>.Empty;
            Memory("BufferedStack", () =>
            {
                for (int i = 0; i < size; i++) s4 = s4.Push(i);
            });

            // make sure stuff doesn't get GC'd during the measurement
            Console.WriteLine(s1.IsEmpty);
            Console.WriteLine(s2.IsEmpty);
            Console.WriteLine(s3.IsEmpty);
            Console.WriteLine(s4.IsEmpty);
        }

        static void QueueBenchmark()
        {
            var n = 20;

            Benchmark("Collections.ImmutableQueue", () =>
            {
                var s = ImmutableQueue<int>.Empty;
                s = s.Enqueue(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Dequeue(out k);
                    for (int i = 0; i < k; i++) s = s.Enqueue(i);
                }
            });

            Benchmark("DoubleStackQueue", () =>
            {
                var s = DoubleStackQueue<int>.Empty;
                s = s.Enqueue(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Dequeue(out k);
                    for (int i = 0; i < k; i++) s = s.Enqueue(i);
                }
            });

            Benchmark("StackArrayQueue", () =>
            {
                var s = StackArrayQueue<int>.Empty;
                s = s.Enqueue(n);
                while (!s.IsEmpty)
                {
                    int k;
                    s = s.Dequeue(out k);
                    for (int i = 0; i < k; i++) s = s.Enqueue(i);
                }
            });

            int size = 1000000;

            var s1 = ImmutableQueue<int>.Empty;
            Memory("Collections.ImmutableQueue", () =>
            {
                for (int i = 0; i < size; i++) s1 = s1.Enqueue(i);
                int item;
                s1 = s1.Dequeue(out item);
                for (int i = 0; i < size; i++) s1 = s1.Enqueue(i);
            });

            var s2 = DoubleStackQueue<int>.Empty;
            Memory("DoubleStackQueue", () =>
            {
                for (int i = 0; i < size; i++) s2 = s2.Enqueue(i);
                int item;
                s2 = s2.Dequeue(out item);
                for (int i = 0; i < size; i++) s2 = s2.Enqueue(i);
            });

            var s3 = StackArrayQueue<int>.Empty;
            Memory("StackArrayQueue", () =>
            {
                for (int i = 0; i < size; i++) s3 = s3.Enqueue(i);
                int item;
                s3 = s3.Dequeue(out item);
                for (int i = 0; i < size; i++) s3 = s3.Enqueue(i);
            });

            // make sure stuff doesn't get GC'd during the measurement
            Console.WriteLine(s1.IsEmpty);
            Console.WriteLine(s2.IsEmpty);
            Console.WriteLine(s3.IsEmpty);
        }

        static void Main()
        {
            StackBenchmark();
            QueueBenchmark();
            Console.Read();
        }
    }
}
