namespace FDTD.Space2D.Boundaries.PEC
{
    public class PEC2DMinY : Boundary2DMinY
    {
        public override void Process(double[,] Field)
        {
            for (int i = 0, count_i = Field.GetLength(0); i < count_i; i++)
                Field[i, 0] = Field[i, 1];
        }
    }
}