# Unity 2D Elevator Simulation
### Underpin Technology Assignment

---

## Quick Setup (5 minutes)

### Requirements
- Unity 6 (6000.x LTS) with **2D** template
- TextMeshPro package (included by default in Unity 6)

### Steps

1. **Open Unity Hub** → New Project → **2D (Core)** template → name it `ElevatorSim`

2. **Copy scripts** — drag all 5 `.cs` files from the `Assets/Scripts/` folder into your Unity project's `Assets/Scripts/` folder:
   - `ElevatorController.cs`
   - `ElevatorDispatcher.cs`
   - `FloorButton.cs`
   - `SceneBuilder.cs`
   - `ElevatorStatusUpdater.cs`

3. **Create the scene**:
   - In the Unity Hierarchy, right-click → **Create Empty** → name it `SceneBuilder`
   - In the Inspector, click **Add Component** → search `SceneBuilder` → add it
   - Make sure **Build On Awake** is checked (it is by default)

4. **Press Play** ▶ — the entire scene builds automatically!

5. **Export** → Assets menu → **Export Package** → select all → save as `ElevatorSim.unitypackage`

---

## How It Works

### Architecture

```
ElevatorDispatcher (Singleton)
    │
    ├── receives RequestFloor(int) from FloorButton
    │
    └── selects best ElevatorController using:
            1. Closest IDLE elevator
            2. Closest BUSY elevator (fallback)
```

### Files

| Script | Responsibility |
|--------|---------------|
| `ElevatorController.cs` | Single elevator: movement queue, smooth lerp, door animation |
| `ElevatorDispatcher.cs` | Routes floor requests to the nearest available elevator |
| `FloorButton.cs` | UI button per floor; shows pending/arrived state |
| `SceneBuilder.cs` | Procedurally builds the entire scene on Play |
| `ElevatorStatusUpdater.cs` | Live floor/state display in status bar |

### Elevator Selection Logic

The dispatcher uses a **two-pass nearest-first** algorithm:

- **Pass 1:** Find the closest *idle* elevator → dispatch immediately
- **Pass 2:** If all elevators are busy, add to queue of the closest one

This prevents all three elevators from racing to the same floor.

### Movement

Elevators use **smooth ease-in/ease-out interpolation** (`t² × (3 - 2t)`) — no instant teleporting. Speed is configurable per elevator via the Inspector.

### Visual Feedback

| Colour | Meaning |
|--------|---------|
| 🔵 Blue | Elevator idle |
| 🟡 Yellow | Elevator moving |
| 🟢 Green | Doors open at floor |
| 🟠 Orange button | Floor request pending |
| 🟢 Green button | Elevator has arrived |

---

## Customisation

In the `SceneBuilder` Inspector you can change:
- `Floor Count` — default 4 (Ground + 3)
- `Elevator Count` — default 3
- `Floor Height` — world units between floors
- `Floor Width` — width of the building

Per-elevator settings (on each `ElevatorController`):
- `Move Speed` — units/second
- `Door Wait Time` — seconds doors stay open

---

## What Was Implemented

✅ 3 elevators with independent movement and request queues  
✅ 4 floors (Ground, 1, 2, 3)  
✅ Floor call buttons with pending/arrived visual feedback  
✅ Nearest-idle-first dispatcher (no all-respond problem)  
✅ Smooth ease-in/ease-out movement (no teleporting)  
✅ Door open/close animation  
✅ Live status strip showing floor and idle/busy state  
✅ Procedural scene — no manual GameObject setup needed  
✅ Clean, commented, single-responsibility scripts  
