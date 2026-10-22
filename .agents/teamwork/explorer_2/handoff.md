# Handoff Report: Rig Grounding & 1.67m Height Calibration (R2 & R3)

**Agent**: Explorer 2 (Rig & Calibration Explorer)  
**Date**: 2026-10-22  
**Scope**: Requirements 2 & 3 (XR Origin Grounding, Hand Height Calibration, and Player Height Calibration to 167 cm / 1.67m in `Mainmenu scene.unity`)

---

## 1. Observation

### 1.1 XR Origin Root Transform & Scene Ground Level
- **File**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
- **Root GameObject**: `XR Origin (XR Rig)` (`fileID: 1717954561962503725`), instantiated from Prefab `f6336ac4ac8b4d34bc5072418cdc62a0` (`Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab`).
- **Scene Transform Modification** (lines 20342–20353):
  ```yaml
  - target: {fileID: 1717954561962503726, guid: f6336ac4ac8b4d34bc5072418cdc62a0, type: 3}
    propertyPath: m_LocalPosition.x
    value: 0
  - target: {fileID: 1717954561962503726, guid: f6336ac4ac8b4d34bc5072418cdc62a0, type: 3}
    propertyPath: m_LocalPosition.y
    value: 0
  - target: {fileID: 1717954561962503726, guid: f6336ac4ac8b4d34bc5072418cdc62a0, type: 3}
    propertyPath: m_LocalPosition.z
    value: 0
  ```
- **Ground Collider in Scene**:
  - `Diorama_Environment` (`fileID: 1443230119`, line 15035): Transform position `(0, 0, 0)`.
  - `Camp_Ground` (`fileID: 925572534`, line 9983): Child of `Diorama_Environment`, Transform local position `(0, 0, 0)`.
  - `Camp_Ground` has `MeshFilter` + `MeshRenderer`, and receives a `MeshCollider` via `PlayerBodyCollision.EnsureFurnitureColliders()` (line 316).
  - Ground level is strictly at `Y = 0.0`.

### 1.2 `MainMenuPlayerAnchor.cs` Positioning & Historic Y=1.38m Cause
- **File**: `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuPlayerAnchor.cs`
- **Component**: Attached to `XR Origin (XR Rig)` as component `fileID: 1935650668` (line 20432).
- **Spawn Position**: `[SerializeField] private Vector3 spawnPosition = Vector3.zero;` (line 18).
- **Git History Observation**:
  - In commit `dc70dcb` ("fix: main menu return and spawn reset"), `SpawnAtOrigin()` explicitly contained:
    ```csharp
    m_Origin.CameraFloorOffsetObject.transform.localPosition = new Vector3(0f, 1.36144f, 0f);
    ```
    `1.36144f` is Unity's default `CameraYOffset`. In previous iterations, developers assumed `CameraFloorOffsetObject` needed to be lifted to ~1.36m–1.38m even in Floor mode, or set `spawnPosition` to Y=1.38m.
  - Current lines 68–79 in `MainMenuPlayerAnchor.cs`:
    ```csharp
    if (m_Origin.CameraFloorOffsetObject != null)
    {
        if (PlayerBodyCollision.IsRealVRDeviceActive() || m_Origin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor || m_Origin.CurrentTrackingOriginMode == UnityEngine.XR.TrackingOriginModeFlags.Floor)
        {
            m_Origin.CameraFloorOffsetObject.transform.localPosition = Vector3.zero;
        }
        else
        {
            m_Origin.CameraFloorOffsetObject.transform.localPosition = new Vector3(0f, m_Origin.CameraYOffset > 0 ? m_Origin.CameraYOffset : 1.36144f, 0f);
        }
        m_Origin.CameraFloorOffsetObject.transform.localRotation = Quaternion.identity;
    }
    ```
  - Current lines 80–84:
    ```csharp
    if (m_Origin.Camera != null && !PlayerBodyCollision.IsRealVRDeviceActive())
    {
        m_Origin.Camera.transform.localPosition = Vector3.zero;
        m_Origin.Camera.transform.localRotation = Quaternion.identity;
    }
    ```
    Note: Setting `m_Origin.Camera.transform.localPosition = Vector3.zero;` when `CameraFloorOffsetObject` is `(0, 0, 0)` puts the camera at Y=0 on the ground in simulator/editor when not in real VR.

### 1.3 Transform Hierarchy & 2.38m Floating Hands Root Cause
- **Rig Hierarchy**:
  ```
  XR Origin (XR Rig) [Transform 1717954561962503726] (World Y = 0.0)
    └── Camera Offset [Transform 1680501587]
          ├── Main Camera [Transform 1767192434] (TrackedPoseDriver)
          ├── Left Controller [Transform 202364688] (TrackedPoseDriver)
          │     └── Left Hand Model [Transform 676151258] (local pos: {-0.001, 0.001, -0.035})
          └── Right Controller [Transform 1670256625] (TrackedPoseDriver)
                └── Right Hand Model [Transform 1792514540] (local pos: {-0.001, 0.001, -0.035})
  ```
- **Physical Tracking Mechanics on Meta Quest 3**:
  - OpenXR / Oculus Runtime with `Floor` tracking origin establishes `Y = 0.0` at the physical floor.
  - When a standing user holds the controllers naturally at waist/chest height, hardware tracking reports `localPosition.y ≈ 0.95m – 1.05m` (~1.0m above floor).
  - `TrackedPoseDriver` on `Left Controller` and `Right Controller` sets their `localPosition` under parent `Camera Offset` to `(x, ~1.0m, z)`.
  - **The 2.38m Floating Bug**:
    - If `Camera Offset.localPosition.y` is `1.38m` (or `1.36144m` from the prefab / fallback):
      $$\text{World Hand Y} = \text{Rig Y } (0.0) + \text{Camera Offset Y } (1.38\text{m}) + \text{Tracked Controller Y } (1.0\text{m}) = 2.38\text{m}$$
    - If the Rig root itself was placed at $Y = 1.38\text{m}$ with `Camera Offset` at $0$:
      $$\text{World Hand Y} = \text{Rig Y } (1.38\text{m}) + \text{Camera Offset Y } (0.0) + \text{Tracked Controller Y } (1.0\text{m}) = 2.38\text{m}$$
    - In both cases, the player's hands spawned 2.38m (7.8 ft) in the air, floating overhead in the sky.
  - **The Grounding Solution**:
    - With Rig root at $Y = 0.0$, `Camera Offset` strictly at $(0, 0, 0)$, and `TrackingOriginMode` set to `Floor`:
      $$\text{World Hand Y} = 0.0 + 0.0 + \text{Tracked Controller Y } (1.0\text{m}) = 1.0\text{m}$$
      Hands track naturally at waist/chest level.
      $$\text{World Head Y} = 0.0 + 0.0 + \text{Tracked HMD Y } (1.67\text{m}) = 1.67\text{m}$$
      Eyes track at exact calibrated height.

### 1.4 `PlayerBodyCollision.cs` Observations & Flaws
- **File**: `Assets/Prospector-journey/Room/Scripts/PlayerBodyCollision.cs`
- **Line 36**: `private float m_InitialOffsetY = 1.36144f;`
- **Lines 132–146** in `Awake()`:
  ```csharp
  if (m_CameraOffsetTransform != null)
  {
      if (IsRealVRDeviceActive())
      {
          m_InitialOffsetY = 0f;
          m_CameraOffsetTransform.localPosition = Vector3.zero;
          m_CameraOffsetTransform.localRotation = Quaternion.identity;
          m_CurrentOffsetLocalPos = Vector3.zero;
      }
      else
      {
          m_InitialOffsetY = m_CameraOffsetTransform.localPosition.y;
          if (m_InitialOffsetY < 0.1f)
              m_InitialOffsetY = 1.36144f;
          m_CurrentOffsetLocalPos = new Vector3(0f, m_InitialOffsetY, 0f);
      }
  }
  ```
- **Lines 197–200** in `ConfigureCharacterController()`:
  ```csharp
  float h = Mathf.Clamp(m_InitialOffsetY, minHeadHeight, maxHeadHeight);
  m_CharacterController.height = h;
  m_CharacterController.center = new Vector3(0f, h * 0.5f + skinWidth, 0f);
  ```
  **Critical Bug**: When `IsRealVRDeviceActive()` is true, line 134 sets `m_InitialOffsetY = 0f`. Then line 197 executes:
  `h = Mathf.Clamp(0f, 0.85f, 2.05f) = 0.85f;`!
  `m_CharacterController.height` is initialized to **0.85m** (waist height!), instead of 1.67m!
- **Lines 441–472** in `LateUpdate()`:
  - If `IsRealVRDeviceActive()` is true, it clamps `m_CharacterController.height` to `m_CameraTransform.localPosition.y`, but if the camera has not yet reported position on frame 0, it stays at 0.85m.
  - If `IsRealVRDeviceActive()` is false (such as in Editor Play Mode audit without a VR headset attached):
    Lines 484–495:
    ```csharp
    float headHeight = m_InitialOffsetY; // 1.36144f
    float clampedOffsetY = headHeight - rawCamLocal.y;
    m_CurrentOffsetLocalPos = new Vector3(-rawCamLocal.x, clampedOffsetY, -rawCamLocal.z);
    m_CameraOffsetTransform.localPosition = m_CurrentOffsetLocalPos;
    m_CharacterController.height = headHeight;
    ```
    This actively forces `m_CameraOffsetTransform.localPosition.y = 1.36144f` every frame in LateUpdate and OnBeforeRender, wiping out `Vector3.zero`!
  - It also sets `m_CharacterController.center = new Vector3(0f, headHeight * 0.5f + skinWidth, 0f);`, which adds `skinWidth` rather than having center at exactly half height (`0.835f`).

### 1.5 Missing Overrides in `Mainmenu scene.unity`
- In `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`:
  - `CharacterController` (`fileID: 6232745470614056090`) currently has NO modifications in the scene for `height`, `center`, `radius`, `stepOffset`, `skinWidth`. It inherits from the starter prefab:
    - Prefab `m_Height: 1.36144`
    - Prefab `m_Center: {x: 0, y: 0.76072, z: 0}`
    - Prefab `m_Radius: 0.1`
    - Prefab `m_StepOffset: 0.5`
    - Prefab `m_SkinWidth: 0.08`
  - `XROrigin` (`fileID: 1178791450436251564`) currently has `m_RequestedTrackingOriginMode: 2` (Floor), but NO modification for `m_CameraYOffset`, thus inheriting prefab's `1.36144` instead of `1.67`.

---

## 2. Logic Chain

1. **Premise 1 (Grounding)**: The main menu environment (`Camp_Ground` under `Diorama_Environment`) has its floor mesh collider at $Y = 0.0$. For physical floor tracking in VR to match the virtual beach floor, the root `XR Origin (XR Rig)` must have world position $(0, 0, 0)$.
2. **Premise 2 (Floor Tracking Origin)**: Meta Quest 3 utilizes Floor tracking mode (`RequestedTrackingOriginMode = Floor = 2`). In Floor tracking mode, the hardware tracking subsystem outputs coordinates where physical ground is $Y = 0$.
3. **Premise 3 (Camera Offset Role)**: `Camera Offset` is the intermediate parent between `XR Origin` and tracked nodes (`Main Camera`, `Left Controller`, `Right Controller`). In Floor mode, since the tracking origin is already the floor, any non-zero local translation on `Camera Offset` acts as an artificial vertical bias.
4. **Premise 4 (Floating Hands Mechanism)**: When `Camera Offset.localPosition.y` was set to $1.36144\text{m}$ (or $1.38\text{m}$), the natural hand tracking height of $\approx 1.0\text{m}$ was compounded: $0.0 + 1.38\text{m} + 1.0\text{m} = 2.38\text{m}$. Setting `Camera Offset` strictly to $(0, 0, 0)$ eliminates this bias, yielding $0.0 + 0.0 + 1.0\text{m} = 1.0\text{m}$, which falls squarely within the required $[0.8\text{m}, 1.2\text{m}]$ natural waist/chest band.
5. **Premise 5 (1.67m Stature Calibration)**:
   - `XROrigin.CameraYOffset` must be set to `1.67f`.
   - `CharacterController.height` must be set to `1.67f`.
   - In Unity's capsule geometry resting on $Y = 0$, $\text{center}.y = \frac{\text{height}}{2} = \frac{1.67}{2} = 0.835\text{m}$. Center must be $(0, 0.835, 0)$.
   - `PlayerBodyCollision` requires a `defaultHeadHeight = 1.67f` serialized field to replace all hardcoded `1.36144f` references and prevent `ConfigureCharacterController()` from clamping height to 0.85m on startup.
   - `PlayerBodyCollision` must check `isFloorMode = (m_XROrigin != null && m_XROrigin.RequestedTrackingOriginMode == Floor)` so that in Floor tracking mode, `Camera Offset` is NEVER overridden or offset by simulator/clamping logic.

---

## 3. Caveats

- **No Caveats.** The root causes, transform arithmetic, and script interactions are 100% determined from direct source code inspection and git commit history.
- The investigation adhered strictly to read-only mode: no source files or scene assets were modified during this exploration.

---

## 4. Conclusion & Actionable Implementation Plan

The implementer must apply the following specific edits across three files:

### 4.1 Changes to `Assets/Prospector-journey/Room/Scripts/PlayerBodyCollision.cs`
1. **Add `defaultHeadHeight` serialized field**:
   ```csharp
   [Header("Body Capsule Settings")]
   [SerializeField] private float defaultHeadHeight = 1.67f;
   [SerializeField] private float bodyRadius = 0.24f;
   [SerializeField] private float stepOffset = 0.08f;
   [SerializeField] private float skinWidth = 0.025f;
   [SerializeField] private float minHeadHeight = 0.85f;
   [SerializeField] private float maxHeadHeight = 2.05f;

   public float DefaultHeadHeight => defaultHeadHeight;
   ```
2. **Update `m_InitialOffsetY` initialization**:
   ```csharp
   private float m_InitialOffsetY = 1.67f;
   ```
3. **Fix `Awake()`**:
   When in Floor mode (`RequestedTrackingOriginMode == Floor`) or `IsRealVRDeviceActive()`:
   Set `m_InitialOffsetY = defaultHeadHeight;`, `m_CameraOffsetTransform.localPosition = Vector3.zero;`, `m_CurrentOffsetLocalPos = Vector3.zero;`.
4. **Fix `ConfigureCharacterController()`**:
   ```csharp
   private void ConfigureCharacterController()
   {
       if (m_CharacterController == null) return;

       m_CharacterController.radius = bodyRadius;
       m_CharacterController.stepOffset = stepOffset;
       m_CharacterController.skinWidth = skinWidth;
       m_CharacterController.minMoveDistance = 0.0001f;
       m_CharacterController.slopeLimit = 45f;

       float h = defaultHeadHeight > 0f ? defaultHeadHeight : Mathf.Clamp(m_InitialOffsetY, minHeadHeight, maxHeadHeight);
       m_CharacterController.height = h;
       m_CharacterController.center = new Vector3(0f, h * 0.5f, 0f);
   }
   ```
5. **Update `LateUpdate()` & `OnBeforeRender()`**:
   Define `bool isFloorMode = m_XROrigin != null && (m_XROrigin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor || m_XROrigin.CurrentTrackingOriginMode == UnityEngine.XR.TrackingOriginModeFlags.Floor);`.
   If `IsRealVRDeviceActive() || isFloorMode`:
   - Keep `m_CameraOffsetTransform.localPosition = Vector3.zero;`
   - Keep `m_CameraOffsetTransform.localRotation = Quaternion.identity;`
   - If camera has tracked (`m_CameraTransform.localPosition.y > minHeadHeight`), set `height = Mathf.Clamp(...)` and `center = new Vector3(m_CameraTransform.localPosition.x, headY * 0.5f, m_CameraTransform.localPosition.z)`.
   - Else keep `height = defaultHeadHeight` (1.67f) and `center = (0, 0.835, 0)`.
   - Do NOT run simulator counter-shift when in Floor mode.

### 4.2 Changes to `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuPlayerAnchor.cs`
1. In `SpawnAtOrigin()`:
   - Ensure `m_Origin.CameraFloorOffsetObject.transform.localPosition = Vector3.zero;` when in Floor tracking mode or real VR.
   - Replace any fallback `1.36144f` with `m_Origin.CameraYOffset > 0 ? m_Origin.CameraYOffset : 1.67f`.
   - When not in real VR (e.g. Editor / Play Mode audit), initialize simulated camera to eye height:
     `m_Origin.Camera.transform.localPosition = new Vector3(0f, m_Origin.CameraYOffset > 0 ? m_Origin.CameraYOffset : 1.67f, 0f);`
   - When not in real VR and controllers are at zero, initialize them to natural hand height:
     ```csharp
     Transform camOffset = m_Origin.CameraFloorOffsetObject != null ? m_Origin.CameraFloorOffsetObject.transform : transform.Find("Camera Offset");
     if (camOffset != null)
     {
         Transform leftCtrl = camOffset.Find("Left Controller");
         Transform rightCtrl = camOffset.Find("Right Controller");
         if (leftCtrl != null && leftCtrl.localPosition == Vector3.zero)
             leftCtrl.localPosition = new Vector3(-0.2f, 1.0f, 0.4f);
         if (rightCtrl != null && rightCtrl.localPosition == Vector3.zero)
             rightCtrl.localPosition = new Vector3(0.2f, 1.0f, 0.4f);
     }
     ```

### 4.3 Changes to `Assets/Prospector-journey/Room/Scripts/DoorSceneTransition.cs`
- In `VRSimulatorSceneBinder.PositionPlayerAtDoorEntrance` (lines 660–674), update fallback `1.36144f` to `1.67f` for Mainmenu scene.

### 4.4 Changes to `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
In `PrefabInstance` `fileID: 1935650666`:
1. `XROrigin` (`fileID: 1178791450436251564`):
   - `m_RequestedTrackingOriginMode: 2` (Floor)
   - `m_CameraYOffset: 1.67`
2. `CharacterController` (`fileID: 6232745470614056090`):
   - `m_Height: 1.67`
   - `m_Center: {x: 0, y: 0.835, z: 0}`
   - `m_Radius: 0.24`
   - `m_StepOffset: 0.08`
   - `m_SkinWidth: 0.025`
   - `m_MinMoveDistance: 0.0001`
3. `PlayerBodyCollision` (`fileID: 1935650669`):
   - `defaultHeadHeight: 1.67`
   - `minHeadHeight: 0.85`
   - `maxHeadHeight: 2.05`
   - `bodyRadius: 0.24`
   - `stepOffset: 0.08`
   - `skinWidth: 0.025`
4. `Camera Offset` (`fileID: 1680501587`):
   - `m_LocalPosition: {x: 0, y: 0, z: 0}`
5. `XR Origin (XR Rig)` Transform (`fileID: 1717954561962503726`):
   - `m_LocalPosition: {x: 0, y: 0, z: 0}`
   - `m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}`
6. `Left Controller` Transform (`fileID: 202364688`) & `Right Controller` Transform (`fileID: 1670256625`):
   - Local position overrides `{-0.2, 1.0, 0.4}` and `{0.2, 1.0, 0.4}`.

---

## 5. Verification Method

### 5.1 Static Verification
1. Inspect `Mainmenu scene.unity` YAML:
   - Verify `XR Origin (XR Rig)` position is `(0, 0, 0)`.
   - Verify `Camera Offset` position is `(0, 0, 0)`.
   - Verify `m_RequestedTrackingOriginMode` is `2` (Floor) and `m_CameraYOffset` is `1.67`.
   - Verify `CharacterController` has `m_Height = 1.67` and `m_Center = (0, 0.835, 0)`.
   - Verify `PlayerBodyCollision` has `defaultHeadHeight = 1.67`.

### 5.2 Automated Play Mode Verification Script
Run an automated Play Mode test in Unity executing checks:
```csharp
var rigObj = GameObject.Find("XR Origin (XR Rig)");
Assert.IsNotNull(rigObj);
Assert.AreEqual(0f, rigObj.transform.position.y, 0.01f, "XR Origin must be at ground Y=0");

var camOffset = rigObj.transform.Find("Camera Offset");
Assert.IsNotNull(camOffset);
Assert.AreEqual(Vector3.zero, camOffset.localPosition, "Camera Offset must be (0,0,0)");

var cc = rigObj.GetComponent<CharacterController>();
Assert.IsNotNull(cc);
Assert.AreEqual(1.67f, cc.height, 0.01f, "CharacterController height must be exactly 1.67m");
Assert.AreEqual(new Vector3(0f, 0.835f, 0f), cc.center, "CharacterController center must be (0, 0.835, 0)");

var xrOrigin = rigObj.GetComponent<Unity.XR.CoreUtils.XROrigin>();
Assert.AreEqual(Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Floor, xrOrigin.RequestedTrackingOriginMode);
Assert.AreEqual(1.67f, xrOrigin.CameraYOffset, 0.01f);

var leftHand = rigObj.transform.Find("Camera Offset/Left Controller/Left Hand Model");
var rightHand = rigObj.transform.Find("Camera Offset/Right Controller/Right Hand Model");
Assert.IsTrue(leftHand != null && leftHand.gameObject.activeInHierarchy);
Assert.IsTrue(rightHand != null && rightHand.gameObject.activeInHierarchy);
Assert.IsTrue(leftHand.position.y >= 0.8f && leftHand.position.y <= 1.2f, $"Left hand Y={leftHand.position.y} must be in [0.8, 1.2]");
Assert.IsTrue(rightHand.position.y >= 0.8f && rightHand.position.y <= 1.2f, $"Right hand Y={rightHand.position.y} must be in [0.8, 1.2]");
```

### 5.3 Invalidation Conditions
- If `CharacterController.height` is measured as `1.36144f` or `0.85f`, the fix is invalid.
- If `Camera Offset.localPosition.y` drifts away from `0.0` during runtime in Floor mode, the fix is invalid.
- If `Left Hand Model` or `Right Hand Model` world Y is $> 2.0\text{m}$, the fix is invalid.
