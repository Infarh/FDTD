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
        public event Action<((double[,] Ex, double[,] Ey, double[,] Ez) E, (double[,] Hx, double[,] Hy, double[,] Hz) H)> Initialize;

        private readonly int _Nx, _Ny;
        private readonly double _dx, _dy;

        private double[,] _Eps;
        private double[,] _Mu;
        private double[,] _Sigma;

        public ICollection<Source2D> Sources { get; } = new List<Source2D>();

        public Boundaries2D Boundaries { get; } = new();

        public Solver2D(int Nx, int Ny, double dx, double dy)
        {
            (_Nx, _Ny) = (Nx, Ny);
            (_dx, _dy) = (dx, dy);
        }

        public void SetEpsGrid(Func<int, int, double> Setter)
        {
            _Eps ??= new double[_Nx, _Ny];

            for (var i = 0; i < _Nx; i++)
                for (var j = 0; j < _Ny; j++)
                    if (Setter(i, j) > 0)
                        _Eps[i, j] = Setter(i, j);
        }

        public void SetEpsSpace(Func<double, double, double> Setter)
        {
            _Eps ??= new double[_Nx, _Ny];

            for (var i = 0; i < _Nx; i++)
                for (var j = 0; j < _Ny; j++)
                    if (Setter(i * _dx, j * _dy) > 0)
                        _Eps[i, j] = Setter(i * _dx, j * _dy);
        }

        public ref double SetEps(int i, int j)
        {
            _Eps ??= new double[_Nx, _Ny];
            return ref _Eps[i, j];
        }

        public void SetMuGrid(Func<int, int, double> Setter)
        {
            _Mu ??= new double[_Nx, _Ny];

            for (var i = 0; i < _Nx; i++)
                for (var j = 0; j < _Ny; j++)
                    if (Setter(i, j) > 0)
                        _Mu[i, j] = Setter(i, j);
        }

        public void SetMuSpace(Func<double, double, double> Setter)
        {
            _Mu ??= new double[_Nx, _Ny];

            for (var i = 0; i < _Nx; i++)
                for (var j = 0; j < _Ny; j++)
                    if (Setter(i * _dx, j * _dy) > 0)
                        _Mu[i, j] = Setter(i * _dx, j * _dy);
        }

        public ref double SetMu(int i, int j)
        {
            _Mu ??= new double[_Nx, _Ny];
            return ref _Mu[i, j];
        }

        public void SetSigmaGrid(Func<int, int, double> Setter)
        {
            _Sigma ??= new double[_Nx, _Ny];

            for (var i = 0; i < _Nx; i++)
                for (var j = 0; j < _Ny; j++)
                    if (Setter(i, j) >= 0)
                        _Sigma[i, j] = Setter(i, j);
        }

        public void SetSigmaSpace(Func<double, double, double> Setter)
        {
            _Sigma ??= new double[_Nx, _Ny];

            for (var i = 0; i < _Nx; i++)
                for (var j = 0; j < _Ny; j++)
                    if (Setter(i * _dx, j * _dy) >= 0)
                        _Sigma[i, j] = Setter(i * _dx, j * _dy);
        }

        public ref double SetSigma(int i, int j)
        {
            _Sigma ??= new double[_Nx, _Ny];
            return ref _Sigma[i, j];
        }

        private double dHx(int i, int j, double[,] Ez) => (Ez[i, j + 1] - Ez[i, j]) / _dy;

        private double dHy(int i, int j, double[,] Ez) => -(Ez[i + 1, j] - Ez[i, j]) / _dx;

        private double dHz(int i, int j, double[,] Ex, double[,] Ey)
        {
            var ey_dx = (Ey[i + 1, j] - Ey[i, j]) / _dx;
            var ex_dy = (Ex[i, j + 1] - Ex[i, j]) / _dy;
            return ey_dx - ex_dy;
        }

        private double dEx(int i, int j, double[,] Hz) => (Hz[i, j] - Hz[i, j - 1]) / _dy;

        private double dEy(int i, int j, double[,] Hz) => -(Hz[i, j] - Hz[i - 1, j]) / _dx;

        private double dEz(int i, int j, double[,] Hx, double[,] Hy)
        {
            var hy_dx = (Hy[i, j] - Hy[i - 1, j]) / _dx;
            var hx_dy = (Hx[i, j] - Hx[i, j - 1]) / _dy;
            return hy_dx - hx_dy;
        }

        private void ProcessH(
            double[,] Chx, double[,] Chy, double[,] Chz,
            double[,] ChxE, double[,] ChyE, double[,] ChzE,
            double[,] Hx, double[,] Hy, double[,] Hz,
            double[,] Ex, double[,] Ey, double[,] Ez)
        {
            for (var i = 0; i < _Nx - 1; i++)
                for (var j = 0; j < _Ny - 1; j++)
                {
                    Hx[i, j] = (Chx is null ? Hx[i, j] : Hx[i, j] * Chx[i, j]) - ChxE[i, j] * dHx(i, j, Ez);
                    Hy[i, j] = (Chy is null ? Hy[i, j] : Hy[i, j] * Chy[i, j]) - ChyE[i, j] * dHy(i, j, Ez);
                    Hz[i, j] = (Chz is null ? Hz[i, j] : Hz[i, j] * Chz[i, j]) - ChzE[i, j] * dHz(i, j, Ex, Ey);
                }
        }

        private void ProcessE(
            double[,] Cex, double[,] Cey, double[,] Cez,
            double[,] CexH, double[,] CeyH, double[,] CezH,
            double[,] Hx, double[,] Hy, double[,] Hz,
            double[,] Ex, double[,] Ey, double[,] Ez)
        {
            for (var i = 1; i < _Nx; i++)
                for (var j = 1; j < _Ny; j++)
                {
                    Ex[i, j] = (Cex is null ? Ex[i, j] : Ex[i, j] * Cex[i, j]) + CexH[i, j] * dEx(i, j, Hz);
                    Ey[i, j] = (Cey is null ? Ey[i, j] : Ey[i, j] * Cey[i, j]) + CeyH[i, j] * dEy(i, j, Hz);
                    Ez[i, j] = (Cez is null ? Ez[i, j] : Ez[i, j] * Cez[i, j]) + CezH[i, j] * dEz(i, j, Hx, Hy);
                }
        }

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

        public IEnumerable<Solver2DFrame> Calculation(double T, double dt)
        {
            var Hx = new double[_Nx, _Ny];
            var Hy = new double[_Nx, _Ny];
            var Hz = new double[_Nx, _Ny];

            var Ex = new double[_Nx, _Ny];
            var Ey = new double[_Nx, _Ny];
            var Ez = new double[_Nx, _Ny];

            var sources_h = Sources.Where(s => s.HasH).ToArray();
            var sources_e = Sources.Where(s => s.HasE).ToArray();
            if (sources_h.Length == 0) sources_h = null;
            if (sources_e.Length == 0) sources_e = null;

            var boundaries = Boundaries;

            var (c_hx, c_hx_e) = InitializeCh(dt, _Nx, _Ny, _Sigma, _Mu);
            var (c_hy, c_hy_e) = InitializeCh(dt, _Nx, _Ny, _Sigma, _Mu);
            var (c_hz, c_hz_e) = InitializeCh(dt, _Nx, _Ny, _Sigma, _Mu);

            var (c_ex, c_ex_h) = InitializeCe(dt, _Nx, _Ny, _Sigma, _Eps);
            var (c_ey, c_ey_h) = InitializeCe(dt, _Nx, _Ny, _Sigma, _Eps);
            var (c_ez, c_ez_h) = InitializeCe(dt, _Nx, _Ny, _Sigma, _Eps);

            Initialize?.Invoke(((Ex, Ey, Ez), (Hx, Hy, Hz)));

            var i = 0;
            for (var t = 0d; t < T; t += dt)
            {
                boundaries.ApplyH(Hx, Hy, Hz);
                ProcessH(
                    c_hx, c_hy, c_hz,
                    c_hx_e, c_hy_e, c_hz_e,
                    Hx, Hy, Hz,
                    Ex, Ey, Ez);
                sources_h?.ProcessH(t, Hx, Hy, Hz);

                boundaries.ApplyE(Ex, Ey, Ez);
                ProcessE(
                    c_ex, c_ey, c_ez,
                    c_ex_h, c_ey_h, c_ez_h,
                    Hx, Hy, Hz,
                    Ex, Ey, Ez);
                sources_e?.ProcessE(t, Ex, Ey, Ez);

                yield return new(i++, t, Hx, Hy, Hz, Ex, Ey, Ez);
            }
        }
    }
}
