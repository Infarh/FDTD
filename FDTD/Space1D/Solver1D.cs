using System.Collections.Generic;
using System.Linq;
using FDTD.Space1D.Boundaries;
using FDTD.Space1D.Sources;

// ReSharper disable ForCanBeConvertedToForeach

namespace FDTD.Space1D
{
    public class Solver1D
    {
        private readonly int _Nx;
        private readonly double _dx;

        public ICollection<Source1D> Sources { get; } = new List<Source1D>();

        public Boundaries1D Boundaries { get; } = new();

        public Solver1D(int Nx, double dx) => (_Nx, _dx) = (Nx, dx);

        private double dHy(int i, double[] Ez)
        {
            var ez_dx = (Ez[i + 1] - Ez[i]) / _dx;
            return -ez_dx / Consts.Imp0;
        }

        private double dHz(int i, double[] Ey)
        {
            var ey_dx = (Ey[i + 1] - Ey[i]) / _dx;
            return ey_dx / Consts.Imp0;
        }

        private double dEy(int i, double[] Hz)
        {
            var hz_dx = (Hz[i] - Hz[i - 1]) / _dx;
            return -hz_dx * Consts.Imp0;
        }

        private double dEz(int i, double[] Hy)
        {
            var hy_dx = (Hy[i] - Hy[i - 1]) / _dx;
            return hy_dx * Consts.Imp0;
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

            var sources_e = Sources.Where(s => s.HasE).ToArray();
            var sources_h = Sources.Where(s => s.HasH).ToArray();
            if (sources_e.Length == 0) sources_e = null;
            if (sources_h.Length == 0) sources_h = null;

            var boundaries = Boundaries;

            var i = 0;
            for (var t = 0d; t < T; t += dt)
            {
                boundaries.ApplyH(Hy, Hz);
                ProcessH(Hy, Hz, Ey, Ez);
                sources_h?.ProcessH(t, Hy, Hz);

                boundaries.ApplyE(Ey, Ez);
                ProcessE(Hy, Hz, Ey, Ez);
                sources_e?.ProcessE(t, Ey, Ez);

                yield return new(i++, t, Hy, Hz, Ey, Ez);
            }
        }
    }
}
