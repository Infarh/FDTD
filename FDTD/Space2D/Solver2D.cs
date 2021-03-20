using System;
using System.Collections.Generic;
using System.Linq;

using FDTD.Space2D.Boundaries;
using FDTD.Space2D.Sources;

// ReSharper disable MergeConditionalExpression

namespace FDTD.Space2D
{
    public class Solver2D
    {
        private readonly int _Nx, _Ny;
        private readonly double _dx, _dy;

        private Func<int, int, double> _EpsInitializer;
        private Func<int, int, double> _MuInitializer;
        private Func<int, int, double> _SigmaInitializer;

        public Func<int, int, double> EpsGrid { set => SetEpsGrid(value); }
        public Func<double, double, double> EpsSpace { set => SetEpsSpace(value); }

        public Func<int, int, double> MuGrid { set => SetMuGrid(value); }
        public Func<double, double, double> MuSpace { set => SetMuSpace(value); }

        public Func<int, int, double> SigmaGrid { set => SetSigmaGrid(value); }
        public Func<double, double, double> SigmaSpace { set => SetSigmaSpace(value); }

        public ICollection<Source2D> Sources { get; } = new List<Source2D>();

        public Boundaries2D Boundaries { get; } = new();

        public Solver2D(int Nx, int Ny, double dx, double dy)
        {
            (_Nx, _Ny) = (Nx, Ny);
            (_dx, _dy) = (dx, dy);
        }

        private static double GetValueMin(double Nx, double Ny, Func<int, int, double> Initializer)
        {
            if (Initializer is null) return 1;
            var min = 1d;
            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Nx; j++)
                    min = Math.Max(min, Initializer(i, j));
            return min;
        }

        public double GetMaxStableTimeStep() => 
            Math.Sqrt(GetValueMin(_Nx, _Ny, _EpsInitializer) * GetValueMin(_Nx, _Ny, _MuInitializer))
                / Math.Sqrt(1 / (_dx * _dx) + 1 / (_dy * _dy))
                / Consts.SpeedOfLight;

        public void SetEpsGrid(Func<int, int, double> Setter) => _EpsInitializer = Setter;

        public void SetEpsSpace(Func<double, double, double> Setter) => SetEpsGrid((i, j) => Setter(i * _dx, j * _dx));

        public void SetMuGrid(Func<int, int, double> Setter) => _MuInitializer = Setter;

        public void SetMuSpace(Func<double, double, double> Setter) => SetMuGrid((i, j) => Setter(i * _dx, j * _dx));

        public void SetSigmaGrid(Func<int, int, double> Setter) => _SigmaInitializer = Setter;

        public void SetSigmaSpace(Func<double, double, double> Setter) => SetSigmaGrid((i, j) => Setter(i * _dx, j * _dx));

        private static (double[,] Ch, double[,] ChE) InitializeCh(
            double dt,
            int Nx, int Ny,
            double[,] Sigma, double[,] Mu)
        {
            var ch_e = new double[Nx, Ny];
            if (Sigma is null)
            {
                if (Mu is null)
                {
                    const double mu_inv = 1 / Consts.Mu0;
                    for (var i = 0; i < Nx; i++)
                        for (var j = 0; j < Ny; j++)
                            ch_e[i, j] = dt * mu_inv;
                    return (null, ch_e);
                }

                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                        ch_e[i, j] = dt / (Consts.Mu0 * Mu[i, j]);
                return (null, ch_e);
            }

            var ch = new double[Nx, Ny];
            var dt05 = dt / 2;
            if (Mu is null)
            {
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var sigma = Sigma[i, j] * dt05 / Consts.Mu0;

                        ch[i, j] = (1 - sigma) / (1 + sigma);
                        ch_e[i, j] = dt / (1 + sigma) / Consts.Mu0;
                    }
                return (ch, ch_e);
            }

            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Ny; j++)
                {
                    var mu = Consts.Mu0 * Mu[i, j];
                    var sgm = Sigma[i, j] * dt05 / mu;

                    ch[i, j] = (1 - sgm) / (1 + sgm);
                    ch_e[i, j] = dt / (1 + sgm) / mu;
                }

            return (ch, ch_e);
        }

        private static (double[,] Ce, double[,] CeH) InitializeCe(
            double dt,
            int Nx, int Ny,
            double[,] Sigma, double[,] Eps)
        {
            var ce_h = new double[Nx, Ny];
            if (Sigma is null)
            {
                if (Eps is null)
                {
                    const double eps_inv = 1 / Consts.Eps0;
                    for (var i = 0; i < Nx; i++)
                        for (var j = 0; j < Ny; j++)
                            ce_h[i, j] = dt * eps_inv;
                    return (null, ce_h);
                }

                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                        ce_h[i, j] = dt / (Consts.Eps0 * Eps[i, j]);
                return (null, ce_h);
            }

            var ce = new double[Nx, Ny];
            var dt05 = dt / 2;

            if (Eps is null)
            {
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var sigma = Sigma[i, j] * dt05 / Consts.Eps0;

                        ce[i, j] = (1 - sigma) / (1 + sigma);
                        ce_h[i, j] = dt / (1 + sigma) / Consts.Eps0;
                    }
                return (ce, ce_h);
            }

            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Ny; j++)
                {
                    var eps = Consts.Eps0 * Eps[i, j];
                    var sigma = Sigma[i, j] * dt05 / eps;

                    ce[i, j] = (1 - sigma) / (1 + sigma);
                    ce_h[i, j] = dt / (1 + sigma) / eps;
                }
            return (ce, ce_h);
        }

        private static double[,] CreateArray(int Nx, int Ny, Func<int, int, double> Initializer)
        {
            if (Initializer is null) return null;
            var result = new double[Nx, Ny];
            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Ny; j++)
                    result[i, j] = Initializer(i, j);
            return result;
        }

        private static (double[,] Eps, double[,] Mu, double[,] Sigma) CreateMesh(
            int Nx,
            int Ny,
            Func<int, int, double> Eps,
            Func<int, int, double> Mu,
            Func<int, int, double> Sigma) =>
            (CreateArray(Nx, Ny, Eps), CreateArray(Nx, Ny, Mu), CreateArray(Nx, Ny, Sigma));

        public Mesh2D GetMesh(double dt)
        {
            var (eps, mu, sigma) = CreateMesh(
                _Nx, _Ny,
                _EpsInitializer,
                _MuInitializer,
                _SigmaInitializer);

            var eps_min = 1d;
            if (eps != null)
                for (var i = 0; i < _Nx; i++)
                    for (var j = 0; j < _Nx; j++)
                        eps_min = Math.Min(eps_min, Math.Abs(eps[i, j]));

            var mu_min = 1d;
            if (mu != null)
                for (var i = 0; i < _Nx; i++)
                    for (var j = 0; j < _Nx; j++)
                        mu_min = Math.Min(mu_min, Math.Abs(mu[i, j]));

            var v_max = Consts.SpeedOfLight / Math.Sqrt(eps_min * mu_min);
            var max_stable_dt = Math.Sqrt(1 / (_dx * _dx) + 1 / (_dy * _dy)) / v_max;

            if (dt > max_stable_dt)
                throw new ArgumentOutOfRangeException(nameof(dt), dt, $"Указанное значение dt {dt} превышает максимально допустимый шаг {max_stable_dt}");

            var chx = InitializeCh(dt, _Nx, _Ny, sigma, mu);
            var chy = InitializeCh(dt, _Nx, _Ny, sigma, mu);
            var chz = InitializeCh(dt, _Nx, _Ny, sigma, mu);

            var cex = InitializeCe(dt, _Nx, _Ny, sigma, eps);
            var cey = InitializeCe(dt, _Nx, _Ny, sigma, eps);
            var cez = InitializeCe(dt, _Nx, _Ny, sigma, eps);

            var sources_h = Sources.Where(s => s.HasH).ToArray();
            var sources_e = Sources.Where(s => s.HasE).ToArray();

            var Hx = new double[_Nx, _Ny];
            var Hy = new double[_Nx, _Ny];
            var Hz = new double[_Nx, _Ny];

            var Ex = new double[_Nx, _Ny];
            var Ey = new double[_Nx, _Ny];
            var Ez = new double[_Nx, _Ny];

            return new(
                dt,
                _Nx, _Ny,
                _dx, _dy,
                Ex, Ey, Ez,
                Hx, Hy, Hz,
                chx, chy, chz,
                cex, cey, cez,
                Boundaries,
                sources_h, sources_e);
        }
    }
}
