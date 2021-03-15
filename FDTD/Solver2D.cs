using System;
using System.Collections.Generic;
using System.Linq;

namespace FDTD
{
    public class Solver2D
    {
        private const double imp0 = 120 * Math.PI;

        private readonly int _Nx, _Ny;
        private readonly double _dx, _dy;

        private readonly double[,] _Ex, _Ey, _Ez; // 0..N-2
        private readonly double[,] _Hx, _Hy, _Hz; // 1..N-1

        public ICollection<Source2D> Sources { get; } = new List<Source2D>();

        public Solver2D(int Nx, int Ny, double dx, double dy)
        {
            (_Nx, _Ny) = (Nx, Ny);
            (_dx, _dy) = (dx, dy);

            _Ex = new double[Nx, Ny];
            _Ey = new double[Nx, Ny];
            _Ez = new double[Nx, Ny];

            _Hx = new double[Nx, Ny];
            _Hy = new double[Nx, Ny];
            _Hz = new double[Nx, Ny];
        }

        private double dHx(int i, int j)
        {
            var ez_dy = (_Ez[i, j + 1] - _Ez[i, j]) / _dy;
            var ey_dz = 0;
            return (ez_dy - ey_dz) / imp0;
        }

        private double dHy(int i, int j)
        {
            var ex_dz = 0;
            var ez_dx = (_Ez[i + 1, j] - _Ez[i, j]) / _dx;
            return (ex_dz - ez_dx) / imp0;
        }

        private double dHz(int i, int j)
        {
            var ey_dx = (_Ey[i + 1, j] - _Ey[i, j]) / _dx;
            var ex_dy = (_Ex[i, j + 1] - _Ex[i, j]) / _dy;
            return (ey_dx - ex_dy) / imp0;
        }

        private double dEx(int i, int j)
        {
            var hz_dy = (_Hz[i, j] - _Hz[i, j - 1]) / _dy;
            var hy_dz = 0;
            return (hz_dy - hy_dz) * imp0;
        }

        private double dEy(int i, int j)
        {
            var hx_dz = 0;
            var hz_dx = (_Hz[i, j] - _Hz[i - 1, j]) / _dx;
            return (hx_dz - hz_dx) * imp0;
        }

        private double dEz(int i, int j)
        {
            var hy_dx = (_Hy[i, j] - _Hy[i - 1, j]) / _dx;
            var hx_dy = (_Hx[i, j] - _Hx[i, j - 1]) / _dy;
            return (hy_dx - hx_dy) * imp0;
        }

        private void ProcessH()
        {
            for (var i = 0; i < _Nx - 1; i++)
                for (var j = 0; j < _Ny - 1; j++)
                {
                    _Hx[i, j] -= dHx(i, j);
                    _Hy[i, j] -= dHy(i, j);
                    _Hz[i, j] -= dHz(i, j);
                }
        }

        private void ProcessE()
        {
            for (var i = 1; i < _Nx; i++)
                for (var j = 1; j < _Ny; j++)
                {
                    _Ex[i, j] += dEx(i, j);
                    _Ey[i, j] += dEy(i, j);
                    _Ez[i, j] += dEz(i, j);
                }
        }

        private void ApplyBoundariesH() { }
        private void ApplyBoundariesE() { }

        private void ApplySourceH(Source2D[] sources, double t)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(_Hx, _Hy, _Hz, t);
        }

        private void ApplySourceE(Source2D[] sources, double t)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(_Ex, _Ey, _Ez, t);
        }

        private void PrcessTime(double T, double dt)
        {
            var sources = Sources.ToArray();

            for (var t = 0d; t < T; t += dt)
            {
                ApplyBoundariesH();
                ProcessH();
                ApplySourceH(sources, t);

                ApplyBoundariesE();
                ProcessE();
                ApplySourceE(sources, t);
            }
        }

        public static void Test()
        {
            static double Sqr(double x) => x * x;
            var solver = new Solver2D(1024, 1024, 1, 1)
            {
                Sources =
                {
                    new (3,5) { Ez = t => Math.Exp(-Sqr(t - 30)/100) }
                },
            };

            solver.PrcessTime(450, 1);
        }
    }

    public class Source2D
    {
        private readonly int _i, _j;

        private readonly Func<double, double> _Ex, _Ey, _Ez;
        private readonly Func<double, double> _Hx, _Hy, _Hz;

        public Func<double, double> Ex { init => _Ex = value; }
        public Func<double, double> Ey { init => _Ey = value; }
        public Func<double, double> Ez { init => _Ez = value; }

        public Func<double, double> Hx { init => _Hx = value; }
        public Func<double, double> Hy { init => _Hy = value; }
        public Func<double, double> Hz { init => _Hz = value; }

        public Source2D(
            int i, int j,
            Func<double, double> Ex = null,
            Func<double, double> Ey = null,
            Func<double, double> Ez = null,
            Func<double, double> Hx = null,
            Func<double, double> Hy = null,
            Func<double, double> Hz = null
            )
        {
            (_i, _j) = (i, j);
            (_Ex, _Ey, _Ez) = (Ex, Ey, Ez);
            (_Hx, _Hy, _Hz) = (Hx, Hy, Hz);
        }

        public void ProcessE(double[,] Ex, double[,] Ey, double[,] Ez, double t)
        {
            if (_Ex != null) Ex[_i, _j] = _Ex(t);
            if (_Ey != null) Ey[_i, _j] = _Ey(t);
            if (_Ez != null) Ez[_i, _j] = _Ez(t);
        }

        public void ProcessH(double[,] Hx, double[,] Hy, double[,] Hz, double t)
        {
            if (_Hx != null) Hx[_i, _j] = _Hx(t);
            if (_Hy != null) Hy[_i, _j] = _Hy(t);
            if (_Hz != null) Hz[_i, _j] = _Hz(t);
        }
    }
}
