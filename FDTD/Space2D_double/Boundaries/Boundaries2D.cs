namespace FDTD.Space2D.Boundaries
{
    public class Boundaries2D
    {
        public BoundaryX X { get; } = new();
        public class BoundaryX
        {
            public Boundary2DMinX Min
            {
                set
                {
                    MinEx = value;
                    MinEy = value;
                    MinEz = value;
                    MinHx = value;
                    MinHy = value;
                    MinHz = value;
                }
            }

            public Boundary2DMinX MinEx { get; set; }
            public Boundary2DMinX MinEy { get; set; }
            public Boundary2DMinX MinEz { get; set; }
            public Boundary2DMinX MinHx { get; set; }
            public Boundary2DMinX MinHy { get; set; }
            public Boundary2DMinX MinHz { get; set; }

            public Boundary2DMaxX Max
            {
                set
                {
                    MaxEx = value;
                    MaxEy = value;
                    MaxEz = value;
                    MaxHx = value;
                    MaxHy = value;
                    MaxHz = value;
                }
            }

            public Boundary2DMaxX MaxEx { get; set; }
            public Boundary2DMaxX MaxEy { get; set; }
            public Boundary2DMaxX MaxEz { get; set; }
            public Boundary2DMaxX MaxHx { get; set; }
            public Boundary2DMaxX MaxHy { get; set; }
            public Boundary2DMaxX MaxHz { get; set; }
        }

        public BoundaryY Y { get; } = new();
        public class BoundaryY
        {
            public Boundary2DMinY Min
            {
                set
                {
                    MinEx = value;
                    MinEy = value;
                    MinEz = value;
                    MinHx = value;
                    MinHy = value;
                    MinHz = value;
                }
            }

            public Boundary2DMinY MinEx { get; set; }
            public Boundary2DMinY MinEy { get; set; }
            public Boundary2DMinY MinEz { get; set; }
            public Boundary2DMinY MinHx { get; set; }
            public Boundary2DMinY MinHy { get; set; }
            public Boundary2DMinY MinHz { get; set; }

            public Boundary2DMaxY Max
            {
                set
                {
                    MaxEx = value;
                    MaxEy = value;
                    MaxEz = value;
                    MaxHx = value;
                    MaxHy = value;
                    MaxHz = value;
                }
            }

            public Boundary2DMaxY MaxEx { get; set; }
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
}