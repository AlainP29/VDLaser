using CommunityToolkit.Mvvm.ComponentModel;
using VDLaser.Core.Interfaces;

namespace VDLaser.ViewModels.Base
{
    /// <summary>
    /// Provides a base class for view models that supports property change notification and resource management through
    /// the IDisposable pattern.
    /// </summary>
    /// <remarks>ViewModelBase implements the IDisposable interface to allow derived view models to release
    /// resources, such as event subscriptions, when they are no longer needed. Inherit from this class to ensure
    /// consistent property change notification and proper cleanup in MVVM applications. Derived classes should override
    /// Dispose(bool disposing) to dispose managed resources as needed.</remarks>
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {
        protected void LogContextual(ILogService log, string action, string details = "")
        {
            if (log == null) return;

            string vmName = this.GetType().Name.Replace("ViewModel", "VM");

            log.Information("[{VmName}] Action: {Action} | Mode: {Mode} | Details: {Details}",
                vmName, action, log.CurrentProfile, details);
        }
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                // Dispose managed resources here (e.g., unsubscribe from events in derived classes)
                // Example: _someService.PropertyChanged -= MyHandler;
            }
            // Free unmanaged resources if any (rare in ViewModels)
            _disposed = true;
        }
        /// 
        ~ViewModelBase()
        {
            Dispose(false);
        }
    }
}
