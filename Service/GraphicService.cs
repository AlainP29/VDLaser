﻿using System;
using VDLaser.Model;

namespace VDLaser.Service
{
    class GraphicService : IGraphicService
    {
        public void GetGraphic(Action<GraphicItems, Exception> callback)
        {
            var item = new GraphicItems();
            callback?.Invoke(item, null);
        }
    }
}
