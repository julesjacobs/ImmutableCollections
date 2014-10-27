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
            f();
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
            Stacks.Benchmarks.Run();
            Queues.Benchmarks.Run();
            Console.Read();
        }
    }
}
