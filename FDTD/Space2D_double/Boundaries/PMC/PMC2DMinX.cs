using System;

namespace FDTD.Space2D.Boundaries.PMC
{
    public class PMC2DMinX : Boundary2DMinX
    {
        public override void Process(double[,] Field)
        {
            int Nx = Field.GetLength(0);
            for (int j = 0; j < Field.GetLength(1); j++)
            {
                Field[0, j] = 0.0;       // обнуление нормальной компоненты
                Field[1, j] = -Field[1, j]; // инверсия тангенциальной
            }
        }
    }
}