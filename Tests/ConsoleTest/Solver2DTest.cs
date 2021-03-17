using System;
using System.Diagnostics;
using System.IO;

using FDTD;

using static System.Math;

namespace ConsoleTest
{
    internal class Solver2DTest
    {
        private static double Sqr(double x) => x * x;

        public static void Run()
        {
            const int Nx = 100;
            const int Ny = 100;

            const double dx = 1;
            const double dy = 1;

            var solver = new Solver2D(Nx, Ny, dx, dy)
            {
                Sources =
                {
                    new (50, 50) { Ez = t => 1 * Exp(-Sqr(t - 30) / 100) }
                },
            };

            const double total_time = 100;
            const double dt = 1;

            const string result_dir_path = "Field2D";
            var result_dir = new DirectoryInfo(result_dir_path);
            if (!result_dir.Exists) 
                result_dir.Create();
            else
                result_dir.Clear("*.csv");

            const string ez_field_file_path = "ez_{0:00}.csv";

            var timer = Stopwatch.StartNew();
            foreach (var frame in solver.Calculation(total_time, dt))
            {
                var ez_file_path = string.Format(ez_field_file_path, frame.Index);
                result_dir.CreateText(ez_file_path).DisposeAfter(frame, (file, data) => data.WriteEzTo(file));
            }

            Console.WriteLine(@"Расчёт завершён за {0:hh\:mm\:ss\.fff} мс", 
                timer.Elapsed);
        }
    }
}
