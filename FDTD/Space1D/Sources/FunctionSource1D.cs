using System;

namespace FDTD.Space1D.Sources
{
    public class FunctionSource1D : Source1D
    {
        protected readonly int _i;

        protected readonly Func<double, double> _Ey, _Ez;
        protected readonly Func<double, double> _Hy, _Hz;

        public Func<double, double> Ey { init => _Ey = value; }
        public Func<double, double> Ez { init => _Ez = value; }

        public Func<double, double> Hy { init => _Hy = value; }
        public Func<double, double> Hz { init => _Hz = value; }

        public override bool HasE => _Ey != null || _Ez != null;
        public override bool HasH => _Hy != null || _Hz != null;

        public FunctionSource1D(
            int i,
            Func<double, double> Ey = null,
            Func<double, double> Ez = null,
            Func<double, double> Hy = null,
            Func<double, double> Hz = null
        )
        {
            _i = i;
            (_Ey, _Ez) = (Ey, Ez);
            (_Hy, _Hz) = (Hy, Hz);
        }

        public override void ProcessE(double[] Ey, double[] Ez, double t)
        {
            if (_Ey != null) Ey[_i] += _Ey(t);
            if (_Ez != null) Ez[_i] += _Ez(t);
        }

        public override void ProcessH(double[] Hy, double[] Hz, double t)
        {
            if (_Hy != null) Hy[_i] += _Hy(t);
            if (_Hz != null) Hz[_i] += _Hz(t);
        }
    }
}