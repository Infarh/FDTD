using System;

namespace FDTD.Space2D.Boundaries.PMC
{
    public class PMC2DMaxX : Boundary2DMaxX
    {
        public override void Process(double[,] Field)
        {
            int Nx = Field.GetLength(0);
            for (int j = 0; j < Field.GetLength(1); j++)
            {
                Field[Nx - 1, j] = 0.0;
                Field[Nx - 2, j] = -Field[Nx - 2, j];
            }
        }
    }
}