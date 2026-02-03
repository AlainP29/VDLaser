using System;

namespace VDLaser.Core.Models
{
    /// <summary>
    /// Arguments d'événement pour les données reçues depuis GRBL.
    /// Peut contenir soit une ligne texte, soit un buffer binaire.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Ligne de texte reçue depuis GRBL (ASCII ou UTF-8).
        /// Peut être null si les données sont binaires uniquement.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// Données brutes reçues depuis la liaison série.
        /// Peut être null si une ligne texte est passée.
        /// </summary>
        public byte[]? Buffer { get; }

        /// <summary>
        /// Constructeur pour réception texte (cas GRBL standard).
        /// </summary>
        public DataReceivedEventArgs(string text)
        {
            Text = text;
            Buffer = null;
        }

        /// <summary>
        /// Constructeur pour réception binaire (si besoin).
        /// </summary>
        public DataReceivedEventArgs(byte[] buffer)
        {
            Buffer = buffer;
            Text = null;
        }
    }
}
