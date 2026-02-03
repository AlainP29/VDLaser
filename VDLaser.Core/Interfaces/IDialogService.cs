using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDLaser.Core.Interfaces
{
    public interface IDialogService
    {
        Task ShowErrorAsync(string message, string title = "Erreur");
        Task ShowInfoAsync(string message, string title = "Information");
        Task<bool> AskConfirmationAsync(string message, string title = "Confirmation");
    }
}
