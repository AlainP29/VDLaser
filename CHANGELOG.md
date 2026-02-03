# Changelog
All notable changes to this project will be documented in this file.

The format is based on **Keep a Changelog**, and this project adheres to **Semantic Versioning**.

---

## [1.0.0] - 2026-02-03
### Added
- Complete rewrite of the application using **.NET 8** and **WPF**.
- New MVVM architecture with separate Core and ViewModels projects.
- GRBL communication service (real-time status, alarms, error handling).
- Serial port connection management with automatic detection.
- G-code file loading, parsing, and validation.
- 2D toolpath preview for G-code visualization.
- Manual jogging controls (X/Y/Z) with configurable step sizes.
- Laser control (power, on/off, continuous/pulse mode).
- Execution engine for sending G-code line by line.
- Logging and message console for GRBL feedback.
- Unit test project (VDLaser.Tests).

### Changed
- Updated project structure to a multi-project solution:
  - `VDLaser` (UI)
  - `VDLaser.Core`
  - `VDLaser.ViewModels`
  - `VDLaser.Tests`
- Improved separation of concerns and maintainability.

### Removed
- Old .NET Framework version of the application.
- Legacy UI and outdated GRBL communication code.

---
