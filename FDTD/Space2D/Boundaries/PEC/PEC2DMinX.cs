namespace FDTD.Space2D.Boundaries.PEC
{
    public class PEC2DMinX : Boundary2DMinX
    {
        public override void Process(double[,] Field)
        {
            for (int j = 0; j < Field.GetLength(1); j++)
                Field[0, j] = 0;
        }
    }
}