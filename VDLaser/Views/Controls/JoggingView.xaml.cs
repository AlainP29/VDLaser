using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using VDLaser.ViewModels.Controls;

namespace VDLaser.Views.Controls
{
    /// <summary>
    /// Logique d'interaction pour JoggingView.xaml
    /// </summary>
    public partial class JoggingView : UserControl
    {
        private DispatcherTimer _longPressTimer;
        private bool _isLongPressTriggered;
        private string _currentDirection;
        private const int LongPressDelayMs = 200;
        private bool _isMoving = false;
        public JoggingView()
        {
            InitializeComponent();
            var serviceProvider = (App.Current as App)?.ServiceProvider;
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetRequiredService<JoggingViewModel>();
            }
            _longPressTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(LongPressDelayMs)
            };
            _longPressTimer.Tick += OnLongPressTimerTick;
        }
        private async void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is not JoggingViewModel vm)
                return;

            if (!vm.IsSelectedKeyboard) 
                return;

            if (e.IsRepeat)
                return;

            switch (e.Key)
            {
                case Key.Up:
                    _isMoving = true;
                    vm.JogUpStartCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Down:
                    _isMoving = true;
                    vm.JogDownStartCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Left:
                    _isMoving = true;
                    vm.JogLeftStartCommand.Execute(null);
                    e.Handled = true;
                    break;
                case Key.Right:
                    _isMoving = true;
                    vm.JogRightStartCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }
        private async void OnPreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (DataContext is not JoggingViewModel vm)
                return;
            if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right)
            {
                vm.JogStopCommand.Execute(null);
                _isMoving = false;
                e.Handled = true;
            }
        }
        private async void OnLaserPreviewDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is JoggingViewModel vm && vm.CanExecuteLaserPreview())
            {
                try
                {
                    await vm.StartLaserPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de l'activation preview : {ex.Message}", "Erreur VDLaser", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }
        private async void OnLaserPreviewUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is JoggingViewModel vm)
            {
                try
                {
                    await vm.StopLaserPreview();  // Appel async pour désactiver le laser (M5)
                    //await vm.StopLaserPreviewCommand.ExecuteAsync(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la désactivation preview : {ex.Message}", "Erreur VDLaser", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            e.Handled = true;
        }
        private async void OnLaserPreviewMouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (DataContext is JoggingViewModel vm)
                {
                    try
                    {
                        await vm.StopLaserPreview();  // Force l'extinction pour sécurité
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erreur sécurité preview : {ex.Message}", "Erreur VDLaser", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            e.Handled = true;
        }
        private void OnJogButtonMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is JoggingViewModel vm && sender is Button btn && e.LeftButton == MouseButtonState.Pressed)
            {
                _isLongPressTriggered = false;
                _currentDirection = btn.Tag as string;
                _longPressTimer.Start();

                e.Handled = true;
            }
        }
        private void OnLongPressTimerTick(object? sender, EventArgs e)
        {
            _longPressTimer.Stop();
            if (!_isLongPressTriggered && DataContext is JoggingViewModel vm)
            {
                _isLongPressTriggered = true;

                switch (_currentDirection)
                {
                    case "Up": vm.JogUpStartCommand.Execute(null); break;
                    case "Down": vm.JogDownStartCommand.Execute(null); break;
                    case "Left": vm.JogLeftStartCommand.Execute(null); break;
                    case "Right": vm.JogRightStartCommand.Execute(null); break;
                    case "NW": vm.JogNWStartCommand.Execute(null); break;
                    case "SW": vm.JogSWStartCommand.Execute(null); break;
                    case "SE": vm.JogSEStartCommand.Execute(null); break;
                    case "NE": vm.JogNEStartCommand.Execute(null); break;
                }
            }
        }
        private void OnJogButtonMouseUp(object sender, MouseButtonEventArgs e)
        {
            _longPressTimer.Stop();
            if (DataContext is JoggingViewModel vm && e.LeftButton == MouseButtonState.Released)
            {

                if (_isLongPressTriggered)
                {
                    vm.JogStopCommand.Execute(null);
                }
                else
                {
                    if (sender is Button btn)
                    {
                        vm.JogCommand.Execute(btn.CommandParameter);
                    }
                }
                _isLongPressTriggered = false;
                _currentDirection = null;
                e.Handled = true;
            }
        }
        private void OnJogButtonMouseLeave(object sender, MouseEventArgs e)
        {
            _longPressTimer.Stop();
            if (e.LeftButton == MouseButtonState.Pressed && DataContext is JoggingViewModel vm)
            {
                vm.JogStopCommand.Execute(null);
                _isLongPressTriggered = false;
                _currentDirection = null;
                e.Handled = true;
            }
        }
    }
}
