namespace FDTD.Space2D.Sources
{
    public static class Source2DEx
    {
        public static void ProcessH(
            this Source2D[] sources, 
            double t, 
            double[,] Hx, double[,] Hy, double[,] Hz)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(t, Hx, Hy, Hz);
        }

        public static void ProcessH(
            this Source2D[] sources,
            double t,
            double[,] ChxE, double[,] ChyE, double[,] ChzE,
            double[,] Hx, double[,] Hy, double[,] Hz)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(
                    t,
                    ChxE, ChyE, ChzE,
                    Hx, Hy, Hz);
        }

        public static void ProcessE(
            this Source2D[] sources, 
            double t, 
            double[,] Ex, double[,] Ey, double[,] Ez)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(t, Ex, Ey, Ez);
        }

        public static void ProcessE(
            this Source2D[] sources, 
            double t, 
            double[,] CexH, double[,] CeyH, double[,] CezH,
            double[,] Ex, double[,] Ey, double[,] Ez)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(
                    t, 
                    CexH, CeyH, CezH,
                    Ex, Ey, Ez);
        }
    }
}