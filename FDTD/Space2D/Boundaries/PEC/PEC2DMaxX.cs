namespace FDTD.Space2D.Boundaries.PEC
{
    public class PEC2DMaxX : Boundary2DMaxX
    {
        public override void Process(double[,] Field)
        {
            int lastX = Field.GetLength(0) - 1;
            for (int j = 0; j < Field.GetLength(1); j++)
                Field[lastX, j] = 0;
        }
    }
}