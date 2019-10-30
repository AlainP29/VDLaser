using System;
using VDGrbl.Model;

namespace VDGrbl.Design
{
    public class DesignGrblService : IGrblService
    {
        public void GetMachineState(Action<GrblItems, Exception> callback)
        {
            var item = new GrblItems("Machine state [design]");
            callback?.Invoke(item, null);
        }
        public void GetControle(Action<GrblItems, Exception> callback)
        {
            var item = new GrblItems("Controle [design]");
            callback?.Invoke(item, null);
        }
    }
}
