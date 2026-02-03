using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace VDLaser.Core.Services
{
    public partial class KeyboardShortcutManager : ObservableObject
    {
        // Définition des raccourcis clavier avec ObservableProperty
        [ObservableProperty]
        private Key _jogUpKey = Key.W; // Par défaut, "W" pour déplacer vers le haut

        [ObservableProperty]
        private Key _jogDownKey = Key.S; // Par défaut, "S" pour déplacer vers le bas

        [ObservableProperty]
        private Key _jogLeftKey = Key.A; // Par défaut, "A" pour déplacer à gauche

        [ObservableProperty]
        private Key _jogRightKey = Key.D; // Par défaut, "D" pour déplacer à droite

        [ObservableProperty]
        private Key _increaseSpeedKey = Key.OemPlus; // "+" pour augmenter la vitesse

        [ObservableProperty]
        private Key _decreaseSpeedKey = Key.OemMinus; // "-" pour diminuer la vitesse

        // Commandes
        public IRelayCommand JogUpCommand { get; }
        public IRelayCommand JogDownCommand { get; }
        public IRelayCommand JogLeftCommand { get; }
        public IRelayCommand JogRightCommand { get; }
        public IRelayCommand IncreaseSpeedCommand { get; }
        public IRelayCommand DecreaseSpeedCommand { get; }

        // Constructeur
        public KeyboardShortcutManager()
        {
            // Initialisation des commandes
            JogUpCommand = new RelayCommand(OnJogUp);
            JogDownCommand = new RelayCommand(OnJogDown);
            JogLeftCommand = new RelayCommand(OnJogLeft);
            JogRightCommand = new RelayCommand(OnJogRight);
            IncreaseSpeedCommand = new RelayCommand(OnIncreaseSpeed);
            DecreaseSpeedCommand = new RelayCommand(OnDecreaseSpeed);
        }

        // Méthodes d'action pour chaque commande
        private void OnJogUp()
        {
            // Logique pour déplacer la tête vers le haut
        }

        private void OnJogDown()
        {
            // Logique pour déplacer la tête vers le bas
        }

        private void OnJogLeft()
        {
            // Logique pour déplacer la tête à gauche
        }

        private void OnJogRight()
        {
            // Logique pour déplacer la tête à droite
        }

        private void OnIncreaseSpeed()
        {
            // Logique pour augmenter la vitesse
        }

        private void OnDecreaseSpeed()
        {
            // Logique pour diminuer la vitesse
        }

        // Méthode pour gérer les pressions de touches et déclencher les actions correspondantes
        public void HandleKeyPress(Key key)
        {
            switch (key)
            {
                case var k when k == JogUpKey:
                    JogUpCommand.Execute(null);
                    break;
                case var k when k == JogDownKey:
                    JogDownCommand.Execute(null);
                    break;
                case var k when k == JogLeftKey:
                    JogLeftCommand.Execute(null);
                    break;
                case var k when k == JogRightKey:
                    JogRightCommand.Execute(null);
                    break;
                case var k when k == IncreaseSpeedKey:
                    IncreaseSpeedCommand.Execute(null);
                    break;
                case var k when k == DecreaseSpeedKey:
                    DecreaseSpeedCommand.Execute(null);
                    break;
            }
        }
    }
}
