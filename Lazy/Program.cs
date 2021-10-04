using System;
using System.Threading;

namespace LazyTest
{
    class MyLazy<T> where T : class
    {
        private T _value = default;
        private Func<T> _factory;

        public MyLazy(Func<T> factory)
        {
            _factory = factory;
        }

        public T Value
        {
            get
            {
                if (_value == default)
                {
                    Interlocked.CompareExchange(ref _value, _factory(), default);
                }
                return _value;
            }
        }
    }

    class Boolean
    {
        public Boolean() => BoolProp = false;
        public bool BoolProp { get; set; }
    }

    class Program
    {
        static Boolean EagerBool = default;
        static readonly Lazy<Boolean> LazyBool = new Lazy<Boolean>(() => new Boolean());
        static readonly MyLazy<Boolean> MyLazyBool = new MyLazy<Boolean>(() => new Boolean());

        static Boolean MyBoolProp
        {
            get
            {
                if (EagerBool == default)
                {
                    Interlocked.CompareExchange(ref EagerBool, new Boolean(), default);
                }
                return EagerBool;
            }
        }

        static void Main(string[] args)
        {
            //Release run:
            //MyBoolProp: 61 ms
            //MyLazyBool: 31 ms
            //LazyBool: 12 ms
            //EagerBool: 5 ms

            {
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                for (int i = 0; i < 10_000_000; i++)
                {
                    var _ = MyBoolProp.BoolProp;
                }

                timer.Stop();
                System.Console.WriteLine("MyBoolProp: {0} ms", timer.ElapsedMilliseconds);
            }

            {
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                for (int i = 0; i < 10_000_000; i++)
                {
                    var _ = MyLazyBool.Value.BoolProp;
                }

                timer.Stop();
                System.Console.WriteLine("MyLazyBool: {0} ms", timer.ElapsedMilliseconds);
            }

            {
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                for (int i = 0; i < 10_000_000; i++)
                {
                    var _ = LazyBool.Value.BoolProp;
                }

                timer.Stop();
                System.Console.WriteLine("LazyBool: {0} ms", timer.ElapsedMilliseconds);
            }

            {
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                for (int i = 0; i < 10_000_000; i++)
                {
                    var _ = EagerBool.BoolProp;
                }

                timer.Stop();
                System.Console.WriteLine("EagerBool: {0} ms", timer.ElapsedMilliseconds);
            }
        }
    }
}
