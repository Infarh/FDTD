using System;

namespace FDTD.Space3D
{
    public readonly struct Solver3DFrame
    {
        public int Index { get; }
        public double Time { get; }
        public double[,,] Hx { get; }
        public double[,,] Hy { get; }
        public double[,,] Hz { get; }
        public double[,,] Ex { get; }
        public double[,,] Ey { get; }
        public double[,,] Ez { get; }

        public Solver3DFrame(int Index, double Time, double[,,] Hx, double[,,] Hy, double[,,] Hz, double[,,] Ex, double[,,] Ey, double[,,] Ez)
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

        public void CopyExTo(double[,,] Ex) => Buffer.BlockCopy(this.Ex, 0, Ex, 0, Buffer.ByteLength(this.Ex));
        public void CopyEyTo(double[,,] Ey) => Buffer.BlockCopy(this.Ey, 0, Ey, 0, Buffer.ByteLength(this.Ey));
        public void CopyEzTo(double[,,] Ez) => Buffer.BlockCopy(this.Ez, 0, Ez, 0, Buffer.ByteLength(this.Ez));

        public void CopyHxTo(double[,,] Hx) => Buffer.BlockCopy(this.Hx, 0, Hx, 0, Buffer.ByteLength(this.Hx));
        public void CopyHyTo(double[,,] Hy) => Buffer.BlockCopy(this.Hy, 0, Hy, 0, Buffer.ByteLength(this.Hy));
        public void CopyHzTo(double[,,] Hz) => Buffer.BlockCopy(this.Hz, 0, Hz, 0, Buffer.ByteLength(this.Hz));
    }
}