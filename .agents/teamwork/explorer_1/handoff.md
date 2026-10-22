# Technical Exploration Report: Requirement 1 (R1) - Main Menu World-Space Stationary Canvas & 360° Immersion

## 1. Observation

### 1.1 `MainMenuVRController.cs` Follow Logic
- **File**: `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs`
- **Follow Configuration** (Lines 60–71):
  ```csharp
  [Header("VR Lazy Follow Settings")]
  [Tooltip("Enable smooth VR follow that stays at eye height and respects horizontal gaze.")]
  [SerializeField] private bool enableLazyFollow = true;
  [Tooltip("Reference to the VR camera. Defaults to Camera.main.")]
  [SerializeField] private Transform targetCamera;
  [Tooltip("Distance from the camera in meters.")]
  [SerializeField] private float followDistance = 1.6f;
  [Tooltip("Height offset relative to camera eye level.")]
  [SerializeField] private float heightOffset = 0.0f;
  [Tooltip("Horizontal smoothing speed.")]
  [SerializeField] private float followSpeed = 6.0f;
  ```
- **Runtime Initialization** (Lines 141–144):
  ```csharp
  if (enableLazyFollow)
  {
      SnapToFront();
  }
  ```
- **Snap Execution** (Lines 431–452):
  ```csharp
  public void SnapToFront()
  {
      if (!enableLazyFollow) return;
      ...
      Vector3 snapPos = targetCamera.position + currentHorizontalFacing * followDistance;
      snapPos.y = targetCamera.position.y + heightOffset;
      transform.position = snapPos;
      transform.rotation = ComputeTargetRotation(snapPos, currentHorizontalFacing);
      ...
  }
  ```
- **Per-Frame Head Gaze Tracking in `LateUpdate`** (Lines 454–525):
  ```csharp
  private void LateUpdate()
  {
      if (!enableLazyFollow)
      {
          IsMenuMoving = false;
          return;
      }
      ...
      // Smoothly rotate facing direction towards camera gaze forward at all times
      if (angle > 0.1f)
      {
          float rotSpeed = Mathf.Lerp(followSpeed, followSpeed * 1.6f, Mathf.Clamp01(angle / 45f));
          currentHorizontalFacing = Vector3.Slerp(currentHorizontalFacing, camFwd, rotSpeed * Time.deltaTime).normalized;
      }
      ...
      Vector3 targetPos = camPos + currentHorizontalFacing * targetDist;
      targetPos.y = camPos.y + heightOffset;
      float posSpeed = isPlayerWalking ? 18f : 12f;
      transform.position = Vector3.Lerp(transform.position, targetPos, posSpeed * Time.deltaTime);

      Quaternion targetRot = ComputeTargetRotation(transform.position, currentHorizontalFacing);
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, followSpeed * Time.deltaTime);
      ...
  }
  ```
- **Facing & Orientation Mathematics** (Lines 530–550):
  ```csharp
  // Canvas forward points away from camera so front face is visible
  Vector3 desiredFwd = -toCamera.normalized;
  ...
  Vector3 horizontalDir = forwardDir.normalized;
  Quaternion yawRot = Quaternion.LookRotation(horizontalDir, Vector3.up);
  Quaternion pitchRot = Quaternion.Euler(pitch, 0f, 0f);
  return yawRot * pitchRot;
  ```
  Note: When facing camera at origin $(0, 0, 0)$ looking toward $+Z$, `forwardDir` is $(0, 0, 1)$, giving `yawRot = Quaternion.identity` (`(0, 0, 0, 1)`).

### 1.2 `VR_MainMenu_Canvas` in `Mainmenu scene.unity`
- **File**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
- **GameObject**: `VR_MainMenu_Canvas` (ID: `1321170159`, Line 13791)
- **Component List** (Lines 13783–13789):
  - `RectTransform` (ID: `1321170165`)
  - `Canvas` (ID: `1321170164`, World Space `m_RenderMode: 2`)
  - `GraphicRaycaster` (ID: `1321170163`)
  - `TrackedDeviceGraphicRaycaster` (ID: `1321170162`)
  - `MainMenuVRController` (ID: `1321170161`)
  - `AudioSource` (ID: `1321170160`)
  - `CanvasGroup` (ID: `1321170166`)
- **MainMenuVRController Serialized Values** (Lines 13905–13945):
  - Line 13936: `enableLazyFollow: 1`
  - Line 13937: `targetCamera: {fileID: 1650475068}` (Points to `Main Camera` in `XR Origin (XR Rig)`)
- **RectTransform Transform Coordinates** (Lines 14006–14025):
  ```yaml
  RectTransform:
    m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
    m_LocalPosition: {x: 0, y: 0, z: 1.85}
    m_LocalScale: {x: 0.00165, y: 0.00165, z: 0.00165}
    m_AnchorMin: {x: 0.5, y: 0.5}
    m_AnchorMax: {x: 0.5, y: 0.5}
    m_AnchoredPosition: {x: 0, y: 1.45}
    m_SizeDelta: {x: 1500, y: 950}
  ```
  The scene file already specifies the target position `(0, 1.45, 1.85)` and rotation `Quaternion.identity` (`Euler(0, 0, 0)`), but runtime execution overwrote it every frame because `enableLazyFollow == 1`.

### 1.3 Input & XR Interactor Architecture
- **EventSystem & XRUIInputModule** (`Mainmenu scene.unity`, Lines 21884–21954):
  - GameObject `2094711395` houses `UnityEngine.EventSystems.EventSystem` and `UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule`.
  - `m_EnableXRInput: 1`, `m_EnableMouseInput: 1`, `m_EnableTouchInput: 1`.
- **Canvas Raycaster**:
  - `TrackedDeviceGraphicRaycaster` (ID: `1321170162`) on `VR_MainMenu_Canvas` receives ray and poke events dispatched by `XRUIInputModule`.
- **XR Origin Interactors**:
  - `XR Origin (XR Rig)` includes `Left_NearFarInteractor` and `Right_NearFarInteractor` (Ray interaction, up to 10m range) and `Poke Interactor` (Near poke interaction).
- **Audio Feedback Script** (`VRButtonAudioFeedback.cs`, Lines 33–39):
  - `if ((mainMenuController != null && mainMenuController.IsMenuMoving) || ...) { return; }`
  - When `enableLazyFollow = false`, `mainMenuController.IsMenuMoving` remains `false` permanently, ensuring hover and click sounds trigger smoothly without suppressions.

### 1.4 Audit Script State
- **File**: `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuPlayModeComprehensiveAudit.cs`
- **Lines 321–387 (TEST 6)**:
  - Previously checked that rotating the camera by 15° and 45° caused the menu to rotate and follow gaze.
  - This test will fail under the new requirement unless updated to assert that the menu remains firmly planted in world space when the camera turns.

---

## 2. Logic Chain

1. **Root Cause of Gaze-Glued Menu**:
   - `enableLazyFollow` is set to `true` (scene value `1`).
   - In `MainMenuVRController.Start()`, `SnapToFront()` immediately snaps `transform.position` to `targetCamera.position + forward * followDistance` and aligns orientation to the camera gaze.
   - In `MainMenuVRController.LateUpdate()`, every frame recalculates `currentHorizontalFacing` and smooth-lerps `transform.position` and `transform.rotation` to track the camera.
   - This prevents the player from looking 360° at the environment without the canvas following their gaze.

2. **Mechanism to Eliminate Head-Follow**:
   - Setting `enableLazyFollow = false`:
     - In `Start()`: bypasses `SnapToFront()`.
     - In `LateUpdate()`: hits the early return `if (!enableLazyFollow) { IsMenuMoving = false; return; }`, executing zero translation and zero rotation.
     - In `SnapToFront()`: guarded by `if (!enableLazyFollow) return;`.
   - No other script in `Mainmenu scene.unity` or the project moves or rotates `VR_MainMenu_Canvas`.

3. **Guaranteed Stationary Placement**:
   - The canvas RectTransform in `Mainmenu scene.unity` is at `anchoredPosition.y = 1.45`, `localPosition.z = 1.85` (world position `(0, 1.45, 1.85)`) with `localRotation = (0, 0, 0, 1)` (Euler `(0, 0, 0)`).
   - The player spawns at `(0, 0, 0)` facing $+Z$ (`Quaternion.identity`).
   - The canvas with rotation `Quaternion.identity` has its front face directed toward $-Z$ (toward $(0, 0, 0)$), presenting the UI directly in front of the player at 1.85m distance and 1.45m height.
   - Adding explicit code enforcement in `MainMenuVRController.cs`:
     ```csharp
     [Header("Stationary 3D World Placement")]
     [SerializeField] private Vector3 stationaryWorldPosition = new Vector3(0f, 1.45f, 1.85f);
     [SerializeField] private Vector3 stationaryWorldEuler = Vector3.zero;
     ```
     and in `Start()`:
     ```csharp
     if (enableLazyFollow)
     {
         SnapToFront();
     }
     else
     {
         PlantAtStationaryPosition();
     }
     ```
     ensures deterministic placement at runtime even if the scene file or transform is nudged.

4. **360° Immersion Behavior**:
   - At 1.85m distance, the 1500x950 canvas (scale 0.00165 = 2.475m x 1.5675m) covers ~67.5° horizontal field of view.
   - Looking left (-90°), right (+90°), up (+Y), or backward (180°) completely turns the player's view away from the menu board, offering an unobstructed 360° view of the ocean, pirate ship, rocks, camp ground, and sky.

5. **Interactor Parity & Stability**:
   - `XRUIInputModule` and `TrackedDeviceGraphicRaycaster` do not depend on menu motion.
   - Removing camera-following motion eliminates cursor jitter caused by head micro-tremors, making Ray Interactor button targeting substantially more accurate.
   - Poke interaction becomes predictable: the board is a solid virtual surface in world space rather than an object that floats away when the player approaches.

---

## 3. Caveats

1. **Poke Interaction Range**:
   - From player spawn `(0, 0, 0)`, the canvas is 1.85m away. Poke interaction requires physical proximity (< 0.2m from finger/controller tip to canvas surface). Thus, from spawn, Ray interaction is the primary interaction mode. Poke interaction is used if the player physically steps forward within Guardian or teleports closer.
2. **Audit Script Dependency**:
   - `MainMenuPlayModeComprehensiveAudit.cs` (TEST 6) must be updated by the Test/Verification agent. If left unchanged, it will expect the menu to turn 15° and 45° with the camera and trigger a test failure.
3. **Editor Tooling / Builders**:
   - `MainMenuTranslucentUIBuilder.cs` configures `followDistance` and `heightOffset` via `SerializedObject`. It should be noted that builders should not re-enable `enableLazyFollow`.

---

## 4. Conclusion & Actionable Specification

### 4.1 Changes to `MainMenuVRController.cs`
- **Path**: `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs`
1. Line 62: Change `enableLazyFollow` default to `false`:
   ```csharp
   [Tooltip("Enable smooth VR follow that stays at eye height and respects horizontal gaze.")]
   [SerializeField] private bool enableLazyFollow = false;
   ```
2. Add stationary placement serialized fields:
   ```csharp
   [Header("Stationary 3D World Placement")]
   [Tooltip("World position when enableLazyFollow is false.")]
   [SerializeField] private Vector3 stationaryWorldPosition = new Vector3(0f, 1.45f, 1.85f);
   [Tooltip("World Euler rotation when enableLazyFollow is false (faces player spawn at 0,0,0).")]
   [SerializeField] private Vector3 stationaryWorldEuler = Vector3.zero;
   ```
3. Update `Start()` (Lines 141–145):
   ```csharp
   if (enableLazyFollow)
   {
       SnapToFront();
   }
   else
   {
       PlantAtStationaryPosition();
   }
   ```
4. Add method:
   ```csharp
   public void PlantAtStationaryPosition()
   {
       transform.position = stationaryWorldPosition;
       transform.rotation = Quaternion.Euler(stationaryWorldEuler);
       IsMenuMoving = false;
   }
   ```

### 4.2 Changes to `Mainmenu scene.unity`
- **Path**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
1. On `MainMenuVRController` component (`fileID: 1321170161`, Line 13936):
   - Set `enableLazyFollow: 0`
   - Set `stationaryWorldPosition: {x: 0, y: 1.45, z: 1.85}`
   - Set `stationaryWorldEuler: {x: 0, y: 0, z: 0}`
2. On `RectTransform` component (`fileID: 1321170165`, Line 14012–14023):
   - Ensure `m_LocalPosition: {x: 0, y: 1.45, z: 1.85}`
   - Ensure `m_AnchoredPosition: {x: 0, y: 1.45}`
   - Ensure `m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}`

### 4.3 Changes for Test Agent (`MainMenuPlayModeComprehensiveAudit.cs`)
- Update TEST 6 (Lines 321–387) to assert stationary behavior:
  - Rotate camera by 15°, 45°, 90°.
  - Assert that `canvas.transform.position` remains `(0, 1.45, 1.85)` (within tolerance $\pm 0.01$).
  - Assert that `canvas.transform.rotation` remains `Quaternion.identity` (within tolerance $\pm 0.01$).
  - Assert that angle between canvas forward and camera forward changes corresponding to camera turn (proving menu does NOT follow gaze).

---

## 5. Verification Method

1. **Static Inspection**:
   - Check `MainMenuVRController.cs` has `enableLazyFollow = false`.
   - Check `Mainmenu scene.unity` line 13936 has `enableLazyFollow: 0`.
   - Check `VR_MainMenu_Canvas` RectTransform has position `(0, 1.45, 1.85)` and rotation `(0, 0, 0)`.
2. **Runtime Head Gaze Independence Verification**:
   - Enter Play Mode on `Mainmenu scene.unity`.
   - Rotate camera yaw to +45° and -45°: observe canvas remains stationary at `(0, 1.45, 1.85)`.
   - Rotate camera yaw to 90° (left/right) and 180° (behind): verify full 360° view of ocean, pirate ship, and camp environment is unobstructed.
3. **XR Interaction Verification**:
   - Point controller ray at `Btn_StartExpedition`, `Btn_Tutorial`, `Btn_Settings`, `Btn_Credits`, `Btn_Quit`: verify hover sound triggers.
   - Click `Btn_Settings` with ray interactor trigger: verify Game Options panel opens.
   - Cycle Turn Type and Locomotion Type, click `Save and Exit`: verify settings persist and return to main panel.
   - Verify zero errors or exceptions in Unity Console.
