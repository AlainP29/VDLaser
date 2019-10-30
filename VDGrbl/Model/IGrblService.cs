using System;

namespace VDGrbl.Model
{
    public interface IGrblService
    {
        void GetMachineState(Action<GrblItems, Exception> callback);
        void GetControle(Action<GrblItems, Exception> callback);
    }
}
