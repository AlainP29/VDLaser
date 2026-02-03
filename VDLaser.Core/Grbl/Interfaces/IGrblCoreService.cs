using System.ComponentModel;
using VDLaser.Core.Grbl.Models;
using VDLaser.Core.Grbl.Services;
using VDLaser.Core.Models;

namespace VDLaser.Core.Grbl.Interfaces
{
    public interface IGrblCoreService : IDisposable,INotifyPropertyChanged
    {
        bool IsConnected { get; }
        bool IsLaserPower { get; }
        bool HasLoadedSettings { get; }
        event EventHandler? StatusUpdated;
        GrblState State { get; }
        bool IsLastErrorAfterOk();
        int? GetLastRxErrorCode();


        event EventHandler<bool>? ConnectionStateChanged;

        event EventHandler<IReadOnlyCollection<GrblSetting>>? SettingsUpdated;
        event EventHandler<GrblInfo>? InfoUpdated;
        event EventHandler? StatusLineReceived;
        event EventHandler<DataReceivedEventArgs>? DataReceived;
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SendCommandAsync(string command);         // Envoi de commandes G-code ou $
        Task SendRealtimeCommandAsync(byte command);   // Pour ? (status), ! (hold), ~ (resume), etc.
        Task HomeAsync();                              // $H
        Task UnlockAsync();                            // $X
        Task GetSettingsAsync();                      // $$$
        // TX bas niveau (appelé UNIQUEMENT par la queue)
        void SendLine(string command);
        void MarkSettingsLoaded();
        void RaiseDataReceived(string text);
    }
}
