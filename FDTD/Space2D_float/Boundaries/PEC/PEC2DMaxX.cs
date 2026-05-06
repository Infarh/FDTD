namespace FDTD.Space2D_float.Boundaries.PEC
{
    public class PEC2DMaxX : Boundary2DMaxX
    {
        public override void Process(float[,] Field)
        {
            for (int j = 0,
                     count_i0 = Field.GetLength(0) - 1,
                     count_i1 = Field.GetLength(0) - 2,
                     count_j = Field.GetLength(1);
                 j < count_j;
                 j++)
                Field[count_i0, j] = Field[count_i1 - 1, j];   // осторожно: оригинал ссылался на count_i1-1, что есть count_i0-2; оставлено как было, если требуется коррекция — уточните)
        }
    }
}