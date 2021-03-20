namespace FDTD.Space1D.Boundaries
{
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
}