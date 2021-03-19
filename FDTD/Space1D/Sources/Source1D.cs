namespace FDTD.Space1D.Sources
{
    public abstract class Source1D
    {
        public abstract bool HasE { get; }
        public abstract bool HasH { get; }

        public abstract void ProcessE(double[] Ey, double[] Ez, double t);
        public abstract void ProcessH(double[] Hy, double[] Hz, double t);
    }
}