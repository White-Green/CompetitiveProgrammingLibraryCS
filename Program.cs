using System;
using System.Collections.Generic;
using System.IO;
using Extensions;
using static System.Math;
using static Extensions.MathExtension;
using static Extensions.ConsoleInputExtension;
using static Extensions.ConsoleOutputExtension;

class Solver
{
    public void Solve()
    {
        //Solve Code Here
    }

    //Other Functions Here
}

class EntryPoint
{
    static void Main(string[] args)
    {
        new Solver().Solve();
        Flush();
    }
}

class PriorityQueue<T>
{
    private readonly List<Tuple<int, T>> _list = new List<Tuple<int, T>>();

    public PriorityQueue()
    {
    }

    public void Push(T item, int priority)
    {
        var itemIndex = _list.Count;
        _list.Add(Tuple.Create(priority, item));
        int parentIndex;
        while ((parentIndex = GetParentIndex(itemIndex)) != -1 && priority > _list[parentIndex].Item1)
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
        return item2 >= _list.Count ? null : Tuple.Create(item1, item2);
    }

    public T Pop()
    {
        var item = _list[0].Item2;
        Tuple<int, int> childrenIndex;
        int index = 0;
        while ((childrenIndex = GetChildrenIndex(index)) != null)
        {
            if (_list[childrenIndex.Item1].Item1 > _list[childrenIndex.Item2].Item1)
                _list[index] = _list[childrenIndex.Item1];
            else
                _list[index] = _list[childrenIndex.Item2];
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

struct Rational
{
    private long _numerator;
    private long _denominator;
    public long Numerator => _numerator;
    public long Denominator => _denominator;

    public Rational(long numerator, long denominator)
    {
        var gcd = GCD(numerator, denominator);
        _numerator = numerator     / gcd;
        _denominator = denominator / gcd;
    }

    public static Rational operator +(Rational a, Rational b)
    {
        return new Rational(a._numerator * b._denominator + b._numerator * a._denominator,
                            a._denominator * b._denominator);
    }

    public static Rational operator -(Rational a, Rational b)
    {
        return new Rational(a._numerator * b._denominator - b._numerator * a._denominator,
                            a._denominator * b._denominator);
    }

    public static Rational operator *(Rational a, Rational b)
    {
        return new Rational(a._numerator * b._numerator, a._denominator * b._denominator);
    }

    public static Rational operator /(Rational a, Rational b)
    {
        return new Rational(a._numerator * b._denominator, a._denominator * b._numerator);
    }

    public static explicit operator Rational(int i)
    {
        return new Rational(i, 1);
    }

    public static explicit operator Rational(long l)
    {
        return new Rational(l, 1);
    }

    public static explicit operator Rational(double d)
    {
        long denominator = 1;
        while (d % 1 != 0)
        {
            denominator *= 2;
            d *= 2;
        }

        return new Rational((long) d, denominator);
    }

    public static explicit operator Rational(float f)
    {
        long denominator = 1;
        while (f % 1 != 0)
        {
            denominator *= 2;
            f *= 2;
        }

        return new Rational((long) f, denominator);
    }
}

class UnionFind
{
    private readonly int[] _array;

    public UnionFind(int N)
    {
        _array = new int[N];
        for (int i = 0; i < N; i++)
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
            if (_inputQueue.Count == 0) Console.ReadLine().Split(' ').ForEach(val => _inputQueue.Enqueue(val));
            return _inputQueue.Dequeue();
        }

        public static implicit operator int(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0) Console.ReadLine().Split(' ').ForEach(val => _inputQueue.Enqueue(val));
            return int.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator long(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0) Console.ReadLine().Split(' ').ForEach(val => _inputQueue.Enqueue(val));
            return long.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator float(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0) Console.ReadLine().Split(' ').ForEach(val => _inputQueue.Enqueue(val));
            return float.Parse(_inputQueue.Dequeue());
        }

        public static implicit operator double(ConsoleInputExtension _)
        {
            if (_inputQueue.Count == 0) Console.ReadLine().Split(' ').ForEach(val => _inputQueue.Enqueue(val));
            return double.Parse(_inputQueue.Dequeue());
        }
    }

    public static class ConsoleOutputExtension
    {
        private static StreamWriter _writer = new StreamWriter(Console.OpenStandardOutput()) {AutoFlush = false};

        public static void Cout(object o)
        {
            _writer.Write(o);
        }

        public static void CoutF(object o)
        {
            _writer.Write(o);
            _writer.Flush();
        }

        public static void Coutln(object o)
        {
            _writer.WriteLine(o);
        }

        public static void CoutlnF(object o)
        {
            _writer.WriteLine(o);
            _writer.Flush();
        }

        public static void Flush()
        {
            _writer.Flush();
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