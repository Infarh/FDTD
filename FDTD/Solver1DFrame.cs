using System;
using System.IO;

namespace FDTD
{
    public readonly struct Solver1DFrame
    {
        public int Index { get; }
        public double Time { get; }
        public double[] Hy { get; }
        public double[] Hz { get; }
        public double[] Ey { get; }
        public double[] Ez { get; }

        public Solver1DFrame(int Index, double Time, double[] Hy, double[] Hz, double[] Ey, double[] Ez)
        {
            this.Index = Index;
            this.Time = Time;
            this.Hy = Hy;
            this.Hz = Hz;
            this.Ey = Ey;
            this.Ez = Ez;
        }

        private static void WriteTo(TextWriter writer, double[] Field)
        {
            for (int i = 0, count = Field.Length - 1; i < count; i++)
                writer.Write("{0}; ", Field[i]);
            writer.WriteLine(Field[^1]);
        }

        public void WriteEyTo(TextWriter writer) => WriteTo(writer, Ey);
        public void WriteEzTo(TextWriter writer) => WriteTo(writer, Ez);

        public void WriteHyTo(TextWriter writer) => WriteTo(writer, Hy);
        public void WriteHzTo(TextWriter writer) => WriteTo(writer, Hz);

        public void CopyEyTo(double[] Ey) => Buffer.BlockCopy(this.Ey, 0, Ey, 0, Buffer.ByteLength(this.Ey));
        public void CopyEzTo(double[] Ez) => Buffer.BlockCopy(this.Ez, 0, Ez, 0, Buffer.ByteLength(this.Ez));

        public void CopyHyTo(double[] Hy) => Buffer.BlockCopy(this.Hy, 0, Hy, 0, Buffer.ByteLength(this.Hy));
        public void CopyHzTo(double[] Hz) => Buffer.BlockCopy(this.Hz, 0, Hz, 0, Buffer.ByteLength(this.Hz));

        public void Deconstruct(out int Index, out double Time, out (double[] Ey, double[] Ez) E, out (double[] Hy, double[] Hz) H)
        {
            Index = this.Index;
            Time = this.Time;
            E = (Ey, Ez); 
            H = (Hy, Hz);
        }
    }
}
