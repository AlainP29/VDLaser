using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VDLaser.ViewModels.Plotter;

namespace VDLaser.Views.Controls
{
    public partial class PlotterView : UserControl
    {
        bool _viewInitialized = false;
        private ViewportController? _viewportController;
        public PlotterView()
        {
            InitializeComponent();

            this.DataContextChanged += (s, e) =>
            {
                if (e.OldValue is PlotterViewModel oldVm)
                {
                    oldVm.PropertyChanged -= Vm_PropertyChanged;
                    oldVm.OnRequestAutoCenter -= DoAutoCenter;
                    oldVm.OnRequestResetView -= DoResetView;
                }

                if (e.NewValue is PlotterViewModel vm)
                {
                    vm.PropertyChanged += Vm_PropertyChanged;
                    vm.OnRequestAutoCenter += DoAutoCenter;
                    vm.OnRequestResetView += DoResetView;

                    if (vm.IsSimulating) UpdateAnimationState(vm);
                }
            };
        }
        private void ViewportCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not Canvas canvas) return;

            if (DataContext is PlotterViewModel vm)
            {
                vm.Viewport.ViewportSize = new Size(canvas.ActualWidth, canvas.ActualHeight);
                _viewportController = vm.ViewportController;
            }
            DoResetView();
        }
        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is not PlotterViewModel vm) return;

            if (e.PropertyName == nameof(PlotterViewModel.ToolPathGeometry))
            {
                if (!_viewInitialized && vm.ToolPathGeometry != null)
                {
                    Dispatcher.BeginInvoke(new Action(() => this.DoAutoCenter()));
                    _viewInitialized = true;
                }
            }
            else if (e.PropertyName == nameof(PlotterViewModel.ToolPathGeometry) ||
                     e.PropertyName == nameof(PlotterViewModel.SimulationSpeed))
            {
                if (vm.IsSimulating) UpdateAnimationState(vm);
            }
            else if (e.PropertyName == nameof(PlotterViewModel.IsSimulating))
            {
                Dispatcher.BeginInvoke(new Action(() => UpdateAnimationState(vm)));
            }

        }
        /// <summary>
        /// met à jour l'animation de la trajectoire de l'outil en fonction de l'état de simulation.
        /// </summary>
        /// <param name="vm"></param>
        private void UpdateAnimationState(PlotterViewModel vm)
        {
            if (vm.EngravePathGeometry == null || vm.EngravePathGeometry.Figures.Count == 0) return;

            if (vm.IsSimulating)
            {
                var totalTime = vm.FileViewModel?.Stats?.EstimatedTime ?? TimeSpan.FromSeconds(10);
                double speed = vm.SimulationSpeed > 0 ? vm.SimulationSpeed : 1.0;
                double duration = Math.Max(0.1, totalTime.TotalSeconds / speed);

                var animation = new MatrixAnimationUsingPath
                {
                    PathGeometry = vm.EngravePathGeometry,
                    Duration = new Duration(TimeSpan.FromSeconds(duration)),
                    RepeatBehavior = RepeatBehavior.Forever,
                    DoesRotateWithTangent = true
                };

                ToolMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, animation);
            }
            else
            {
                ToolMatrixTransform.BeginAnimation(MatrixTransform.MatrixProperty, null);
            }
        }
        private void ViewportCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var pos = e.GetPosition(ViewportCanvas);
            _viewportController?.OnMouseWheel(pos, e.Delta);
            if (DataContext is PlotterViewModel vm)
            e.Handled = true;
        }

        private void ViewportCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewportCanvas.CaptureMouse();
            var pos = e.GetPosition(ViewportCanvas);
            _viewportController?.OnMouseDown(pos);
            if (DataContext is PlotterViewModel vm)
            e.Handled = true;
            Trace.WriteLine($"After Move: X={pos.X};Y={pos.Y}");

        }

        private void ViewportCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(ViewportCanvas);
                _viewportController?.OnMouseMove(pos);
                if (DataContext is PlotterViewModel vm)
                    Trace.WriteLine($"After Move: X={pos.X};Y={pos.Y}");
            }
            //e.Handled = true;
        }

        private void ViewportCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewportCanvas.ReleaseMouseCapture();
            _viewportController?.OnMouseUp();
            //e.Handled = true;
        }

        private void ViewportCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            ViewportCanvas.Cursor = Cursors.Hand;
            if (DataContext is PlotterViewModel vm)
            {
                vm.IsHovered = true; 
            }
            e.Handled = true;
        }

        private void ViewportCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (DataContext is PlotterViewModel vm)
            {
                vm.IsHovered = false;
            }
            e.Handled = true;
        }

        private void DoAutoCenter()
        {
            if (DataContext is not PlotterViewModel vm || vm.FileViewModel?.Stats == null) return;

            double canvasW = ViewportCanvas.ActualWidth;
            double canvasH = ViewportCanvas.ActualHeight;
            if (canvasW <= 0 || canvasH <= 0) return;

            Rect bounds = vm.ToolPathBounds ?? Rect.Empty;
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0) return;
            double margin = 0.9;
            double scaleX = (canvasW / bounds.Width) * margin;
            double scaleY = (canvasH / bounds.Height) * margin;
            double finalScale = Math.Min(scaleX, scaleY);

            vm.Viewport.Scale = finalScale;

            double centerX = canvasW / 2;
            double centerY = canvasH / 2;

            vm.Viewport.TranslationX = centerX - (bounds.X + bounds.Width / 2) * finalScale;
            vm.Viewport.TranslationY = centerY + (bounds.Y + bounds.Height / 2) * finalScale;
            Trace.WriteLine($"After Move: canvasW={canvasW};TX={vm.Viewport.TranslationX}");

        }
        private void DoResetView()
        {
            if (DataContext is not PlotterViewModel vm) return;

            vm.Viewport.Scale = 1.0;
            vm.Viewport.TranslationX = 0;
            vm.Viewport.TranslationY = 300;

        }
        private void ViewportCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is PlotterViewModel vm)
            {
                vm.Viewport.ViewportSize = e.NewSize;
                Trace.WriteLine($"Viewport resized: NewSize={e.NewSize}");
            }
        }

    }
}
