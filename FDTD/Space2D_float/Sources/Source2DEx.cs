namespace FDTD.Space2D_float.Sources
{
    public static class Source2DEx
    {
        public static void ProcessH(
            this Source2D[] sources,
            float t,
            float[,] Hx, float[,] Hy, float[,] Hz)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessH(t, Hx, Hy, Hz);
        }

        public static void ProcessH(
            this Source2D[] sources,
            float t,
            float[,] ChxE, float[,] ChyE, float[,] ChzE,
            float[,] Hx, float[,] Hy, float[,] Hz)
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
            float t,
            float[,] Ex, float[,] Ey, float[,] Ez)
        {
            if (sources is not { Length: > 0 }) return;
            for (var i = 0; i < sources.Length; i++)
                sources[i].ProcessE(t, Ex, Ey, Ez);
        }

        public static void ProcessE(
            this Source2D[] sources,
            float t,
            float[,] CexH, float[,] CeyH, float[,] CezH,
            float[,] Ex, float[,] Ey, float[,] Ez)
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