namespace FDTD.Space2D_float.Boundaries
{
    public abstract class Boundary2D
    {
        public abstract void Process(float[,] Field);
    }
}