using System;

namespace FDTD.Space2D.Boundaries.PMC
{
    public class PMC2DMaxY : Boundary2DMaxY
    {
        public override void Process(double[,] Field)
        {
            int Ny = Field.GetLength(1);
            for (int i = 0; i < Field.GetLength(0); i++)
            {
                Field[i, Ny - 1] = 0.0;
                Field[i, Ny - 2] = -Field[i, Ny - 2];
            }
        }
    }
}