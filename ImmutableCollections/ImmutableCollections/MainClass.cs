using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ImmutableCollections
{
    public class Benchmark
    {
        public static void Time(string name, Action f)
        {
            //if (name == "Collections.ImmutableList") return;
            //if (name == "FixedVector") return;
            //if (name == "Collections.ImmutableDictionary") return;
            f(); // warmup: let the CLR genererate code for generics, get caches hot, etc.
            GC.GetTotalMemory(true);
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                f();
            }
            watch.Stop();
            Console.WriteLine("{0}: {1} ms", name, watch.ElapsedMilliseconds);
        }

        public static void Memory(string name, Action f)
        {
            var initial = GC.GetTotalMemory(true);
            f();
            var final = GC.GetTotalMemory(true);
            Console.WriteLine("{0}: {1} bytes", name, final - initial);
        }
    }

    public class MainClass
    {
        static void Main()
        {
            /*
            Console.WriteLine("=== STACKS ===");
            Stacks.Benchmarks.Run();

            Console.WriteLine("=== QUEUES ===");
            Queues.Benchmarks.Run();

            Console.WriteLine("=== VECTORS ===");
            Vectors.Benchmarks.Run();
            */

            Console.WriteLine("=== SORTEDMAPS ===");
            SortedMaps.Benchmarks.Run();

            
            /*
            var v = Vectors.ResizeVector<int>.Empty;

            for (int i = 0; i < 100000; i++)
            {
                v = v.Add(i);
            }

            Console.WriteLine(v.Lookup(5000));

            v = v.Set(5000, 42);

            Console.WriteLine(v.Lookup(5000));
            */


            Console.Read();
        }
    }
}
