namespace FDTD.Space2D.Boundaries
{
    public abstract class Boundary2D
    {
        public abstract void Process(double[,] Field);
    }
}