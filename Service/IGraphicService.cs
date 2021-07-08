using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    public interface IGraphicService
    {
        void GetGraphic(Action<GraphicItems, Exception> callback);
    }
}
