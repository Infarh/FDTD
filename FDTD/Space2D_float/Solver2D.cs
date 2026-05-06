using System;
using System.Collections.Generic;
using System.Linq;

using FDTD.Space2D_float.Boundaries;
using FDTD.Space2D_float.Sources;

// ReSharper disable MergeConditionalExpression

namespace FDTD.Space2D_float
{
    public class Solver2D
    {
        private readonly int _Nx, _Ny;
        private readonly float _dx, _dy;

        private Func<int, int, float> _EpsInitializer;
        private Func<int, int, float> _MuInitializer;
        private Func<int, int, float> _SigmaInitializer;

        public Func<int, int, float> EpsGrid { set => SetEpsGrid(value); }
        public Func<float, float, float> EpsSpace { set => SetEpsSpace(value); }

        public Func<int, int, float> MuGrid { set => SetMuGrid(value); }
        public Func<float, float, float> MuSpace { set => SetMuSpace(value); }

        public Func<int, int, float> SigmaGrid { set => SetSigmaGrid(value); }
        public Func<float, float, float> SigmaSpace { set => SetSigmaSpace(value); }

        public ICollection<Source2D> Sources { get; } = new List<Source2D>();

        public Boundaries2D Boundaries { get; } = new();

        public Solver2D(int Nx, int Ny, float dx, float dy)
        {
            (_Nx, _Ny) = (Nx, Ny);
            (_dx, _dy) = (dx, dy);
        }

        private static float GetValueMin(float Nx, float Ny, Func<int, int, float> Initializer)
        {
            if (Initializer is null) return 1f;
            var min = 1f;
            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Nx; j++)
                    min = MathF.Max(min, Initializer(i, j));
            return min;
        }

        public float GetMaxStableTimeStep() =>
            MathF.Sqrt(GetValueMin(_Nx, _Ny, _EpsInitializer) * GetValueMin(_Nx, _Ny, _MuInitializer))
                / MathF.Sqrt(1f / (_dx * _dx) + 1f / (_dy * _dy))
                / (float)Consts.SpeedOfLight;

        public void SetEpsGrid(Func<int, int, float> Setter) => _EpsInitializer = Setter;

        public void SetEpsSpace(Func<float, float, float> Setter) => SetEpsGrid((i, j) => Setter(i * _dx, j * _dx));

        public void SetMuGrid(Func<int, int, float> Setter) => _MuInitializer = Setter;

        public void SetMuSpace(Func<float, float, float> Setter) => SetMuGrid((i, j) => Setter(i * _dx, j * _dx));

        public void SetSigmaGrid(Func<int, int, float> Setter) => _SigmaInitializer = Setter;

        public void SetSigmaSpace(Func<float, float, float> Setter) => SetSigmaGrid((i, j) => Setter(i * _dx, j * _dx));

        private static (float[,] Ch, float[,] ChE) InitializeCh(
            float dt,
            int Nx, int Ny,
            float[,] Sigma, float[,] Mu)
        {
            var ch_e = new float[Nx, Ny];
            if (Sigma is null)
            {
                if (Mu is null)
                {
                    float mu_inv = 1f / (float)Consts.Mu0;
                    for (var i = 0; i < Nx; i++)
                        for (var j = 0; j < Ny; j++)
                            ch_e[i, j] = dt * mu_inv;
                    return (null, ch_e);
                }

                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                        ch_e[i, j] = dt / ((float)Consts.Mu0 * Mu[i, j]);
                return (null, ch_e);
            }

            var ch = new float[Nx, Ny];
            float dt05 = dt / 2f;
            if (Mu is null)
            {
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var sigma = Sigma[i, j] * dt05 / (float)Consts.Mu0;

                        ch[i, j] = (1f - sigma) / (1f + sigma);
                        ch_e[i, j] = dt / ((1f + sigma) * (float)Consts.Mu0);
                    }
                return (ch, ch_e);
            }

            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Ny; j++)
                {
                    var mu = (float)Consts.Mu0 * Mu[i, j];
                    var sgm = Sigma[i, j] * dt05 / mu;

                    ch[i, j] = (1f - sgm) / (1f + sgm);
                    ch_e[i, j] = dt / ((1f + sgm) * mu);
                }

            return (ch, ch_e);
        }

        private static (float[,] Ce, float[,] CeH) InitializeCe(
            float dt,
            int Nx, int Ny,
            float[,] Sigma, float[,] Eps)
        {
            var ce_h = new float[Nx, Ny];
            if (Sigma is null)
            {
                if (Eps is null)
                {
                    float eps_inv = 1f / (float)Consts.Eps0;
                    for (var i = 0; i < Nx; i++)
                        for (var j = 0; j < Ny; j++)
                            ce_h[i, j] = dt * eps_inv;
                    return (null, ce_h);
                }

                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                        ce_h[i, j] = dt / ((float)Consts.Eps0 * Eps[i, j]);
                return (null, ce_h);
            }

            var ce = new float[Nx, Ny];
            float dt05 = dt / 2f;

            if (Eps is null)
            {
                for (var i = 0; i < Nx; i++)
                    for (var j = 0; j < Ny; j++)
                    {
                        var sigma = Sigma[i, j] * dt05 / (float)Consts.Eps0;

                        ce[i, j] = (1f - sigma) / (1f + sigma);
                        ce_h[i, j] = dt / ((1f + sigma) * (float)Consts.Eps0);
                    }
                return (ce, ce_h);
            }

            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Ny; j++)
                {
                    var eps = (float)Consts.Eps0 * Eps[i, j];
                    var sigma = Sigma[i, j] * dt05 / eps;

                    ce[i, j] = (1f - sigma) / (1f + sigma);
                    ce_h[i, j] = dt / ((1f + sigma) * eps);
                }
            return (ce, ce_h);
        }

        private static float[,] CreateArray(int Nx, int Ny, Func<int, int, float> Initializer)
        {
            if (Initializer is null) return null;
            var result = new float[Nx, Ny];
            for (var i = 0; i < Nx; i++)
                for (var j = 0; j < Ny; j++)
                    result[i, j] = Initializer(i, j);
            return result;
        }

        private static (float[,] Eps, float[,] Mu, float[,] Sigma) CreateMesh(
            int Nx,
            int Ny,
            Func<int, int, float> Eps,
            Func<int, int, float> Mu,
            Func<int, int, float> Sigma) =>
            (CreateArray(Nx, Ny, Eps), CreateArray(Nx, Ny, Mu), CreateArray(Nx, Ny, Sigma));

        public Mesh2D GetMesh(float dt)
        {
            var (eps, mu, sigma) = CreateMesh(
                _Nx, _Ny,
                _EpsInitializer,
                _MuInitializer,
                _SigmaInitializer);

            //
            var eps_min = 1f;
            if (eps != null)
                for (var i = 0; i < _Nx; i++)
                    for (var j = 0; j < _Ny; j++)
                        eps_min = MathF.Min(eps_min, MathF.Abs(eps[i, j]));

            var mu_min = 1f;
            if (mu != null)
                for (var i = 0; i < _Nx; i++)
                    for (var j = 0; j < _Ny; j++)
                        mu_min = MathF.Min(mu_min, MathF.Abs(mu[i, j]));

            var v_max = (float)Consts.SpeedOfLight / MathF.Sqrt(eps_min * mu_min);
            var max_stable_dt = MathF.Sqrt(1f / (_dx * _dx) + 1f / (_dy * _dy)) / v_max;

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

            var Hx = new float[_Nx, _Ny];
            var Hy = new float[_Nx, _Ny];
            var Hz = new float[_Nx, _Ny];

            var Ex = new float[_Nx, _Ny];
            var Ey = new float[_Nx, _Ny];
            var Ez = new float[_Nx, _Ny];

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