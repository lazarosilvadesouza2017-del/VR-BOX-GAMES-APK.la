<div align="center">

# 🕷️ Spider-Man Swinging Simulator

### A physics-driven first-person web-swinging and traversal system built in Unity

[![C#](https://img.shields.io/badge/C%23-Unity-blue?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Unity](https://img.shields.io/badge/Engine-Unity-black?logo=unity&logoColor=white)](https://unity.com/)
[![Game Jolt](https://img.shields.io/badge/Play-Game%20Jolt-red?logo=gamejolt&logoColor=white)](https://gamejolt.com/games/spider-man_sim/738907)

**A Unity physics experiment focused on making Spider-Man-style traversal feel fast, fluid, and responsive.**

<br>

[**▶ Play the Game**](https://gamejolt.com/games/spider-man_sim/738907)

</div>

---

## Overview

**Spider-Man Swinging Simulator** is a Unity-based traversal project built around the physics of web swinging and superhero movement.

Rather than relying entirely on scripted movement, the project uses Unity's physics system to create responsive traversal using **rigid bodies, spring joints, raycasts, 3D vectors, forces, and momentum**.

The project was developed with a particular focus on making movement feel fluid and satisfying while combining several traversal mechanics into one movement system.

The game was published on Game Jolt and has received **1,000+ downloads**.

The main goal of this project was to explore how **physics-based movement can be used to create fluid character traversal**.

## 🎮 Controls

| Input | Action |
| --- | --- |
| `W A S D` | Move |
| `Mouse` | Look around |
| `Left Shift` | Sprint / wall-run upward |
| `Left Ctrl` | Wall-run downward |
| `Space` | Jump / wall jump |
| `Left Mouse Button` | Shoot web / begin swinging |
| `Release Left Mouse Button` | Release web |
| `Esc` | Pause |

---

## Project Structure

The primary gameplay scripts are located in:

```text
Assets/
└── Scripts/
    ├── WebShooters.cs
    ├── PlayerMovement.cs
    ├── WallRunning.cs
    ├── WallClimbing.cs
    ├── FirstPerson.cs
    ├── PauseMenu.cs
    └── ...
```

### Key Systems

| Script | Responsibility |
| --- | --- |
| `WebShooters.cs` | Web targeting, swinging, spring-joint physics, web rendering |
| `PlayerMovement.cs` | Ground movement, sprinting, jumping, air control |
| `WallRunning.cs` | Wall detection, wall running, wall sticking, wall jumping |
| `WallClimbing.cs` | Wall detection and vertical climbing |
| `FirstPerson.cs` | First-person camera and camera effects |
| `PauseMenu.cs` | Pause and resume functionality |

---

## Tech Used

**Engine**

- Unity

**Programming**

- C#
- Unity Physics
- Rigidbody dynamics
- Spring joints
- Raycasting
- Sphere casting
- 3D vector mathematics

**Unity Systems**

- Line Renderer
- TextMesh Pro
- Physics system
- Materials and shaders
- Audio system
- Unity UI

---

## Running the Project

### Requirements

- Unity
- Visual Studio or another C# IDE

### Setup

Clone the repository:

```bash
git clone https://github.com/allxn753/Spider-Man-Swinging-Simulator.git
```

Open the project using Unity Hub and load the main scene under:

```text
Assets/Scenes/
```

The gameplay systems can be found under:

```text
Assets/Scripts/
```

---

## 🎮 Play the Finished Build

The playable build is available on Game Jolt:

<div align="center">

### [PLAY SPIDER-MAN SWINGING SIMULATOR](https://gamejolt.com/games/spider-man_sim/738907)

</div>

---

## ⚠️ Disclaimer

This is an independent fan-made project created for educational and experimental purposes.

Spider-Man and associated characters, names, imagery, and intellectual property belong to their respective rights holders. This project is not affiliated with, endorsed by, or sponsored by Marvel, Sony, or Insomniac Games.

Some visual and audio assets used in the project were not created by the developer.

---

## 👤 Author

**Allen Mathew**

Physics & Computing @ University of Waterloo

[GitHub](https://github.com/allxn753)

---

<div align="center">

**Built with C# + Unity**

*Physics • Movement • Simulation • Game Development*

</div>
