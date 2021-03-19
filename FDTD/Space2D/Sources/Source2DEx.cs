namespace FDTD.Space2D.Sources
{
    public static class Source2DEx
    {
        public static void ProcessH(this Source2D[] sources, double t, double[,] Hx, double[,] Hy, double[,] Hz)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(Hx, Hy, Hz, t);
        }

        public static void ProcessE(this Source2D[] sources, double t, double[,] Ex, double[,] Ey, double[,] Ez)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(Ex, Ey, Ez, t);
        }
    }
}