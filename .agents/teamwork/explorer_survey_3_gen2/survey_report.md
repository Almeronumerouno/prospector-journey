# Technical Architectural Survey & Implementation Strategy Report

**Explorer**: Explorer 3 Gen 2 (VR Codebase & Testing Architecture Surveyor)  
**Date**: 2026-10-12  
**Target Project**: Prospector Journey (`c:\CodeKuliah\prospector-journey`)  
**Target Scenes**: `Assets/Prospector-journey/Tutorial/Tutorial scene.unity` & `Assets/Prospector-journey/Room/Room scene.unity`

---

## Executive Summary
This survey provides an exhaustive technical and code-level investigation for porting and synchronizing VR gameplay mechanics, collision handling, inventory & tool persistence, door transitions, and Play Mode verification from `Room scene.unity` into `Tutorial scene.unity`.

Key architectural findings:
1. `PlayerBodyCollision.cs` contains complete 1:1 camera-to-body locomotion sync and E/Q eye-height compensation, but its `EnsureFurnitureColliders()` and collision-ignore logic are hardcoded solely for `Environment_ProspectorCabin` and lack support for `Environment2` and root props (`House_1`, `Bench`, etc.). Furthermore, all foliage prefabs in `Environment2` (`Grass`, `Flowers`, `Mushroom`, `Branch`) currently possess active `MeshCollider` components that will cause severe locomotion snagging unless explicitly ignored.
2. `ProspectorInventory.cs` currently assumes items are picked up dynamically from the room; its `InitializeSlots()` does not inspect pre-existing children in `storedItemsContainer`. An auto-initialization hook (`InitializePreStoredItems()`) must be added to register pre-configured `tool-shovel` and `Gold_Pan` instances at startup.
3. `DoorSceneTransition.cs` provides a complete proximity (`1.85m`) and tool-checking routine (`HasRequiredToolsForExit()`) that verifies both `"shovel"` and `"Gold_Pan"` in bag or hand, triggering a VR Screen Fade (`[VR_Screen_Fader]`) before loading the target scene. For `Tutorial scene.unity`, the cabin entrance is located at `House_1` (`(19.64, 1.2, 15.6)`) targeting `"Room scene"`.
4. Testing infrastructure uses `Assembly-CSharp.dll` without modular `.asmdef` boundaries. Play Mode automated audits can be executed via a dedicated `TutorialPlayModeAuditRunner.cs` component combined with an Editor Test Runner invoking tests and asserting zero console errors.

---

## 1. Deep Dive: `PlayerBodyCollision.cs` & Locomotion Sync

### 1.1 Existing Architecture (`Assets/Prospector-journey/Room/Scripts/PlayerBodyCollision.cs`)
- **Class Attributes**: `[RequireComponent(typeof(XROrigin))]`, `[RequireComponent(typeof(CharacterController))]`, `[DefaultExecutionOrder(20000)]`.
- **Core Parameters**:
  - `bodyRadius = 0.24f`
  - `stepOffset = 0.08f`
  - `skinWidth = 0.025f`
  - `minHeadHeight = 0.85f`, `maxHeadHeight = 2.05f`
  - `m_InitialOffsetY = 1.36144f` (derived from `Camera Offset.localPosition.y`)

### 1.2 Camera Y Eye-Height Locking on E/Q & 1:1 Locomotion Sync
In `PlayerBodyCollision.cs` (lines 187–253):
```csharp
Vector3 rawCamLocal = ReadRawHmdLocalPosition();
float headHeight = m_InitialOffsetY;
float clampedOffsetY = headHeight - rawCamLocal.y;

// Counter-shift Camera Offset in XYZ so Main Camera AND both controllers stay locked:
m_CurrentOffsetLocalPos = new Vector3(-rawCamLocal.x, clampedOffsetY, -rawCamLocal.z);
m_CameraOffsetTransform.localPosition = m_CurrentOffsetLocalPos;

m_CharacterController.height = headHeight;
m_CharacterController.center = new Vector3(0f, headHeight * 0.5f + skinWidth, 0f);

// Convert HMD horizontal delta to CharacterController.Move on XR Origin
Vector3 worldMove = Vector3.zero;
Vector3 localXZDelta = new Vector3(localDelta.x, 0f, localDelta.z);
if (localXZDelta.sqrMagnitude > 0.00000001f)
{
    worldMove = m_CameraOffsetTransform.TransformVector(localXZDelta);
    worldMove.y = 0f;
}
...
m_CharacterController.Move(worldMove);
```
**Mechanism Verification**:
1. When pressing `E` or `Q` in the XR Interaction Simulator (or adjusting physical VR height), `rawCamLocal.y` changes.
2. Because `m_CameraOffsetTransform.localPosition.y` is set to `m_InitialOffsetY - rawCamLocal.y`, the resulting world camera height is:
   $$\text{CameraWorldY} = \text{XROrigin.y} + (\text{headHeight} - \text{rawCamLocal.y}) + \text{rawCamLocal.y} = \text{XROrigin.y} + \text{headHeight}$$
   This holds world camera Y strictly invariant, preventing any drift or height shift.
3. Horizontal translation `localXZDelta` is stripped of Y and applied via `m_CharacterController.Move()`, which advances the root `XR Origin` transform through Unity PhysX. All child transforms (`Camera Offset`, `Main Camera`, `Left Controller`, `Right Controller`) move together at identical 1:1 velocity.
4. Pre-render synchronization is guaranteed via `Application.onBeforeRender` and `RenderPipelineManager.beginContextRendering` (lines 78–80, 239–252), eliminating 1-frame jitter.

### 1.3 Collision Setup and the `Tutorial scene.unity` Obstacle Challenge
Currently, `EnsureFurnitureColliders()` (lines 123–146) only searches for `Environment_ProspectorCabin`:
```csharp
var env = GameObject.Find("Environment_ProspectorCabin");
if (env == null) return;
```
If run in `Tutorial scene.unity`, this returns immediately because the environment root is named `Environment2`.

#### Live Environment Survey Findings (`Tutorial scene.unity`)
Live query of `Tutorial scene.unity` reveals:
- **`Environment2` Children (133 objects total)**:
  - **Ground**: `Ground_01` (3), `Ground_02` (10), `Ground_03` (3) — All have active `MeshCollider` (walkable surface).
  - **Solid Obstacles**: `Tree_01` (3), `Tree_02` (12), `Tree_03` (7), `Tree_04` (9), `Tree_05` (9), `Stump_01` (3), `Rock_01` (3), `Rock_02` (2), `Rock_03` (6), `Rock_04` (3), `Rock_05` (12), `Bush_02` (1), `Bush_03` (6).
  - **Foliage / Decals**: `Grass_01` (15), `Grass_02` (4), `Flowers_01` (4), `Flowers_02` (2), `Mushroom_01` (9), `Mushroom_02` (4), `Branch_01` (3).
  - **CRITICAL DISCOVERY**: **Every single foliage object in `Environment2` currently has an active `MeshCollider` attached!** Without ignoring collisions, the player's `CharacterController` (`bodyRadius = 0.24`, `stepOffset = 0.08`) will continuously snag and collide on flat flowers, mushrooms, and grass blades.
- **Root Props (Outside `Environment2`)**:
  - `House_1` at `(19.70, -0.10, 22.10)` (MeshCollider active)
  - `House_2` at `(27.17, 0.50, -1.54)` (MeshCollider active)
  - `House_3` at `(-3.37, 0.23, 18.72)` (MeshCollider active)
  - `Bench` at `(12.69, 0.50, 12.00)` (MeshCollider active)
  - `Crate` (2 instances) at `(19.60, 0.50, 12.60)` and `(20.40, 0.50, 13.10)` (MeshCollider active)
  - `Fence` (4 instances) along `Z = 10.0` (MeshCollider active)
  - `Pine Tree_1`, `Pine Tree_2` (MeshCollider active)
  - `Lantern` (2 instances) (MeshCollider active)
  - `Flower Pot` (MeshCollider active)

### 1.4 Required Code Extension for `PlayerBodyCollision.cs`
To support both scenes cleanly without breaking Room scene:
```csharp
private void EnsureFurnitureColliders()
{
    EnsureEnvironmentColliders("Environment_ProspectorCabin");
    EnsureEnvironmentColliders("Environment2");
    EnsureRootObstacleColliders();
}

private void EnsureEnvironmentColliders(string rootName)
{
    var env = GameObject.Find(rootName);
    if (env == null) return;

    var meshFilters = env.GetComponentsInChildren<MeshFilter>(true);
    foreach (var mf in meshFilters)
    {
        if (mf == null || mf.sharedMesh == null) continue;
        var go = mf.gameObject;

        if (go.GetComponentInParent<XRGrabInteractable>() != null) continue;
        if (go.GetComponentInParent<TeleportationAnchor>() != null) continue;
        if (go.name == "workbench" || go.name == "rugRound" || go.name.Contains("Visuals") || go.name.Contains("Anchor"))
            continue;

        if (go.GetComponent<Collider>() == null)
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh;
        }
    }
}

private void EnsureRootObstacleColliders()
{
    string[] rootObstaclePrefixes = { "House_", "Bench", "Crate", "Fence", "Pine Tree_", "Lantern", "Flower Pot" };
    var scene = gameObject.scene;
    foreach (var root in scene.GetRootGameObjects())
    {
        bool isObstacle = false;
        foreach (var prefix in rootObstaclePrefixes)
        {
            if (root.name.StartsWith(prefix)) { isObstacle = true; break; }
        }
        if (!isObstacle) continue;

        var meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        foreach (var mf in meshFilters)
        {
            if (mf == null || mf.sharedMesh == null) continue;
            var go = mf.gameObject;
            if (go.GetComponent<Collider>() == null)
            {
                var mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }
    }
}
```

In `IgnoreGrabbableAndTeleportCollisions()`:
```csharp
// Ignore flat foliage/decals so player never snags on grass/flowers/mushrooms/branches
string[] foliageKeywords = { "Grass", "Flowers", "Mushroom", "Branch" };
var allColliders = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
foreach (var col in allColliders)
{
    if (col == null || col == m_CharacterController) continue;
    string cName = col.gameObject.name;
    for (int i = 0; i < foliageKeywords.Length; i++)
    {
        if (cName.IndexOf(foliageKeywords[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
        {
            Physics.IgnoreCollision(m_CharacterController, col, true);
            break;
        }
    }
}
```

---

## 2. Deep Dive: Inventory & Tool System

### 2.1 Overview of Components (`Assets/Prospector-journey/Room/Scripts/ProspectorInventory.cs`)
- **Singleton**: `ProspectorInventory.Instance`
- **Slots**: `maxSlots = 4`, backed by `InventorySlotData[]` storing:
  - `isOccupied` (bool)
  - `itemName` (string, e.g., `"tool-shovel"`)
  - `itemDisplayName` (string formatted via `FormatDisplayName()`, e.g., `"Sekop (Shovel)"`)
  - `itemGameObject` (GameObject)
  - `grabInteractable` (`XRGrabInteractable`)
- **Container**: `storedItemsContainer` (Transform, inactive during play). In `Room scene.unity`, this is named `[Stored_Items_Container]`.
- **VR Lazy-Follow**: Smooth floating canvas following player gaze with `followDistance = 0.85m`, `heightOffset = -0.12m`, `deadzoneAngle = 18f`, `maxPitchAngle = 12f`.
- **Toast Notifications**: Managed by `ToastNotification` panel + `TextMeshProUGUI` positioned `0.75m` in front of player with `1.8s` display lifetime.

### 2.2 Pre-Stored Items Initialization Gap & Solution
- **Current Behavior**: `ProspectorInventory.InitializeSlots()` initializes an empty slot array. It does not inspect existing child GameObjects of `storedItemsContainer`.
- **Requirement R2**: When entering `Tutorial scene.unity`, `tool-shovel` and `Gold_Pan` must be pre-stored in the player's satchel bag.
- **Solution**: Add `InitializePreStoredItems()` to `ProspectorInventory.cs` called during `Start()`:
  ```csharp
  private void InitializePreStoredItems()
  {
      if (storedItemsContainer == null) return;

      for (int i = 0; i < storedItemsContainer.childCount; i++)
      {
          var child = storedItemsContainer.GetChild(i);
          var grab = child.GetComponent<XRGrabInteractable>();
          if (grab != null)
          {
              int slotIndex = GetFirstEmptySlot();
              if (slotIndex >= 0 && slotIndex < maxSlots)
              {
                  child.gameObject.SetActive(false);
                  var rb = child.GetComponent<Rigidbody>();
                  if (rb != null)
                  {
                      rb.linearVelocity = Vector3.zero;
                      rb.angularVelocity = Vector3.zero;
                      rb.isKinematic = true;
                  }

                  m_Slots[slotIndex].isOccupied = true;
                  m_Slots[slotIndex].itemName = child.gameObject.name;
                  m_Slots[slotIndex].itemDisplayName = FormatDisplayName(child.gameObject.name);
                  m_Slots[slotIndex].itemGameObject = child.gameObject;
                  m_Slots[slotIndex].grabInteractable = grab;
              }
          }
      }
  }
  ```
  This ensures that when `tool-shovel` and `Gold_Pan` are nested under `[Stored_Items_Container]` in `Tutorial scene.unity`, they are instantly parsed into Slot 1 and Slot 2.

### 2.3 Quick-Storing & Retrieving Mechanics
- **Quick-Storing (`QuickStoreHeldItem`)**:
  - **Inputs**:
    - Keyboard: `C` or `X` (via `Keyboard.current.cKey.wasPressedThisFrame`).
    - VR Controller: Primary button (`A` on right, `X` on left) when NOT in proximity of door trigger; or Secondary button (`B`/`Y`) if holding an item.
  - **Execution**:
    1. Resolves held `XRGrabInteractable` via `GetHeldInteractable()`.
    2. Drops item cleanly from interactor: `interactionManager.SelectExit(interactor, grab)`.
    3. Reparents object to `storedItemsContainer`, zeroes velocities, sets `rb.isKinematic = true`, and sets `SetActive(false)`.
    4. Populates slot data and plays `storeClip`.
    5. Displays Toast notification: `ShowToast($"[+] {cleanName} masuk ke Tas")`.
    6. If no tool is held: plays `errorClip` and shows Toast: `"Tidak ada alat yang sedang dipegang!"`.
- **Retrieving (`RetrieveItem`)**:
  - **Inputs**:
    - Keyboard: `I` or `Tab` toggles bag UI (`ToggleInventory()`).
    - VR Controller: Secondary button (`B`/`Y`) toggles bag UI when hands are free.
    - Slot Click: Clicking slot button (via XR Poke Interactor, Near-Far laser, or mouse) triggers `OnSlotClicked(index)`.
  - **Execution**:
    1. Reparents item to root (`null`), calls `SetActive(true)`.
    2. Spawns item at hand position (`controller.position + forward * 0.10m`) or in front of panel.
    3. Re-enables Rigidbody (`isKinematic = false`, `useGravity = true`).
    4. Automatically enters select state on free hand: `interactionManager.SelectEnter(targetHand, grab)`.
    5. Resets slot data, plays `retrieveClip`, and refreshes slot visual state.

### 2.4 Tool Prefab Configuration (`tool-shovel` & `Gold_Pan`)
From inspection of `Room scene.unity`:
1. **`tool-shovel`** (Source: `Assets/Prospector-journey/Room/Models/tool-shovel.fbx`):
   - `Rigidbody`: `mass = 1.8`, `linearDamping = 0`, `angularDamping = 0.05`, `useGravity = true`, `isKinematic = false`, `interpolation = Interpolate`, `collisionDetection = Continuous`, `constraints = FreezeAll` (unfrozen by `XRRestingProp` on grab).
   - `BoxCollider`: `center = (0, 0.15, 0)`, `size = (0.08, 0.3, 0.04)`.
   - `XRGrabInteractable`: `selectMode = Multiple`, `movementType = VelocityTracking`, `throwOnDetach = true`, `forceGravityOnDetach = true`, `farAttachMode = Near`, `attachTransform = attach1`, `secondaryAttachTransform = attach2`.
   - `XRRestingProp`: `m_FreezeUntilGrabbed = true`, `m_DirectGrabOnly = true`, `m_SecondaryBreakDistance = 0.85f`.
   - `XRGeneralGrabTransformer`: `twoHandedRotationMode = FirstHandDirectedTowardsSecondHand`, `allowTwoHandedScaling = false`.
   - Child transforms: `attach1` (rear grip) and `attach2` (front shaft).
2. **`Gold_Pan`** (Source: `Assets/Prospector-journey/Room/Models/Gold_Pan.fbx`):
   - `Rigidbody`: `mass = 1.0`, `useGravity = true`, `isKinematic = false`, `interpolation = Interpolate`, `constraints = FreezeAll`.
   - `MeshCollider`: `convex = true`, sharing pan mesh.
   - `XRGrabInteractable`: `selectMode = Multiple`, `movementType = VelocityTracking`, `attachTransform = attach1`, `secondaryAttachTransform = attach2`.
   - `XRRestingProp`: `m_FreezeUntilGrabbed = true`, `m_DirectGrabOnly = true`.
   - `XRGeneralGrabTransformer`: Configured for two-handed rim holding.
   - Child transforms: `attach1` (right rim) and `attach2` (left rim).

---

## 3. Deep Dive: Door Transition System

### 3.1 Architecture (`Assets/Prospector-journey/Room/Scripts/DoorSceneTransition.cs`)
- **Proximity Detection**:
  - Distance check: `Vector3.Distance(doorCenter.position, Camera.main.transform.position) <= interactionDistance` (`1.85m`).
  - Proximity change triggers smooth canvas fade on `Door_Interaction_Prompt` via `promptCanvasGroup` (0.15s).
- **Tool Requirement Check (`HasRequiredToolsForExit`)**:
  ```csharp
  public bool HasRequiredToolsForExit(out bool hasShovel, out bool hasPan)
  {
      hasShovel = false;
      hasPan = false;

      if (ProspectorInventory.Instance != null)
      {
          hasShovel = ProspectorInventory.Instance.HasItemInBagOrHand("shovel");
          hasPan = ProspectorInventory.Instance.HasItemInBagOrHand("Gold_Pan");
      }

      if (!hasShovel || !hasPan)
      {
          var grabs = FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
          foreach (var grab in grabs)
          {
              if (grab != null && grab.isSelected)
              {
                  string n = grab.gameObject.name;
                  if (n.IndexOf("shovel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                      hasShovel = true;
                  if (n.IndexOf("Gold_Pan", System.StringComparison.OrdinalIgnoreCase) >= 0)
                      hasPan = true;
              }
          }
      }

      return hasShovel && hasPan;
  }
  ```
- **Interaction Logic**:
  - Inputs: Keyboard `E`, VR Right Controller `PrimaryButton` (`A`) or `Trigger`, VR Left Controller `PrimaryButton` (`X`) or `Trigger`, or XRI `XRSimpleInteractable.activated`.
  - Missing Tools Behavior:
    - Sets prompt text to red warning: `Belum Bisa Keluar! Wajib Bawa: [X] Sekop  [X] Gold Pan`.
    - Triggers `ProspectorInventory.Instance.PlayErrorSound()` and `ShowToast("Belum bisa keluar! Bawa Sekop & Gold Pan dulu.")`.
    - Transition is strictly blocked.
  - Allowed Tools Behavior:
    - Sets prompt text to green confirmation: `[E] Masuk Rumah / Keluar Rumah ... [OK] Sekop & Gold Pan Siap`.
    - Plays `doorCreakClip` / `doorOpenClip`.
    - Hides prompt UI.
    - Spawns persistent world-space camera canvas `[VR_Screen_Fader]` and fades screen to solid black over `fadeDuration = 0.75s`.
    - Invokes `SceneManager.LoadSceneAsync(targetSceneName)`.

### 3.2 Cabin Door Setup in `Tutorial scene.unity`
- **Target Cabin**: `House_1` at `(19.70, -0.10, 22.10)`.
  - Front door entrance coordinate: approximately `(19.64, 1.20, 15.60)`.
  - Target Scene parameter: Set `targetSceneName = "Room scene"`.
- **Build Settings Validation**:
  - `Assets/Prospector-journey/Tutorial/Tutorial scene.unity` (Build Index 2, enabled).
  - `Assets/Prospector-journey/Room/Room scene.unity` (Build Index 5, enabled).
  - Both scenes are registered in Build Settings; asynchronous loading between them is fully supported.

---

## 4. Unity Testing Architecture & Verification Strategy

### 4.1 Project Testing Structure Analysis
- The project does not currently use isolated `.asmdef` files for game logic; all Prospector scripts reside in `Assembly-CSharp.dll`.
- In `Room scene.unity`, an in-scene helper GameObject `_TestHelper` with `RoomInteractionTestHelper.cs` is used for runtime simulation and interactive testing.

### 4.2 Play Mode Audit Architecture for `Tutorial scene.unity`
To fulfill Requirement R4 programmatically with 100% pass verification and zero console errors, we propose a two-tier testing suite:

#### Tier 1: In-Scene Play Mode Component (`TutorialPlayModeAuditRunner.cs`)
- Attached to a test GameObject `_TutorialAuditRunner` in `Tutorial scene.unity`.
- Executes an automated test suite across sequential frames/seconds:
  1. **Phase 1: Environment & Rig Collision Audit (R1)**:
     - Asserts `XR Origin (XR Rig)` has `PlayerBodyCollision` and `CharacterController`.
     - Asserts `CharacterController.radius == 0.24f`, `stepOffset == 0.08f`, `skinWidth == 0.025f`.
     - Asserts all root obstacle props (`House_1`, `House_2`, `House_3`, `Bench`, `Crate`, `Fence`, `Lantern`, `Flower Pot`) have active colliders.
     - Asserts all foliage decals (`Grass_01`, `Flowers_01`, `Mushroom_01`, `Branch_01`) have collisions ignored with `CharacterController` (`Physics.GetIgnoreCollision == true`).
     - Simulates elevation change (`rawCamLocal.y` shift) and asserts world camera Y remains strictly locked.
  2. **Phase 2: Inventory Initialization Audit (R2)**:
     - Asserts `ProspectorInventory.Instance != null`.
     - Asserts Slot 0 is occupied by `tool-shovel`.
     - Asserts Slot 1 is occupied by `Gold_Pan`.
  3. **Phase 3: Tool Retrieval & Quick-Store Audit (R2)**:
     - Simulates opening inventory (`ToggleInventory()`).
     - Simulates retrieving tool from Slot 0 (`RetrieveItem(0)`).
     - Asserts tool GameObject is active, non-kinematic, and grabbed by interactor.
     - Simulates quick-storing tool (`QuickStoreHeldItem()`).
     - Asserts tool is deactivated under `storedItemsContainer`.
     - Asserts `ToastNotification` becomes active with store message.
  4. **Phase 4: Cabin Door Transition Audit (R3)**:
     - Asserts `DoorSceneTransition` exists near `House_1` targeting `"Room scene"`.
     - Asserts `HasRequiredToolsForExit()` evaluates to `true` when tools are present.
     - Temporarily mocks missing tool and asserts `HasRequiredToolsForExit()` evaluates to `false` and blocks interaction.
  5. **Phase 5: Audit Finalization**:
     - Logs comprehensive summary:
       `[AUDIT COMPLETE] R1: PASS | R2: PASS | R3: PASS | R4: PASS`.
     - Exposes static `TutorialPlayModeAuditRunner.AllTestsPassed = true`.

#### Tier 2: Headless Editor Test Runner (`TutorialSceneAuditRunner.cs`)
- Can be executed directly via `Unity_RunCommand` or Menu Item `Tools/Prospector/Run Tutorial Scene Audit`:
  - Validates scene setup in Edit Mode.
  - Automatically triggers Play Mode (`EditorApplication.EnterPlaymode()`).
  - Monitors `TutorialPlayModeAuditRunner`.
  - Exits Play Mode (`EditorApplication.ExitPlaymode()`).
  - Queries `Unity_GetConsoleLogs` to verify 0 compile errors and 0 runtime exceptions.

---

## 5. Comprehensive Summary Matrix & Action Plan

| Component | Current State in Tutorial Scene | Required Target State | Implementation Task |
|---|---|---|---|
| **CharacterController** | Present on `XR Origin (XR Rig)`, but with `radius=0.1`, `stepOffset=0.5`, `skinWidth=0.08` | `radius=0.24`, `stepOffset=0.08`, `skinWidth=0.025` | Reconfigure in scene & via `PlayerBodyCollision.ConfigureCharacterController()` |
| **PlayerBodyCollision** | **Missing** from `XR Origin (XR Rig)` | Attached & configured with execution order 20000 | Attach to `XR Origin (XR Rig)` in `Tutorial scene.unity` |
| **Furniture/Obstacle Detection** | Only searches `Environment_ProspectorCabin` | Searches `Environment_ProspectorCabin`, `Environment2`, and root obstacle props | Update `PlayerBodyCollision.EnsureFurnitureColliders()` |
| **Foliage Decal Collisions** | Active `MeshCollider` on all grass, flowers, mushrooms, branches | Collisions ignored with `CharacterController` via `Physics.IgnoreCollision` | Extend `PlayerBodyCollision.IgnoreGrabbableAndTeleportCollisions()` |
| **[Prospector_Inventory]** | **Missing** from `Tutorial scene.unity` | Present with Canvas, ToastNotification, AudioSource, Container | Duplicate/instantiate prefab hierarchy from `Room scene.unity` |
| **Pre-Stored Tools** | **Missing** | `tool-shovel` & `Gold_Pan` pre-stored in bag | Add `InitializePreStoredItems()` to `ProspectorInventory.cs` and place configured instances under `Stored_Items_Container` |
| **Cabin Door Transition** | **Missing** | Door slab trigger + `Door_Interaction_Prompt` + `DoorSceneTransition` at `House_1` targeting `"Room scene"` | Add door slab & prompt at `House_1` entrance |
| **Play Mode Audit Runner** | **Missing** | Dedicated automated audit component verifying R1–R4 | Implement `TutorialPlayModeAuditRunner.cs` and run test pass |

---
**Report Status**: Completed and verified via live Unity Editor queries and codebase inspection.
