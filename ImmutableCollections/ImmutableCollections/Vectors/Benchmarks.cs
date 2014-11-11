using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace ImmutableCollections.Vectors
{
    class Benchmarks
    {
        public static void Run()
        {
            
            int size = 1000000;

            var msVec = ImmutableList<int>.Empty;
            var fvec = new Fvec<int>();
            var fixedMergeVec = FixedMergeVector<int>.Empty;
            var mergeVec = MergeVector<int>.Empty;
            var fixedVec = new FixedVector<int>();
            var resizeVec = ResizeVector<int>.Empty;





            Console.WriteLine("=========================== Add ===========================");

            Benchmark.Time("Collections.ImmutableList", () =>
            {
                msVec = ImmutableList<int>.Empty;
                for (int i = 0; i < size; i++) msVec = msVec.Add(i);
            });

            Benchmark.Time("Fvec", () =>
            {
                fvec = new Fvec<int>();
                for (int i = 0; i < size; i++) fvec = fvec.Add(i);
            });

            Benchmark.Time("FixedMergeVector", () =>
            {
                fixedMergeVec = FixedMergeVector<int>.Empty;
                for (int i = 0; i < size; i++) fixedMergeVec = fixedMergeVec.Add(i);
            });

            Benchmark.Time("MergeVector", () =>
            {
                mergeVec = MergeVector<int>.Empty;
                for (int i = 0; i < size; i++) mergeVec = mergeVec.Add(i);
            });

            Benchmark.Time("FixedVector", () =>
            {
                fixedVec = new FixedVector<int>();
                for (int i = 0; i < size; i++) fixedVec = fixedVec.Add(i);
            });

            Benchmark.Time("ResizeVector", () =>
            {
                resizeVec = ResizeVector<int>.Empty;
                for (int i = 0; i < size; i++) resizeVec = resizeVec.Add(i);
            });
            



            Console.WriteLine("=========================== Set ===========================");

            Benchmark.Time("Collections.ImmutableList", () =>
            {
                for (int i = 0; i < size; i++) msVec = msVec.SetItem((i * 234) % size, -i);
            });

            Benchmark.Time("Fvec", () =>
            {
                for (int i = 0; i < size; i++) fvec = fvec.Set((i * 234) % size, -i);
            });

            Benchmark.Time("FixedMergeVector", () =>
            {
                for (int i = 0; i < size; i++) fixedMergeVec = fixedMergeVec.Set((i * 234) % size, -i);
            });

            Benchmark.Time("MergeVector", () =>
            {
                for (int i = 0; i < size; i++) mergeVec = mergeVec.Set((i * 234) % size, -i);
            });

            Benchmark.Time("FixedVector", () =>
            {
                for (int i = 0; i < size; i++) fixedVec = fixedVec.Set((i * 234) % size, -i);
            });

            Benchmark.Time("ResizeVector", () =>
            {
                for (int i = 0; i < size; i++) resizeVec = resizeVec.Set((i * 234) % size, -i);
            });




            Console.WriteLine("=========================== Lookup ===========================");
            int x;

            Benchmark.Time("Collections.ImmutableList", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = msVec[(i * 234) % size];
            });

            Benchmark.Time("Fvec", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = fvec[(i * 234) % size];
            });

            Benchmark.Time("FixedMergeVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = fixedMergeVec.Lookup((i * 234) % size);
            });

            Benchmark.Time("MergeVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = mergeVec.Lookup((i * 234) % size);
            });
            Benchmark.Time("MergeVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = mergeVec.Lookup((i * 234) % size);
            });
            Benchmark.Time("MergeVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = mergeVec.Lookup((i * 234) % size);
            });
            Benchmark.Time("MergeVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = mergeVec.Lookup((i * 234) % size);
            });

            Benchmark.Time("FixedVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = fixedVec.Lookup((i * 234) % size);
            });

            Benchmark.Time("ResizeVector", () =>
            {
                for (int k = 0; k < 10; k++)
                    for (int i = 0; i < size; i++) x = resizeVec.Lookup((i * 234) % size);
            });




            Console.WriteLine("=========================== Memory ===========================");

            msVec = ImmutableList<int>.Empty;
            Benchmark.Memory("Collections.ImmutableList", () =>
            {
                for (int i = 0; i < size; i++) msVec = msVec.Add(i);
            });

            fvec = new Fvec<int>();
            Benchmark.Memory("Fvec", () =>
            {
                for (int i = 0; i < size; i++) fvec = fvec.Add(i);
            });

            fixedMergeVec = FixedMergeVector<int>.Empty;
            Benchmark.Memory("FixedMergeVector", () =>
            {
                for (int i = 0; i < size; i++) fixedMergeVec = fixedMergeVec.Add(i);
            });

            mergeVec = MergeVector<int>.Empty;
            Benchmark.Memory("MergeVector", () =>
            {
                for (int i = 0; i < size; i++) mergeVec = mergeVec.Add(i);
            });

            fixedVec = new FixedVector<int>();
            Benchmark.Memory("FixedVector", () =>
            {
                for (int i = 0; i < size; i++) fixedVec = fixedVec.Add(i);
            });

            resizeVec = ResizeVector<int>.Empty;
            Benchmark.Memory("ResizeVector", () =>
            {
                for (int i = 0; i < size; i++) resizeVec = resizeVec.Add(i);
            });



            // necessary for accurate memory measurement, otherwise the CLR will get smart on us and GC them too early
            Console.WriteLine(msVec[0]);
            Console.WriteLine(fvec[0]);
            Console.WriteLine(fixedMergeVec.Lookup(0));
            Console.WriteLine(mergeVec.Lookup(0));
            Console.WriteLine(fixedVec.Lookup(0));
        }
    }
}
