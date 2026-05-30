using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using FDTD.Space2D_float;                // float‑версия
using FDTD.Space2D_float.Boundaries;
using FDTD.Space2D_float.Boundaries.ABC;
using FDTD.Space2D_float.Sources;
using FloatSolver = FDTD.Space2D_float.Solver2D;
using FloatMesh = FDTD.Space2D_float.Mesh2D;

// Предполагается, что double‑версия лежит в пространствах имён FDTD.Space2D.*
using DoubleSolver = FDTD.Space2D.Solver2D;
using DoubleMesh = FDTD.Space2D.Mesh2D;

class FDTD_D_F_Benchmark
{
    // Параметры сетки – фиксированный пространственный шаг
    const float dx = 0.01f, dy = 0.01f;        // 1 см
    const float CourantFactor = 0.99f;         // запас устойчивости

    // Параметры источника (гауссов импульс)
    static readonly float f0 = 1e9f;          // 1 ГГц
    static readonly float tau = 1.0f / f0;     // характерная длительность

    // Функции источника для float и double
    static float GaussianPulseFloat(float t) => MathF.Exp(-((t - 3 * tau) * (t - 3 * tau)) / (tau * tau));
    static double GaussianPulseDouble(double t) => Math.Exp(-((t - 3 * tau) * (t - 3 * tau)) / (tau * tau));

    // Конфигурация тестовых прогонов
    static readonly int[] MeshSizes = { 50, 100, 200, 400, 800};   // Nx = Ny
    static readonly int[] TimeSteps = { 100, 500, 800 }; // число временных итераций

    public static void Run()
    {
        Console.WriteLine("=== Сравнение double и float FDTD-решателей ===");
        Console.WriteLine("Разные размеры сетки и разное количество шагов\n");

        // Заголовок таблицы
        Console.WriteLine($"{"Nx=Ny",-6} {"Шагов",-7} | {"float время(мс)",-14} {"float память(байт)",-17} | {"double время(мс)",-15} {"double память(байт)",-18} | {"max Δ abs",-10} {"mean Δ abs",-10} {"max Δ rel",-10}");
        Console.WriteLine(new string('-', 120));

        foreach (int N in MeshSizes)
        {
            int Nx = N, Ny = N;
            int srcI = Nx / 2, srcJ = Ny / 2;   // источник в центре

            // Создаём солверы с материалами – свободное пространство
            var solverFloat = new FloatSolver(Nx, Ny, dx, dy);
            solverFloat.EpsGrid = (i, j) => 1f;
            solverFloat.MuGrid = (i, j) => 1f;
            var solverDouble = new DoubleSolver(Nx, Ny, dx, dy);
            solverDouble.EpsGrid = (i, j) => 1.0;
            solverDouble.MuGrid = (i, j) => 1.0;

            // Вычисляем dt и берём минимальный для надёжности
            float dtFloat = CourantFactor * solverFloat.GetMaxStableTimeStep();
            double dtDouble = CourantFactor * solverDouble.GetMaxStableTimeStep();
            float dt = (float)Math.Min(dtFloat, dtDouble);

            // Добавляем источники
            solverFloat.Sources.Add(new Source2D(srcI, srcJ, Ez: GaussianPulseFloat));
            solverDouble.Sources.Add(new FDTD.Space2D.Sources.Source2D(srcI, srcJ, Ez: GaussianPulseDouble));

            // Граничные условия – ABC (обнуление границ)
            solverFloat.Boundaries.X.Min = new ABC2DMinX();
            solverFloat.Boundaries.X.Max = new ABC2DMaxX();
            solverFloat.Boundaries.Y.Min = new ABC2DMinY();
            solverFloat.Boundaries.Y.Max = new ABC2DMaxY();
            solverDouble.Boundaries.X.Min = new FDTD.Space2D.Boundaries.ABC.ABC2DMinX();
            solverDouble.Boundaries.X.Max = new FDTD.Space2D.Boundaries.ABC.ABC2DMaxX();
            solverDouble.Boundaries.Y.Min = new FDTD.Space2D.Boundaries.ABC.ABC2DMinY();
            solverDouble.Boundaries.Y.Max = new FDTD.Space2D.Boundaries.ABC.ABC2DMaxY();

            // Создаём меши (память под массивы выделяется здесь)
            FloatMesh meshFloat = solverFloat.GetMesh(dt);
            DoubleMesh meshDouble = solverDouble.GetMesh(dt);

            foreach (int steps in TimeSteps)
            {
                float totalTime = steps * dt;

                // --- Замер float-версии ---
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                long memBeforeFloat = GC.GetTotalMemory(true);

                var swFloat = Stopwatch.StartNew();
                var floatFrames = meshFloat.Calculation(totalTime).ToList();
                swFloat.Stop();

                long memAfterFloat = GC.GetTotalMemory(false);
                long memAllocatedFloat = Math.Max(0, memAfterFloat - memBeforeFloat);
                var lastFloat = floatFrames.Last();

                // --- Замер double-версии ---
                GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                long memBeforeDouble = GC.GetTotalMemory(true);

                var swDouble = Stopwatch.StartNew();
                var doubleFrames = meshDouble.Calculation(totalTime).ToList();
                swDouble.Stop();

                long memAfterDouble = GC.GetTotalMemory(false);
                long memAllocatedDouble = Math.Max(0, memAfterDouble - memBeforeDouble);
                var lastDouble = doubleFrames.Last();

                // --- Оценка точности по Ez (исключая граничные индексы) ---
                double maxAbsDiff = 0, sumAbsDiff = 0;
                double maxRelDiff = 0;
                int points = 0;
                for (int i = 1; i < Nx - 1; i++)
                {
                    for (int j = 1; j < Ny - 1; j++)
                    {
                        double valFloat = lastFloat.Ez[i, j];
                        double valDouble = lastDouble.Ez[i, j];
                        double absDiff = Math.Abs(valFloat - valDouble);
                        maxAbsDiff = Math.Max(maxAbsDiff, absDiff);
                        sumAbsDiff += absDiff;
                        points++;

                        double mag = Math.Max(Math.Abs(valFloat), Math.Abs(valDouble));
                        if (mag > 1e-30)
                        {
                            double relDiff = absDiff / mag;
                            if (relDiff > maxRelDiff) maxRelDiff = relDiff;
                        }
                    }
                }
                double meanAbsDiff = sumAbsDiff / points;

                // --- Вывод строки таблицы ---
                Console.WriteLine(
                    $"{N,-6} {steps,-7} | {swFloat.ElapsedMilliseconds,14} {memAllocatedFloat,17} | " +
                    $"{swDouble.ElapsedMilliseconds,15} {memAllocatedDouble,18} | " +
                    $"{maxAbsDiff,10:E2} {meanAbsDiff,10:E2} {maxRelDiff,10:E2}");

                // Освобождаем кадры, чтобы память не суммировалась внутри одного Mesh'а
                floatFrames = null;
                doubleFrames = null;
                GC.Collect();
            }

            // Для следующего размера сетки освобождаем меши (и массивы полей)
            meshFloat = null;
            meshDouble = null;
        }

        Console.WriteLine("\nГотово. Нажмите любую клавишу...");
        Console.ReadKey();
    }

    // Вспомогательная функция (не используется в таблице, но можно оставить для справки)
    static long EstimateFieldMemory<T>(int nx, int ny, int numArrays) where T : unmanaged
    {
        unsafe { return numArrays * nx * ny * sizeof(T); }
    }
}