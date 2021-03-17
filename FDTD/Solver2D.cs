using System;
using System.Collections.Generic;
using System.Linq;

namespace FDTD
{
    public class Solver2D
    {
        private const double imp0 = 120 * Math.PI;

        private readonly int _Nx, _Ny;
        private readonly double _dx, _dy;

        public ICollection<Source2D> Sources { get; } = new List<Source2D>();

        public Boundaries2D Boundaries { get; } = new();

        public Solver2D(int Nx, int Ny, double dx, double dy)
        {
            (_Nx, _Ny) = (Nx, Ny);
            (_dx, _dy) = (dx, dy);
        }

        private double dHx(int i, int j, double[,] Ez)
        {
            var ez_dy = (Ez[i, j + 1] - Ez[i, j]) / _dy;
            return ez_dy / imp0;
        }

        private double dHy(int i, int j, double[,] Ez)
        {
            var ez_dx = (Ez[i + 1, j] - Ez[i, j]) / _dx;
            return - ez_dx / imp0;
        }

        private double dHz(int i, int j, double[,] Ex, double[,] Ey)
        {
            var ey_dx = (Ey[i + 1, j] - Ey[i, j]) / _dx;
            var ex_dy = (Ex[i, j + 1] - Ex[i, j]) / _dy;
            return (ey_dx - ex_dy) / imp0;
        }

        private double dEx(int i, int j, double[,] Hz)
        {
            var hz_dy = (Hz[i, j] - Hz[i, j - 1]) / _dy;
            return hz_dy * imp0;
        }

        private double dEy(int i, int j, double[,] Hz)
        {
            var hz_dx = (Hz[i, j] - Hz[i - 1, j]) / _dx;
            return - hz_dx * imp0;
        }

        private double dEz(int i, int j, double[,] Hx, double[,] Hy)
        {
            var hy_dx = (Hy[i, j] - Hy[i - 1, j]) / _dx;
            var hx_dy = (Hx[i, j] - Hx[i, j - 1]) / _dy;
            return (hy_dx - hx_dy) * imp0;
        }

        private void ProcessH(double[,] Hx, double[,] Hy, double[,] Hz, double[,] Ex, double[,] Ey, double[,] Ez)
        {
            for (var i = 0; i < _Nx - 1; i++)
                for (var j = 0; j < _Ny - 1; j++)
                {
                    Hx[i, j] -= dHx(i, j, Ez);
                    Hy[i, j] -= dHy(i, j, Ez);
                    Hz[i, j] -= dHz(i, j, Ex, Ey);
                }
        }

        private void ProcessE(double[,] Hx, double[,] Hy, double[,] Hz, double[,] Ex, double[,] Ey, double[,] Ez)
        {
            for (var i = 1; i < _Nx; i++)
                for (var j = 1; j < _Ny; j++)
                {
                    Ex[i, j] += dEx(i, j, Hz);
                    Ey[i, j] += dEy(i, j, Hz);
                    Ez[i, j] += dEz(i, j, Hx, Hy);
                }
        }

        private static void ApplySourceH(Source2D[] sources, double t, double[,] Hx, double[,] Hy, double[,] Hz)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(Hx, Hy, Hz, t);
        }

        private static void ApplySourceE(Source2D[] sources, double t, double[,] Ex, double[,] Ey, double[,] Ez)
        {
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(Ex, Ey, Ez, t);
        }

        public IEnumerable<Solver2DFrame> Calculation(double T, double dt)
        {
            var Hx = new double[_Nx, _Ny];
            var Hy = new double[_Nx, _Ny];
            var Hz = new double[_Nx, _Ny];

            var Ex = new double[_Nx, _Ny];
            var Ey = new double[_Nx, _Ny];
            var Ez = new double[_Nx, _Ny];

            var sources = Sources.ToArray();

            var i = 0;
            for (var t = 0d; t < T; t += dt)
            {
                Boundaries.ApplyH(Hx, Hy, Hz);
                ProcessH(Hx, Hy, Hz, Ex, Ey, Ez);
                ApplySourceH(sources, t, Hx, Hy, Hz);

                Boundaries.ApplyE(Ex, Ey, Ez);
                ProcessE(Hx, Hy, Hz, Ex, Ey, Ez);
                ApplySourceE(sources, t, Ex, Ey, Ez);

                yield return new(i++, t, Hx, Hy, Hz, Ex, Ey, Ez);
            }
        }
    }

    public class Source2D
    {
        private readonly int _i, _j;

        private readonly Func<double, double> _Ex, _Ey, _Ez;
        private readonly Func<double, double> _Hx, _Hy, _Hz;

        public Func<double, double> Ex { init => _Ex = value; }
        public Func<double, double> Ey { init => _Ey = value; }
        public Func<double, double> Ez { init => _Ez = value; }

        public Func<double, double> Hx { init => _Hx = value; }
        public Func<double, double> Hy { init => _Hy = value; }
        public Func<double, double> Hz { init => _Hz = value; }

        public Source2D(
            int i, int j,
            Func<double, double> Ex = null,
            Func<double, double> Ey = null,
            Func<double, double> Ez = null,
            Func<double, double> Hx = null,
            Func<double, double> Hy = null,
            Func<double, double> Hz = null
            )
        {
            (_i, _j) = (i, j);
            (_Ex, _Ey, _Ez) = (Ex, Ey, Ez);
            (_Hx, _Hy, _Hz) = (Hx, Hy, Hz);
        }

        public void ProcessE(double[,] Ex, double[,] Ey, double[,] Ez, double t)
        {
            if (_Ex != null) Ex[_i, _j] = _Ex(t);
            if (_Ey != null) Ey[_i, _j] = _Ey(t);
            if (_Ez != null) Ez[_i, _j] = _Ez(t);
        }

        public void ProcessH(double[,] Hx, double[,] Hy, double[,] Hz, double t)
        {
            if (_Hx != null) Hx[_i, _j] = _Hx(t);
            if (_Hy != null) Hy[_i, _j] = _Hy(t);
            if (_Hz != null) Hz[_i, _j] = _Hz(t);
        }
    }

    public class Boundaries2D
    {
        public BoundaryX X { get; } = new();
        public class BoundaryX
        {
            public Boundary2DMinX MinEx { get; set; }
            public Boundary2DMinX MinEy { get; set; }
            public Boundary2DMinX MinEz { get; set; }
            public Boundary2DMinX MinHx { get; set; }
            public Boundary2DMinX MinHy { get; set; }
            public Boundary2DMinX MinHz { get; set; }

            public Boundary2DMinX MaxEx { get; set; }
            public Boundary2DMaxX MaxEy { get; set; }
            public Boundary2DMaxX MaxEz { get; set; }
            public Boundary2DMaxX MaxHx { get; set; }
            public Boundary2DMaxX MaxHy { get; set; }
            public Boundary2DMaxX MaxHz { get; set; }
        }

        public BoundaryY Y { get; } = new();
        public class BoundaryY
        {
            public Boundary2DMinY MinEx { get; set; }
            public Boundary2DMinY MinEy { get; set; }
            public Boundary2DMinY MinEz { get; set; }
            public Boundary2DMinY MinHx { get; set; }
            public Boundary2DMinY MinHy { get; set; }
            public Boundary2DMinY MinHz { get; set; }

            public Boundary2DMinY MaxEx { get; set; }
            public Boundary2DMaxY MaxEy { get; set; }
            public Boundary2DMaxY MaxEz { get; set; }
            public Boundary2DMaxY MaxHx { get; set; }
            public Boundary2DMaxY MaxHy { get; set; }
            public Boundary2DMaxY MaxHz { get; set; }
        }

        public void ApplyH(double[,] Hx, double[,] Hy, double[,] Hz)
        {
            var x = X;
            x.MinHx?.Process(Hx);
            x.MinHy?.Process(Hy);
            x.MinHz?.Process(Hz);

            x.MaxHx?.Process(Hx);
            x.MaxHy?.Process(Hy);
            x.MaxHz?.Process(Hz);

            var y = Y;
            y.MinHx?.Process(Hx);
            y.MinHy?.Process(Hy);
            y.MinHz?.Process(Hz);

            y.MaxHx?.Process(Hx);
            y.MaxHy?.Process(Hy);
            y.MaxHz?.Process(Hz);
        }

        public void ApplyE(double[,] Ex, double[,] Ey, double[,] Ez)
        {
            var x = X;
            x.MinEx?.Process(Ex);
            x.MinEy?.Process(Ey);
            x.MinEz?.Process(Ez);

            x.MaxEx?.Process(Ex);
            x.MaxEy?.Process(Ey);
            x.MaxEz?.Process(Ez);

            var y = Y;
            y.MinEx?.Process(Ex);
            y.MinEy?.Process(Ey);
            y.MinEz?.Process(Ez);

            y.MaxEx?.Process(Ex);
            y.MaxEy?.Process(Ey);
            y.MaxEz?.Process(Ez);
        }
    }

    public abstract class Boundary2D
    {
        public abstract void Process(double[,] Field);
    }

    public abstract class Boundary2DMinX : Boundary2D { }

    public abstract class Boundary2DMaxX : Boundary2D { }

    public class ABC2DMinX : Boundary2DMinX
    {
        public override void Process(double[,] Field)
        {
            for (int j = 0, count_j = Field.GetLength(1); j < count_j; j++)
                Field[0, j] = Field[1, j];
        }
    }

    public class ABC2DMaxX : Boundary2DMaxX
    {
        public override void Process(double[,] Field)
        {
            for (int j = 0, 
                     count_i0 = Field.GetLength(0) - 1, 
                     count_i1 = Field.GetLength(0) - 2, 
                     count_j = Field.GetLength(1); 
                 j < count_j; 
                 j++)
                Field[count_i0, j] = Field[count_i1 - 1, j];
        }
    }

    public abstract class Boundary2DMinY : Boundary2D { }

    public abstract class Boundary2DMaxY : Boundary2D { }

    public class ABC2DMinY : Boundary2DMinY
    {
        public override void Process(double[,] Field)
        {
            for (int i = 0, count_i = Field.GetLength(0); i < count_i; i++)
                Field[i, 0] = Field[i, 1];
        }
    }

    public class ABC2DMaxY : Boundary2DMaxY
    {
        public override void Process(double[,] Field)
        {
            for (int i = 0, count_j0 = Field.GetLength(0) - 1, 
                     count_j1 = Field.GetLength(0) - 2, 
                     count_i = Field.GetLength(1); 
                 i < count_i; 
                 i++)
                Field[i, count_j0] = Field[i, count_j1 - 1];
        }
    }
}
