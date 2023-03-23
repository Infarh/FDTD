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

       //Console.WriteLine("Завершено!");
       //Console.ReadLine();
    }
}
