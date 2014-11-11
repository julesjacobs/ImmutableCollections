using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImmutableCollections
{
    public static class ArrayUtils
    {
        public static T[] Insert<T>(this T[] xs, int i, T x)
        {
            var tmp = new T[xs.Length + 1];
            Array.Copy(xs, 0, tmp, 0, i);
            tmp[i] = x;
            Array.Copy(xs, i, tmp, i + 1, xs.Length - i);
            return tmp;
        }

        public static T[] Remove<T>(this T[] xs, int i)
        {
            var n = xs.Length - 1;
            var tmp = new T[n];
            Array.Copy(xs, 0, tmp, 0, i);
            Array.Copy(xs, i + 1, tmp, i, n - i);
            return tmp;
        }

        public static T[] Slice<T>(this T[] xs, int i, int len)
        {
            var tmp = new T[len];
            Array.Copy(xs, i, tmp, 0, len);
            return tmp;
        }

        public static T[] Copy<T>(this T[] xs)
        {
            return Slice(xs, 0, xs.Length);
        }
    }
}
