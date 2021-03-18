using System;
using System.Diagnostics;
using System.IO;

using FDTD;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

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
            const double dt = dx / Consts.SpeedOfLight;
            const int steps_count = 100;
            const double total_time = steps_count * dt;

            var solver = new Solver2D(Nx, Ny, dx, dy)
            {
                Sources =
                {
                    new (Nx/2, Ny/2) { Ez = t => 1 * Exp(-Sqr(t - total_time / 3d) / total_time) }
                },
            };
            solver.SetEpsGrid((i, j) => 1);
            solver.SetMuGrid((i, j) => 1);


            const string result_dir_path = "Field2D";
            var result_dir = new DirectoryInfo(result_dir_path);
            if (!result_dir.Exists)
                result_dir.Create();
            else
                result_dir.Clear("*.csv");

            const string ez_field_file_path = "ez_{0:00}.csv";

            var timer = Stopwatch.StartNew();
            var gif = new Image<Rgba32>(Nx, Ny);
            //(double[,] Ex, double[,] Ey, double[,] Ez) E;
            //(double[,] Hx, double[,] Hy, double[,] Hz) H;

            foreach (var frame in solver.Calculation(total_time, dt))
            {
                var ez_file_path = string.Format(ez_field_file_path, frame.Index);
                result_dir.CreateText(ez_file_path).DisposeAfter(frame, (file, data) => data.WriteEzTo(file));

                //var img = new Image<Rgba32>(Nx, Ny);
                var img = gif.Frames.CreateFrame();

                var field = frame.Hx;
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var pixel = GetPixelValue(field[i, j]);
                        img[i, j] = new Rgba32(pixel, pixel, pixel, 255);
                    }

                Console.WriteLine(field[Nx / 2, Ny / 2]);

                //Console.WriteLine(power);
                //if (power > 100)
                //    Console.WriteLine("111");

                //(_, _, E, H) = frame;
            }

            Console.WriteLine(@"Расчёт завершён за {0:hh\:mm\:ss\.fff} мс",
                timer.Elapsed);
            gif.Save("ez.gif", new GifEncoder());
        }

        private static byte GetPixelValue(double value)
        {
            return (byte)(255 / (1 + Exp(-value)));
        }
    }
}
