namespace FDTD.Space1D.Boundaries
{
    public abstract class Boundary1D
    {
        public abstract void Process(double[] Field);
    }
}