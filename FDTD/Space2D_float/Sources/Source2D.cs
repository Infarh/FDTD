using System;

namespace FDTD.Space2D_float.Sources
{
    public class Source2D
    {
        private readonly int _i, _j;

        private readonly Func<float, float> _Ex, _Ey, _Ez;
        private readonly Func<float, float> _Hx, _Hy, _Hz;

        public Func<float, float> Ex { get => _Ex; init => _Ex = value; }
        public Func<float, float> Ey { get => _Ey; init => _Ey = value; }
        public Func<float, float> Ez { get => _Ez; init => _Ez = value; }

        public Func<float, float> Hx { get => _Hx; init => _Hx = value; }
        public Func<float, float> Hy { get => _Hy; init => _Hy = value; }
        public Func<float, float> Hz { get => _Hz; init => _Hz = value; }

        public bool HasE => _Ex != null || _Ey != null || _Ez != null;
        public bool HasH => _Hx != null || _Hy != null || _Hz != null;

        public Source2D(
            int i, int j,
            Func<float, float> Ex = null,
            Func<float, float> Ey = null,
            Func<float, float> Ez = null,
            Func<float, float> Hx = null,
            Func<float, float> Hy = null,
            Func<float, float> Hz = null
        )
        {
            (_i, _j) = (i, j);
            (_Ex, _Ey, _Ez) = (Ex, Ey, Ez);
            (_Hx, _Hy, _Hz) = (Hx, Hy, Hz);
        }

        public void ProcessH(float t, float[,] Hx, float[,] Hy, float[,] Hz)
        {
            if (_Hx != null) Hx[_i, _j] += _Hx(t);
            if (_Hy != null) Hy[_i, _j] += _Hy(t);
            if (_Hz != null) Hz[_i, _j] += _Hz(t);
        }

        public void ProcessH(
            float t,
            float[,] ChxE, float[,] ChyE, float[,] ChzE,
            float[,] Hx, float[,] Hy, float[,] Hz)
        {
            if (_Hx != null) Hx[_i, _j] += ChxE[_i, _j] * _Hx(t);
            if (_Hy != null) Hy[_i, _j] += ChyE[_i, _j] * _Hy(t);
            if (_Hz != null) Hz[_i, _j] += ChzE[_i, _j] * _Hz(t);
        }

        public void ProcessE(float t, float[,] Ex, float[,] Ey, float[,] Ez)
        {
            if (_Ex != null) Ex[_i, _j] += _Ex(t);
            if (_Ey != null) Ey[_i, _j] += _Ey(t);
            if (_Ez != null) Ez[_i, _j] += _Ez(t);
        }

        public void ProcessE(
            float t,
            float[,] CexH, float[,] CeyH, float[,] CezH,
            float[,] Ex, float[,] Ey, float[,] Ez)
        {
            if (_Ex != null) Ex[_i, _j] += CexH[_i, _j] * _Ex(t);
            if (_Ey != null) Ey[_i, _j] += CeyH[_i, _j] * _Ey(t);
            if (_Ez != null) Ez[_i, _j] += CezH[_i, _j] * _Ez(t);
        }
    }
}