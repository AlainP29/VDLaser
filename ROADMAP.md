# VDLaser Roadmap

This document outlines the planned features, improvements, and long‑term goals for the VDLaser project.  
The roadmap is not a strict schedule but a vision of where the project is heading.

---

## 🟩 Short‑Term Goals (Next Releases)

### **Core Features**
- [ ] Improve GRBL real‑time status parsing (buffer, overrides, feed/spindle)
- [ ] Add configurable homing and soft limits
- [ ] Add support for GRBL settings ($$) reading and editing
- [X] Add pause/resume/stop controls during G‑code execution
- [ ] Improve error handling and recovery after GRBL alarms

### **UI & UX**
- [ ] Add zoom, pan, and grid to the 2D preview
- [ ] Add dark/light theme support
- [X] Add a console history with filtering (errors, warnings, commands)
- [X] Add a settings panel for serial port and machine configuration

### **G‑code Handling**
- [ ] Add G‑code syntax highlighting
- [ ] Add G‑code validation before execution
- [X] Add estimated engraving time calculation

---

## 🟧 Mid‑Term Goals

### **File Import & Processing**
- [ ] Add SVG import with path conversion to G‑code
- [ ] Add DXF import
- [ ] Add image‑to‑G‑code conversion (raster engraving)
- [ ] Add material presets (wood, leather, cardboard, acrylic)

### **Machine Control**
- [ ] Add jog presets (fast, medium, fine)
- [X] Add custom macros (user‑defined G‑code buttons)
- [ ] Add support for Z‑axis probing (if hardware supports it)

### **Simulation & Safety**
- [ ] Add simulation mode without hardware
- [X] Add bounding box preview before engraving
- [ ] Add safety warnings (laser power, missing homing, etc.)

---

## 🟥 Long‑Term Goals

### **Advanced Features**
- [ ] Add plugin system for custom modules
- [ ] Add support for alternative firmware (e.g., FluidNC, Smoothieware)
- [ ] Add 3D preview for Z‑axis engraving depth
- [ ] Add camera alignment (webcam overlay for positioning)

### **Cross‑Platform & Architecture**
- [ ] Evaluate migration to .NET MAUI or Avalonia for cross‑platform support
- [ ] Add hardware abstraction layer for non‑GRBL controllers

### **Community & Documentation**
- [ ] Add full user manual
- [ ] Add developer documentation (API, architecture)
- [ ] Add sample G‑code files and demo projects

---

## 🧩 Completed (History)

### **Version 1.0.0**
- Initial .NET 8 rewrite
- New MVVM architecture
- GRBL communication service
- G‑code preview and execution
- Jogging controls
- Laser power control
- Unit test project

---

If you want to propose new ideas or improvements, feel free to open an issue or submit a pull request.
