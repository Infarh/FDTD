namespace FDTD.Space1D.Boundaries.ABC
{
    public class ABC1DMin : Boundary1DMin
    {
        public override void Process(double[] Field) => Field[0] = Field[1];
    }
}