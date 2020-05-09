using System;
using System.Globalization;
using System.Linq;
using System.IO;

namespace ConsoleTest
{
    internal static class FDTD1D
    {
        internal class DataWriter : IDisposable
        {
            private readonly string _Format;
            private readonly StreamWriter _Writer;

            public DataWriter(string FileName, string Format = "f3")
            {
                _Format = Format;
                _Writer = new StreamWriter(FileName, false);
            }

            public void Write(params double[] Data)
            {
                const string separator = ";";
                var formatter = CultureInfo.CurrentCulture;
                var data = Data.Select(v => v.ToString(_Format, formatter));
                var str = string.Join(separator, data);
                _Writer.WriteLine(str);
                //Console.WriteLine(str);
            }

            public void Dispose() => _Writer?.Dispose();
        }

        private const double __Impl0 = 377;
        private const int __Size = 200;

        private static readonly double[] __Hy = new double[__Size];
        private static readonly double[] __Ez = new double[__Size];

        public static void Start()
        {
            using var writer = new DataWriter("data.csv");
            const int max_time = 250;
            for (var t = 0; t < max_time; t++)
            {
                ProcessHy();
                ProcessEz();

                __Ez[50] += F0(t, t0: 30, tau: 100);

                if (t % 10 == 0)
                    writer.Write(__Ez);
            }
        }

        private static double F0(double t, double t0, double tau)
        {
            var t1 = t - t0;
            return Math.Exp(-t1 * t1 / tau);
        }

        private static void ProcessHy()
        {
            for (var i = 0; i < __Size - 1; i++)
                __Hy[i] += (__Ez[i + 1] - __Ez[i]) / __Impl0;
        }

        private static void ProcessEz()
        {
            for (var i = 1; i < __Size; i++)
                __Ez[i] += (__Hy[i] - __Hy[i - 1]) * __Impl0;
        }
    }
}