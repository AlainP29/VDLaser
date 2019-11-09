using System;
using VDLaser.Model;
using VDLaser.Service;

namespace VDLaser.Design
{
    public class DesignGraphicService : IGraphicService
    {
        public void GetGraphic(Action<GraphicItems, Exception> callback)
        {
            var item = new GraphicItems("Graphic [design]");
            callback?.Invoke(item, null);
        }
    }
}
