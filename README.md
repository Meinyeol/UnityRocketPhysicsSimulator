# 🚀 Rocket Physics Simulator in Unity

_Author: Meinyeol_

This project is a realistic rocket flight simulator built in Unity using C#. It focuses on accurate physical behavior including thrust calculation, gimbal-based stabilization, realistic wind forces, fuel consumption, and mass dynamics—aiming to emulate the behavior of rockets like SpaceX’s Falcon 9.

---

## ✨ Features Implemented

### 🔧 Physics-Based Rocket Flight

- **Realistic thrust calculation** using:  
  `Thrust = exhaustVelocity * fuelBurnRate * engineCount`
- **Dynamic TWR (Thrust-to-Weight Ratio)** computed in real-time
- **Fuel consumption per engine** based on fixed timestep
- **Real-time rocket mass updates** as fuel burns

### 🌀 Gimbal-Based Stabilization

- **Dynamic selection of active engines** to apply torque for stability
- **Per-engine torque calculation** using position and thrust vector
- **Central engine prioritized for optimal torque efficiency**
- **Angle and speed limits** for gimbal rotation
- **Correction direction based on rocket orientation deviation**

### 🌬️ Realistic Wind Simulation

- **Altitude-based wind strength** (calculated with smooth Perlin noise)
- **Lateral wind forces** applied off-center to simulate instability
- **Vertical turbulence component** adds realistic variation
- **Visual debug rays** show wind direction and magnitude in scene

### 🧩 Modular Components

- **Automatic engine placement system**
- **Realistic crew capsule, fuel tank, and landing legs**
- **Dynamic center of mass calculation** for accurate physics

### 🕹️ Controls

- `Space`: Launch or stop the rocket
- `1–9`: Toggle individual engines
- Debug and gizmo views for mass, center of mass, and forces

---

## 🚧 To-Do / Future Improvements

- 🧩 **Stage Separation System**  
  Split the rocket into different physical stages before entering space

- 🖼️ **Graphical Enhancements**  
  Add better particles, shaders, HUD, animations, sound design

- 🛬 **Autonomous Landing System**  
  Full-body recovery (except the crew capsule), with controlled descent, velocity management and landing gear deployment

---

## 🛠️ Technologies Used

- Unity 2021+
- C# (MonoBehaviour, Rigidbody Physics, Coroutines)
- FixedUpdate for deterministic physics behavior
- Modular prefab logic
- Visual debugging with Gizmos

---

## ▶️ How to Run

1. Clone the repository:
   ```bash
   git clone https://github.com/meinyeol/UnityRocketPhysicsSimulator.git
   ```
