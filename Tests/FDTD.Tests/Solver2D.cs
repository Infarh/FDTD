using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Solver = FDTD.Solver2D;

namespace FDTD.Tests
{
    [TestClass]
    public class Solver2D
    {
        [TestMethod]
        public void CalculationTest()
        {
            const int Nx = 100;
            const int Ny = 100;

            const double dx = 1;
            const double dy = 1;

            var solver = new Solver(Nx, Ny, dx, dy)
            {

            };

            const double total_time = 100;
            const double dt = 1;

            const int frames_count = (int)(total_time / dt);
            var Ex = new double[frames_count][,];
            var Ey = new double[frames_count][,];
            var Ez = new double[frames_count][,];

            var Hx = new double[frames_count][,];
            var Hy = new double[frames_count][,];
            var Hz = new double[frames_count][,];

            for (var i = 0; i < frames_count; i++)
            {
                Ex[i] = new double[Nx, Ny];
                Ey[i] = new double[Nx, Ny];
                Ez[i] = new double[Nx, Ny];

                Hx[i] = new double[Nx, Ny];
                Hy[i] = new double[Nx, Ny];
                Hz[i] = new double[Nx, Ny];
            }

            foreach (var frame in solver.Calculation(total_time, dt))
            {
                frame.CopyExTo(Ex[frame.Index]);

                //Buffer.BlockCopy(ex, 0, Ex[frame_index], 0, Buffer.ByteLength(ex));


                //Assert.That.Value(ex).IsNotNull();
                //Assert.That.Value(ey).IsNotNull();
                //Assert.That.Value(ez).IsNotNull();

                //Assert.That.Value(hx).IsNotNull();
                //Assert.That.Value(hy).IsNotNull();
                //Assert.That.Value(hz).IsNotNull();
            }
        }
    }
}