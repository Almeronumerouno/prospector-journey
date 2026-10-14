# Project: VR Gameplay Logic & Tutorial Scene Porting

## Architecture
- Target Project: Prospector Journey (Unity 6 / 2022.3+ VR with XR Interaction Toolkit 3.5.1, New Input System, URP)
- Reference Scene: `Assets/Prospector-journey/Room/Room scene.unity`
- Target Scene: `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`
- Core Modules:
  1. `ProspectorJourney.Room.PlayerBodyCollision`: HMD/CharacterController synchronization, camera offset counter-shifting, camera Y eye-height clamp, environment collider assurance, and foliage/tool collision ignores.
  2. `ProspectorJourney.Room.ProspectorInventory`: VR floating bag inventory, lazy-follow camera tracking, pre-stored item initialization, slot management, quick-store (`C`/`X`, VR buttons), retrieve auto-grab, and `ToastNotification` display.
  3. `ProspectorJourney.Room.DoorSceneTransition`: Distance proximity trigger, required tools check (`tool-shovel` + `Gold_Pan`), interactive billboard prompt UI (`Door_Interaction_Prompt`), VR screen fader, and asynchronous scene loading.
  4. Physical Grabbable Tools: `tool-shovel` and `Gold_Pan` instances with `Rigidbody`, colliders, `XRGrabInteractable`, `XRRestingProp`, `XRGeneralGrabTransformer`, and dual attach transforms.
  5. Play Mode Testing Suite: Automated in-engine Play Mode test harness verifying all requirements (R1–R4) with zero errors.

## Code Layout
- `Assets/Prospector-journey/Room/Scripts/PlayerBodyCollision.cs` — Player locomotion & collision logic
- `Assets/Prospector-journey/Room/Scripts/ProspectorInventory.cs` — Inventory, toast, and item slot logic
- `Assets/Prospector-journey/Room/Scripts/DoorSceneTransition.cs` — Door transition & tool check logic
- `Assets/Prospector-journey/Tutorial/Tutorial scene.unity` — Target tutorial scene
- `Assets/Prospector-journey/Tutorial/Scripts/TutorialPlayModeAuditRunner.cs` — Play Mode test and audit harness
- `Assets/Prospector-journey/Tutorial/Editor/TutorialPlayModeRunner.cs` — Editor test execution script

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | PlayerBodyCollision & CC Configuration | Attach and configure PlayerBodyCollision + CharacterController (radius=0.24, stepOffset=0.08, skinWidth=0.025) on XR Origin in Tutorial scene | M2 | R1 |
| 2 | Environment Obstacle Colliders | Ensure solid colliders on Environment2 trees, rocks, stumps, and root props (House_1..3, Bench, Crate, Fence, Pine Tree_1..2, Lantern, Flower Pot) | M1, M2 | R1 |
| 3 | Foliage & Tool Collision Ignore | Prevent CharacterController from snagging on Environment2 foliage (Grass, Flowers, Mushroom, Branch) and grabbable tools | M1, M2 | R1 |
| 4 | Locomotion & Camera Y-Height Lock | 1:1 speed synchronization and zero camera Y-height drift when rotating/pressing E/Q | M1 | R1 |
| 5 | [Prospector_Inventory] & Toast Setup | Set up [Prospector_Inventory], InventoryCanvas, ToastNotification, Stored_Items_Container, and audio clips in Tutorial scene | M2 | R2 |
| 6 | Pre-equipped Tools Initialization | Pre-equip tool-shovel and Gold_Pan in Stored_Items_Container with bag slot registration upon entering Tutorial scene | M1, M2 | R2 |
| 7 | Tool Grab, Retrieve & Quick-Store | Retrieve tool to hand from bag UI; quick-store via C/X or VR buttons with Toast feedback; XRRestingProp handling | M1, M2 | R2 |
| 8 | Cabin Door & Interaction Prompt | Add Cabin_Door_Slab + Door_Interaction_Prompt facing player at House_1 entrance in Tutorial scene targeting "Room scene" | M2 | R3 |
| 9 | Door Required Tools Check & Transition | Enforce tool check (tool-shovel + Gold_Pan), block exit if missing, allow exit with audio + fade if carrying both | M1, M2 | R3 |
| 10| Dedicated Play Mode Test Harness | Programmatic Play Mode verification suite executing checks 1-5 with zero errors | M3 | R4 |
| 11| Forensic Audit & Parity Verification | Comprehensive independent verification of code and scene parity between Room and Tutorial | M4 | R4 |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | C# VR Script Extensions | Extend `PlayerBodyCollision.cs` (obstacle colliders + foliage ignores) and `ProspectorInventory.cs` (pre-stored items initialization) | none | DONE |
| M2 | Tutorial Scene Hierarchy & Prefab Synchronization | Port `PlayerBodyCollision`, `[Prospector_Inventory]`, pre-stored `tool-shovel` & `Gold_Pan`, and `Cabin_Door_Slab` with `DoorSceneTransition` into `Tutorial scene.unity` | M1 | DONE |
| M3 | Dedicated Play Mode Test Harness & Execution | Implement `TutorialPlayModeAuditRunner.cs`, run in Play Mode, verify 100% test assertions pass with zero console errors | M2 | DONE |
| M4 | Final Victory Audit & Verification | Run independent review, challenger tests, and forensic audit to confirm clean integrity | M3 | DONE |

## Interface Contracts
### PlayerBodyCollision ↔ Environment Colliders
- `EnsureFurnitureColliders()`: In `Tutorial scene`, searches `Environment2` children and root props (`House_1..3`, `Bench`, `Crate`, `Fence`, `Pine Tree_1..2`, `Lantern`, `Flower Pot`), adding `MeshCollider` if missing.
- `IgnoreGrabbableAndTeleportCollisions()`: Discovers all colliders on objects starting with `Grass`, `Flower`, `Mushroom`, `Branch`, and calls `Physics.IgnoreCollision(m_CharacterController, col, true)`.

### ProspectorInventory ↔ Stored Items
- `InitializePreStoredItems()`: Scans `storedItemsContainer` children at start; for each child (e.g. `tool-shovel`, `Gold_Pan`), adds to `m_Slots`, sets `isOccupied = true`, configures display name and `grabInteractable`, and calls `UpdateUI()`.
- `HasItemInBagOrHand(string name)`: Correctly returns true if shovel or gold pan is either in bag or currently held by left/right interactor.

### DoorSceneTransition ↔ ProspectorInventory
- `HasRequiredToolsForExit(out bool hasShovel, out bool hasPan)`: Queries `ProspectorInventory.Instance` and hand interactors for shovel and gold pan.
- `TryInteract()`: If both ready, plays sfx, initiates screen fade, and calls `SceneManager.LoadSceneAsync(targetSceneName)`.
