using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace ImmutableCollections.Stacks
{
    public class Benchmarks
    {
        public static void Run()
        {
            var n = 25;

            Benchmark.Time("Collections.ImmutableStack", () =>
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

            Benchmark.Time("LinkedListStack", () =>
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

            Benchmark.Time("NullLinkedListStack", () =>
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

            Benchmark.Time("BufferedStack", () =>
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
            Benchmark.Memory("Collections.ImmutableStack", () =>
            {
                for (int i = 0; i < size; i++) s1 = s1.Push(i);
            });

            var s2 = LinkedListStack<int>.Empty;
            Benchmark.Memory("LinkedListStack", () =>
            {
                for (int i = 0; i < size; i++) s2 = s2.Push(i);
            });

            var s3 = NullLinkedListStack<int>.Empty;
            Benchmark.Memory("NullLinkedListStack", () =>
            {
                for (int i = 0; i < size; i++) s3 = s3.Push(i);
            });

            var s4 = BufferedStack<int>.Empty;
            Benchmark.Memory("BufferedStack", () =>
            {
                for (int i = 0; i < size; i++) s4 = s4.Push(i);
            });

            // make sure stuff doesn't get GC'd during the measurement
            Console.WriteLine(s1.IsEmpty);
            Console.WriteLine(s2.IsEmpty);
            Console.WriteLine(s3.IsEmpty);
            Console.WriteLine(s4.IsEmpty);
        }
    }
}
