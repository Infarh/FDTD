﻿using System.IO;

using FDTD;

using static System.Math;
// ReSharper disable ConvertToUsingDeclaration

namespace ConsoleTest
{
    internal class Program
    {
        private static double Sqr(double x) => x * x;

        public static void Main(string[] args)
        {
            var solver = new Solver1D(200, 1)
            {
                Sources = { new(50) { Ez = t => 2 * Exp(-Sqr(t - 30) / 100) } }
            };

            using (var file = File.CreateText("solver1d.txt"))
                foreach (var frame in solver.Calculation(250, 1))
                    frame.WriteEzTo(file);

        }
    }
}
