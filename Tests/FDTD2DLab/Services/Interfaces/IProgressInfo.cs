using System;
using System.Threading;

namespace FDTD2DLab.Services.Interfaces
{
    public interface IProgressInfo : IDisposable
    {
        IProgress<double> Progress { get; }
        IProgress<string> Information { get; }
        IProgress<string> Status { get; }

        CancellationToken Cancel { get; }
    }
}
