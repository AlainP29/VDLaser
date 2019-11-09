using System;
using VDLaser.Model;
using VDLaser.Service;

namespace VDLaser.Design
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
