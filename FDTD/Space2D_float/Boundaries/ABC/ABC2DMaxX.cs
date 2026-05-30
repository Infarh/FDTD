namespace FDTD.Space2D_float.Boundaries.ABC
{
    public class ABC2DMaxX : Boundary2DMaxX
    {
        public override void Process(float[,] Field)
        {
            int lastX = Field.GetLength(0) - 1;
            for (int j = 0; j < Field.GetLength(1); j++)
                Field[lastX, j] = 0;
        }
    }
}