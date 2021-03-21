using System.IO;
using FDTD.Space1D;
using FDTD.Space1D.Boundaries.ABC;
using FDTD.Space1D.Sources;
using static System.Math;

namespace ConsoleTest
{
    internal static class Solver1DTest
    {
        private static double Sqr(double x) => x * x;

        public static void Run()
        {
            var solver = new Solver1D(200, 1)
            {
                Sources =
                {
                    new FunctionSource1D(50) { Ez = t => 2 * Exp(-Sqr(t - 30) / 100) },
                },
                Boundaries =
                {
                    MinEz = new ABC1DMin(),
                    MaxHy = new ABC1DMax()
                }
            };

            using var file = File.CreateText("solver1d.txt");
            foreach (var frame in solver.Calculation(250, 1))
                frame.WriteEzTo(file);
        }
    }
}
