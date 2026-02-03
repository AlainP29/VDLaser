using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace VDLaser.ViewModels.Plotter
{
    public partial class ViewportState : ObservableObject
    {
        [ObservableProperty]
        private double _scale = 1.0;

        [ObservableProperty]
        private double _translationX = 0.0;

        [ObservableProperty]
        private double _translationY = 0.0;

        [ObservableProperty]
        private Size _viewportSize = new(0, 0);
        [ObservableProperty]
        private bool _isInteracting;


        /// <summary>
        /// Rectangle visible en coordonnées monde
        /// </summary>
        public Rect VisibleWorldBounds
        {
            get
            {
                if (Scale <= 0 || ViewportSize.Width <= 0 || ViewportSize.Height <= 0)
                    return Rect.Empty;

                double left = -TranslationX / Scale;
                double right = left + ViewportSize.Width / Scale;

                double top = TranslationY / Scale;  // Compensation pour flip Y
                double bottom = top - ViewportSize.Height / Scale;

                return new Rect(
                    left,
                    bottom,
                    right - left,
                    top - bottom);
            }
        }

        partial void OnScaleChanged(double value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));

        partial void OnTranslationXChanged(double value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));

        partial void OnTranslationYChanged(double value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));

        partial void OnViewportSizeChanged(Size value)
            => OnPropertyChanged(nameof(VisibleWorldBounds));
    }
}