# First Person Controller Setup Guide - New Input System

## Prerequisites

Before setting up this controller, ensure you have the **Input System** package installed:

1. Open **Window > Package Manager**
2. Search for "Input System"
3. Click **Install**
4. Unity will prompt you to restart - click **Yes**

## Quick Setup

### 1. Import Input Actions Asset

1. Import the `FPSInputActions.inputactions` file into your project
2. Select it in the Project window
3. In the Inspector, click **Generate C# Class** (optional but recommended for type safety)
4. Place the generated class in your Scripts folder

### 2. Hierarchy Structure

Create the following hierarchy in your scene:

```
Player (Empty GameObject)
├── FirstPersonController.cs
├── FirstPersonCamera.cs
├── CharacterController component
└── CameraHolder (Empty GameObject)
    └── Main Camera
```

### 3. Step-by-Step Setup

#### Step 1: Create the Player Object
1. Create an empty GameObject and name it "Player"
2. Reset its transform (Position: 0,0,0)
3. Add a `CharacterController` component
   - Set Radius: 0.3
   - Set Height: 2.0
   - Set Center: 0,0,0

#### Step 2: Add Character Controller Script
1. Add the `FirstPersonController.cs` script to the Player GameObject
2. In the Inspector, configure:
   - **Movement Settings:**
     - Walk Speed: 5
     - Sprint Speed: 8
     - Crouch Speed: 2.5
     - Prone Speed: 1.5
     - Jump Height: 1.5
   - **Ground Check:**
     - Set Ground Mask to detect ground (e.g., "Ground" layer)
   - **Input:**
     - Drag the `FPSInputActions` asset into the Input Actions field

#### Step 3: Create Camera Hierarchy
1. Create an empty GameObject as a child of Player, name it "CameraHolder"
2. Move your Main Camera as a child of CameraHolder
3. Reset both transforms to local position (0,0,0)

#### Step 4: Add Camera Controller Script
1. Add the `FirstPersonCamera.cs` script to the Player GameObject
2. Assign references in the Inspector:
   - **Camera Holder:** Drag the CameraHolder GameObject
   - **Player Camera:** Drag the Main Camera
   - **Character Controller:** Drag the Player GameObject (or leave to auto-assign)
   - **Input Actions:** Drag the `FPSInputActions` asset

#### Step 5: Configure Camera Settings
- Mouse Sensitivity: 2 (adjust to preference)
- Standing Camera Height: 0.7
- Crouching Camera Height: 0.3
- Prone Camera Height: -0.3
- Enable Head Bob: ✓
- Enable Camera Tilt: ✓

### 4. Layer Setup (Important!)
1. Create a layer called "Ground" (or use "Default")
2. Assign your floor/terrain to this layer
3. In FirstPersonController, set Ground Mask to include only ground layers

## Input Actions Configuration

The `FPSInputActions.inputactions` asset contains two Action Maps:

### Player Action Map
- **Move** (Vector2): WASD keys
- **Look** (Vector2): Mouse delta
- **Jump** (Button): Space
- **Sprint** (Button): Left Shift
- **Crouch** (Button): Left Ctrl or C
- **Prone** (Button): X or Z

### UI Action Map
- **ToggleCursor** (Button): Escape

### Default Controls
- **WASD** - Movement
- **Mouse** - Look around
- **Left Shift** - Sprint (hold)
- **Left Ctrl or C** - Crouch toggle
- **X or Z** - Prone toggle
- **Space** - Jump (only while standing)
- **ESC** - Toggle cursor lock

## Customizing Input Bindings

### Method 1: Edit the .inputactions file
1. Double-click `FPSInputActions.inputactions` in the Project window
2. This opens the Input Actions editor window
3. Select any action and modify its bindings in the right panel
4. Click **Save Asset** when done

### Method 2: Runtime Rebinding
For player-configurable controls, you can use the Input System's rebinding API:

```csharp
// Example rebinding code
var rebindOperation = moveAction.PerformInteractiveRebinding()
    .OnComplete(operation => {
        // Rebinding completed
        operation.Dispose();
    })
    .Start();
```

## Advanced Configuration

### Stance Heights
The controller uses three stance heights:
- **Standing**: 2.0 units (standard height)
- **Crouching**: 1.2 units (60% of standing)
- **Prone**: 0.6 units (30% of standing)

Adjust these in the Inspector based on your game's scale.

### Camera Positioning
Camera heights are relative to the character controller center:
- Standing: +0.7 (near head)
- Crouching: +0.3 (lowered)
- Prone: -0.3 (ground level)

### Head Bob Tuning
For realistic head bob:
- Frequency: 2.0 (steps per second feel)
- Amplitude: 0.05 (subtle movement)
- Sprint Multiplier: 1.5 (faster bob when sprinting)

To disable, uncheck "Enable Head Bob"

### Mouse Sensitivity and Smoothing
- **Mouse Sensitivity**: Raw sensitivity multiplier (default: 2.0)
- **Look Smoothness**: Higher = smoother but more input lag (default: 10.0)
- **Invert Y**: Check to invert vertical look

## Input System Benefits

This implementation uses Unity's new Input System, which provides:

✅ **Better multi-device support** - Easy gamepad/keyboard switching  
✅ **Action-based input** - More maintainable than string-based input  
✅ **Runtime rebinding** - Players can customize controls  
✅ **Input buffering** - More responsive feel  
✅ **Better debugging** - Input debugger window available  
✅ **Composite bindings** - WASD automatically creates Vector2  

## Common Issues

### "InputAction has not been enabled" error
**Solution:** Make sure the Input Actions asset is assigned in both controller scripts and that `OnEnable()` is being called.

### Input not responding
1. Check that the Input Actions asset is properly assigned
2. Verify the asset is enabled (should happen automatically)
3. Check the Input Debugger: **Window > Analysis > Input Debugger**

### Character falls through floor
- Check that your ground has a collider
- Verify Ground Mask includes the correct layer
- Ensure Character Controller radius isn't too small

### Camera rotates weirdly
- Verify CameraHolder is a direct child of Player
- Check that Camera is a child of CameraHolder
- Make sure all local positions start at (0,0,0)

### Mouse sensitivity feels different
The new Input System reads mouse delta directly, so sensitivity scaling is different from legacy input. The default multiplier of 0.02f in the camera script provides a good starting point.

## Debugging Input

### Using the Input Debugger
1. Open **Window > Analysis > Input Debugger**
2. Expand your Input Actions asset
3. Watch actions light up as you press keys
4. Check bindings and see which devices are connected

### Common Debug Steps
```csharp
// Add to Start() to verify actions are loaded
Debug.Log($"Move Action: {moveAction != null}");
Debug.Log($"Look Action: {lookAction != null}");

// Add to Update() to see input values
Debug.Log($"Move Input: {moveAction.ReadValue<Vector2>()}");
Debug.Log($"Look Input: {lookAction.ReadValue<Vector2>()}");
```

## Extension Points

### Adding Gamepad Support
The Input Actions asset already supports multiple input schemes. To add gamepad:

1. Open the Input Actions editor
2. Select an action (e.g., Move)
3. Add binding: **Gamepad > Left Stick**
4. Save the asset

### Adding a Lean System
```csharp
// In FirstPersonCamera.cs, add a new input action for lean:
private InputAction leanAction;

// In Start():
leanAction = playerActionMap.FindAction("Lean");

// In Update():
float leanInput = leanAction.ReadValue<float>();
currentTilt = leanInput * leanAngle;
```

### Adding Stamina
```csharp
// In FirstPersonController.cs:
private float currentStamina = 100f;

private float GetTargetSpeed()
{
    if (isSprinting && currentStamina > 0)
    {
        currentStamina -= Time.deltaTime * 10f;
        return sprintSpeed;
    }
    // Regular speed logic...
}
```

## Performance Notes
- Input System has slightly more overhead than legacy input, but it's negligible
- Actions are cached in Awake() for efficiency
- The `ReadValue<>()` method is optimized for frequent calls
- Event-based input (button presses) reduces Update() overhead

## URP Specific Notes
This controller works seamlessly with URP. Consider:
- Adding post-processing volume to camera
- Using camera stacking if needed for UI
- Adjusting camera culling mask for performance
- Setting up URP-specific effects like motion blur

## Migration from Legacy Input
If you have existing code using the legacy Input class:

| Legacy Input | New Input System |
|-------------|------------------|
| `Input.GetAxis("Horizontal")` | `moveAction.ReadValue<Vector2>().x` |
| `Input.GetKey(KeyCode.Space)` | `jumpAction.IsPressed()` |
| `Input.GetKeyDown(KeyCode.Space)` | Subscribe to `jumpAction.performed` |
| `Input.GetAxis("Mouse X")` | `lookAction.ReadValue<Vector2>().x` |
