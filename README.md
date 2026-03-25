# 🎆 fWorks – Fireworks Simulation (WPF)

A real-time fireworks simulation built in WPF, ported from Caleb Miller’s CodePen implementation using a custom particle system and rendering loop.

---

## ✨ Features

- Real-time fireworks simulation
- Dynamic colors and visual effects
- Multiple firework shell types
- Physics-based particle system
- Custom simulation loop
- Frame statistics tracking
- Automated demo sequences

---

## 🖥 Preview

The application renders animated fireworks using a particle system with:
- explosion dynamics  
- fading trails  
- randomized patterns  
- smooth motion and timing  

<p align="center">
  <img src="Images/fireworkApp.gif" width="795" />
</p>

---

## ⚙️ Technologies

- **.NET (WPF)**
- **C#**
- Custom rendering via `FrameworkElement`
- Object-oriented simulation architecture

---

## 🚀 Getting Started

### Requirements

- Windows OS  
- .NET SDK (project targets `net10.0-windows`, but can be adapted to earlier versions)

### Run the project

```bash
git clone https://github.com/CarloAndreoli/FireworkSimulatorWpfApp.git
cd fWorks
dotnet run