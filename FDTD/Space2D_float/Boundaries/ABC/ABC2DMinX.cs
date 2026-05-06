namespace FDTD.Space2D_float.Boundaries.ABC
{
    public class ABC2DMinX : Boundary2DMinX
    {
        public override void Process(float[,] Field)
        {
            for (int j = 0; j < Field.GetLength(1); j++)
                Field[0, j] = 0;
        }
    }
}