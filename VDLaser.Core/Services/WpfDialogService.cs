using System.Windows;
using VDLaser.Core.Interfaces;

namespace VDLaser.Core.Services
{
    public class WpfDialogService : IDialogService
    {
        public Task ShowErrorAsync(string message, string title = "Erreur")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            return Task.CompletedTask;
        }

        public Task ShowInfoAsync(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        public Task<bool> AskConfirmationAsync(string message, string title = "Confirmation")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return Task.FromResult(result == MessageBoxResult.Yes);
        }
    }
}