using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using FDTD.Space2D;
using FDTD.Space2D.Boundaries.ABC;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

using static System.Math;
using static FDTD.Signals.Function;

namespace ConsoleTest
{
    internal class Solver2DTest
    {
        public static void Run()
        {
            const int Nx = 400;
            const int Ny = 100;

            const double dx = 1;
            const double dy = 1;
            const double dt = 1e-9;
            const int steps_count = 5000;
            const double total_time = steps_count * dt;

            var solver = new Solver2D(Nx, Ny, dx, dy)
            {
                Sources =
                {
                    //new (Nx/4, Ny/2, Ez: Function.ExpPulse(total_time / 8))
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
            //solver.SetEpsGrid((i, j) => Sqr(i - Nx / 2) + Sqr(j - Ny / 2) < Nx * Ny / 4d / 2 ? 1 : 4);
            //solver.SetMuGrid((i, j) => 1);
            solver.SetSigmaGrid((i, j) => 0.00005);
            //solver.SetSigmaGrid((i, j) => Sqr(i - Nx / 2) + Sqr(j - Ny / 2) < Nx * Ny / 4d / 2 ? 0 : 0.001);


            const string result_dir_path = "Field2D";
            var result_dir = new DirectoryInfo(result_dir_path);
            if (result_dir.Exists)
                result_dir.Clear("*.csv");
            else
                result_dir.Create();

            const string ez_field_file_path = "ez_{0:00}.csv";

            var gif = new Image<Rgba32>(Nx, Ny);

            var max_dt = solver.GetMaxStableTimeStep();

            var mesh = solver.GetMesh(dt);

            Enumerable.Range(0, Ny).Foreach(j => mesh.Ez[Nx / 4, j] = 1);

            //mesh.Hx[Nx / 4, Ny / 2] = 1;

            var timer = Stopwatch.StartNew();
            var v_min = double.PositiveInfinity;
            var v_max = double.NegativeInfinity;
            //using var signals = File.CreateText("signals.txt");
            foreach (var frame in mesh.Calculation(total_time).Skip(1).Where(frame => frame.Index % 25 == 0))
            {
                var elapsed = timer.Elapsed;

                //var ss = Enumerable.Range(0, 21).Select(i => frame.Ez[Nx / 4 - 10 + i, Ny / 2]).ToArray();
                //signals.WriteLine("{0};{1}",
                //    frame.Index,
                //    string.Join(";", ss));

                var frame_time = elapsed.TotalSeconds / frame.Index;
                var remained = TimeSpan.FromSeconds((steps_count - frame.Index) * frame_time);
                Console.Title = $"{(frame.Index + 1) / (double)steps_count:p1} - {elapsed:hh\\:mm\\:ss} ({frame_time * 1000:f2}мс/frame - {1 / frame_time:f3} fps) осталось {remained:hh\\:mm\\:ss}";

                var ez_file_path = string.Format(ez_field_file_path, frame.Index);
                result_dir.CreateText(ez_file_path).DisposeAfter(frame, (file, data) => data.WriteEzTo(file));

                var img = gif.Frames.CreateFrame();
                //img.Metadata.GetGifMetadata().FrameDelay = 0;

                var p_max = double.NegativeInfinity;
                var field = frame.Ez;
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var power = frame.GetPowerAbs(i, j);
                        p_max = Max(power, p_max);
                        var db = 10 * Log10(power);

                        var (red, green, blue) = PixelLn(db, -45, -80);

                        img[i, j] = new Rgba32(red, green, blue, 255);

                        if (double.IsNaN(db) || double.IsInfinity(db)) continue;
                        v_min = Min(v_min, db);
                        v_max = Max(v_max, db);
                    }

                Console.WriteLine("{0,9:f6} {1:f3} {2:f3}", field[Nx / 4, Ny / 2], v_min, v_max);
            }

            Console.WriteLine("-------------------");
            Console.WriteLine("min:{0}", v_min);
            Console.WriteLine("max:{0}", v_max);

            Console.WriteLine(@"Расчёт завершён за {0:hh\:mm\:ss\.fff} мс",
                timer.Elapsed);
            gif.Save("ez_abc.gif", new GifEncoder());
        }

        private static (byte Red, byte Green, byte Blue) PixelLn(double dbValue, double dbMax, double dbMin)
        {
            const double k = -6;
            const double grn0 = 1 / 3d;
            const double glu0 = 2 / 3d;

            static double Sgm(double x) => Exp(-x * x);

            var db_value = dbValue - dbMax;
            var db_min = dbMin - dbMax;
            var red = Sgm((db_value / db_min) * k);
            var grn = Sgm((db_value / db_min - grn0) * k);
            var blu = Sgm((db_value / db_min - glu0) * k);

            return ((byte)(255 * red), (byte)(255 * grn), (byte)(255 * blu));
        }

        private static byte Pixel(double x) => (byte)(255 * Sigmoid(x, 0.0001));

        private static double Sigmoid(double x, double alpha) => 1 / (1 + Exp(-x / alpha));
    }
}
