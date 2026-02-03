# VDLaser – Plan Structuré du Projet

## 1. Vue d’ensemble du projet
**VDLaser** est un logiciel de gravure laser 2D développé en **.NET 8**, suivant l’architecture **MVVM** et utilisant une communication série avec un firmware **GRBL**. L’application inclut une interface moderne et modulaire, permettant de gérer les fichiers GCode, le jogging, les paramètres GRBL, les alarmes/erreurs et l’état machine.

---

## 2. Structure générale de la solution VDLaser

```
VDLaser (Projet principal WPF): Contient les éléments UI (Views, Converters, Behaviors), les assets, et l'App.xaml.
├── Resources               # Ressources globales
│     ├── Assets            # Images, icônes
│     ├── Styles            # Styles XAML globaux
├── Converters              # Tous les value converters WPF
│     ├── BooleanToStringConverter.cs
│     ├── BoolToVisibility.cs
│     ├── BrushColorConverter.cs
│     ├── ConsoleTypeToBrushConverter.cs
│     ├── DotToCommaConverter.cs
│     ├── EqualityToBoolConverter.cs
│     ├── IntToTimeConverter.cs
│     ├── InverseBooleanConverter.cs
│     ├── InverseBooleanToVisibilityConverter.cs
│     ├── IsHomingConverter.cs
│     ├── LaserColorConverter.cs
│     ├── MachineStateToTextConverter.cs
│     ├── MachineStateToVisibilityConverter.cs
│     ├── NegativeConverter.cs
│     ├── NullToVisibilityConverter.cs
│     ├── OffsetConverter.cs
│     ├── PercentageConverter.cs
│     ├── PositionToStringConverter.cs
│     ├── StatusToColorConverter.cs
│     ├── StringToIntConverter.cs
│     ├── TabToVisibilityConverter.cs
├── Views                   # Vues WPF
│     ├── Controls          # UserControls
│     │     ├── ConsoleView.xaml 
│     │     ├── ControleView.xaml 
│     │     ├── GCodeFileView.xaml 
│     │     ├── JoggingView.xaml
│     │     ├── MachineStateView.xaml 
│     │     ├── PlotterView.xaml 
│     │     ├── SerialPortSettingView.xaml 
│     │     ├── SettingView.xaml 
│     ├── Main              # Fenêtres principales
│     │	    ├── MainWindow.xaml
│     ├── Behaviors         # Behaviors WPF
│     │	    ├── Behaviors.cs
├── App.xaml                # Entrypoint WPF   
├── VDLaser.Core: Contient la logique métier, les services, les modèles, les interfaces, et les outils liés à GRBL, GCode, et la communication série.
│     ├── Codes             # Enums et codes constants
│     │     ├── Alarmcodes.cs
│     │     ├── ErrorCodes.cs
│     │     ├── GrblSettingCodes.cs
│     ├── GCode             # Tout ce qui touche au GCode
│     │     ├── Interfaces
│     │     │     ├── IGCodeAnalyzer.cs
│     │     │     ├── IGcodeFileService.cs
│     │     │     ├── IGCodeFormatter.cs
│     │     │     ├── IGCodeGeometryService.cs
│     │     │     ├── IGcodeJobService.cs
│     │     │     ├── IGCodeParser.cs
│     │     ├── Models      # Modèles spécifiques GCode
│     │     │     ├── GCodeError.cs
│     │     │     ├── GCodeState.cs
│     │     ├── Parsers     # Parsers GCode
│     │     │     ├── GCodeParser.cs
│     │     ├── GCodeAnalyzer.cs
│     │     ├── GCodeStats.cs
│     │     ├── GCodeTimeEstimator.cs
│     ├── Grbl              # Tout ce qui touche à GRBL
│     │     ├── Commands    # Commandes GRBL
│     │     │     ├── GrblCommand.cs
│     │     │     ├── GrblCommandResult.cs
│     │     ├── Errors      # Exceptions et erreurs
│     │     │     ├── GrblConnectionError.cs
│     │     │     ├── GrblConnectionException.cs
│     │     ├── Interfaces
│     │     │     └── IGrblCommandQueue.cs
│     │     │     ├── IGrblCoreService.cs
│     │     │     ├── IGrblInfoParser.cs
│     │     │     ├── IGrblResponseParser.cs
│     │     │     ├── IGrblSettingsParser.cs
│     │     │     ├── IGrblStateParser.cs
│     │     │     ├── IGrblSubParser.cs 
│     │     ├── Models      # Modèles spécifiques GRBL
│     │     │     ├── GrblAlarm.cs
│     │     │     ├── GrblInfo.cs
│     │     │     ├── GrblSetting.cs
│     │     │     ├── GrblState.cs
│     │     ├── Parsers     # Parsers GRBL
│     │     │     ├── GrblInfoParser.cs
│     │     │     ├── GrblResponseParser.cs
│     │     │     ├── GrblSettingsParser.cs
│     │     │     ├── GrblStateParser.cs
│     │     ├── Services       # Services spécifiques GRBL
│     │     │     ├── GrblCommandQueueService.cs
│     │     │     ├── GrblCoreService.cs
│     ├── Interfaces            # Interfaces générales
│     │     ├── IDialogService.cs
│     │     ├── ILogService.cs
│     │     ├── ISerialConnection.cs
│     │     ├── ISerialPortService.cs
│     │     ├── ISettingService.cs
│     │     ├── IStatusPollingService.cs
│     ├── Models                # Modèles généraux (non spécifiques à GCode/GRBL)
│     │     ├── AppSettings.cs
│     │     ├── ConsoleItems.cs
│     │     ├── DataReceivedEventArgs
│     │     ├── GrblCommandEventArgs
│     │     ├── GrblItems.cs
│     │     ├── JoggingItems.cs
│     │     ├── SettingItems.cs
│     ├── Services              # Services généraux (non GRBL)
│     │     ├── SerialPortConnection.cs
│     │     ├── SerialPortService.cs
│     │     ├── SerilogLogService
│     │     ├── SettingService.cs
│     │     ├── StatusPollingService.cs
│     │     ├── WpfDialogService.cs
│     ├── Tools                 # Outils utilitaires
│     │     ├── Geometry
│     │     │     ├── GcodeGeometryService.cs
│     │     │     ├── GcodeSegment.cs
│     │     │     ├── GeometryEngine.cs
│     │     │     ├── Point2D.cs
├── VDLaser.Tests: Contient les tests unitaires.
│     ├── UnitTests
│     │     ├── Fakes
│     │	    │     ├── FakeSerialConnection.cs
│     │	    ├── AlarmTests
│     │	    ├── ConverterTests.cs
│     │	    ├── ErrorTests
│     │	    ├── GeometryEngineTests.cs
│     │	    ├── GrblCoreServiceTests.cs
│     │	    ├── GrblStateParserTests.cs
│     │	    ├── ParsesAllSettingsBeyond31.cs
│     │	    ├── StatusPollingServiceTests.cs
├── VDLaser.ViewModels:     # VM pour controls
│     ├── Base              # Base commune
│     │     ├── ViewModelBase.cs
│     ├── Controls
│     │     ├── ConsoleViewModel.cs
│     │     ├── ControleViewModel.cs
│     │     ├── GcodeFileViewModel.cs
│     │     ├── GcodeItemViewModel.cs
│     │     ├── JoggingViewModel.cs
│     │     ├── MachinbeStateViewModel.cs
│     │     ├── SerialPortSettingViewModel.cs
│     │     ├── SettingViewModel.cs
│     ├── Main              # VM principal
│     │     ├── MainWindowViewModel.cs
│     ├── Plotter
│     │     ├── PlotterViewModel.cs
│     │     ├── ViewportController.cs
│     │     ├── ViewportState.cs

```

---
## 3. Détail des dossiers VDLaser

### 3.1 Assets
- Contient les icônes, images et ressources visuelles.

### 3.2 Converters
- Convertisseurs WPF utilisés dans les bindings :

---
### 3.3 Vues (Views)
#### 3.3.1 Controls
- **ConsoleView** : affichage des trames GRBL, erreurs/alertes
- **ControleView** : commandes (Home, Kill Alarm…)
- **GCodeFileView** : gestion/envoi des fichiers GCode
- **JoggingView** : déplacement manuel X-Y, puissance laser, commandes GCode rapides
- **MachineStateView** : affichage des positions et vitesse machine
- **SerialPortSettingView** : configuration/connection du port série
- **SettingView** : gestion des paramètres GRBL ($$) et envoi de commandes Grbl

#### 3.3.2 Main Window
- `MainWindow.xaml` : conteneur principal utilisant une grille de contrôles

---
### 3.4 Core (moteur, modèles, parsers, services)

#### 3.4.1 Codes
- **AlarmCodes** : Définition des codes d'alarmes GRBL (versions 0.9 et 1.1).
- **ErrorCodes** : codes d’erreurs GRBL
- **GrblSettingCodes** : catalogue des paramètres $...

#### 3.4.2 Errors
 - **GCodeError** : Modèle pour représenter les erreurs dans un fichier GCode, incluant la ligne, le message et le type d'erreur.
 - **GCodeState** : Représente l'état d'analyse d'un fichier GCode, incluant les statistiques et les erreurs détectées.

#### 3.4.3 GCode

- GCodeAnalyzer.cs
- GCodeGeometry.cs
- GCodeParser.cs
- GCodeTimeEstimator.cs

#### 3.4.3.2 Models
- AppSettings
- ConsoleItems
- GCodeItems
- GrblItems
- JoggingItems
- SerialPortSettingItems
- SettingItems
#### 3.4.3 Services
- GcodeFileResult.cs: Modèle de résultat pour les opérations sur les fichiers GCode, incluant succès, erreurs et statistiques.
- GcodeFileService.cs: Charge et parse un fichier G-code
- GcodeJobService.cs: Gère l'exécution d'un job G-code : envoi séquentiel des lignes via la queue de commandes, avec pause/resume/stop
- GrblCoreService.cs: Service central pour la communication série avec GRBL
- SerialPortConnection.cs
- SerialPortService.cs
- SerilogLogService.cs
- SettingService.cs
- StatusPollingService.cs
- 
#### 3.4.4 Interfaces
- IGCodeService
- IGrblCoreService
- ISerialPortService
- ISettingService

#### 3.4.5 Tools
- GeometryEngine

### 3.5 GRBL

- **GrblParser.cs** (Orchestrateur)

#### 3.5.1 Models Grbl
- **GrblAlarm** : Modèle de classe pour une alarme GRBL, avec sévérité et timestamp.
- GrblError
- GrblSetting
- **GrblState**:Intégration des alarmes dans l'état global de GRBL (inclut Alarm et AlarmHistory).
- GrblStatus

#### 3.5.2 Parsers Grbl
- IGrblResponseParser / GrblResponseParser
- IGrblStatusParser / GrblStatusParser
- IGrblSettingsParser / GrblSettingsParser
- IGrblInfoParser / GrblInfoParser
- IGrblVersionParser / GrblVersionParser

---
### 3.5 Tests (VDLaser.Tests)
#### 3.5.1 UnitTests
- **AlarmTests**:Tests unitaires pour valider les alarmes.
- ConverterTests
- ErrorTests
- GeometryEngineTests

---
### 3.6. ViewModels
#### 3.6.1 Base
- `ViewModelBase` : objet observable commun

#### 3.6.2 Controls
- ConsoleViewModel
- ControleViewModel
- GCodeFileViewModel
- JoggingViewModel
- MachineStateViewModel
- SerialPortSettingViewModel
- SettingViewModel

#### 3.6.3 Main
- MainWindowViewModel

---
## 4. Arbre de syntaxe abstrait (AST)
Représentation conceptuelle de l’AST utilisé dans VDLaser pour analyser et structurer les réponses GRBL.

```
AST
├── GrblResponse
│   ├── OkResponse
│   ├── ErrorResponse
│   │   ├── ErrorCode
│   │   └── Message
│   ├── AlarmResponse
│   │   ├── AlarmCode
│   │   └── Severity
│   └── UnknownResponse
│
├── StatusReport
│   ├── State
│   ├── MPos
│   │   ├── X
│   │   ├── Y
│   │   └── Z
│   ├── WPos
│   │   ├── X
│   │   ├── Y
│   │   └── Z
│   └── AdditionalFlags
│
├── GrblSettings
│   ├── Setting
│   │   ├── ID
│   │   ├── Value
│   │   └── Description
│   └── Setting...
│
└── GrblInfo
    ├── Version
    ├── Build
    ├── Options
    └── AdditionalInfo
```

## 5. Architecture cible

ViewModels
   ↓ (enqueue)
IGrblCommandQueue
   ↓ (séquentiel)
GrblCoreService
   ↓ (raw TX/RX)
SerialPort

### 5.1 Flux d’une commande GRBL séquencé (G1, $H, $$...)
GrblCommandQueueService.cs : gestion séquentielle des commandes GRBL avec file d’attente et attente de réponse (G-code, $°)

SettingViewModel
    ↓
IGrblCommandQueue.EnqueueAsync("$$", WaitForOk = true)
    ↓
GrblCommandQueue
    ↓
GrblCoreService.Send(...)
    ↓
GRBL
    ↓
DataReceived
    ↓
"ok"  ==> TaskCompletionSource.SetResult()

### 5.2 Flux d’un état machine asynchrone <Idle... (STATUS)
GrblCoreService.cs : écoute asynchrone des trames de statut GRBL et mise à jour de l’état machine via les parsers.

SerialPort
   ↓
GrblCoreService
   ↓
Parsers
   ↓
GrblState   ←── STATUS ASYNCHRONE
   ↓
StateUpdated
   ↓
MachineStateViewModel


### 5.2 Flux de polling de l’état machine GRBL au démarrage
ConnectAsync()
   ↓
ConnectionStateChanged(true)
   ↓
StatusPollingService.Start()
   ↓
? ? ? ? ?  (10 Hz)
   ↓
Parsers → GrblState → StateUpdated
   ↓
MachineStateViewModel

### 5.3 Diagramme de séquence : Polling de l’état machine GRBL
Timer Tick
   |
   |-- if !_isRequestPending
   |
   |-- core.SendRealtimeCommand('?')
           |
           |-- GRBL --> <Idle|MPos...>
                   |
                   |-- GrblStateParser.Parse
                   |-- Core.StatusUpdated
                           |
                           |-- polling._isRequestPending = false
                           |-- MachineStateVM.Refresh()

### 5.4 Diagramme de séquence : Connexion à GRBL
UI
 └─ MainWindowViewModel
     └─ ConnectCommand
         └─ GrblCoreService.ConnectAsync()
             ├─ Open COM
             ├─ Handshake GRBL ($I)
             ├─ Timeout sécurisé
             ├─ Chargement settings ($$)
             └─ Connected = true
### 5.5 Diagramme de séquence : Envoi d’une commande GCODE (G1 X10 Y10)
GcodeParser
   ↓
GcodeCommand   (métier, pur)
   ↓
GCodeItemViewModel   (UI)


### 6 Diagramme de classes
┌────────────────────────┐
│      ControleViewModel │
└───────────▲────────────┘
            │ uses
┌───────────┴────────────┐
│     IGrblCoreService   │◄─────────────────────────────┐
└───────────▲────────────┘                              │
            │ implements                                │
┌───────────┴────────────┐               subscribes     │
│     GrblCoreService    │◄──────────┐                  │
└───────┬───────┬────────┘           │                  │
        │       │                    │                  │
        │       │ uses               │ uses             │
        │       │                    │                  │
┌───────▼───┐  ┌▼────────────────┐  ┌───────────────────▼────────────┐
│SerialPort │  │IGrblSubParser[] │  │   StatusPollingService         │
└───────────┘  └───────▲─────────┘  └────────────────────────────────┘
                        │ implements
             ┌──────────┴───────────┐
             │   GrblStateParser    │
             └──────────────────────┘

┌───────────────────────────────┐
│    GrblCommandQueueService    │
└───────────▲───────────────────┘
            │ uses
┌───────────┴────────────┐
│     IGrblCoreService   │
└────────────────────────┘
