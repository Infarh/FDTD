namespace FDTD.Space2D.Boundaries.ABC
{
    public class ABC2DMinY : Boundary2DMinY
    {
        public override void Process(double[,] Field)
        {
            for (int i = 0; i < Field.GetLength(0); i++)
                Field[i, 0] = 0;
        }
    }
}