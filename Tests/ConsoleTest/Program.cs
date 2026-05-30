using System;

namespace ConsoleTest;

internal static class Program
{
    public static void Main(string[] args)
    {
        Benchmark.CheckData();
        Benchmark.Run();

        //Solver1DTest.Run();
        //Solver2DTest.Run();
        FDTD_D_F_Benchmark.Run();

        Console.WriteLine("Завершено!");
       Console.ReadLine();
    }
}
