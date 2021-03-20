using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using FDTD.Signals;
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
            const int Ny = 400;

            const double dx = 1;
            const double dy = 1;
            const double dt = 1e-9;
            const int steps_count = 2000;
            const double total_time = steps_count * dt;

            const int tau_n = 100;
            const double tau = dt * tau_n;

            var source = ExpPulse(tau);

            var solver = new Solver2D(Nx, Ny, dx, dy)
            {
                Sources =
                {
                    new (Nx/4, Ny/2, Ez: source)
                },
                Boundaries =
                {
                    X =
                    {
                        //Min = new ABC2DMinX(),
                        //Max = new ABC2DMaxX()
                    },
                    Y =
                    {
                        //Min = new ABC2DMinY(),
                        //Max = new ABC2DMaxY()
                    }
                }
            };
            //solver.SetEpsGrid((i, _) => i < Nx / 2 ? 1 : 4);
            //solver.SetSigmaGrid((i, _) => i < 2 * Nx / 4 ? 0 : Sqr((i - 2 * Nx / 4d) / (Nx / 4d)) * 0.0001);
            const int PMLlen = 50;
            const int PMLxMin = PMLlen;
            const int PMLxMax = Nx - PMLlen;
            const int PMLyMin = PMLlen;
            const int PMLyMax = Ny - PMLlen;
            const double PMLsigma = 0.0015;
            solver.SetSigmaGrid((i, j) => (i, j) switch
            {
                ( <= PMLxMin, >= PMLyMin and <= PMLyMax) => Pow4(-((double)i - PMLxMin) / PMLlen) * PMLsigma,
                ( >= PMLxMax, >= PMLyMin and <= PMLyMax) => Pow4(+((double)i - PMLxMax) / PMLlen) * PMLsigma,
                ( >= PMLxMin and <= PMLxMax, <= PMLyMin) => Pow4(-((double)j - PMLyMin) / PMLlen) * PMLsigma,
                ( >= PMLxMin and <= PMLxMax, >= PMLyMax) => Pow4(+((double)j - PMLyMax) / PMLlen) * PMLsigma,
                ( <= PMLxMin, <= PMLyMin) => Pow4(Sqrt(Sqr(i - PMLlen) + Sqr(j - PMLlen)) / PMLlen) * PMLsigma,
                ( >= PMLxMax, <= PMLyMin) => Pow4(Sqrt(Sqr(i - PMLxMax) + Sqr(j - PMLlen)) / PMLlen) * PMLsigma,
                ( <= PMLxMin, >= PMLyMax) => Pow4(Sqrt(Sqr(i - PMLlen) + Sqr(j - PMLyMax)) / PMLlen) * PMLsigma,
                ( >= PMLxMax, >= PMLyMax) => Pow4(Sqrt(Sqr(i - PMLxMax) + Sqr(j - PMLyMax)) / PMLlen) * PMLsigma,
                _ => 0
            });
            //solver.SetEpsGrid((i, j) => Sqr(i - Nx / 2) + Sqr(j - Ny / 2) < Nx * Ny / 4d / 2 ? 1 : 4);
            //solver.SetMuGrid((i, j) => 1);
            //solver.SetSigmaGrid((i, j) => 0.00005);
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

            //Enumerable.Range(0, Ny).Foreach(j => mesh.Ez[Nx / 4, j] = 1);

            //mesh.Hx[Nx / 4, Ny / 2] = 1;

            var timer = Stopwatch.StartNew();
            var v_max = double.NegativeInfinity;
            using var signals = File.CreateText("signals.txt");
            signals.WriteLine("N;Pin;Pout;ExIn;EyIn;EzIn;HxIn;HyIn;HzIn;ExOut;EyOut;EzOut;HxOut;HyOut;HzOut;");
            foreach (var frame in mesh.Calculation(total_time)/*.Skip(1).Where(frame => frame.Index % 25 == 0)*/)
            {
                //signals.WriteLine("{0};{1}",
                //    frame.Index,
                //    string.Join(";",
                //       /*  1 */ source(frame.Time),

                //       /*  2 */ frame.GetPowerAbsdb(Nx / 4 + 00, Ny / 2),
                //       /*  3 */ frame.GetPowerAbsdb(Nx / 4 + 10, Ny / 2),
                //       /*  4 */ frame.GetPowerAbsdb(Nx / 4 + 30, Ny / 2),

                //       /*  5 */ frame.Ex[Nx / 4 + 00, Ny / 2],
                //       /*  6 */ frame.Ey[Nx / 4 + 00, Ny / 2],
                //       /*  7 */ frame.Ez[Nx / 4 + 00, Ny / 2],
                //       /*  8 */ frame.Hx[Nx / 4 + 00, Ny / 2],
                //       /*  9 */ frame.Hy[Nx / 4 + 00, Ny / 2],
                //       /* 10 */ frame.Hz[Nx / 4 + 00, Ny / 2],

                //       /* 11 */ frame.Ex[Nx / 4 + 10, Ny / 2],
                //       /* 12 */ frame.Ey[Nx / 4 + 10, Ny / 2],
                //       /* 13 */ frame.Ez[Nx / 4 + 10, Ny / 2],
                //       /* 14 */ frame.Hx[Nx / 4 + 10, Ny / 2],
                //       /* 15 */ frame.Hy[Nx / 4 + 10, Ny / 2],
                //       /* 16 */ frame.Hz[Nx / 4 + 10, Ny / 2],

                //       /* 17 */ frame.Ex[Nx / 4 + 30, Ny / 2],
                //       /* 18 */ frame.Ey[Nx / 4 + 30, Ny / 2],
                //       /* 19 */ frame.Ez[Nx / 4 + 30, Ny / 2],
                //       /* 20 */ frame.Hx[Nx / 4 + 30, Ny / 2],
                //       /* 21 */ frame.Hy[Nx / 4 + 30, Ny / 2],
                //       /* 22 */ frame.Hz[Nx / 4 + 30, Ny / 2]
                //        )
                //    );

                if (frame.Index % 25 != 0 || frame.Index == 0) continue;

                var elapsed = timer.Elapsed;
                var frame_time = elapsed.TotalSeconds / frame.Index;
                var remained = TimeSpan.FromSeconds((steps_count - frame.Index) * frame_time);
                Console.Title = $"{(frame.Index + 1) / (double)steps_count:p1} - {elapsed:hh\\:mm\\:ss} ({frame_time * 1000:f2}мс/frame - {1 / frame_time:f3} fps) осталось {remained:hh\\:mm\\:ss}";

                var ez_file_path = string.Format(ez_field_file_path, frame.Index);
                result_dir.CreateText(ez_file_path).DisposeAfter(frame, (file, data) => data.WriteEzTo(file));

                var img = gif.Frames.CreateFrame();

                var field = frame.Ez;
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var db = frame.GetPowerAbsdb(i, j);

                        //var (red, green, blue) = PixelLn(db, 0, -80);
                        var (red, green, blue) = Pixel(field[i, j], -2, 2);

                        blue = (i, j) switch
                        {
                            ( <= PMLxMin, >= PMLyMin and <= PMLyMax) => 100,
                            ( >= PMLxMax, >= PMLyMin and <= PMLyMax) => 100,
                            ( >= PMLxMin and < PMLxMax, <= PMLyMin) => 100,
                            ( >= PMLxMin and < PMLxMax, >= PMLyMax) => 100,
                            ( <= PMLxMin, <= PMLyMin) => 100,
                            ( <= PMLxMin, >= PMLyMax) => 100,
                            ( >= PMLxMin, <= PMLyMin) => 100,
                            ( >= PMLxMin, >= PMLyMax) => 100,
                            _ => 0
                        };

                        img[i, j] = new Rgba32(red, green, blue, 255);

                        if (double.IsNaN(db) || double.IsInfinity(db)) continue;
                        v_max = Max(v_max, db);
                    }

                Console.WriteLine(string.Join(" ",
                    new[]
                    {
                        frame.GetPowerAbsdb(1 * Nx/8, Ny/2),
                        frame.GetPowerAbsdb(2 * Nx/8, Ny/2),
                        frame.GetPowerAbsdb(3 * Nx/8, Ny/2),
                        frame.GetPowerAbsdb(4 * Nx/8, Ny/2),
                        frame.GetPowerAbsdb(5 * Nx/8, Ny/2),
                        frame.GetPowerAbsdb(6 * Nx/8, Ny/2),
                        frame.GetPowerAbsdb(7 * Nx/8, Ny/2),
                    }.Select(v => $"{v,-8: ###0.##;-###0.##}")));
            }

            Console.WriteLine("-------------------");
            Console.WriteLine("max:{0}", v_max);

            Console.WriteLine(@"Расчёт завершён за {0:hh\:mm\:ss\.fff} мс",
                timer.Elapsed);
            gif.Save("field.gif", new GifEncoder());
        }

        private static (byte Red, byte Green, byte Blue) PixelLn(double dbValue, double dbMax, double dbMin)
        {
            const double k = -6;
            const double grn0 = 1 / 3d;
            const double glu0 = 2 / 3d;

            var db_value = dbValue - dbMax;
            var db_min = dbMin - dbMax;
            var red = db_value < dbMax ? 1 : Gauss((db_value / db_min) * k);
            var grn = Gauss((db_value / db_min - grn0) * k);
            var blu = Gauss((db_value / db_min - glu0) * k);

            return ((byte)(255 * red), (byte)(255 * grn), (byte)(255 * blu));
        }

        private static (byte Red, byte Green, byte Blue) Pixel(double x, double Min, double Max)
        {
            var min = Math.Min(Min, Max);
            var max = Math.Max(Min, Max);
            var x0 = (min + max) / 2;
            var d = (max - min) / 3;

            var red = x > max ? 1 : Gauss((x - max) / d);
            var grn = x < min ? 1 : Gauss((x - min) / d);
            //var blu = Gauss((x - x0) / d);
            var blu = 0;

            return ((byte)(255 * red), (byte)(255 * grn), (byte)(255 * blu));
        }

        private static byte Pixel(double x) => (byte)(255 * Sigmoid(x, 0.0001));

        private static double Sigmoid(double x, double alpha) => 1 / (1 + Exp(-x / alpha));
    }
}
