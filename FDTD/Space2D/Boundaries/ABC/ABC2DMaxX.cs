namespace FDTD.Space2D.Boundaries.ABC
{
    public class ABC2DMaxX : Boundary2DMaxX
    {
        public override void Process(double[,] Field)
        {
            for (int j = 0,
                     count_i0 = Field.GetLength(0) - 1,
                     count_i1 = Field.GetLength(0) - 2,
                     count_j = Field.GetLength(1);
                 j < count_j;
                 j++)
                Field[count_i0, j] = Field[count_i1 - 1, j];
        }
    }
}