using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using FDTD;
using FDTD.Space2D;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using static System.Math;

namespace ConsoleTest
{
    internal class Solver2DTest
    {
        private static double Sqr(double x) => x * x;

        public static void Run()
        {
            const int Nx = 200;
            const int Ny = 200;

            const double dx = 1;
            const double dy = 1;
            const double dt = 1e-9;
            const int steps_count = 5000;
            const double total_time = steps_count * dt;

            var solver = new Solver2D(Nx, Ny, dx, dy)
            {
                Sources =
                {
                    new (Nx/2, Ny/2) { Ez = t => 1 * Exp(-Sqr(t - total_time / 3d) / total_time) }
                },
                Boundaries =
                {
                    X =
                    {
                        Min = new ABC2DMinX(),
                        Max = new ABC2DMaxX()
                    },
                    Y =
                    {
                        Min = new ABC2DMinY(),
                        Max = new ABC2DMaxY()
                    }
                }
            };
            solver.SetEpsGrid((i, j) => Sqr(i - Nx / 2) + Sqr(j - Ny / 2) < Nx * Ny / 4d / 2 ? 1 : 4);
            //solver.SetMuGrid((i, j) => 1);
            solver.SetSigmaGrid((i, j) => Sqr(i - Nx / 2) + Sqr(j - Ny / 2) < Nx * Ny / 4d / 2 ? 0 : 0.001);


            const string result_dir_path = "Field2D";
            var result_dir = new DirectoryInfo(result_dir_path);
            if (!result_dir.Exists)
                result_dir.Create();
            else
                result_dir.Clear("*.csv");

            const string ez_field_file_path = "ez_{0:00}.csv";

            var gif = new Image<Rgba32>(Nx, Ny);
            //(double[,] Ex, double[,] Ey, double[,] Ez) E;
            //(double[,] Hx, double[,] Hy, double[,] Hz) H;

            var timer = Stopwatch.StartNew();
            foreach (var frame in solver.Calculation(total_time, dt).Skip(1).Where(frame => frame.Index % 25 == 0))
            {
                var elapsed = timer.Elapsed;

                var frame_time = elapsed.TotalSeconds / frame.Index;
                var remained = TimeSpan.FromSeconds((steps_count - frame.Index) * frame_time);
                Console.Title = $"{frame.Index / (double)steps_count:p} - {elapsed:hh\\:mm\\:ss} ({frame_time * 1000:f2}мс/frame - {1 / frame_time:f3} fps) осталось {remained:hh\\:mm\\:ss}";

                var ez_file_path = string.Format(ez_field_file_path, frame.Index);
                result_dir.CreateText(ez_file_path).DisposeAfter(frame, (file, data) => data.WriteEzTo(file));

                var img = gif.Frames.CreateFrame();
                //img.Metadata.GetGifMetadata().FrameDelay = 0;

                var power = 0d;
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var red = Pixel(frame.Ez[i, j]);
                        var green = Pixel(frame.Ey[i, j]);
                        var blue = Pixel(frame.Ez[i, j]);
                        img[i, j] = new Rgba32(red, green, blue, 255);

                        power += Sqr(frame.Ex[i, j]);
                        power += Sqr(frame.Ey[i, j]);
                        power += Sqr(frame.Ez[i, j]);

                        power += Sqr(frame.Hx[i, j]);
                        power += Sqr(frame.Hy[i, j]);
                        power += Sqr(frame.Hz[i, j]);
                    }

                Console.WriteLine(power);
            }

            Console.WriteLine(@"Расчёт завершён за {0:hh\:mm\:ss\.fff} мс",
                timer.Elapsed);
            gif.Save("ez_abc.gif", new GifEncoder());
        }

        private static byte Pixel(double x) => (byte)(255 * Sigmoid(x, 0.0001));

        private static double Sigmoid(double x, double alpha) => 1 / (1 + Exp(-x / alpha));
    }
}
