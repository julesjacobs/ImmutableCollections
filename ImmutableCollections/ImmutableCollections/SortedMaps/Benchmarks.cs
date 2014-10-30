using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Immutable;

namespace ImmutableCollections.SortedMaps
{
    public class Benchmarks
    {
        public static void Run()
        {
            var size = 100000;

            var btree = BTree<int, int>.Empty;
            var imsorteddict = ImmutableSortedDictionary<int, int>.Empty;
            var imdict = ImmutableDictionary<int, int>.Empty;
            var mutsorteddict = new SortedDictionary<int, int>();
            int v;




            Console.WriteLine("=========================== Filling ===========================");

            Benchmark.Time("Collections.ImmutableSortedDictionary", () =>
            {
                imsorteddict = ImmutableSortedDictionary<int, int>.Empty;
                for (int i = 0; i < size; i++) imsorteddict = imsorteddict.SetItem(i, i);
            });

            Benchmark.Time("Collections.ImmutableDictionary", () =>
            {
                imdict = ImmutableDictionary<int, int>.Empty;
                for (int i = 0; i < size; i++) imdict = imdict.SetItem(i, i);
            });

            Benchmark.Time("Collections.SortedDictionary", () =>
            {
                mutsorteddict = new SortedDictionary<int, int>();
                for (int i = 0; i < size; i++) mutsorteddict[i] = i;
            });

            Benchmark.Time("BTree", () =>
            {
                imsorteddict = ImmutableSortedDictionary<int, int>.Empty;
                for (int i = 0; i < size; i++) btree = btree.Set(i, i);
            });

            Benchmark.Time("BTree (mutate)", () =>
            {
                imsorteddict = ImmutableSortedDictionary<int, int>.Empty;
                for (int i = 0; i < size; i++) btree = btree.MutateSet(i, i);
            });

            Console.WriteLine("=========================== Memory ===========================");

            btree = BTree<int, int>.Empty;
            imsorteddict = ImmutableSortedDictionary<int, int>.Empty;
            imdict = ImmutableDictionary<int, int>.Empty;
            mutsorteddict = new SortedDictionary<int, int>();

            Benchmark.Memory("Collections.ImmutableSortedDictionary", () =>
            {
                for (int i = 0; i < size; i++) imsorteddict = imsorteddict.SetItem(i, i);
            });

            Benchmark.Memory("Collections.ImmutableDictionary", () =>
            {
                for (int i = 0; i < size; i++) imdict = imdict.SetItem(i, i);
            });

            Benchmark.Memory("Collections.SortedDictionary", () =>
            {
                for (int i = 0; i < size; i++) mutsorteddict[i] = i;
            });

            Benchmark.Memory("BTree", () =>
            {
                for (int i = 0; i < size; i++) btree = btree.Set(i, i);
            });


            Console.WriteLine("=========================== Lookup ===========================");

            Benchmark.Time("Collections.ImmutableSortedDictionary", () =>
            {
                for (int i = 0; i < size; i++) imsorteddict.TryGetValue(i, out v);
            });

            Benchmark.Time("Collections.ImmutableDictionary", () =>
            {
                for (int i = 0; i < size; i++) imdict.TryGetValue(i, out v);
            });

            Benchmark.Time("Collections.SortedDictionary", () =>
            {
                for (int i = 0; i < size; i++) mutsorteddict.TryGetValue(i, out v);
            });

            Benchmark.Time("BTree", () =>
            {
                for (int i = 0; i < size; i++) btree.TryGetValue(i, out v);
            });


            Console.WriteLine("=========================== Setting ===========================");

            Benchmark.Time("Collections.ImmutableSortedDictionary", () =>
            {
                for (int i = 0; i < size; i++) imsorteddict = imsorteddict.SetItem((i * 234) % size, i);
            });

            Benchmark.Time("Collections.ImmutabledDictionary", () =>
            {
                for (int i = 0; i < size; i++) imdict = imdict.SetItem((i * 234) % size, i);
            });

            Benchmark.Time("Collections.SortedDictionary", () =>
            {
                for (int i = 0; i < size; i++) mutsorteddict[(i * 234) % size] = i;
            });

            Benchmark.Time("BTree", () =>
            {
                for (int i = 0; i < size; i++) btree = btree.Set((i * 234) % size, i);
            });

            Benchmark.Time("BTree (mutate)", () =>
            {
                for (int i = 0; i < size; i++) btree = btree.MutateSet((i * 234) % size, i);
            });
        }
    }
}
