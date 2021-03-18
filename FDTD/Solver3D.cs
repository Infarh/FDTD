using System;
using System.Collections.Generic;
using System.Linq;

namespace FDTD
{
    public class Solver3D
    {
        private readonly int _Nx, _Ny, _Nz;
        private readonly double _dx, _dy, _dz;

        private readonly double[,,] _Ex, _Ey, _Ez; // 0..N-2
        private readonly double[,,] _Hx, _Hy, _Hz; // 1..N-1

        public ICollection<Source3D> Sources { get; } = new List<Source3D>();

        public Solver3D(int Nx, int Ny, int Nz, double dx, double dy, double dz)
        {
            (_Nx, _Ny, _Nz) = (Nx, Ny, Nz);
            (_dx, _dy, _dz) = (dx, dy, dz);

            _Ex = new double[Nx, Ny, Nz];
            _Ey = new double[Nx, Ny, Nz];
            _Ez = new double[Nx, Ny, Nz];

            _Hx = new double[Nx, Ny, Nz];
            _Hy = new double[Nx, Ny, Nz];
            _Hz = new double[Nx, Ny, Nz];
        }

        private double dHx(int i, int j, int k)
        {
            var ez_dy = (_Ez[i, j + 1, k] - _Ez[i, j, k]) / _dy;
            var ey_dz = (_Ey[i, j, k + 1] - _Ey[i, j, k]) / _dz;
            return (ez_dy - ey_dz) / Consts.Imp0;
        }

        private double dHy(int i, int j, int k)
        {
            var ex_dz = (_Ex[i, j, k + 1] - _Ex[i, j, k]) / _dz;
            var ez_dx = (_Ez[i + 1, j, k] - _Ez[i, j, k]) / _dx;
            return (ex_dz - ez_dx) / Consts.Imp0;
        }

        private double dHz(int i, int j, int k)
        {
            var ey_dx = (_Ey[i + 1, j, k] - _Ey[i, j, k]) / _dx;
            var ex_dy = (_Ex[i, j + 1, k] - _Ex[i, j, k]) / _dy;
            return (ey_dx - ex_dy) / Consts.Imp0;
        }

        private double dEx(int i, int j, int k)
        {
            var hz_dy = (_Hz[i, j, k] - _Hz[i, j - 1, k]) / _dy;
            var hy_dz = (_Hy[i, j, k] - _Hy[i, j, k - 1]) / _dz;
            return (hz_dy - hy_dz) * Consts.Imp0;
        }

        private double dEy(int i, int j, int k)
        {
            var hx_dz = (_Hx[i, j, k] - _Hx[i, j, k - 1]) / _dz;
            var hz_dx = (_Hz[i, j, k] - _Hz[i - 1, j, k]) / _dx;
            return (hx_dz - hz_dx) * Consts.Imp0;
        }

        private double dEz(int i, int j, int k)
        {
            var hy_dx = (_Hy[i, j, k] - _Hy[i - 1, j, k]) / _dx;
            var hx_dy = (_Hx[i, j, k] - _Hx[i, j - 1, k]) / _dy;
            return (hy_dx - hx_dy) * Consts.Imp0;
        }

        private void ProcessH()
        {
            for (var i = 0; i < _Nx - 1; i++)
                for (var j = 0; j < _Ny - 1; j++)
                    for (var k = 0; k < _Nz - 1; j++)
                    {
                        _Hx[i, j, k] -= dHx(i, j, k);
                        _Hy[i, j, k] -= dHy(i, j, k);
                        _Hz[i, j, k] -= dHz(i, j, k);
                    }
        }

        private void ProcessE()
        {
            for (var i = 1; i < _Nx; i++)
                for (var j = 1; j < _Ny; j++)
                    for (var k = 1; k < _Nz; j++)
                    {
                        _Ex[i, j, k] += dEx(i, j, k);
                        _Ey[i, j, k] += dEy(i, j, k);
                        _Ez[i, j, k] += dEz(i, j, k);
                    }
        }

        private void ApplyBoundariesH() { }
        private void ApplyBoundariesE() { }

        private void ApplySourceH(Source3D[] sources, double t)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(_Hx, _Hy, _Hz, t);
        }

        private void ApplySourceE(Source3D[] sources, double t)
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
            var solver = new Solver3D(1024, 1024, 1024, 1, 1, 1)
            {
                Sources =
                {
                    new (3,5,7) { Ez = t => Math.Exp(-Sqr(t - 30)/100) }
                },
            };

            solver.PrcessTime(450, 1);
        }
    }

    public class Source3D
    {
        private readonly int _i, _j, _k;

        private readonly Func<double, double> _Ex, _Ey, _Ez;
        private readonly Func<double, double> _Hx, _Hy, _Hz;

        public Func<double, double> Ex { init => _Ex = value; }
        public Func<double, double> Ey { init => _Ey = value; }
        public Func<double, double> Ez { init => _Ez = value; }

        public Func<double, double> Hx { init => _Hx = value; }
        public Func<double, double> Hy { init => _Hy = value; }
        public Func<double, double> Hz { init => _Hz = value; }

        public Source3D(
            int i, int j, int k,
            Func<double, double> Ex = null,
            Func<double, double> Ey = null,
            Func<double, double> Ez = null,
            Func<double, double> Hx = null,
            Func<double, double> Hy = null,
            Func<double, double> Hz = null
            )
        {
            (_i, _j, _k) = (i, j, k);
            (_Ex, _Ey, _Ez) = (Ex, Ey, Ez);
            (_Hx, _Hy, _Hz) = (Hx, Hy, Hz);
        }

        public void ProcessE(double[,,] Ex, double[,,] Ey, double[,,] Ez, double t)
        {
            if (_Ex != null) Ex[_i, _j, _k] = _Ex(t);
            if (_Ey != null) Ey[_i, _j, _k] = _Ey(t);
            if (_Ez != null) Ez[_i, _j, _k] = _Ez(t);
        }

        public void ProcessH(double[,,] Hx, double[,,] Hy, double[,,] Hz, double t)
        {
            if (_Hx != null) Hx[_i, _j, _k] = _Hx(t);
            if (_Hy != null) Hy[_i, _j, _k] = _Hy(t);
            if (_Hz != null) Hz[_i, _j, _k] = _Hz(t);
        }
    }
}
