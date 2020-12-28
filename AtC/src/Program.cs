using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Extensions;
using DSA;
using static System.Math;
using static Extensions.MathExtension;
using static Extensions.ConsoleInputExtension;
using static Extensions.ConsoleOutputExtension;

class Solver
{
    public void Solve()
    {
        checked
        {
            //Solve Code Here
        }
    }
}

#if !DEBUG
class EntryPoint
{
    static void Main(string[] args)
    {
        new Solver().Solve();
        Flush();
    }
}

#endif

namespace DSA
{
    public class PriorityQueue<T>
    {
        private readonly List<Tuple<int, T>> _list = new List<Tuple<int, T>>();

        public int Count => _list.Count;

        public void Push(T item, int priority)
        {
            _list.Add(Tuple.Create(priority, item));

            int itemIndex = Count - 1, parentIndex;
            while ((parentIndex = GetParentIndex(itemIndex)) != -1 &&
                   priority > _list[parentIndex].Item1)
            {
                Swap(itemIndex, parentIndex);
                itemIndex = parentIndex;
            }
        }

        private int GetParentIndex(int index)
        {
            if (index == 0) return -1;
            return ((index + 1) >> 1) - 1;
        }

        private Tuple<int, int> GetChildrenIndex(int index)
        {
            var item2 = (index + 1) << 1;
            var item1 = item2 - 1;
            return Tuple.Create(item1, item2);
        }

        public T Pop()
        {
            if (Count <= 0) throw new IndexOutOfRangeException();
            var item = _list[0].Item2;
            _list[0] = _list[Count - 1];
            _list.RemoveAt(Count - 1);

            int index = 0;
            Tuple<int, int> childrenIndex = GetChildrenIndex(index);
            while (childrenIndex.Item1 < Count || childrenIndex.Item2 < Count)
            {
                if (childrenIndex.Item2 >= Count || _list[childrenIndex.Item1].Item1 > _list[childrenIndex.Item2].Item1)
                {
                    if (_list[childrenIndex.Item1].Item1 <= _list[index].Item1) return item;

                    Swap(index, childrenIndex.Item1);
                    index = childrenIndex.Item1;
                }
                else
                {
                    if (_list[childrenIndex.Item2].Item1 <= _list[index].Item1) return item;
                    Swap(index, childrenIndex.Item2);
                    index = childrenIndex.Item2;
                }

                childrenIndex = GetChildrenIndex(index);
            }

            return item;
        }

        public T Peek()
        {
            return _list[0].Item2;
        }

        private void Swap(int index1, int index2)
        {
            var tmp = _list[index1];
            _list[index1] = _list[index2];
            _list[index2] = tmp;
        }
    }

    public class UnionFind<T>
    {
        private readonly (int, int)[] _array; //index,count

        public UnionFind(int size)
        {
            _array = new (int, int)[size];
            for (int i = 0; i < size; i++)
            {
                _array[i] = (i, 1);
            }
        }

        public (int index, int count) GetRootNode(int n)
        {
            if (_array[n].Item1 == n) return _array[n];
            return _array[n] = GetRootNode(_array[n].Item1);
        }

        public void UnionGroup(int a, int b)
        {
            var rootA = GetRootNode(a);
            var rootB = GetRootNode(b);
            if (rootA == rootB) return;
            _array[rootB.Item1].Item2 += rootA.Item2;
            _array[rootA.Item1] = rootB;
        }

        public bool IsSameGroup(int a, int b) => GetRootNode(a).Item1 == GetRootNode(b).Item1;

        public bool IsRoot(int n) => _array[n].Item1 == n;
    }

    public delegate T SegTreeCombiner<T>(T item1, T item2);

    public class SegTree<T>
    {
        private readonly T _defaultItem;
        private readonly SegTreeCombiner<T> _func;
        private T[] List;
        private int size;

        public SegTree(T[] list, T defaultItem, SegTreeCombiner<T> func)
        {
            _defaultItem = defaultItem;
            _func = func;
            size = 1;
            while (size < list.Length) size <<= 1;
            List = new T[2 * size - 1];
            for (int i = 0; i < list.Length; i++) List[i + size - 1] = list[i];
            for (int i = list.Length; i < size; i++) List[i + size - 1] = defaultItem;
            for (int i = size - 1 - 1; i >= 0; i--)
            {
                List[i] = _func(List[2 * i + 1], List[2 * i + 2]);
            }
        }

        public T Get(int index)
        {
            return List[index + size - 1];
        }

        public void Update(int index, T value)
        {
            index += size - 1;
            List[index] = value;
            while (index > 0)
            {
                index = (index - 1) >> 1;
                List[index] = _func(List[2 * index + 1], List[2 * index + 2]);
            }
        }

        public T Query(int a, int b)
        {
            return Query(a, b, 0, 0, size);
        }

        private T Query(int a, int b, int k, int l, int r)
        {
            if (r <= a || b <= l) return _defaultItem;
            if (a <= l && r <= b) return List[k];
            return _func(Query(a, b, k * 2 + 1, l, (l + r) >> 1), Query(a, b, k * 2 + 2, (l + r) >> 1, r));
        }
    }

    public static class BinarySearch
    {
        public delegate bool Terms<T>(T i);

        public static int UpperBound(int initLeft, int initRight, Terms<int> term)
        {
            int left = initLeft - 1, right = initRight;
            while (right - left > 1)
            {
                int mid = (left + right) >> 1;
                if (mid != initLeft - 1 && term(mid)) left = mid;
                else right = mid;
            }

            return left;
        }

        public static long UpperBound(long initLeft, long initRight, Terms<long> term)
        {
            long left = initLeft - 1, right = initRight;
            while (right - left > 1)
            {
                long mid = (left + right) >> 1;
                if (mid != initLeft - 1 && term(mid)) left = mid;
                else right = mid;
            }

            return left;
        }

        public static int LowerBound(int initLeft, int initRight, Terms<int> term)
        {
            int left = initLeft - 1, right = initRight;
            while (right - left > 1)
            {
                int mid = (left + right) >> 1;
                if (mid != initRight && term(mid)) right = mid;
                else left = mid;
            }

            return right;
        }

        public static long LowerBound(long initLeft, long initRight, Terms<long> term)
        {
            long left = initLeft - 1, right = initRight;
            while (right - left > 1)
            {
                long mid = (left + right) >> 1;
                if (term(mid)) right = mid;
                else left = mid;
            }

            return right;
        }
    }
}


struct ModInt
{
    public long Value { get; }
    public static long Mod { get; set; } = 1000000000L + 7;

    public ModInt(long l)
    {
        long value = l % Mod;
        while (value < 0) value += Mod;
        Value = value % Mod;
    }

    public static implicit operator long(ModInt m)
    {
        return m.Value;
    }

    public static implicit operator ModInt(long l)
    {
        return new ModInt(l);
    }

    public static ModInt operator +(ModInt a, ModInt b)
    {
        return new ModInt(a.Value + b.Value);
    }

    public static ModInt operator -(ModInt a, ModInt b)
    {
        return new ModInt(a.Value - b.Value);
    }

    public static ModInt operator -(ModInt a)
    {
        return new ModInt(-a.Value);
    }

    public static ModInt operator *(ModInt a, ModInt b)
    {
        return new ModInt(a.Value * b.Value);
    }

    public static ModInt operator /(ModInt a, ModInt b)
    {
        return new ModInt(a * b.Inverse());
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

static class ModIntMath
{
    public static ModInt Pow(this ModInt a, ModInt b)
    {
        var pow = b.Value;
        var m = a;
        ModInt ans = new ModInt(1);
        while (pow != 0)
        {
            if ((pow & 1) == 1)
            {
                ans *= m;
            }

            pow >>= 1;
            m *= m;
        }

        return ans;
    }

    public static ModInt Inverse(this ModInt m)
    {
        var gcd = ExtGCD(m, ModInt.Mod, out var x, out _);
        if (gcd != 1) throw new InvalidOperationException("入力はModと互いに素である必要があります");
        return x;
    }
}


namespace Extensions
{
    public class ConsoleInputExtension
    {
        private static readonly ConsoleInputExtension _cin = new ConsoleInputExtension();

        public static ConsoleInputExtension Cin => _cin;

        private static readonly Queue<string> _inputQueue = new Queue<string>();

        private static void FillQueue(int count = 1)
        {
            while (_inputQueue.Count < count)
                Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .ForEach(val => _inputQueue.Enqueue(val));
        }

        public static implicit operator string(ConsoleInputExtension _)
        {
            FillQueue();
            return _inputQueue.Dequeue();
        }

        public static implicit operator char[](ConsoleInputExtension _)
        {
            FillQueue();
            return _inputQueue.Dequeue().ToCharArray();
        }

        public static implicit operator int(ConsoleInputExtension _)
        {
            FillQueue();
            return int.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator uint(ConsoleInputExtension _)
        {
            FillQueue();
            return uint.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator long(ConsoleInputExtension _)
        {
            FillQueue();
            return long.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator ulong(ConsoleInputExtension _)
        {
            FillQueue();
            return ulong.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator float(ConsoleInputExtension _)
        {
            FillQueue();
            return float.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator double(ConsoleInputExtension _)
        {
            FillQueue();
            return double.Parse(_inputQueue.Dequeue());
        }

        public ArrayInputBuffer this[int n] => new ArrayInputBuffer(n);

        public class ArrayInputBuffer
        {
            private int _length;

            public ArrayInputBuffer(int length)
            {
                _length = length;
            }

            public static implicit operator string[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new string[self._length];
                for (int i = 0; i < self._length; i++) array[i] = _inputQueue.Dequeue();
                return array;
            }

            public static implicit operator int[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new int[self._length];
                for (int i = 0; i < self._length; i++) array[i] = int.Parse(_inputQueue.Dequeue());
                return array;
            }

            public static implicit operator uint[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new uint[self._length];
                for (int i = 0; i < self._length; i++) array[i] = uint.Parse(_inputQueue.Dequeue());
                return array;
            }

            public static implicit operator long[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new long[self._length];
                for (int i = 0; i < self._length; i++) array[i] = long.Parse(_inputQueue.Dequeue());
                return array;
            }

            public static implicit operator ulong[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new ulong[self._length];
                for (int i = 0; i < self._length; i++) array[i] = ulong.Parse(_inputQueue.Dequeue());
                return array;
            }

            public static implicit operator float[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new float[self._length];
                for (int i = 0; i < self._length; i++) array[i] = float.Parse(_inputQueue.Dequeue());
                return array;
            }

            public static implicit operator double[](ArrayInputBuffer self)
            {
                FillQueue(self._length);
                var array = new double[self._length];
                for (int i = 0; i < self._length; i++) array[i] = double.Parse(_inputQueue.Dequeue());
                return array;
            }
        }
    }

    public static class ConsoleOutputExtension
    {
        private static StringBuilder _builder = new StringBuilder();

        public static void Cout<T>(params T[] o)
        {
            for (var i = 0; i < o.Length; i++)
            {
                _builder.Append(o[i]);
                if (i < o.Length - 1) _builder.Append(' ');
            }
        }

        public static void Cout<T>(List<T> o)
        {
            var enumerator = o.GetEnumerator();
            if (!enumerator.MoveNext()) return;
            while (true)
            {
                _builder.Append(enumerator.Current);
                if (enumerator.MoveNext()) _builder.Append(' ');
                else break;
            }
        }

        public static void CoutF<T>(params T[] o)
        {
            Cout(o);
            Flush();
        }

        public static void CoutF<T>(List<T> o)
        {
            Cout(o);
            Flush();
        }

        public static void Coutln<T>(params T[] o)
        {
            Cout(o);
            _builder.Append('\n');
        }

        public static void Coutln<T>(List<T> o)
        {
            Cout(o);
            _builder.Append('\n');
        }

        public static void CoutlnF<T>(params T[] o)
        {
            Coutln(o);
            Flush();
        }

        public static void CoutlnF<T>(List<T> o)
        {
            Coutln(o);
            Flush();
        }

        public static void Flush()
        {
            Console.Write(_builder.ToString());
            _builder.Clear();
        }
    }

    public static class MathExtension
    {
        /// <summary>
        /// 最小公倍数
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        public static int LCM(int num1, int num2)
        {
            var gcd = GCD(num1, num2);
            return num1 * (num2 / gcd);
        }

        public static long LCM(long num1, long num2)
        {
            var gcd = GCD(num1, num2);
            return num1 * (num2 / gcd);
        }

        /// <summary>
        /// 最大公約数
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        public static int GCD(int num1, int num2)
        {
            int a = Math.Max(num1, num2);
            int b = Math.Min(num1, num2);
            if (b == 0) return a;
            int mod;
            while ((mod = a % b) != 0)
            {
                a = b;
                b = mod;
            }

            return b;
        }

        public static long GCD(long num1, long num2)
        {
            long a = Math.Max(num1, num2);
            long b = Math.Min(num1, num2);
            if (b == 0) return a;
            long mod;
            while ((mod = a % b) != 0)
            {
                a = b;
                b = mod;
            }

            return b;
        }

        /// <summary>
        /// solve ax+by=GCD(a,b)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>gcd(a,b)</returns>
        public static int ExtGCD(int a, int b, out int x, out int y)
        {
            x = y = 0;
            if (b == 0)
            {
                x = 1;
                y = 0;
                return a;
            }

            var d = ExtGCD(b, a % b, out y, out x);
            y -= a / b * x;
            return d;
        }

        public static long ExtGCD(long a, long b, out long x, out long y)
        {
            x = y = 0;
            if (b == 0)
            {
                x = 1;
                y = 0;
                return a;
            }

            var d = ExtGCD(b, a % b, out y, out x);
            y -= a / b * x;
            return d;
        }

        /// <summary>
        /// 素因数分解
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static List<(int prime, int count)> PrimeFactorization(int num)
        {
            var n = num;
            var primes = new List<(int, int)>();
            for (int i = 2; i * i < num; i++)
            {
                var k = 0;
                while (n % i == 0)
                {
                    n /= i;
                    k++;
                }

                if (k == 0) continue;
                primes.Add((i, k));
            }

            if (n > 1)
            {
                primes.Add((n, 1));
            }

            return primes;
        }

        public static List<(int prime, int count)> PrimeFactorization(long num)
        {
            var n = num;
            var primes = new List<(int, int)>();
            for (long i = 2; i * i < num; i++)
            {
                var k = 0;
                while (n % i == 0)
                {
                    n /= i;
                    k++;
                }

                if (k == 0) continue;
                primes.Add(((int) i, k));
            }

            if (n > 1)
            {
                primes.Add(((int) n, 1));
            }

            return primes;
        }
    }

    public static class EnumerableExtension
    {
        public delegate void Function<in T>(T val);

        public static void ForEach<T>(this IEnumerable<T> enumerable, Function<T> function)
        {
            foreach (var x in enumerable)
            {
                function(x);
            }
        }
    }
}