namespace FDTD.Space2D.Boundaries.ABC
{
    public class ABC2DMinX : Boundary2DMinX
    {
        public override void Process(double[,] Field)
        {
            for (int j = 0, count_j = Field.GetLength(1); j < count_j; j++)
                Field[0, j] = Field[1, j];
        }
    }
}