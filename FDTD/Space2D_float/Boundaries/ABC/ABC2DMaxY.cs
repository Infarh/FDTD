namespace FDTD.Space2D_float.Boundaries.ABC
{
    public class ABC2DMaxY : Boundary2DMaxY
    {
        public override void Process(float[,] Field)
        {
            int lastY = Field.GetLength(1) - 1;
            for (int i = 0; i < Field.GetLength(0); i++)
                Field[i, lastY] = 0;
        }
    }
}