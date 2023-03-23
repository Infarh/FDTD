using System;
using System.Globalization;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace ConsoleTest;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class Benchmark
{
    public static void Run() => _ = BenchmarkRunner.Run<Benchmark>();

    private const string __Test1 = "123456";
    private const string __Test2 = "+3,14159265358979323846";
    private const string __Test3 = "-12345673,14159265358979323846";
    private const string __Test4 = ",123456";
    private const string __Test5 = "123456,";

    public static void CheckData()
    {
        CheckData_TryParse();
        CheckData_IsDoubleCustom();
    }

    private static void CheckData_TryParse()
    {
        if (!IsDoubleTryParse(__Test1)) throw new InvalidOperationException("Test1 err");
        if (!IsDoubleTryParse(__Test2)) throw new InvalidOperationException("Test2 err");
        if (!IsDoubleTryParse(__Test3)) throw new InvalidOperationException("Test3 err");
        if (!IsDoubleTryParse(__Test4)) throw new InvalidOperationException("Test4 err");
        if (!IsDoubleTryParse(__Test5)) throw new InvalidOperationException("Test5 err");
    }

    private static void CheckData_IsDoubleCustom()
    {
        if (!IsDoubleCustom(__Test1)) throw new InvalidOperationException("Test1 err");
        if (!IsDoubleCustom(__Test2)) throw new InvalidOperationException("Test2 err");
        if (!IsDoubleCustom(__Test3)) throw new InvalidOperationException("Test3 err");
        if (!IsDoubleCustom(__Test4)) throw new InvalidOperationException("Test4 err");
        if (!IsDoubleCustom(__Test5)) throw new InvalidOperationException("Test5 err");
    }

    private static bool IsDoubleTryParse(string str) => double.TryParse(str, out _);

    private static bool IsDoubleCustom(string str)
    {
        var is_fraction = false;
        //IFormatProvider provider = CultureInfo.CurrentCulture;
        //var s = NumberFormatInfo.GetInstance(provider).NumberDecimalSeparator[0];
        for (var i = 0; i < str.Length; i++)
        {
            if (is_fraction)
            {
                if (!char.IsDigit(str, i))
                    return false;
            }
            else
            {
                var c = str[i];
                if (char.IsDigit(c)) continue;
                switch (str[i])
                {
                    default:
                        return false;
                    case '.':
                    case ',':
                        is_fraction = true;
                        break;
                    case '+' when i == 0:
                    case '-' when i == 0:
                        break;
                }
            }
        }

        return true;
    }

    //[Benchmark]
    //public void TryParse()
    //{
    //    _ = IsDoubleTryParse(__Test1);
    //    _ = IsDoubleTryParse(__Test2);
    //    _ = IsDoubleTryParse(__Test3);
    //    _ = IsDoubleTryParse(__Test4);
    //    _ = IsDoubleTryParse(__Test5);
    //}

    //[Benchmark]
    //public void Custom()
    //{
    //    _ = IsDoubleCustom(__Test1);
    //    _ = IsDoubleCustom(__Test2);
    //    _ = IsDoubleCustom(__Test3);
    //    _ = IsDoubleCustom(__Test4);
    //    _ = IsDoubleCustom(__Test5);
    //}

    //[Benchmark] public double doule_Parse() => double.Parse(__Test3);
    //[Benchmark] public double Convert_ToDouble() => Convert.ToDouble(__Test3);

    
}
