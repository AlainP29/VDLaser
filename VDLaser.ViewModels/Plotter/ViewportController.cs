using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace VDLaser.ViewModels.Plotter
{
    public sealed partial class ViewportController
    {
        private readonly ViewportState _viewport;
        private Point? _lastMouse;

        public ViewportController(ViewportState viewport)
        {
            _viewport = viewport;
        }

        public void OnMouseWheel(Point mousePos, int delta)
        {
            double zoomFactor = delta > 0 ? 1.1 : 0.9;
            double oldScale = _viewport.Scale;
            double newScale = oldScale * zoomFactor;

            // Limites de zoom
            if (newScale < 0.5 || newScale > 10) return;

            // Point sous la souris en coordonnées "Monde" (avant zoom)
            double worldX = (mousePos.X - _viewport.TranslationX) / oldScale;
            double worldY = (mousePos.Y - _viewport.TranslationY) / oldScale;

            _viewport.Scale = newScale;

            // Ajustement de la translation pour que le point (worldX, worldY) 
            // reste exactement sous (mousePos.X, mousePos.Y)
            _viewport.TranslationX = mousePos.X - (worldX * newScale);
            _viewport.TranslationY = mousePos.Y - (worldY * newScale);
        }

        public void OnMouseDown(Point pos)
        {
            _lastMouse = pos;
            _viewport.IsInteracting = true;
        }

        public void OnMouseMove(Point pos)
        {
            if (_lastMouse == null) return;

            Vector delta = pos - _lastMouse.Value;

            double sensitivity = 1.5;

            _viewport.TranslationX += delta.X * sensitivity;
            _viewport.TranslationY -= delta.Y * sensitivity;
            //_viewport.TranslationY += delta.Y * sensitivity;

            _viewport.TranslationX = Math.Clamp(_viewport.TranslationX, -5000, 5000);
            _viewport.TranslationY = Math.Clamp(_viewport.TranslationY, -5000, 5000);

            _lastMouse = pos;
        }

        public void OnMouseUp()
        {
            _lastMouse = null;
            _viewport.IsInteracting = false;
        }
    }
}