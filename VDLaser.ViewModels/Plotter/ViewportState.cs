using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using System.Windows;
using VDLaser.Core.Interfaces;

namespace VDLaser.ViewModels.Plotter
{
    /// <summary>
    /// Observable state for the viewport in the VDLaser plotter.
    /// Manages scale, translations, and visible size, with compensation for Y-axis flip
    /// typical in CNC coordinates (Y up) vs. WPF (Y down).
    /// </summary>
    public partial class ViewportState : ObservableObject
    {
        #region Fields
        private readonly ILogService _log;
        #endregion

        #region Properties
        /// <summary>
        /// Zoom/scale factor of the viewport. Default: 1.0. (100%)
        /// Clamped between 0.5 and 10 to prevent extreme zooms.
        /// </summary>
        [ObservableProperty]
        private double _scale = 1.0;
        /// <summary>
        /// Horizontal (X) translation in screen pixels.
        /// </summary>
        [ObservableProperty]
        private double _translationX = 0.0;
        /// <summary>
        /// Vertical (Y) translation in screen pixels. Compensation for Y flip applied in calculations.
        /// </summary>
        [ObservableProperty]
        private double _translationY = 0.0;
        /// <summary>
        /// Current viewport size (width/height in pixels).
        /// Updated on view resize.
        /// </summary>
        [ObservableProperty]
        private Size _viewportSize = new(0, 0);
        /// <summary>
        /// Indicates if a user interaction is ongoing (e.g., drag/pan).
        /// Used for rendering optimization (e.g., bitmap cache).
        /// </summary>
        [ObservableProperty]
        private bool _isInteracting;
        #endregion

        public ViewportState() { }
        public ViewportState(ILogService log)
        {
            _log = log;
            _log.Information("[ViewportState] Initialized with scale=1.0, translations=0,0");
        }

        #region Calculated Properties
        /// <summary>
        /// Visible rectangle in world coordinates (after scale and translations).
        /// Compensation for Y flip: positive Y upwards.
        /// Returns Rect.Empty if scale or size invalid.
        /// </summary>
        public Rect VisibleWorldBounds
        {
            get
            {
                if (Scale <= 0 || ViewportSize.Width <= 0 || ViewportSize.Height <= 0)
                    return Rect.Empty;

                double left = -TranslationX / Scale;
                double right = left + ViewportSize.Width / Scale;

                double top = TranslationY / Scale;
                double bottom = top - ViewportSize.Height / Scale;

                var bounds = new Rect(
                    left,
                    bottom,
                    right - left,
                    top - bottom);

                _log.Debug("[ViewportState] VisibleWorldBounds computed: {Bounds}", bounds);
                return bounds;
            }
        }
        #endregion

        #region Property Change Handlers
        partial void OnScaleChanged(double value)
        {
            // Clamp scale for stability
            Scale = Math.Max(0.5, Math.Min(10, value));
            OnPropertyChanged(nameof(VisibleWorldBounds));
        }

        partial void OnTranslationXChanged(double value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));

        partial void OnTranslationYChanged(double value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));

        partial void OnViewportSizeChanged(Size value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));
        #endregion

        #region Helper Methods
        /// <summary>
        /// Validates and coerces state values to prevent invalid states.
        /// Can be called after batch updates.
        /// </summary>
        public void EnsureValidState()
        {
            if (Scale < 0.1 || Scale > 20)
            {
                _log.Warning("[ViewportState] Scale out of bounds, resetting to 1.0");
                Scale = 1.0;
            }
            if(ViewportSize.Width < 0 || ViewportSize.Height < 0)
            {
                _log.Warning("[ViewportState] ViewportSize has negative dimensions, resetting to 0,0");
                ViewportSize = new Size(0, 0);
            }
            if (double.IsNaN(TranslationX) || double.IsNaN(TranslationY))
            {
                _log.Warning("[ViewportState] Translation contains NaN, resetting to 0,0");
                TranslationX = 0.0;
                TranslationY = 0.0;
            }
            if (double.IsInfinity(TranslationX) || double.IsInfinity(TranslationY))
            {
                _log.Warning("[ViewportState] Translation contains Infinity, resetting to 0,0");
                TranslationX = 0.0;
                TranslationY = 0.0;
            }
            // Add other validations if needed (e.g., translation bounds)
        }
        #endregion
    }
}