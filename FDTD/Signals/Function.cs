using System;
using static System.Math;

namespace FDTD.Signals
{
    public static class Function
    {
        public static double Sqr(double x) => x * x;

        public static double Pow3(double x) => x * x * x;
        public static double Pow4(double x) => x * x * x * x;

        public static double Gauss(double t) => Exp(-t * t);

        public static Func<double, double> ExpSqr(double t0 = 0, double tau = 1, double k = 3) => 
            t => Gauss((t - t0) / (tau / k));

        public static Func<double, double> ExpPulse(double tau, double t0 = 0, double k = 3) => 
            ExpSqr(tau - t0, tau, k);
    }
}
