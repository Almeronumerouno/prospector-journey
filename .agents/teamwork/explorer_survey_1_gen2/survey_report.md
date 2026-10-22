# Authoritative Room Scene Reference Survey Report (`Room scene.unity`)

**Explorer**: Explorer 1 Gen 2 (Room Scene Surveyor)  
**Reference Scene**: `Assets/Prospector-journey/Room/Room scene.unity`  
**Target Scene to Port To**: `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`  
**Timestamp**: 2026-10-12T16:29:00Z  

---

## Executive Summary

This report establishes the complete, 100% authoritative reference architecture of all VR player systems, locomotion, inventory UI, physical tools (`tool-shovel`, `Gold_Pan`), and scene transition door mechanics surveyed directly from `Assets/Prospector-journey/Room/Room scene.unity`.

All YAML documents, PrefabInstances, components, serialized field values, audio clips, and script GUIDs have been parsed and verified. This survey serves as the exact blueprint for porting and synchronizing `Tutorial scene.unity`.

---

## 1. XR Origin (XR Rig) Hierarchy, Components & Configuration

In `Room scene.unity`, the XR Origin is instantiated as `PrefabInstance 859651291` referencing the source prefab:
- **Source Prefab**: `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (GUID: `a2a0d931eebd6a1479e56fc82c3b78db`, FileID: `100100000`).

### 1.1 Root GameObject: `XR Origin (XR Rig)`
- **Layer**: `2` (`Ignore Raycast`)
- **Tag**: `Untagged`
- **PrefabInstance Transform Position in Room**: `(0, 0, 0.35)` | Rotation: `(0, 0, 0)`

#### Attached Components:
1. **Transform** (Class 4, target `1717954561962503726`)
2. **XROrigin** (Class 114, target `1178791450436251564`, GUID `e0cb9aa70a22847b5925ee5f067c10a9`)
   - `m_Camera`: references `Main Camera` (`1767192439`)
   - `m_CameraFloorOffsetObject`: references `Camera Offset` (`1680501586`)
3. **CharacterController** (Class 143, target `6232745470614056090`)
   - **Prefab Overrides in Room Scene**:
     - `m_Radius`: `0.24`
     - `m_StepOffset`: `0.08`
     - `m_SkinWidth`: `0.025`
     - `m_MinMoveDistance`: `0.0001`
     - `m_SlopeLimit`: `45`
     - `m_Height`: `1.36144` (dynamically updated by `PlayerBodyCollision`)
     - `m_Center`: `{x: 0, y: 0.76072, z: 0}` (dynamically updated by `PlayerBodyCollision` to `{0, headHeight * 0.5 + skinWidth, 0}`)
     - `m_Material`: PhysicsMaterial `{fileID: 678803731}` (dynamicFriction: 0.6, staticFriction: 0.6, bounciness: 0)
4. **InputActionManager** (Class 114, target `6520291404937146767`, GUID `017c5e3933235514c9520e1dace2a4b2`)
5. **XRInputModalityManager** (Class 114, target `5826056641483426609`, GUID `82bc72d2ecc8add47b2fe00d40318500`)
6. **XRInteractionGroup / GazeInteractor** (Class 114, target `5033801203051696737`, GUID `c9b3d17eeb2e6bc47ada81d8f7f638d8`, disabled)
7. **PlayerBodyCollision** (**Added Component** `1901275664` in Room Scene):
   - **Script**: `Assets/Prospector-journey/Room/Scripts/PlayerBodyCollision.cs` (GUID: `64c4702045e23724887e7d066299cfb9`)
   - **Execution Order**: `[DefaultExecutionOrder(20000)]`
   - **Configured Property Values**:
     - `bodyRadius`: `0.24`
     - `stepOffset`: `0.08`
     - `skinWidth`: `0.025`
     - `minHeadHeight`: `0.85`
     - `maxHeadHeight`: `2.05`

### 1.2 Hierarchy Structure of `XR Origin (XR Rig)`
```text
XR Origin (XR Rig) [PrefabInstance 859651291]
├── Camera Offset [GO 1680501586, Transform 1680501587, localPos: (0, 1.36144, 0)]
│   ├── Main Camera [GO 1767192433, Transform 1767192434]
│   │   ├── Camera [1767192439]
│   │   ├── AudioListener [1767192437]
│   │   ├── TrackedPoseDriver [6232745470614056083] (TrackingType: RotationAndPosition)
│   │   ├── UniversalAdditionalCameraData [Added Component 859651293] (RenderShadows=1, DepthTexture=On, OpaqueTexture=On)
│   │   └── TunnelingVignette [Locomotion Vignette mesh & parameters]
│   ├── Gaze Stabilized
│   │   └── Gaze Stabilized Attach
│   ├── Left Controller [GO 202364687, Transform 202364688]
│   │   ├── NearFarInteractor [4778211696441940833]
│   │   ├── XRInteractionGroup [942810691211101373] (GroupName: "Left")
│   │   ├── ActionBasedController / XRController [5967689310316253315]
│   │   └── TrackedPoseDriver [6693052528577237899]
│   ├── Left Controller Teleport Stabilized Origin
│   ├── Right Controller [GO 1670256624, Transform 1670256625]
│   │   ├── NearFarInteractor [5663893676086941514]
│   │   ├── XRInteractionGroup [6678509202150728127] (GroupName: "Right")
│   │   ├── ActionBasedController / XRController [4238984354899526239]
│   │   └── TrackedPoseDriver [4602308928622519009]
│   └── Right Controller Teleport Stabilized Origin
├── Locomotion [GO 5167925059111895691, Transform 6981642495833523204]
│   ├── LocomotionMediator [6640002710935096923] (GUID: 6fa7b4195685c3846be746c74f0ab2f8)
│   ├── LocomotionProvider [7399425242140339816] (GUID: 6a26c941eb8a46f7b6d00416227ab8c0)
│   ├── SnapTurn [SnapTurnProvider 7347985736721345035, enableTurnLeftRight=1]
│   ├── ContinuousTurn [ContinuousTurnProvider 6480925242510836759, turnSpeed=60]
│   ├── ContinuousMove [ContinuousMoveProvider 153982007679157697, moveSpeed=1, enableStrafe=1, useGravity=1]
│   ├── Jump [JumpLocomotionProvider]
│   ├── Grab Move [GrabMoveProvider]
│   ├── Teleportation [TeleportationProvider]
│   └── Climb [ClimbProvider]
├── [Right InteractionAttachController] Attach [Added GO 1681052504, localPos: (0, 1.36144, 0)]
│   └── [Right InteractionAttachController] Attach Child [GO 827924896]
└── [Left InteractionAttachController] Attach [Added GO 432481278, localPos: (0, 1.36144, 0)]
    └── [Left InteractionAttachController] Attach Child [GO 1424857011]
```

### 1.3 PlayerBodyCollision Mechanics
- In `LateUpdate()` and `OnBeforeRender()`:
  - Counter-shifts `Camera Offset` in X and Z: `m_CameraOffsetTransform.localPosition = new Vector3(-rawCamLocal.x, clampedOffsetY, -rawCamLocal.z)`.
  - Clamps eye-height offset: `clampedOffsetY = initialOffsetY - rawCamLocal.y`, keeping eye height constant when rotating/pressing `E`/`Q` in XR Interaction Simulator.
  - Converts horizontal HMD delta into `CharacterController.Move()`.
  - Collision ignores: calls `Physics.IgnoreCollision` between `CharacterController` and all colliders on `XRGrabInteractable`, `TeleportationAnchor`, and `"rugRound"`.
  - Solid obstacles: `EnsureFurnitureColliders()` scans `Environment_ProspectorCabin` and adds solid `MeshCollider` components to solid environment geometry.

---

## 2. `[Prospector_Inventory]` Hierarchy, Components & Configuration

The inventory is a root-level GameObject in `Room scene.unity`:
- **GameObject**: `[Prospector_Inventory]` (`1787724447`)
- **Transform**: `1787724450` (Local Position: `(0, 0, 0)`, Rotation: `(0, 0, 0, 1)`, Scale: `(1, 1, 1)`)

### 2.1 Components on Root `[Prospector_Inventory]`
1. **Transform** (`1787724450`)
2. **AudioSource** (`1787724448`):
   - `m_PlayOnAwake`: 0
   - `m_SpatialBlend`: 0 (2D UI audio)
3. **ProspectorInventory** (`1787724449`):
   - **Script**: `Assets/Prospector-journey/Room/Scripts/ProspectorInventory.cs` (GUID: `326bfa1eeb8ba014bb4cdb3ff2832049`)
   - **Serialized Configuration**:
     - `inventoryPanel`: `{fileID: 1310287892}` (`InventoryCanvas`)
     - `maxSlots`: `4`
     - `storedItemsContainer`: `{fileID: 1081478301}` (`[Stored_Items_Container]`)
     - `enableLazyFollow`: `true` (smooth VR floating follow)
     - `targetCamera`: `null` (auto-finds `Camera.main`)
     - `followDistance`: `0.85`
     - `heightOffset`: `-0.12`
     - `deadzoneAngle`: `18`
     - `catchUpThreshold`: `2.0`
     - `followSpeed`: `4.0`
     - `heightSpeed`: `3.5`
     - `enableSubtlePitch`: `true`
     - `maxPitchAngle`: `12`
     - `interactionManager`: `null` (auto-finds in scene)
     - `rightNearFar`: `null` (auto-finds on Right Controller)
     - `leftNearFar`: `null` (auto-finds on Left Controller)
     - `slotButtons`:
       - Slot 1: `{fileID: 747982263}` (`Slot_1`)
       - Slot 2: `{fileID: 1981878496}` (`Slot_2`)
       - Slot 3: `{fileID: 819247684}` (`Slot_3`)
       - Slot 4: `{fileID: 743756506}` (`Slot_4`)
     - `slotNameTexts`:
       - Slot 1: `{fileID: 431592106}`
       - Slot 2: `{fileID: 1720037537}`
       - Slot 3: `{fileID: 1341337744}`
       - Slot 4: `{fileID: 2101118865}`
     - `slotActionTexts`:
       - Slot 1: `{fileID: 1030447049}`
       - Slot 2: `{fileID: 1718981522}`
       - Slot 3: `{fileID: 894251048}`
       - Slot 4: `{fileID: 1664641807}`
     - `slotBackgrounds`:
       - Slot 1: `{fileID: 747982262}`
       - Slot 2: `{fileID: 1981878495}`
       - Slot 3: `{fileID: 819247683}`
       - Slot 4: `{fileID: 743756505}`
     - `storeHeldButton`: `{fileID: 1301088898}` (`StoreHeldButton`)
     - `storeHeldButtonText`: `{fileID: 997169006}` (`StoreHeldText`)
     - `closeButton`: `{fileID: 1416725593}` (`CloseButton`)
     - `statusText`: `{fileID: 51794356}` (`StatusText`)
     - `toastPanel`: `{fileID: 1616964857}` (`ToastNotification`)
     - `toastText`: `{fileID: 842807856}` (`Text` under ToastNotification)
     - `emptySlotBg`: `{r: 0.12, g: 0.08, b: 0.05, a: 0.95}`
     - `occupiedSlotBg`: `{r: 0.24, g: 0.15, b: 0.09, a: 0.95}`
     - `goldAccentColor`: `{r: 0.85, g: 0.65, b: 0.25, a: 1.0}`
     - `emptyTextColor`: `{r: 0.55, g: 0.50, b: 0.45, a: 0.7}`
     - `audioSource`: `{fileID: 1787724448}`
     - **Audio Clips**:
       - `openClip`: `SFX_UI_Quick_1.ogg` (GUID: `cbca6f23424bd42c9a2cae00f5b644ea`)
       - `closeClip`: `SFX_UI_Quick_2.ogg` (GUID: `eb6747799fac14af58d8c4fc5410fc1e`)
       - `storeClip`: `SFX_UI_Game_Positive.ogg` (GUID: `a6e6cb127e7044a3eaa8772d8a314706`)
       - `retrieveClip`: `SFX_UI_Click_2.ogg` (GUID: `39eeabf0f489f4e46a024aa733bf1b4b`)
       - `errorClip`: `SFX_UI_Game_Negative.ogg` (GUID: `a6d54794b4ce94de1b7b1587af379192`)
       - `hoverClip`: `SFX_UI_Button_1.ogg` (GUID: `3c7b3ef689cc94af0bd824489050effd`)
       - `clickClip`: `SFX_UI_Click_1.ogg` (GUID: `8436c8cf5779f4fbda9ea5a4acee8d73`)

### 2.2 Hierarchy of `[Prospector_Inventory]`
```text
[Prospector_Inventory] [GO 1787724447, Transform 1787724450]
├── Stored_Items_Container [GO 1081478300, Transform 1081478301, Active: false]
│   └── (In Tutorial scene: holds pre-stored tool-shovel and Gold_Pan)
├── InventoryCanvas [GO 1310287892, RectTransform 1310287893, Active: false]
│   ├── Canvas [1310287898] (WorldSpace, Size: 720 x 520, Scale: 0.0016)
│   ├── CanvasScaler [1310287897]
│   ├── GraphicRaycaster [1310287896]
│   ├── TrackedDeviceGraphicRaycaster [1310287895]
│   ├── CanvasGroup [1310287899]
│   ├── PanelBackground [GO 1222851502, Image 1222851504]
│   ├── Header [GO 1133379261]
│   │   ├── TitleText ("TAS PROSPEKTOR / SATCHEL") [TMP 1098674996]
│   │   ├── SubtitleText ("Penyimpanan Peralatan Tambang") [TMP 927289569]
│   │   └── CloseButton [Button 1416725593, VRButtonAudioFeedback 1416725594]
│   ├── StoreHeldBar [GO 1640167661]
│   │   └── StoreHeldButton [Button 1301088898, VRButtonAudioFeedback 1301088899]
│   │       └── StoreHeldText [TMP 997169006]
│   ├── GridContainer [GO 1636257329]
│   │   ├── Slot_1 [Button 747982263, Image 747982262, VRButtonAudioFeedback 747982264]
│   │   │   ├── SlotBorder [Image 1729424881]
│   │   │   ├── TagText ("[ SLOT 1 ]") [TMP 1109043324]
│   │   │   ├── NameText [TMP 431592106]
│   │   │   └── ActionText [TMP 1030447049]
│   │   ├── Slot_2 [Button 1981878496, Image 1981878495, VRButtonAudioFeedback 1981878497]
│   │   │   ├── SlotBorder [Image 137351000]
│   │   │   ├── TagText ("[ SLOT 2 ]") [TMP 1111666497]
│   │   │   ├── NameText [TMP 1720037537]
│   │   │   └── ActionText [TMP 1718981522]
│   │   ├── Slot_3 [Button 819247684, Image 819247683, VRButtonAudioFeedback 819247685]
│   │   │   ├── SlotBorder [Image 1824814055]
│   │   │   ├── TagText ("[ SLOT 3 ]") [TMP 1712115602]
│   │   │   ├── NameText [TMP 1341337744]
│   │   │   └── ActionText [TMP 894251048]
│   │   └── Slot_4 [Button 743756506, Image 743756505, VRButtonAudioFeedback 743756507]
│   │       ├── SlotBorder [Image 211073093]
│   │       ├── TagText ("[ SLOT 4 ]") [TMP 1276562005]
│   │       ├── NameText [TMP 2101118865]
│   │       └── ActionText [TMP 1664641807]
│   └── StatusBar [GO 990385649]
│       └── StatusText [TMP 51794356]
└── ToastNotification [GO 1616964857, RectTransform 1616964858, Active: false]
    ├── Canvas [1616964859] (WorldSpace, Size: 480 x 65, Scale: 0.0016)
    └── Border [GO 1193579999]
        └── Background [GO 984839007]
            └── Text [TMP 842807856]
```

### 2.3 Controls & Interactions Supported
- Keyboard:
  - `I` / `Tab`: `ToggleInventory()`
  - `C` / `X`: `QuickStoreHeldItem()`
- VR Controller:
  - Right `B` or Left `Y`: Toggle Satchel UI (or quick-store if holding tool)
  - Right `A` or Left `X`: `QuickStoreHeldItem()` (when not near door)
- Retrieval: Clicking any occupied slot button unpacks the item and auto-snaps to free hand.

---

## 3. `tool-shovel` & `Gold_Pan` Component & Transform Survey

In `Room scene.unity`, both items are located under `Zone_Main_Prospector_Workbench` (`537001751`).

### 3.1 `tool-shovel`
- **PrefabInstance**: `2122980739`
- **Source Model**: `Assets/Prospector-journey/Room/Models/tool-shovel.fbx` (GUID: `a9b6f002ed24e2b41a299c7565b5efab`)
- **Layer**: `0` (`Default`) | **Tag**: `Untagged`
- **Transform**:
  - Local Position: `(-0.655, 0.957, 1.588)`
  - Local Rotation: `(15, 45, -6.47)`
  - Local Scale: `(1, 1, 1)`
- **Material Override**: `Tool-Material.mat` (GUID: `59e96263582ba0b468453c681d190659`)
- **Attached Objects**:
  - `attach1` (GO `391156552`, Transform `391156553`):
    - Local Position: `(0, 0.05, 0)`
    - Local Rotation: `(60, 0, 0)` (`{x: 0.5, y: 0, z: 0, w: 0.8660254}`)
  - `attach2` (GO `375342302`, Transform `375342303`):
    - Local Position: `(0, 0.17, 0)`
    - Local Rotation: `(60, 0, 0)` (`{x: 0.5, y: 0, z: 0, w: 0.8660254}`)
- **Attached Components**:
  1. **BoxCollider** (`2122980745`):
     - `m_Center`: `{x: 0, y: 0.15, z: 0}`
     - `m_Size`: `{x: 0.08, y: 0.3, z: 0.04}`
     - `m_IsTrigger`: `0`
  2. **Rigidbody** (`2122980744`):
     - `m_Mass`: `1.8`
     - `m_LinearDamping`: `0`
     - `m_AngularDamping`: `0.05`
     - `m_UseGravity`: `1`
     - `m_IsKinematic`: `0`
     - `m_Interpolate`: `1` (`Interpolate`)
     - `m_CollisionDetection`: `1` (`Discrete`)
     - `m_Constraints`: `126` (`FreezeAll`)
  3. **XRGrabInteractable** (`2122980743`):
     - `m_AttachTransform`: `{fileID: 391156553}` (`attach1`)
     - `m_SecondaryAttachTransform`: `{fileID: 375342303}` (`attach2`)
     - `m_InteractionLayers`: `1` (Default)
     - `m_MovementType`: `2` (`VelocityTracking`)
     - `m_ThrowOnDetach`: `1`
     - `m_ThrowVelocityScale`: `1.5`
     - `m_ThrowAngularVelocityScale`: `1.0`
     - `m_ForceGravityOnDetach`: `1`
     - `m_FarAttachMode`: `1` (`Near` — prevents far laser grab)
     - `m_MatchAttachPosition`: `1`
     - `m_MatchAttachRotation`: `1`
     - `m_SnapToColliderVolume`: `1`
     - `m_UseDynamicAttach`: `0`
  4. **XRRestingProp** (`2122980742`):
     - **Script**: `Assets/Prospector-journey/Room/Scripts/XRRestingProp.cs` (GUID: `412b71b5d2b4efd489263b77f011fbc9`)
     - `m_FreezeUntilGrabbed`: `1`
     - `m_DirectGrabOnly`: `1`
     - `m_AttachLocalPosition`: `{x: 0, y: 0, z: 0}`
     - `m_AttachLocalEulerAngles`: `{x: 0, y: 0, z: 0}`
     - `m_SecondaryBreakDistance`: `0.85`
  5. **XRGeneralGrabTransformer** (`2122980748`):
     - `m_PermittedDisplacementAxes`: `7`
     - `m_TwoHandedRotationMode`: `1`
     - `m_AllowOneHandedScaling`: `1`
     - `m_AllowTwoHandedScaling`: `0`
     - `m_ClampScaling`: `1`
     - `m_MinimumScaleRatio`: `0.25`
     - `m_MaximumScaleRatio`: `2.0`
     - `m_ScaleMultiplier`: `0.25`

### 3.2 `Gold_Pan`
- **PrefabInstance**: `2018871572`
- **Source Model**: `Assets/Prospector-journey/Room/Models/Gold_Pan.obj` (GUID: `1a0fcbc1bc065984ca21139c0f33b62e`)
- **Layer**: `0` (`Default`) | **Tag**: `Untagged`
- **Transform**:
  - Local Position: `(-0.59, 0.89, 1.8)`
  - Local Rotation: `{x: 0, y: 0.1908, z: 0, w: 0.9816}` (Euler: `(0, 21.99, 0)`)
  - Local Scale: `(0.82, 0.82, 0.82)`
- **Material Override**: `Material-Wood.mat` (GUID: `d2866cd76bfbb0e4e8e6cba8eb01814e`)
- **Attached Objects**:
  - `attach1` (GO `497355701`, Transform `497355702`):
    - Local Position: `(0.2, 0.05, 0)`
    - Local Rotation: `(0, 0, 0)`
  - `attach2` (GO `1763807606`, Transform `1763807607`):
    - Local Position: `(-0.2, 0.05, 0)`
    - Local Rotation: `(0, 0, 0)`
- **Attached Components**:
  1. **MeshCollider** (`2018871576`):
     - `m_Convex`: `1`
     - `m_IsTrigger`: `0`
     - `m_Mesh`: `Gold_Pan` Mesh (`-2432090755550338912`)
  2. **Rigidbody** (`2018871579`):
     - `m_Mass`: `1.2`
     - `m_LinearDamping`: `0`
     - `m_AngularDamping`: `0.05`
     - `m_UseGravity`: `1`
     - `m_IsKinematic`: `0`
     - `m_Interpolate`: `1` (`Interpolate`)
     - `m_CollisionDetection`: `2` (`Continuous`)
     - `m_Constraints`: `126` (`FreezeAll`)
  3. **XRGrabInteractable** (`2018871578`):
     - `m_AttachTransform`: `{fileID: 497355702}` (`attach1`)
     - `m_SecondaryAttachTransform`: `{fileID: 1763807607}` (`attach2`)
     - `m_InteractionLayers`: `1` (Default)
     - `m_MovementType`: `2` (`VelocityTracking`)
     - `m_ThrowOnDetach`: `1`
     - `m_ThrowVelocityScale`: `1.5`
     - `m_ThrowAngularVelocityScale`: `1.0`
     - `m_ForceGravityOnDetach`: `1`
     - `m_FarAttachMode`: `1` (`Near`)
     - `m_MatchAttachPosition`: `1`
     - `m_MatchAttachRotation`: `1`
     - `m_SnapToColliderVolume`: `1`
     - `m_StartingMultipleGrabTransformers`: `[{fileID: 2018871583}]`
  4. **XRRestingProp** (`2018871577`):
     - `m_FreezeUntilGrabbed`: `1`
     - `m_DirectGrabOnly`: `1`
     - `m_AttachLocalPosition`: `{x: 0, y: 0, z: 0}`
     - `m_AttachLocalEulerAngles`: `{x: 0, y: 0, z: 0}`
     - `m_SecondaryBreakDistance`: `0.85`
  5. **XRGeneralGrabTransformer** (`2018871583`):
     - `m_PermittedDisplacementAxes`: `-1` (All)
     - `m_TwoHandedRotationMode`: `1`
     - `m_AllowOneHandedScaling`: `0`
     - `m_AllowTwoHandedScaling`: `0`

---

## 4. Cabin Door, Prompt & Scene Transition

In `Room scene.unity`, the door interaction system comprises `Cabin_Door_Slab` and `Door_Interaction_Prompt`.

### 4.1 `Cabin_Door_Slab`
- **GameObject**: `Cabin_Door_Slab` (`1042778239`)
- **Parent**: `Cabin_Architecture` (`166936368`, Transform `166936369`)
- **Transform**: Local Pos `(-0.06, 1.17, -3.48)` | Local Scale `(1.2, 2.24, 0.08)`
- **Components**:
  1. **MeshFilter** (`1042778243`) & **MeshRenderer** (`1042778241`)
  2. **BoxCollider** (`1042778242`):
     - `m_Center`: `{x: 0, y: 0, z: 0}`
     - `m_Size`: `{x: 1, y: 1, z: 1}` (effective scale 1.2 x 2.24 x 0.08)
     - `m_IsTrigger`: `0` (Solid collider that physically stops player body)
  3. **XRSimpleInteractable** (`1042778245`):
     - `m_InteractionLayers`: `1` (Default)
     - `m_SelectMode`: `0` (Single)
  4. **DoorSceneTransition** (`1042778244`):
     - **Script**: `Assets/Prospector-journey/Room/Scripts/DoorSceneTransition.cs` (GUID: `4d2dd7193bf540644aa6c6c52d7b24c0`)
     - `targetSceneName`: `"Tutorial scene"` (In Tutorial scene, configure this to `"Room scene"`)
     - `interactionDistance`: `1.85`
     - `doorCenter`: `{fileID: 1042778240}` (`Cabin_Door_Slab` Transform)
     - `promptPanel`: `{fileID: 1797127868}` (`Door_Interaction_Prompt`)
     - `promptCanvasGroup`: `{fileID: 1797127869}` (`Door_Interaction_Prompt` CanvasGroup)
     - `promptText`: `{fileID: 551002857}` (`PromptText` TextMeshProUGUI)
     - `audioSource`: `null` (auto-created on Awake with `spatialBlend = 1.0f`)
     - `doorOpenClip`: `SFX_UI_Click_1.ogg` (GUID: `8436c8cf5779f4fbda9ea5a4acee8d73`)
     - `doorCreakClip`: `SFX_Impact_Log02.ogg` (GUID: `5fa2a1e03047f4f37a8f475c272636e7`)
     - `fadeDuration`: `0.75`
     - `fadeColor`: `{r: 0, g: 0, b: 0, a: 1}`

### 4.2 `Door_Interaction_Prompt`
- **GameObject**: `Door_Interaction_Prompt` (`1797127868`)
- **RectTransform** (`1797127871`):
  - Local Position: `(-0.06, 1.45, -3.2)`
  - Local Rotation: `{x: 0, y: 1, z: 0, w: -0.00000004371139}` (Euler: `(0, 180, 0)`)
  - Local Scale: `(0.0018, 0.0018, 0.0018)`
  - SizeDelta: `(420, 150)`
- **Canvas** (`1797127870`):
  - `m_RenderMode`: `2` (`WorldSpace`)
  - `m_Camera`: references `Main Camera` (`859651297`)
- **CanvasGroup** (`1797127869`):
  - `m_Alpha`: `0`
  - `m_Interactable`: `0`
  - `m_BlocksRaycasts`: `0`
- **Children**:
  - `Border` (`1798876588`): Image (fe87c0e1cc204ed48ad3b37840f39efc)
  - `Background` (`1867798244`): Image (fe87c0e1cc204ed48ad3b37840f39efc)
  - `TitleText` (`228416111`): TextMeshProUGUI (f4688fdb7df04437aeb418b961361dc5)
  - `PromptText` (`551002855`): TextMeshProUGUI (`551002857`)

### 4.3 Door Transition & Tools Enforcement Logic
- Distance check: Activated when camera is within `1.85m` of `doorCenter`.
- UI billboard rotation: `FacePromptToPlayer()` rotates prompt front face towards player.
- Required tools check: `HasRequiredToolsForExit(out bool hasShovel, out bool hasPan)` checks both `ProspectorInventory.Instance.HasItemInBagOrHand("shovel")` and `"Gold_Pan"`, or handheld interactables.
  - If both present: Prompt displays green `[OK] Sekop & Gold Pan Siap (VR: [A] / Trigger)` and allows transition upon pressing `E` (or VR `A` / Trigger).
  - If either is missing: Prompt displays red warning `Belum Bisa Keluar! Wajib Bawa: [X] Sekop [X] Gold Pan`, triggers error SFX and Toast notification, blocking transition.
- Transition sequence: Plays creak/open audio -> fades screen to black via `[VR_Screen_Fader]` over `0.75s` -> calls `SceneManager.LoadSceneAsync(targetSceneName)`.

---

## 5. Porting Blueprint: Delta Matrix for `Tutorial scene.unity`

| Component / System | Present in `Room scene.unity` | Status in `Tutorial scene.unity` | Action Required in `Tutorial scene.unity` |
|---|---|---|---|
| `PlayerBodyCollision` on `XR Origin (XR Rig)` | ✅ Yes (`1901275664`) | ❌ Missing | Add `PlayerBodyCollision` component to root GO of `XR Origin (XR Rig)` |
| `CharacterController` values | ✅ Overridden (radius=0.24, stepOffset=0.08, skinWidth=0.025, minMove=0.0001, PhysMat=678803731) | ❌ Default prefab values (radius=0.1, step=0.5, skin=0.08) | Apply overrides to target `6232745470614056090` |
| Environment obstacle colliders | ✅ `EnsureFurnitureColliders()` covers room cabin props | ❌ Lacks colliders for outdoor trees, rocks, stumps, houses | Extend `EnsureFurnitureColliders()` to scan `Environment2` & houses, fences, benches, crates |
| `[Prospector_Inventory]` | ✅ Yes (Root GO `1787724447` + `InventoryCanvas` + `ToastNotification` + `Stored_Items_Container`) | ❌ Completely missing | Instantiate `[Prospector_Inventory]` matching Room scene setup |
| Pre-equipped tools in Bag | In room on workbench | Must be pre-stored in bag upon entering Tutorial | Place `tool-shovel` and `Gold_Pan` under `Stored_Items_Container` with `m_IsActive = 0`, register into slots 0 and 1 |
| Cabin Door Transition | ✅ `Cabin_Door_Slab` + `DoorSceneTransition` + `Door_Interaction_Prompt` targeting `"Tutorial scene"` | ❌ Missing on Tutorial cabin | Add `DoorSceneTransition` targeting `"Room scene"` + `Door_Interaction_Prompt` to cabin door in Tutorial scene |

---

## 6. Verification and Invalidation Criteria

- **YAML Integrity**: Every GUID referenced matches existing project assets in `Assets/Prospector-journey/`.
- **Collider Behavior**: In Play Mode, walking forward against cabin door or furniture does not clip; walking over grass does not stutter.
- **Inventory & Toast**: Pressing `I`/`Tab` opens floating canvas; `C`/`X` stores held tools with Toast feedback.
- **Scene Transition**: Pressing `E` near door without shovel or pan displays error Toast; pressing `E` with both tools starts smooth fade to black and loads target scene.
