namespace FDTD.Space1D.Boundaries.ABC
{
    public class ABC1DMax : Boundary1DMax
    {
        public override void Process(double[] Field) => Field[^1] = Field[^2];
    }
}