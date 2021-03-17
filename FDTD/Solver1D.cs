using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
// ReSharper disable ForCanBeConvertedToForeach

namespace FDTD
{
    public class Solver1D
    {
        private const double imp0 = 120 * Math.PI;

        private readonly int _Nx;
        private readonly double _dx;

        public ICollection<Source1D> Sources { get; } = new List<Source1D>();

        public Boundaries1D Boundaries { get; } = new();

        public Solver1D(int Nx, double dx) => (_Nx, _dx) = (Nx, dx);

        private double dHy(int i, double[] Ez)
        {
            var ez_dx = (Ez[i + 1] - Ez[i]) / _dx;
            return -ez_dx / imp0;
        }

        private double dHz(int i, double[] Ey)
        {
            var ey_dx = (Ey[i + 1] - Ey[i]) / _dx;
            return ey_dx / imp0;
        }

        private double dEy(int i, double[] Hz)
        {
            var hz_dx = (Hz[i] - Hz[i - 1]) / _dx;
            return -hz_dx * imp0;
        }

        private double dEz(int i, double[] Hy)
        {
            var hy_dx = (Hy[i] - Hy[i - 1]) / _dx;
            return hy_dx * imp0;
        }

        private void ProcessH(double[] Hy, double[] Hz, double[] Ey, double[] Ez)
        {
            for (var i = 0; i < _Nx - 1; i++)
            {
                Hy[i] -= dHy(i, Ez);
                Hz[i] -= dHz(i, Ey);
            }
        }

        private void ProcessE(double[] Hy, double[] Hz, double[] Ey, double[] Ez)
        {
            for (var i = 1; i < _Nx; i++)
            {
                Ey[i] += dEy(i, Hz);
                Ez[i] += dEz(i, Hy);
            }
        }

        private static void ApplySourceH(Source1D[] sources, double t, double[] Hy, double[] Hz)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(Hy, Hz, t);
        }

        private static void ApplySourceE(Source1D[] sources, double t, double[] Ey, double[] Ez)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(Ey, Ez, t);
        }

        public IEnumerable<Solver1DFrame> Calculation(double T, double dt)
        {
            var Hy = new double[_Nx];
            var Hz = new double[_Nx];

            var Ey = new double[_Nx];
            var Ez = new double[_Nx];

            var sources = Sources.ToArray();

            var i = 0;
            for (var t = 0d; t < T; t += dt)
            {
                Boundaries.ApplyH(Hy, Hz);
                ProcessH(Hy, Hz, Ey, Ez);
                ApplySourceH(sources, t, Hy, Hz);

                Boundaries.ApplyE(Ey, Ez);
                ProcessE(Hy, Hz, Ey, Ez);
                ApplySourceE(sources, t, Ey, Ez);

                yield return new(i++, t, Hy, Hz, Ey, Ez);
            }
        }
    }

    public abstract class Source1D
    {
        public abstract void ProcessE(double[] Ey, double[] Ez, double t);
        public abstract void ProcessH(double[] Hy, double[] Hz, double t);
    }

    public class FunctionSource1D : Source1D
    {
        protected readonly int _i;

        protected readonly Func<double, double> _Ey, _Ez;
        protected readonly Func<double, double> _Hy, _Hz;

        public Func<double, double> Ey { init => _Ey = value; }
        public Func<double, double> Ez { init => _Ez = value; }

        public Func<double, double> Hy { init => _Hy = value; }
        public Func<double, double> Hz { init => _Hz = value; }

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

    public class Boundaries1D
    {
        public Boundary1DMin MinEy { get; set; }
        public Boundary1DMin MinEz { get; set; }

        public Boundary1DMin MinHy { get; set; }
        public Boundary1DMin MinHz { get; set; }

        public Boundary1DMax MaxEy { get; set; }
        public Boundary1DMax MaxEz { get; set; }

        public Boundary1DMax MaxHy { get; set; }
        public Boundary1DMax MaxHz { get; set; }

        public void ApplyH(double[] Hy, double[] Hz)
        {
            MinHy?.Process(Hy);
            MinHz?.Process(Hy);

            MaxHy?.Process(Hz);
            MaxHz?.Process(Hz);
        }

        public void ApplyE(double[] Ey, double[] Ez)
        {
            MinEy?.Process(Ey);
            MinEz?.Process(Ez);

            MaxEy?.Process(Ey);
            MaxEz?.Process(Ez);
        }
    }

    public abstract class Boundary1D
    {
        public abstract void Process(double[] Field);
    }

    public abstract class Boundary1DMin : Boundary1D { }

    public abstract class Boundary1DMax : Boundary1D { }

    public class ABC1DMin : Boundary1DMin
    {
        public override void Process(double[] Field) => Field[0] = Field[1];
    }

    public class ABC1DMax : Boundary1DMax
    {
        public override void Process(double[] Field) => Field[^1] = Field[^2];
    }
}
