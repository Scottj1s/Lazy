using System;
using System.Threading;

namespace LazyTest
{
    class MyLazy<T> where T : class
    {
        private T _value = null;
        private Func<T> _factory;

        public MyLazy(Func<T> factory)
        {
            _factory = factory;
        }

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    Interlocked.CompareExchange(ref _value, _factory(), null);
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
        static Boolean EagerBool = null;
        static readonly Lazy<Boolean> LazyBool = new Lazy<Boolean>(() => new Boolean());
        static readonly MyLazy<Boolean> MyLazyBool = new MyLazy<Boolean>(() => new Boolean());

        static Boolean MyBoolProp
        {
            get
            {
                if (EagerBool == null)
                {
                    Interlocked.CompareExchange(ref EagerBool, new Boolean(), null);
                }
                return EagerBool;
            }
        }

        static void Run()
        {
            EagerBool = null;

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
            EagerBool = null;

            {
                var timer = new System.Diagnostics.Stopwatch();
                timer.Start();

                for (int i = 0; i < 10_000_000; i++)
                {
                    if (EagerBool == null)
                    {
                        Interlocked.CompareExchange(ref EagerBool, new Boolean(), null);
                    }

                    var _ = EagerBool.BoolProp;
                }

                timer.Stop();
                System.Console.WriteLine("Inline: {0} ms", timer.ElapsedMilliseconds);
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

        static void Main(string[] args)
        {
            System.Console.WriteLine("Cold:");
            Run();
            System.Console.WriteLine();
            System.Console.WriteLine("Hot:");
            Run();

            //Cold:
            //MyBoolProp: 26 ms
            //Inline: 11 ms
            //MyLazyBool: 32 ms
            //LazyBool: 12 ms
            //EagerBool: 5 ms

            //Hot:
            //MyBoolProp: 29 ms
            //Inline: 8 ms
            //MyLazyBool: 28 ms
            //LazyBool: 8 ms
            //EagerBool: 5 ms
        }
    }
}