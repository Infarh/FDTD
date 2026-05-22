using System;

namespace FDTD.Space2D.Boundaries.PMC
{
    public class PMC2DMinY : Boundary2DMinY
    {
        public override void Process(double[,] Field)
        {
            int Ny = Field.GetLength(1);
            for (int i = 0; i < Field.GetLength(0); i++)
            {
                Field[i, 0] = 0.0;
                Field[i, 1] = -Field[i, 1];
            }
        }
    }
}