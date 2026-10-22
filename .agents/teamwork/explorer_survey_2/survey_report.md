# Tutorial Scene Survey Report

**Scene File**: `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`  
**Date**: 2026-10-12  
**Surveyor**: Explorer 2 (Tutorial Scene Surveyor)  
**Integrity Mode**: Development / Verified Live via Unity MCP & AST/YAML Inspection  

---

## Executive Summary

A comprehensive survey of `Assets/Prospector-journey/Tutorial/Tutorial scene.unity` was conducted directly using the Unity MCP connection and scene YAML asset inspection. The scene contains 21 root GameObjects and 150 PrefabInstances. Key findings include:
1. **XR Origin**: Present as a PrefabInstance of `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab`. It contains a `CharacterController` with default settings (`radius=0.1`, `stepOffset=0.5`, `skinWidth=0.08`), but **lacks `PlayerBodyCollision` completely**.
2. **Environment & Colliders**: `Environment2` contains 135 children from `SimpleNaturePack` and a `TeleportationArea` component. Crucially, all nature prefabs—including small foliage (`Grass_01`, `Grass_02`, `Flowers_01`, `Flowers_02`, `Mushroom_01`, `Mushroom_02`, `Branch_01`)—**already have solid colliders** (`BoxCollider` / `MeshCollider`) on Layer 0 (`Default`). All root village props (`House_1` to `House_3`, `Bench`, `Crate`, `Fence`, `Lantern`, `Flower Pot`, `Pine Tree_1`, `Pine Tree_2`) have solid `MeshCollider` components attached.
3. **Cabin & Door**: `House_1` at `(19.70, -0.10, 22.10)` is the designated cabin/homestead, surrounded by the front yard props (`Bench`, `Crates`, `Flower Pot`, `Lanterns`, `Fences`). However, **no door slab, trigger collider, prompt UI, or `DoorSceneTransition` component exists anywhere in the scene**.
4. **Inventory & Tools**: Neither `[Prospector_Inventory]` nor grabbable tools (`tool-shovel`, `Gold_Pan`) exist in the scene.

---

## 1. XR Origin (XR Rig) Hierarchy & Component Configuration

### 1.1 Hierarchy Layout
```
XR Origin (XR Rig) [Layer: 2 (Ignore Raycast), Tag: Untagged, Pos: (0, 0, 0), Rot: (0, 0, 0)]
├── Camera Offset [Pos: (0, 1.36144, 0)]
│   ├── Main Camera [Tag: MainCamera, Layer: 0]
│   │   ├── Camera [Near: 0.01, Far: 1000]
│   │   ├── AudioListener
│   │   ├── TrackedPoseDriver (New Input System)
│   │   └── TunnelingVignette
│   ├── Gaze Stabilized -> Gaze Stabilized Attach
│   ├── Left Controller [Pos: (0, 0, 0)]
│   │   ├── ControllerInputActionManager
│   │   ├── ActionBasedController
│   │   ├── TrackedPoseDriver
│   │   ├── Left_NearFarInteractor (Near/Far direct & ray interaction)
│   │   ├── Poke Interactor
│   │   └── Teleport Interactor
│   ├── Left Controller Teleport Stabilized Origin -> Left Controller Stabilized Attach
│   ├── Right Controller [Pos: (0, 0, 0)]
│   │   ├── ControllerInputActionManager
│   │   ├── ActionBasedController
│   │   ├── TrackedPoseDriver
│   │   ├── Right_NearFarInteractor (Near/Far direct & ray interaction)
│   │   ├── Poke Interactor
│   │   └── Teleport Interactor
│   └── Right Controller Teleport Stabilized Origin -> Right Controller Stabilized Attach
└── Locomotion [Layer: 2]
    ├── LocomotionMediator
    ├── LocomotionProvider
    ├── SnapTurn (SnapTurnProvider)
    ├── ContinuousTurn (ContinuousTurnProvider, TurnSpeed: 60)
    ├── ContinuousMove (ContinuousMoveProvider, MoveSpeed: 2.0)
    ├── Jump
    ├── Grab Move
    ├── Teleportation (TeleportationProvider)
    └── Climb (ClimbProvider)
```

### 1.2 Components on Root `XR Origin (XR Rig)`
| Component | Type / Class | Current Values | Parity with Room Scene / Requirement R1 |
|---|---|---|---|
| **Transform** | `UnityEngine.Transform` | `pos=(0, 0, 0)`, `rot=(0, 0, 0)`, `scale=(1, 1, 1)` | Root spawn origin |
| **XROrigin** | `Unity.XR.CoreUtils.XROrigin` | `Camera=Main Camera`, `Origin=XR Origin`, `CameraFloorOffsetObject=Camera Offset`, `CameraYOffset=1.36144` | Matches Room scene |
| **CharacterController** | `UnityEngine.CharacterController` | `radius = 0.1`, `height = 1.36144`, `center = (0, 0.76072, 0)`, `stepOffset = 0.5`, `skinWidth = 0.08` | **Needs adjustment**: R1 requires `radius = 0.24`, `stepOffset = 0.08`, `skinWidth = 0.025` |
| **InputActionManager** | `UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager` | References `XRI Default Input Actions.inputactions` | Matches Room scene |
| **XRInputModalityManager**| `UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager` | Hand/Controller mode manager | Matches Room scene |
| **XRGazeAssistance** | `UnityEngine.XR.Interaction.Toolkit.Gaze.XRGazeAssistance` | Disabled | Inactive |
| **PlayerBodyCollision** | `ProspectorJourney.Room.PlayerBodyCollision` | **NOT PRESENT** | **MISSING**: Must be attached with `bodyRadius=0.24`, `stepOffset=0.08`, `skinWidth=0.025` |

---

## 2. Environment GameObjects, Props & Colliders

### 2.1 `Environment2` GameObject
- **Root Object**: `Environment2` (Instance ID: 76372, Layer: 0, Tag: Untagged, Transform `(0, 0, 0)`).
- **Components**:
  1. `Transform` containing **135 child objects** (all `SimpleNaturePack` assets).
  2. `UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea`:
     - `InteractionLayers`: `2147483648` (Teleport layer).
     - `m_Colliders`: `[]` (empty list; automatically targets colliders on children).

### 2.2 Survey of `Environment2` Children (SimpleNaturePack)
| Category | Prefab Asset | Count | Collider Present in Prefab? | Collider Type | Tag / Layer |
|---|---|---|---|---|---|
| **Ground Tiles** | `Ground_01.prefab` | 3 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| | `Ground_02.prefab` | 10 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| | `Ground_03.prefab` | 3 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| **Trees** | `Tree_01.prefab` | 3 | Yes | `BoxCollider` (on trunk, non-trigger) | Untagged / Layer 0 |
| | `Tree_02.prefab` | 12 | Yes | `BoxCollider` (on trunk, non-trigger) | Untagged / Layer 0 |
| | `Tree_03.prefab` | 7 | Yes | `BoxCollider` (on trunk, non-trigger) | Untagged / Layer 0 |
| | `Tree_04.prefab` | 9 | Yes | `BoxCollider` (on trunk, non-trigger) | Untagged / Layer 0 |
| | `Tree_05.prefab` | 9 | Yes | `BoxCollider` (on trunk, non-trigger) | Untagged / Layer 0 |
| **Rocks** | `Rock_01.prefab` | 3 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| | `Rock_02.prefab` | 2 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| | `Rock_03.prefab` | 6 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| | `Rock_04.prefab` | 3 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| | `Rock_05.prefab` | 12 | Yes | `MeshCollider` (non-convex, non-trigger) | Untagged / Layer 0 |
| **Stumps & Bushes** | `Stump_01.prefab` | 3 | Yes | `BoxCollider` (non-trigger) | Untagged / Layer 0 |
| | `Bush_02.prefab` | 1 | Yes | `BoxCollider` (non-trigger) | Untagged / Layer 0 |
| | `Bush_03.prefab` | 6 | Yes | `BoxCollider` (non-trigger) | Untagged / Layer 0 |
| **Foliage (Critical!)**| `Grass_01.prefab` | 15 | **YES** | `BoxCollider` (solid, non-trigger) | Untagged / Layer 0 |
| | `Grass_02.prefab` | 4 | **YES** | `BoxCollider` (solid, non-trigger) | Untagged / Layer 0 |
| | `Flowers_01.prefab` | 4 | **YES** | `BoxCollider` (solid, non-trigger) | Untagged / Layer 0 |
| | `Flowers_02.prefab` | 2 | **YES** | `BoxCollider` (solid, non-trigger) | Untagged / Layer 0 |
| | `Mushroom_01.prefab`| 9 | **YES** | `BoxCollider` (solid, non-trigger) | Untagged / Layer 0 |
| | `Mushroom_02.prefab`| 4 | **YES** | `BoxCollider` (solid, non-trigger) | Untagged / Layer 0 |
| | `Branch_01.prefab` | 3 | **YES** | `MeshCollider` (solid, non-trigger) | Untagged / Layer 0 |

> **Critical Snag Risk Identified**:
> In `SimpleNaturePack`, small foliage items (`Grass_01`, `Grass_02`, `Flowers_01`, `Flowers_02`, `Mushroom_01`, `Mushroom_02`, `Branch_01`) **ship with active solid BoxColliders and MeshColliders by default**. Because they reside on Layer 0 (`Default`) and are `Untagged`, walking through grass or over mushrooms will physically collide with the player's `CharacterController`.  
> **Solution**: `PlayerBodyCollision.cs` must be extended in `EnsureFurnitureColliders()` or `IgnoreGrabbableAndTeleportCollisions()` so that all colliders on objects named `Grass*`, `Flower*`, `Mushroom*`, `Branch*` are registered with `Physics.IgnoreCollision(m_CharacterController, col, true)`.

---

### 2.3 Survey of Root Props & Village Buildings (OccaSoftware)
These objects are root siblings in the scene (not children of `Environment2`):
| GameObject Name | Instance ID | World Position | Rotation Y | Collider Type | IsTrigger | Source Prefab |
|---|---|---|---|---|---|---|
| `House_1` | -3236 | `(19.70, -0.10, 22.10)` | 90.0° | `MeshCollider` | False | `House_1.prefab` |
| `House_2` | -3930 | `(27.17, 0.50, -1.54)` | -25.0° | `MeshCollider` | False | `House_2.prefab` |
| `House_3` | -4426 | `(-3.37, 0.23, 18.72)` | 15.0° | `MeshCollider` | False | `House_3.prefab` |
| `Bench` | -2826 | `(12.69, 0.50, 12.00)` | 0.0° | `MeshCollider` | False | `Bench.prefab` |
| `Crate` (Instance 1) | -4316 | `(19.60, 0.50, 12.60)` | 0.0° | `MeshCollider` | False | `Crate.prefab` |
| `Crate` (Instance 2) | -3246 | `(20.40, 0.50, 13.10)` | 30.0° | `MeshCollider` | False | `Crate.prefab` |
| `Flower Pot` | -1740 | `(16.86, 0.50, 13.40)` | 0.0° | `MeshCollider` | False | `Flower Pot.prefab` |
| `Lantern` (Instance 1) | -1782 | `(12.20, 0.50, 10.60)` | 0.0° | `MeshCollider` | False | `Lantern.prefab` |
| `Lantern` (Instance 2) | -4262 | `(19.41, 0.50, 10.60)` | 0.0° | `MeshCollider` | False | `Lantern.prefab` |
| `Fence` (Instance 1) | -4044 | `(11.32, 0.50, 10.00)` | 0.0° | `MeshCollider` | False | `Fence.prefab` |
| `Fence` (Instance 2) | -1750 | `(8.04, 0.50, 10.00)` | 0.0° | `Fence.prefab` |
| `Fence` (Instance 3) | -4400 | `(14.58, 0.50, 10.00)` | 0.0° | `MeshCollider` | False | `Fence.prefab` |
| `Fence (1)` | -2748 | `(17.79, 0.50, 10.00)` | 0.0° | `MeshCollider` | False | `Fence.prefab` |
| `Pine Tree_1` | -3516 | `(30.73, 0.46, 14.49)` | 0.0° | `MeshCollider` | False | `Pine Tree_1.prefab` |
| `Pine Tree_2` | -3614 | `(8.00, 0.50, 25.00)` | 90.0° | `MeshCollider` | False | `Pine Tree_2.prefab` |

All 15 village building and prop instances have solid `MeshCollider` components attached.

---

## 3. Cabin & Door Analysis

### 3.1 Cabin Location & Spatial Orientation
- **Cabin Chosen**: `House_1` at position `(19.70, -0.10, 22.10)`, rotated `Y = 90.0°`.
- **Layout Evidence**:
  - The entire cluster of decorative props (`Bench`, `Crates`, `Flower Pot`, `Lanterns`, `Fence` line) is arranged between `Z = 10.0` and `Z = 13.4`, directly facing `House_1`.
  - Mesh bounding box of `House_1` in world coordinates:
    - X range: `[12.20, 27.08]` (center ~ 19.64)
    - Z range: `[15.60, 27.03]` (front wall ~ 15.60 - 16.50)
    - Y range: `[-0.40, 12.24]`
  - Entrance doorway of `House_1` is located on the South-facing wall towards the front yard (around `X ≈ 16.5 - 19.5, Z ≈ 15.5 - 16.2, Y ≈ 0.5 - 1.5`).

### 3.2 Door & Transition Status
- **Current State**: Neither door slab, prompt canvas, nor transition trigger exists in `Tutorial scene.unity`.
- **Reference from `Room scene.unity`**:
  - In `Room scene`, `Cabin_Door_Slab` (`MeshRenderer`, `BoxCollider`, `AudioSource`, `XRSimpleInteractable`, `DoorSceneTransition`) is paired with `Door_Interaction_Prompt` (World Space Canvas, `CanvasGroup`, `TextMeshProUGUI`).
  - Target scene in `Room scene`: `"Tutorial scene"`.
  - In `Tutorial scene`, the door must target `"Room scene"`, check for `tool-shovel` + `Gold_Pan`, and trigger identical screen fade and audio feedback.

---

## 4. Inventory & Grabbable Tools Analysis

### 4.1 `[Prospector_Inventory]` Status
- **Current State**: Completely absent in `Tutorial scene.unity`.
- **Reference from `Room scene.unity`**:
  - Root GameObject: `[Prospector_Inventory]` with `ProspectorInventory.cs` (Singleton), `AudioSource`, `Stored_Items_Container`.
  - Child UI: `InventoryCanvas` with `OuterFrame`, `LeatherBackground`, `HeaderBar`, `SlotsContainer` (4 slots), `QuickStoreButton`, `CloseButton`, `StatusBar`.
  - Notification UI: `ToastNotification` with auto-fade text feedback.

### 4.2 Grabbable Tools (`tool-shovel` & `Gold_Pan`) Status
- **Current State**: Neither tool exists anywhere in `Tutorial scene.unity`.
- **Required Configuration (from `Room scene.unity`)**:
  - **`tool-shovel`**:
    - Source: `Assets/Prospector-journey/Room/Models/tool-shovel.fbx`
    - Components: `BoxCollider`, `Rigidbody` (mass: 1.8), `XRGrabInteractable`, `XRRestingProp`, `XRGeneralGrabTransformer`, `attach1`, `attach2`.
  - **`Gold_Pan`**:
    - Source: `Assets/Prospector-journey/Room/Models/Gold_Pan.obj`
    - Components: `MeshCollider` (convex), `Rigidbody` (mass: 1.2), `XRGrabInteractable`, `XRRestingProp`, `XRGeneralGrabTransformer`, `attach1`, `attach2`.
- **Initialization Requirement (R2)**:
  - Both tools must be pre-stored inside `[Prospector_Inventory]` slots upon entering `Tutorial scene.unity` so the player immediately has access to them via bag UI (`I`/`Tab` or VR `B`/`Y`) and quick-store (`C`/`X` or VR `A`/`B`/`X`/`Y`).

---

## 5. Architectural Gap Analysis & Recommendations for Implementer

| System | Gap in Tutorial Scene | Required Implementation Action |
|---|---|---|
| **Player Collision** | Missing `PlayerBodyCollision` on `XR Origin (XR Rig)` | Add `PlayerBodyCollision` to `XR Origin (XR Rig)`, configure `CharacterController` (`radius=0.24`, `stepOffset=0.08`, `skinWidth=0.025`). |
| **Foliage Snag** | Solid colliders on grass/flowers/mushrooms/branches | Update `PlayerBodyCollision.EnsureFurnitureColliders()` / `IgnoreGrabbableAndTeleportCollisions()` to ignore colliders on `Grass*`, `Flower*`, `Mushroom*`, `Branch*`. |
| **Environment Obstacles** | Code in `PlayerBodyCollision` currently hardcoded to `"Environment_ProspectorCabin"` | Update `PlayerBodyCollision.EnsureFurnitureColliders()` to check `Environment2` and root village props when running in `Tutorial scene`. |
| **Inventory UI** | Missing `[Prospector_Inventory]` | Port `[Prospector_Inventory]` hierarchy and bindings from `Room scene.unity` into `Tutorial scene.unity`. |
| **Tools** | Missing `tool-shovel` and `Gold_Pan` | Instantiate configured `tool-shovel` and `Gold_Pan` instances and ensure `ProspectorInventory` initializes with them pre-stored. |
| **Cabin Door** | Missing door trigger & `Door_Interaction_Prompt` | Place `Cabin_Door_Slab` at `House_1` entrance (approx `(19.0, 1.1, 16.0)`), attach `DoorSceneTransition` targeting `"Room scene"`, and link `Door_Interaction_Prompt`. |

