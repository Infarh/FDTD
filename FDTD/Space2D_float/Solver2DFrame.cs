using System;
using System.IO;

namespace FDTD.Space2D_float
{
    public readonly struct Solver2DFrame
    {
        private static float Sqr(float x) => x * x;

        public int Index { get; }
        public float Time { get; }
        public float[,] Hx { get; }
        public float[,] Hy { get; }
        public float[,] Hz { get; }
        public float[,] Ex { get; }
        public float[,] Ey { get; }
        public float[,] Ez { get; }

        public Solver2DFrame(int Index, float Time, float[,] Hx, float[,] Hy, float[,] Hz, float[,] Ex, float[,] Ey, float[,] Ez)
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

        private static void WriteTo(TextWriter writer, float[,] Field)
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

        public void CopyExTo(float[,] Ex) => Buffer.BlockCopy(this.Ex, 0, Ex, 0, Buffer.ByteLength(this.Ex));
        public void CopyEyTo(float[,] Ey) => Buffer.BlockCopy(this.Ey, 0, Ey, 0, Buffer.ByteLength(this.Ey));
        public void CopyEzTo(float[,] Ez) => Buffer.BlockCopy(this.Ez, 0, Ez, 0, Buffer.ByteLength(this.Ez));

        public void CopyHxTo(float[,] Hx) => Buffer.BlockCopy(this.Hx, 0, Hx, 0, Buffer.ByteLength(this.Hx));
        public void CopyHyTo(float[,] Hy) => Buffer.BlockCopy(this.Hy, 0, Hy, 0, Buffer.ByteLength(this.Hy));
        public void CopyHzTo(float[,] Hz) => Buffer.BlockCopy(this.Hz, 0, Hz, 0, Buffer.ByteLength(this.Hz));

        public void Deconstruct(
            out int Index,
            out float Time,
            out (float[,] Ex, float[,] Ey, float[,] Ez) E,
            out (float[,] Hx, float[,] Hy, float[,] Hz) H)
        {
            Index = this.Index;
            Time = this.Time;
            E = (Ex, Ey, Ez);
            H = (Hx, Hy, Hz);
        }

        public (float Hx, float Hy, float Hz) GetH(int i, int j) => (Hx[i, j], Hy[i, j], Hz[i, j]);
        public (float Ex, float Ey, float Ez) GetE(int i, int j) => (Ex[i, j], Ey[i, j], Ez[i, j]);

        public float GetPowerH(int i, int j) => Sqr(Hx[i, j]) + Sqr(Hy[i, j]) + Sqr(Hz[i, j]);
        public float GetPowerE(int i, int j) => Sqr(Ex[i, j]) + Sqr(Ey[i, j]) + Sqr(Ez[i, j]);

        public float GetAbsH(int i, int j) => MathF.Sqrt(GetPowerH(i, j));
        public float GetAbsE(int i, int j) => MathF.Sqrt(GetPowerE(i, j));

        public (float Px, float Py, float Pz) GetPower(int i, int j) => (
            Ey[i, j] * Hz[i, j] - Ez[i, j] * Hy[i, j],
            Ez[i, j] * Hx[i, j] - Ex[i, j] * Hz[i, j],
            Ex[i, j] * Hy[i, j] - Ey[i, j] * Hx[i, j]);

        public float GetPowerAbs(int i, int j)
        {
            var (x, y, z) = GetPower(i, j);
            return MathF.Sqrt(x * x + y * y + z * z);
        }

        public float GetPowerAbsdb(int i, int j)
        {
            var db = 10f * MathF.Log10(GetPowerAbs(i, j));
            return float.IsNaN(db) || float.IsNegativeInfinity(db) ? -180f : db;
        }
    }
}