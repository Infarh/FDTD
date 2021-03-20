using System;
using System.Collections.Generic;

using FDTD.Space2D.Boundaries;
using FDTD.Space2D.Sources;

namespace FDTD.Space2D
{
    public class Mesh2D
    {
        private readonly double _dt;
        private readonly int _Nx, _Ny;
        private readonly double _dx, _dy;

        private readonly double[,] _Hx, _Hy, _Hz;
        private readonly double[,] _Ex, _Ey, _Ez;

        private readonly double[,] _Chx, _Chy, _Chz;
        private readonly double[,] _ChxE, _ChyE, _ChzE;

        private readonly double[,] _Cex, _Cey, _Cez;
        private readonly double[,] _CexH, _CeyH, _CezH;

        private readonly Boundaries2D _Boundaries;

        private readonly Source2D[] _SourcesH, _SourcesE;

        private int _TimeIndex;

        public double[,] Ex => _Ex;
        public double[,] Ey => _Ey;
        public double[,] Ez => _Ez;

        public double[,] Hx => _Hx;
        public double[,] Hy => _Hy;
        public double[,] Hz => _Hz;

        internal Mesh2D(
            double dt,
            int Nx, int Ny,
            double dx, double dy,
            double[,] Hx, double[,] Hy, double[,] Hz,
            double[,] Ex, double[,] Ey, double[,] Ez,
            (double[,] Chx, double[,] ChxE) Chx,
            (double[,] Chy, double[,] ChyE) Chy,
            (double[,] Chz, double[,] ChzE) Chz,
            (double[,] Cex, double[,] CexH) Cex,
            (double[,] Cey, double[,] CeyH) Cey,
            (double[,] Cez, double[,] CezH) Cez,
            Boundaries2D Boundaries,
            Source2D[] SourcesH, Source2D[] SourcesE)
        {
            _dt = dt;

            _Nx = Nx;
            _Ny = Ny;

            _dx = dx;
            _dy = dy;

            _Hx = Hx;
            _Hy = Hy;
            _Hz = Hz;

            _Ex = Ex;
            _Ey = Ey;
            _Ez = Ez;
            _Boundaries = Boundaries;

            (_Chx, _ChxE) = Chx;
            (_Chy, _ChyE) = Chy;
            (_Chz, _ChzE) = Chz;

            (_Cex, _CexH) = Cex;
            (_Cey, _CeyH) = Cey;
            (_Cez, _CezH) = Cez;

            _SourcesH = SourcesH.Length == 0 ? null : SourcesH;
            _SourcesE = SourcesE.Length == 0 ? null : SourcesE;
        }

        public IEnumerable<Solver2DFrame> Calculation(double T)
        {
            var count = (int)(T / _dt);
            var t = _TimeIndex * _dt;

            for (var i = 0; i < count; i++)
            {
                _Boundaries.ApplyH(_Hx, _Hy, _Hz);
                ProcessH(
                    _Nx, _Ny,
                    _dx, _dy,
                    _Chx, _Chy, _Chz,
                    _ChxE, _ChyE, _ChzE,
                    _Hx, _Hy, _Hz,
                    _Ex, _Ey, _Ez);
                _SourcesH?.ProcessH(
                    t,
                    _ChxE, _ChyE, _ChzE,
                    _Hx, _Hy, _Hz);

                _Boundaries.ApplyE(_Ex, _Ey, _Ez);
                ProcessE(
                    _Nx, _Ny,
                    _dx, _dy,
                    _Cex, _Cey, _Cez,
                    _CexH, _CeyH, _CezH,
                    _Hx, _Hy, _Hz,
                    _Ex, _Ey, _Ez);
                _SourcesE?.ProcessE(
                    t,
                    _CexH, _CeyH, _CezH,
                    _Ex, _Ey, _Ez);

                yield return new(
                    _TimeIndex + i, 
                    t, 
                    _Hx, _Hy, _Hz, 
                    _Ex, _Ey, _Ez);

                t += _dt;
            }
            _TimeIndex += count;
        }

        private static void ProcessH(
            int Nx, int Ny,
            double dx, double dy,
            double[,] Chx, double[,] Chy, double[,] Chz,
            double[,] ChxE, double[,] ChyE, double[,] ChzE,
            double[,] Hx, double[,] Hy, double[,] Hz,
            double[,] Ex, double[,] Ey, double[,] Ez)
        {
            static double dHx(int i, int j, double dy, double[,] Ez) => (Ez[i, j + 1] - Ez[i, j]) / dy;
            static double dHy(int i, int j, double dx, double[,] Ez) => -(Ez[i + 1, j] - Ez[i, j]) / dx;
            static double dHz(int i, int j, double dx, double dy, double[,] Ex, double[,] Ey)
            {
                var ey_dx = (Ey[i + 1, j] - Ey[i, j]) / dx;
                var ex_dy = (Ex[i, j + 1] - Ex[i, j]) / dy;
                return ey_dx - ex_dy;
            }

            for (var i = 0; i < Nx - 1; i++)
                for (var j = 0; j < Ny - 1; j++)
                {
                    Hx[i, j] = (Chx is null ? Hx[i, j] : Hx[i, j] * Chx[i, j]) - ChxE[i, j] * dHx(i, j, dy, Ez);
                    Hy[i, j] = (Chy is null ? Hy[i, j] : Hy[i, j] * Chy[i, j]) - ChyE[i, j] * dHy(i, j, dx, Ez);
                    Hz[i, j] = (Chz is null ? Hz[i, j] : Hz[i, j] * Chz[i, j]) - ChzE[i, j] * dHz(i, j, dx, dy, Ex, Ey);
                }
        }

        private static void ProcessE(
            int Nx, int Ny,
            double dx, double dy,
            double[,] Cex, double[,] Cey, double[,] Cez,
            double[,] CexH, double[,] CeyH, double[,] CezH,
            double[,] Hx, double[,] Hy, double[,] Hz,
            double[,] Ex, double[,] Ey, double[,] Ez)
        {
            static double dEx(int i, int j, double dy, double[,] Hz) => (Hz[i, j] - Hz[i, j - 1]) / dy;
            static double dEy(int i, int j, double dx, double[,] Hz) => -(Hz[i, j] - Hz[i - 1, j]) / dx;
            static double dEz(int i, int j, double dx, double dy, double[,] Hx, double[,] Hy)
            {
                var hy_dx = (Hy[i, j] - Hy[i - 1, j]) / dx;
                var hx_dy = (Hx[i, j] - Hx[i, j - 1]) / dy;
                return hy_dx - hx_dy;
            }

            for (var i = 1; i < Nx; i++)
                for (var j = 1; j < Ny; j++)
                {
                    Ex[i, j] = (Cex is null ? Ex[i, j] : Ex[i, j] * Cex[i, j]) + CexH[i, j] * dEx(i, j, dy, Hz);
                    Ey[i, j] = (Cey is null ? Ey[i, j] : Ey[i, j] * Cey[i, j]) + CeyH[i, j] * dEy(i, j, dx, Hz);
                    Ez[i, j] = (Cez is null ? Ez[i, j] : Ez[i, j] * Cez[i, j]) + CezH[i, j] * dEz(i, j, dx, dy, Hx, Hy);
                }
        }
    }
}
