# VDLaser – Plan Structuré du Projet

## 1. Vue d’ensemble du projet
**VDLaser** est un logiciel de gravure laser 2D développé en **.NET 8**, suivant l’architecture **MVVM** et utilisant une communication série avec un firmware **GRBL**. L’application inclut une interface moderne et modulaire, permettant de gérer les fichiers GCode, le jogging, les paramètres GRBL, les alarmes/erreurs et l’état machine.

---

## 2. Structure générale de la solution VDLaser

```
VDLaser (Projet VDLaser)
├── Assets                
├── Converters           
│     ├── BoolTovisibility.cs
│     ├── BrushColorConverter.cs
│     ├── ConsoleTypeToBrushConverter.cs
│     ├── DotToCommaConverter.cs
│     ├── IntToTimeConverter.cs
│     ├── InverseBooleanConverter.cs
│     ├── InverseBooleanToVisibilityConverter.cs
│     ├── PercentageConverter.cs
│     ├── PositionToStringConverter.cs
│     ├── StatusToColorConverter.cs
│     ├── StringToIntConverter.cs
├── Views                
│     ├── Controls
│     │     ├── ConsoleView.xaml 
│     │     ├── ControleView.xaml 
│     │     ├── GCodeFileView.xaml 
│     │     ├── JoggingView.xaml 
│     │     ├── MachineStateView.xaml 
│     │     ├── SerialPortSettingView.xaml 
│     │     ├── SettingView.xaml 
│     ├── Main
│     │	    ├── MainWindow.xaml
│     ├── Behaviors.cs
├── Core (Projet VDLaser. Core)
│     ├── Codes
│     │     ├── Alarmcodes.cs
│     │     ├── ErrorCodes.cs
│     │     ├── GrblSettingCodes.cs
│     ├── GCode
│     │     ├── Models
│     │     │     ├── GCodeError.cs
│     │     │     ├── GCodeState.cs
│     │     ├── GCodeAnalyzer.cs
│     │     ├── GCodeGeometry.cs
│     │     ├── GCodeParser.cs
│     │     ├── GCodeTimeEstimator.cs
│     ├── Grbl
│     │     ├── Commands
│     │     │     ├── GrblCommand.cs
│     │     │     ├── GrblCommandResult.cs
│     │     │     └── IGrblCommandQueue.cs
│     │     ├── Models
│     │     │     ├── GrblAlarm.cs
│     │     │     ├── GrblInfo.cs
│     │     │     ├── GrblSetting.cs
│     │     │     ├── GrblState.cs
│     │     ├── Parsers
│     │     │     ├── GCodeResponseParser.cs
│     │     │     ├── GrblInfoParser.cs
│     │     │     ├── GrblSettingsParser.cs
│     │     │     ├── GrblStateParser.cs
│     │     │     ├── IGrblInfoParser.cs
│     │     │     ├── IGrblResponseParser.cs
│     │     │     ├── IGrblSettingsParser.cs
│     │     │     ├── IGrblStateParser.cs
│     │     │     ├── IGrblVersionParser.cs
│     │     │     ├── IGrblSubParser.cs 
│     │     ├── Services
│     │     │     ├── GrblCommandQueueService.cs
│     ├── Interfaces
│     │     ├── IGCodeService.cs
│     │     ├── IGrblCoreService.cs
│     │     ├── ILogService
│     │     ├── ISerialPortService.cs
│     │     ├── ISettingService.cs
│     ├── Models
│     │     ├── AppSettings.cs
│     │     ├── ConsoleItems.cs
│     │     ├── DataReceivedEventArgs
│     │     ├── GCodeItems.cs
│     │     ├── GrblItems.cs
│     │     ├── JoggingItems.cs
│     │     ├── SettingItems.cs
│     ├── Services
│     │     ├── GCodeService.cs
│     │     ├── GrblCoreService.cs
│     │     ├── SerialPortService.cs
│     │     ├── SerilogLogService
│     │     ├── SettingService.cs
│     ├── Tools
│     │     ├── GeometryEngine.cs
├── Tests (Projet VDLaser.Tests)
│     ├── UnitTests
│     │	    ├── ConverterTests.cs
│     │	    ├── GrblCoreService.cs
│     │	    ├── GrblParserTests.cs
│     │	    ├── MathToolTests.cs
│     │	    ├── SerialPortSettingTests.cs
│     │	    ├── ViewTests.cs
├──ViewModels (Projet VDLaser.ViewModels)
│     ├── Base
│     │     ├── ViewModelBase.cs
│     ├── Controls
│     │     ├── ConsoleViewModel.cs
│     │     ├── ControleViewModel.cs
│     │     ├── GCodeFileViewModel.cs
│     │     ├── JoggingViewModel.cs
│     │     ├── MachinbeStateViewModel.cs
│     │     ├── SerialPortSettingViewModel.cs
│     │     ├── SettingViewModel.cs
│     ├── Main
│     │     ├── MainWindowViewModel.cs
```

---
## 3. Détail des dossiers VDLaser

### 3.1 Assets
- Contient les icônes, images et ressources visuelles.

### 3.2 Converters
Convertisseurs WPF utilisés dans les bindings :
- BoolToVisibility
- BrushColorConverter
- DotToCommaConverter
- IntToTimeConverter
- InverseBooleanConverter
- InverseBooleanToVisibilityConverter
- PercentageConverter
- PositionToStringConverter
- StatusToColorConverter
- StringToIntConverter

---
### 3.3 Vues (Views)
#### 3.3.1 Controls
- **ConsoleView** : affichage des trames GRBL, erreurs/alertes
- **ControleView** : commandes (Home, Stop…)
- **GCodeFileView** : gestion/envoi des fichiers GCode
- **JoggingView** : déplacement manuel X-Y, puissance laser
- **MachineStateView** : affichage positions machine/work
- **SerialPortSettingView** : configuration du port série
- **SettingView** : gestion des paramètres GRBL ($$)

#### 3.3.2 Main Window
- `MainWindow.xaml` : conteneur principal utilisant une grille de contrôles

---
### 3.4 Core (moteur, modèles, parsers, services)

#### 3.4.1 Codes
- **AlarmCodes** : Définition des codes d'alarmes GRBL (versions 0.9 et 1.1).
- **ErrorCodes** : codes d’erreurs GRBL
- **GrblSettingCodes** : catalogue des paramètres

#### 3.4.2 GCode
- GCodeAnalyzer.cs
- GCodeGeometry.cs
- GCodeParser.cs
- GCodeTimeEstimator.cs

#### 3.4.2 Models
- AppSettings
- ConsoleItems
- GCodeItems
- GrblItems
- JoggingItems
- SerialPortSettingItems
- SettingItems

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

