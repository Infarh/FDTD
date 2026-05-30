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

        /// <summary>Гауссов импульс: A * exp(-((t - t0) / τ)^2)</summary>
        public static Func<double, double> Gaussian(double amplitude, double t0, double tau) =>
            t => amplitude * Math.Exp(-Math.Pow((t - t0) / tau, 2));

        /// <summary>Гармонический сигнал: A * sin(2π f t + φ)</summary>
        public static Func<double, double> Sine(double amplitude, double frequency, double phase) =>
            t => amplitude * Math.Sin(2 * Math.PI * frequency * t + phase);

        /// <summary>Импульс Рикера: A * (1 - 2τ²) * exp(-τ²), где τ = π f (t - t0)</summary>
        public static Func<double, double> Ricker(double amplitude, double peakFrequency, double t0) =>
            t =>
            {
                double tau = Math.PI * peakFrequency * (t - t0);
                double tau2 = tau * tau;
                return amplitude * (1 - 2 * tau2) * Math.Exp(-tau2);
            };

    }
}
