using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace ImmutableCollections.Queues
{
    public class Benchmarks
    {
        public static void Run()
        {
            var n = 20;

            Benchmark.Time("Collections.ImmutableQueue", () =>
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

            Benchmark.Time("DoubleStackQueue", () =>
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

            Benchmark.Time("StackArrayQueue", () =>
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
            Benchmark.Memory("Collections.ImmutableQueue", () =>
            {
                for (int i = 0; i < size; i++) s1 = s1.Enqueue(i);
                int item;
                s1 = s1.Dequeue(out item);
                for (int i = 0; i < size; i++) s1 = s1.Enqueue(i);
            });

            var s2 = DoubleStackQueue<int>.Empty;
            Benchmark.Memory("DoubleStackQueue", () =>
            {
                for (int i = 0; i < size; i++) s2 = s2.Enqueue(i);
                int item;
                s2 = s2.Dequeue(out item);
                for (int i = 0; i < size; i++) s2 = s2.Enqueue(i);
            });

            var s3 = StackArrayQueue<int>.Empty;
            Benchmark.Memory("StackArrayQueue", () =>
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
    }
}
