using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Transactions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using VDLaser.Core.Interfaces;

namespace VDLaser.ViewModels.Plotter
{
    /// <summary>
    /// Controller for handling user interactions with the viewport in the VDLaser plotter.
    /// Manages zoom (mouse wheel) and pan (drag) operations, with compensation for Y-axis flip.
    /// Ensures smooth and bounded interactions.
    /// </summary>
    public sealed partial class ViewportController
    {
        #region Fields
        private readonly ViewportState _viewport;
        private readonly ILogService _log;
        private Point? _lastMouse;
        #endregion

        public ViewportController(ViewportState viewport, ILogService log)
        {
            _viewport = viewport;
            _log = log;
            _log.Information("[ViewportController] Initialized for ViewportState");
        }

        #region Interaction Methods
        /// <summary>
        /// Handles mouse wheel events for zooming.
        /// Zooms in/out around the mouse position, with clamping to prevent extreme scales.
        /// </summary>
        /// <param name="mousePos">Position of the mouse in viewport coordinates.</param>
        /// <param name="delta">Mouse wheel delta (positive for zoom in, negative for zoom out).</param>
        public void OnMouseWheel(Point mousePos, int delta)
        {
            _log.Debug("[ViewportController] MouseWheel event: pos={X},{Y}; delta={Delta}", mousePos.X, mousePos.Y, delta);
            double zoomFactor = delta > 0 ? 1.1 : 0.9;
            double oldScale = _viewport.Scale;
            double newScale = oldScale * zoomFactor;

            // Clamp scale for stability
            newScale = Math.Max(0.5, Math.Min(10, newScale));
            if (newScale == oldScale) return; // No change if clamped to limit

            // Point under mouse in world coordinates (before zoom)
            double worldX = (mousePos.X - _viewport.TranslationX) / oldScale;
            double worldY = (mousePos.Y - _viewport.TranslationY) / oldScale;

            _viewport.Scale = newScale;

            // Adjust translation to keep the world point under the mouse position
            _viewport.TranslationX = mousePos.X - (worldX * newScale);
            _viewport.TranslationY = mousePos.Y - (worldY * newScale);
            _log.Information("[ViewportController] Zoom applied: oldScale={Old}, newScale={New}", oldScale, newScale);
        }

        /// <summary>
        /// Handles mouse down events to start a pan interaction.
        /// </summary>
        /// <param name="pos">Position where the mouse was pressed.</param>
        public void OnMouseDown(Point pos)
        {
            _lastMouse = pos;
            _viewport.IsInteracting = true;
            _log.Debug("[ViewportController] MouseDown at {X},{Y}", pos.X, pos.Y);
        }
        /// <summary>
        /// Handles mouse move events during pan (drag).
        /// Applies translation deltas with sensitivity adjusted for current scale.
        /// </summary>
        /// <param name="pos">Current mouse position.</param>
        public void OnMouseMove(Point pos)
        {
            if (_lastMouse == null) return;

            Vector delta = pos - _lastMouse.Value;
            double sensitivity = 1;

            _viewport.TranslationX += delta.X * sensitivity;
            _viewport.TranslationY += delta.Y * sensitivity;
            //_viewport.TranslationY -= delta.Y * sensitivity;

            // Clamp translations to prevent excessive panning (adjust limits as needed)
            _viewport.TranslationX = Math.Clamp(_viewport.TranslationX, -10000, 10000);
            _viewport.TranslationY = Math.Clamp(_viewport.TranslationY, -10000, 10000);

            _lastMouse = pos;
            _log.Debug("[ViewportController] MouseMove: delta={DX},{DY}; new TX={TX}, TY={TY}", delta.X, delta.Y, _viewport.TranslationX, _viewport.TranslationY);
        }
        /// <summary>
        /// Handles mouse up events to end a pan interaction.
        /// </summary>
        public void OnMouseUp()
        {
            _lastMouse = null;
            _viewport.IsInteracting = false;
            _log.Debug("[ViewportController] MouseUp: Interaction ended");
        }
        #endregion
    }
}