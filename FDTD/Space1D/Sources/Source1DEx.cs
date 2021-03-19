namespace FDTD.Space1D.Sources
{
    public static class Source1DEx
    {
        public static void ProcessH(this Source1D[] sources, double t, double[] Hy, double[] Hz)
        {
            if(sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(Hy, Hz, t);
        }

        public static void ProcessE(this Source1D[] sources, double t, double[] Ey, double[] Ez)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(Ey, Ez, t);
        }
    }
}