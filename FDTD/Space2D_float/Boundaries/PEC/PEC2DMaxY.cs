namespace FDTD.Space2D_float.Boundaries.PEC
{
    public class PEC2DMaxY : Boundary2DMaxY
    {
        public override void Process(float[,] Field)
        {
            for (int i = 0,
                     count_i = Field.GetLength(0),
                     count_j0 = Field.GetLength(1) - 1,
                     count_j1 = Field.GetLength(1) - 2;
                 i < count_i;
                 i++)
                Field[i, count_j0] = Field[i, count_j1];
        }
    }
}