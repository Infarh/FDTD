using System;
using System.IO;

namespace FDTD
{
    public readonly struct Solver2DFrame
    {
        public int Index { get; }
        public double Time { get; }
        public double[,] Hx { get; }
        public double[,] Hy { get; }
        public double[,] Hz { get; }
        public double[,] Ex { get; }
        public double[,] Ey { get; }
        public double[,] Ez { get; }

        public Solver2DFrame(int Index, double Time, double[,] Hx, double[,] Hy, double[,] Hz, double[,] Ex, double[,] Ey, double[,] Ez)
        {
            this.Index = Index;
            this.Time = Time;
            this.Hx = Hx;
            this.Hy = Hy;
            this.Hz = Hz;
            this.Ex = Ex;
            this.Ey = Ey;
            this.Ez = Ez;
        }

        private static void WriteTo(TextWriter writer, double[,] Field)
        {
            for (int i = 0, count_i = Field.GetLength(0) - 1, count_j = Field.GetLength(1) - 1; i < count_i; i++)
            {
                for (var j = 0; j < count_j; j++)
                    writer.Write("{0}; ", Field[i, j]);
                writer.WriteLine(Field[i, count_j]);
            }
        }

        public void WriteExTo(TextWriter writer) => WriteTo(writer, Ex);
        public void WriteEyTo(TextWriter writer) => WriteTo(writer, Ey);
        public void WriteEzTo(TextWriter writer) => WriteTo(writer, Ez);

        public void WriteHxTo(TextWriter writer) => WriteTo(writer, Hx);
        public void WriteHyTo(TextWriter writer) => WriteTo(writer, Hy);
        public void WriteHzTo(TextWriter writer) => WriteTo(writer, Hz);

        public void CopyExTo(double[,] Ex) => Buffer.BlockCopy(this.Ex, 0, Ex, 0, Buffer.ByteLength(this.Ex));
        public void CopyEyTo(double[,] Ey) => Buffer.BlockCopy(this.Ey, 0, Ey, 0, Buffer.ByteLength(this.Ey));
        public void CopyEzTo(double[,] Ez) => Buffer.BlockCopy(this.Ez, 0, Ez, 0, Buffer.ByteLength(this.Ez));

        public void CopyHxTo(double[,] Hx) => Buffer.BlockCopy(this.Hx, 0, Hx, 0, Buffer.ByteLength(this.Hx));
        public void CopyHyTo(double[,] Hy) => Buffer.BlockCopy(this.Hy, 0, Hy, 0, Buffer.ByteLength(this.Hy));
        public void CopyHzTo(double[,] Hz) => Buffer.BlockCopy(this.Hz, 0, Hz, 0, Buffer.ByteLength(this.Hz));

        public void Deconstruct(
            out int Index,
            out double Time, 
            out (double[,] Ex, double[,] Ey, double[,] Ez) E, 
            out (double[,] Hx, double[,] Hy, double[,] Hz) H)
        {
            Index = this.Index;
            Time = this.Time;
            E = (Ex, Ey, Ez);
            H = (Hx, Hy, Hz);
        }
    }
}