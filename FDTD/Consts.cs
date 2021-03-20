using System;

namespace FDTD
{
    public static class Consts
    {
        public const double SpeedOfLight = 299_792_458;

        public const double Mu0 = 4 * Math.PI * 1e-7;

        //public const double Eps0 = 1 / Mu0 / SpeedOfLight / SpeedOfLight;
        public const double Eps0 = Mu0 / Imp0 / Imp0;

        public const double Imp0 = 120 * Math.PI;
        public const double Imp0Inv = 1 / Imp0;
    }
}
