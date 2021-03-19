using System;

namespace FDTD.Space3D.Sources
{
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