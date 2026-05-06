using System.Collections.Generic;

using FDTD.Space2D_float.Boundaries;
using FDTD.Space2D_float.Sources;

namespace FDTD.Space2D_float
{
    public class Mesh2D
    {
        private readonly float _dt;
        private readonly int _Nx, _Ny;
        private readonly float _dx, _dy;

        private readonly float[,] _Hx, _Hy, _Hz;
        private readonly float[,] _Ex, _Ey, _Ez;

        private readonly float[,] _Chx, _Chy, _Chz;
        private readonly float[,] _ChxE, _ChyE, _ChzE;

        private readonly float[,] _Cex, _Cey, _Cez;
        private readonly float[,] _CexH, _CeyH, _CezH;

        private readonly Boundaries2D _Boundaries;

        private readonly Source2D[] _SourcesH, _SourcesE;

        private int _TimeIndex;

        public float[,] Ex => _Ex;
        public float[,] Ey => _Ey;
        public float[,] Ez => _Ez;

        public float[,] Hx => _Hx;
        public float[,] Hy => _Hy;
        public float[,] Hz => _Hz;

        internal Mesh2D(
            float dt,
            int Nx, int Ny,
            float dx, float dy,
            float[,] Hx, float[,] Hy, float[,] Hz,
            float[,] Ex, float[,] Ey, float[,] Ez,
            (float[,] Chx, float[,] ChxE) Chx,
            (float[,] Chy, float[,] ChyE) Chy,
            (float[,] Chz, float[,] ChzE) Chz,
            (float[,] Cex, float[,] CexH) Cex,
            (float[,] Cey, float[,] CeyH) Cey,
            (float[,] Cez, float[,] CezH) Cez,
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

        public IEnumerable<Solver2DFrame> Calculation(float T)
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
            float dx, float dy,
            float[,] Chx, float[,] Chy, float[,] Chz,
            float[,] ChxE, float[,] ChyE, float[,] ChzE,
            float[,] Hx, float[,] Hy, float[,] Hz,
            float[,] Ex, float[,] Ey, float[,] Ez)
        {
            static float dHx(int i, int j, float dy, float[,] Ez) => (Ez[i, j + 1] - Ez[i, j]) / dy;
            static float dHy(int i, int j, float dx, float[,] Ez) => -(Ez[i + 1, j] - Ez[i, j]) / dx;
            static float dHz(int i, int j, float dx, float dy, float[,] Ex, float[,] Ey)
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
            float dx, float dy,
            float[,] Cex, float[,] Cey, float[,] Cez,
            float[,] CexH, float[,] CeyH, float[,] CezH,
            float[,] Hx, float[,] Hy, float[,] Hz,
            float[,] Ex, float[,] Ey, float[,] Ez)
        {
            static float dEx(int i, int j, float dy, float[,] Hz) => (Hz[i, j] - Hz[i, j - 1]) / dy;
            static float dEy(int i, int j, float dx, float[,] Hz) => -(Hz[i, j] - Hz[i - 1, j]) / dx;
            static float dEz(int i, int j, float dx, float dy, float[,] Hx, float[,] Hy)
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
