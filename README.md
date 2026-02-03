# VDLaser
VDLaser is a WPF application built with .NET 8 designed to control a 2D laser engraver powered by an Arduino running GRBL.
It provides a modern and reliable interface for sending G‑code, controlling laser parameters, moving axes, and monitoring machine status in real time.

## Key Features
- Serial communication with Arduino (GRBL 1.1+)
- Manual and automated G‑code sending
- 2D preview of toolpaths
- Laser control: preview On/Off, continuous or pulsed mode
- Jogging controls (X/Y/Z) with adjustable step sizes
- G‑code file loading, parsing, and execution
- Real‑time GRBL feedback: Machine status, Alarms, Error messages
- Clean MVVM architecture
- Unit tests included

## Project Structure
VDLaser.sln
 ├── VDLaser               → WPF UI (.NET 8)
 ├── VDLaser.Core          → Business logic, GRBL communication, G‑code parsing
 ├── VDLaser.ViewModels    → MVVM bindings and UI logic
 └── VDLaser.Tests         → Unit tests

## Installation & Usage
- Requirements: W10/11, .NET 8 SDK, Arduino with Grbl 1.1+, USB driver
- Build: dotnet build
- Run: dotnet run --project VDLaser
- Connecting to the Laser Engraver:
  1) Plug the Arduino into your PC via USB
  2) Launch VDLaser
  3) Select the COM port
  4) Click Connect
  5) GRBL should respond with something like: Grbl 1.1h ['$' for help]

 ## Loading G‑code Files
1) Go to File → Open
2) Select a .gcode or .nc file
3) The 2D preview appears automatically
4) Click Start to begin engraving
 
 ## Running Unit tests
 dotnet test

## Technologies
.NET 8/WPF/MVVM/Arduino + GRBL/C#/XAML/SerialPort/xUnit / NUnit

## License
This project is licensed under the Apache License 2.0.
You are free to use, modify, and distribute this software — including in commercial applications — as long as you comply with the terms of the license.

## Contributing
Contributions are welcome.
Feel free to submit issues, improvements, or pull requests.

## Contact
For questions or suggestions, please open an issue on GitHub.
