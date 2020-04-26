using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
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
        //Solve Code Here
        int n = Cin;
        var a = new long[n];
        for (int i = 0; i < n; i++)
        {
            a[i] = Cin;
        }

        var oddSums = new long[n + 1];
        var evenSums = new long[n + 1];
        for (int i = 0; i < n; i++)
        {
            oddSums[i + 1] = oddSums[i] + ((i & 1) == 1 ? a[i] : 0);
            evenSums[i + 1] = evenSums[i] + ((i & 1) == 0 ? a[i] : 0);
        }

        if ((n & 1) == 0)
        {
            Coutln(Max(oddSums[n], evenSums[n]));
            return;
        }

        long ans = oddSums[n];
        for (int i = 0; i < n; i++)
        {
            ans = Max(ans, oddSums[i] + evenSums[n] - evenSums[i + 1]);
            ans = Max(ans, evenSums[i] + oddSums[n] - oddSums[i + 1]);
            if ((i & 1) == 0)
                ans = Max(ans, evenSums[i] + evenSums[n] - evenSums[i + 1]);
            else if (0 <= i - 1 && i + 2 <= n)
                ans = Max(ans, evenSums[n] - evenSums[i + 2] + evenSums[i - 1] + a[i]);
        }

        Coutln(ans);
    }
}

//Other Classes Here

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

        public PriorityQueue()
        {
        }

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

    public class UnionFind
    {
        private readonly int[] _array;

        public UnionFind(int size)
        {
            _array = new int[size];
            for (int i = 0; i < size; i++)
            {
                _array[i] = i;
            }
        }

        public int GetRootNode(int n)
        {
            if (_array[n] == n) return n;
            return _array[n] = GetRootNode(_array[n]);
        }

        public void UnionGroup(int a, int b)
        {
            var rootA = GetRootNode(a);
            var rootB = GetRootNode(b);
            if (rootA == rootB) return;
            _array[rootA] = rootB;
        }

        public bool IsSameGroup(int a, int b) => GetRootNode(a) == GetRootNode(b);

        public bool IsRoot(int n) => _array[n] == n;
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
            //TODO:範囲内に条件を満たす区間が存在しない場合に対応する
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
        return m.Pow((ModInt) (ModInt.Mod - 2));
    }
}


namespace Extensions
{
    public class ConsoleInputExtension
    {
        private static readonly ConsoleInputExtension _cin = new ConsoleInputExtension();

        public static ConsoleInputExtension Cin => _cin;

        private static readonly Queue<string> _inputQueue = new Queue<string>();

        private ConsoleInputExtension()
        {
        }

        public static implicit operator string(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0)
                Console.ReadLine().Split(' ')
                    .ForEach(val => _inputQueue.Enqueue(val));
            return _inputQueue.Dequeue();
        }

        public static implicit operator int(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0)
                Console.ReadLine().Split(' ')
                    .ForEach(val => _inputQueue.Enqueue(val));
            return int.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator long(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0)
                Console.ReadLine().Split(' ')
                    .ForEach(val => _inputQueue.Enqueue(val));
            return long.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator float(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0)
                Console.ReadLine().Split(' ')
                    .ForEach(val => _inputQueue.Enqueue(val));
            return float.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator double(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0)
                Console.ReadLine().Split(' ')
                    .ForEach(val => _inputQueue.Enqueue(val));
            return double.Parse(_inputQueue.Dequeue());
        }
    }

    public static class ConsoleOutputExtension
    {
        private static StringBuilder _builder = new StringBuilder();

        public static void Cout(object o)
        {
            _builder.Append(o);
        }


        public static void CoutF(object o)
        {
            _builder.Append(o);
            Flush();
        }

        public static void Coutln(object o)
        {
            _builder.Append(o);
            _builder.Append('\n');
        }

        public static void Coutln()
        {
            _builder.Append('\n');
        }

        public static void CoutlnF(object o)
        {
            _builder.Append(o);
            _builder.Append('\n');
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
        //最小公倍数
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

        //最大公約数
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