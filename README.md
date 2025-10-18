# Unity First Person Controller (URP)

A production-ready, feature-rich first-person character controller for Unity using the **New Input System** and optimized for **Universal Render Pipeline (URP)**.

## ✨ Features

- **Multiple Stances**: Standing, crouching, and prone with smooth transitions
- **Sprint System**: Hold-to-sprint with configurable speeds
- **Advanced Camera**: Smooth mouse look with adjustable sensitivity and invert options
- **Head Bob**: Fully customizable head bob with separate horizontal/vertical control and stance-based reduction
- **Camera Tilt**: Subtle camera roll when strafing for enhanced immersion
- **Smart Collision**: Prevents standing when obstructed, smooth height transitions
- **Ground Detection**: Reliable ground checking with configurable layer masks
- **Jump System**: Responsive jumping with proper gravity (standing only)
- **Air Control**: Maintains some movement control while airborne
- **New Input System**: Uses Unity's modern Input System for better flexibility and rebinding support

## 🎮 Controls (Default)

| Action | Binding |
|--------|---------|
| Move | WASD |
| Look | Mouse |
| Sprint | Left Shift (hold) |
| Crouch | Left Ctrl or C (toggle) |
| Prone | X or Z (toggle) |
| Jump | Space (standing only) |
| Toggle Cursor | Escape |

## 📦 What's Included

- `FirstPersonController.cs` - Character movement and stance system
- `FirstPersonCamera.cs` - Camera control with head bob and tilt
- `FPSInputActions.inputactions` - Preconfigured Input Actions asset
- `HeadBobVisualizer.cs` - Optional visual tuning tool for head bob
- `SETUP_GUIDE.md` - Comprehensive setup and configuration documentation

## 🚀 Quick Start

### Prerequisites
- Unity 2021.3 or later
- Universal Render Pipeline (URP)
- Input System package (install via Package Manager)

### Installation

1. Install the **Input System** package:
   - Open **Window > Package Manager**
   - Search for "Input System"
   - Click **Install**

2. Import all scripts into your project

3. Create the player hierarchy:
```
Player (Empty GameObject)
├── FirstPersonController.cs
├── FirstPersonCamera.cs
├── CharacterController component
└── CameraHolder (Empty GameObject)
    └── Main Camera
```

4. Configure the CharacterController:
   - Radius: 0.3
   - Height: 2.0
   - Center: (0, 0, 0)

5. Assign the `FPSInputActions.inputactions` asset to both controller scripts

6. Set up Ground Layer and assign to Ground Mask in FirstPersonController

**See [SETUP_GUIDE.md](SETUP_GUIDE.md) for detailed instructions**

## ⚙️ Key Configuration Options

### Movement
- Walk/Sprint/Crouch/Prone speeds
- Acceleration and deceleration
- Air control
- Jump height and gravity

### Camera
- Mouse sensitivity and smoothing
- Vertical look limits (-90° to +90°)
- Invert Y option
- Stance-based camera heights

### Head Bob
- Separate horizontal and vertical amplitudes
- Frequency and smoothing
- Sprint multipliers
- Stance-based reduction (crouch/prone)

### Stance System
- Custom heights for each stance (standing/crouch/prone)
- Automatic collision checking prevents standing in tight spaces
- Smooth transitions with configurable speed

## 🎯 Performance

- Uses `CharacterController` instead of Rigidbody for optimal performance
- Event-driven input reduces Update() overhead
- Efficient ground checking with Physics.CheckSphere
- Smooth interpolation prevents jittery movement
- Zero garbage allocation during runtime

## 🔧 Extension Points

The system is designed for easy extension:

- **Stamina System**: Hook into sprint state and speed calculation
- **Footstep Audio**: Use head bob timer for footstep timing
- **Leaning**: Extend camera tilt system
- **Weapon Sway**: Access camera rotation and movement data
- **Gamepad Support**: Add bindings in Input Actions asset
- **Custom Actions**: Easily add new input actions to the asset

## 📝 Architecture

Built with clean separation of concerns:
- **FirstPersonController**: Handles all character physics and movement
- **FirstPersonCamera**: Manages camera rotation, position, and effects
- **Input Actions**: Decouples input from game logic for easy rebinding

The modular design allows you to swap or modify individual components without affecting others.

## 🐛 Debugging

Use the included **HeadBobVisualizer** for real-time head bob tuning:
1. Attach `HeadBobVisualizer.cs` to your Camera
2. Enable "Show Gizmos" in the Inspector
3. See visual representation of bob ranges in Scene view

For input debugging:
- Open **Window > Analysis > Input Debugger**
- Watch actions activate in real-time
- Verify bindings and connected devices

## 📋 Requirements

- Unity 2021.3+
- Input System package
- Universal Render Pipeline (optional, but optimized for it)

## 📄 License

[Your chosen license]

## 🤝 Contributing

Contributions are welcome! Feel free to submit issues or pull requests.

## 📚 Additional Resources

- [Unity Input System Documentation](https://docs.unity3d.com/Packages/com.unity.inputsystem@1.7/manual/index.html)
- [CharacterController API Reference](https://docs.unity3d.com/ScriptReference/CharacterController.html)
- Full setup guide included: `SETUP_GUIDE.md`

---

**Note**: This controller is optimized for first-person gameplay only. It does not support third-person or other camera modes by design.
