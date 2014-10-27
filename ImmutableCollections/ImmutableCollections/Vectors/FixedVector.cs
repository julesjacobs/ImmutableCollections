using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections.Vectors
{
    public struct FixedVector<T>
    {
        const int level0 = 9;
        const int level1 = 9;
        const int level2 = 7;
        const int level3 = 7;

        const int shift0 = level1 + level2 + level3;
        const int shift1 = level2 + level3;
        const int shift2 = level3;

        const int mask1 = (1 << level1) - 1;
        const int mask2 = (1 << level2) - 1;
        const int mask3 = (1 << level3) - 1;

        const int n1 = 1 << level1;
        const int n2 = 1 << level2;
        const int n3 = 1 << level3;


        T[][][][] a0;

        public T Lookup(int index)
        {
            var i = unchecked((uint)index);
            return a0[i >> shift0][(i >> shift1) & mask1][(i >> shift2) & mask2][i & mask3];
        }

        public FixedVector<T> Set(int index, T x)
        {
            var i = unchecked((uint)index);
            var i0 = i >> shift0;
            var i1 = (i >> shift1) & mask1;
            var i2 = (i >> shift2) & mask2;
            var i3 = i & mask3;

            var a1 = a0[i0];
            var a2 = a1[i1];
            var a3 = a2[i2];

            int n = a3.Length;
            var new_a3 = new T[n];
            Array.Copy(a3, new_a3, n);
            new_a3[i3] = x;

            n = a2.Length;
            var new_a2 = new T[n][];
            Array.Copy(a2, new_a2, n);
            new_a2[i2] = new_a3;

            n = a1.Length;
            var new_a1 = new T[n][][];
            Array.Copy(a1, new_a1, n);
            new_a1[i1] = new_a2;

            n = a0.Length;
            var new_a0 = new T[n][][][];
            Array.Copy(a0, new_a0, n);
            new_a0[i0] = new_a1;

            return new FixedVector<T> { a0 = new_a0 };
        }

        public FixedVector<T> Add(T x)
        {
            if (a0 == null) return new FixedVector<T> { a0 = new[] { new[] { new[] { new[] { x } } } } };

            var a1 = a0[a0.Length - 1];
            var a2 = a1[a1.Length - 1];
            var a3 = a2[a2.Length - 1];

            T[] new_a3;
            if (a3.Length < n3)
            {
                new_a3 = new T[a3.Length + 1];
                Array.Copy(a3, new_a3, a3.Length);
                new_a3[a3.Length] = x;
                goto SET3;
            }
            new_a3 = new[] { x };

            T[][] new_a2;
            if (a2.Length < n2)
            {
                new_a2 = new T[a2.Length + 1][];
                Array.Copy(a2, new_a2, a2.Length);
                new_a2[a2.Length] = new_a3;
                goto SET2;
            }
            new_a2 = new[] { new_a3 };

            T[][][] new_a1;
            if (a1.Length < n1)
            {
                new_a1 = new T[a1.Length + 1][][];
                Array.Copy(a1, new_a1, a1.Length);
                new_a1[a1.Length] = new_a2;
                goto SET1;
            }
            new_a1 = new[] { new_a2 };

            T[][][][] new_a0;
            new_a0 = new T[a0.Length + 1][][][];
            Array.Copy(a0, new_a0, a0.Length);
            new_a0[a0.Length] = new_a1;
            goto SET0;

        SET3:
            new_a2 = new T[a2.Length][];
            Array.Copy(a2, new_a2, a2.Length);
            new_a2[a2.Length - 1] = new_a3;
        SET2:
            new_a1 = new T[a1.Length][][];
            Array.Copy(a1, new_a1, a1.Length);
            new_a1[a1.Length - 1] = new_a2;
        SET1:
            new_a0 = new T[a0.Length][][][];
            Array.Copy(a0, new_a0, a0.Length);
            new_a0[a0.Length - 1] = new_a1;
        SET0:
            return new FixedVector<T> { a0 = new_a0 };
        }
    }
}
