//using System;
//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Running;

//namespace ConsoleTest
//{
//    public class BenchmarkTest
//    {
//        public static void Run() => _ = BenchmarkRunner.Run<BenchmarkTest>();

//        private double _X = 13, _Y = 42, _Z = 3;

//        private double X => _X;
//        private double Y => _Y;
//        private double Z => _Z;

//        [Benchmark]
//        public double Fields() => Math.Sqrt(_X * _X + _Y * _Y + _Z * _Z);

//        [Benchmark]
//        public double Properties() => Math.Sqrt(X * X + Y * Y + Z * Z);
//    }
//}
