using CommunityToolkit.Mvvm.ComponentModel;

namespace VDLaser.ViewModels.Base
{
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {
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
