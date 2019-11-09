using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    public interface IGrblService
    {
        void GetMachineState(Action<GrblItems, Exception> callback);
        void GetControle(Action<GrblItems, Exception> callback);
    }
}
