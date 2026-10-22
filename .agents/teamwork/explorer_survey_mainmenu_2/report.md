# VR Hand Models & Parity Survey Report
**Target**: `Mainmenu scene.unity` parity with `Room scene.unity` and `Tutorial scene.unity`  
**Date**: 2026-10-13  
**Status**: Completed (Read-Only Investigation)  
**Investigator**: `teamwork_preview_explorer` (Survey Explorer 2)

---

## Executive Summary
This investigation analyzed the XR Origin, VR controller models, and hand articulation setups across all three primary scenes:
1. `Assets/Prospector-journey/Room/Room scene.unity`
2. `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`
3. `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`

### Core Discovery:
- In both **`Room scene.unity`** and **`Tutorial scene.unity`**, `XR Origin (XR Rig)` is an instance of the custom prefab **`Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab`** (`GUID: a2a0d931eebd6a1479e56fc82c3b78db`).
  - This prefab already embeds animated Oculus Hand models (**`Left Hand Model.prefab`** and **`Right Hand Model.prefab`**) as children of `Left Controller` and `Right Controller`.
  - In this prefab, generic controller visuals (`Left Controller Visual` and `Right Controller Visual`) are explicitly set to **inactive** (`m_IsActive: 0`).
  - Both hand models are active (`m_IsActive: 1`), have `AnimateHandOnInput.cs` attached, and are wired directly to `XRI Default Input Actions`.
- In **`Mainmenu scene.unity`**, `XR Origin (XR Rig)` is an instance of the raw Unity starter asset **`Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab`** (`GUID: f6336ac4ac8b4d34bc5072418cdc62a0`).
  - It currently renders generic white/grey controller models (`Left Controller Visual` and `Right Controller Visual`).
  - It has **zero hand models** and **no `AnimateHandOnInput` component**.
- 100% parity can be achieved either by switching `Mainmenu scene.unity`'s XR Origin to reference `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (with locomotion disabled for the menu), or by disabling the controller visuals and instantiating `Left Hand Model.prefab` and `Right Hand Model.prefab` under the respective controllers.

---

## 1. XR Origin & Controller Architecture Across Scenes

| Scene | Scene Path | XR Origin Prefab Source | Source Prefab GUID | Hand Models Present | Controller Visuals Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Room Scene** | `Assets/Prospector-journey/Room/Room scene.unity` | `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` | `a2a0d931eebd6a1479e56fc82c3b78db` | Yes (Oculus Hands) | Disabled (`m_IsActive: 0`) |
| **Tutorial Scene** | `Assets/Prospector-journey/Tutorial/Tutorial scene.unity` | `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` | `a2a0d931eebd6a1479e56fc82c3b78db` | Yes (Oculus Hands) | Disabled (`m_IsActive: 0`) |
| **Mainmenu Scene** | `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity` | `Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` | `f6336ac4ac8b4d34bc5072418cdc62a0` | **No (Missing)** | **Active (`m_IsActive: 1`)** |

### Scene Hierarchy in Room & Tutorial Scenes:
```
XR Origin (XR Rig) [GameObject: 1717954561962503725]
├── Camera Offset [GameObject: 1680501586, Transform: 1680501587] (local pos: (0, 1.36144, 0))
│   ├── Main Camera [Transform: 1767192434]
│   ├── Gaze Interactor [Transform: 2196849375614954873] (m_IsActive: 0)
│   ├── Left Controller [GameObject: 202364687, Transform: 202364688] (local pos: (0, 0, 0))
│   │   ├── Near-Far Interactor [Transform: 1666320186578454293] (Active)
│   │   ├── Poke Interactor [Transform: 1543070802843469984] (Active)
│   │   ├── Teleport Interactor [Transform: 1319746309] (Active in gameplay, Inactive in Main Menu)
│   │   ├── Left Controller Visual [Transform: 8366379412631108205] (DISABLED: m_IsActive: 0)
│   │   └── Left Hand Model [Transform: 970303986733013941] (ACTIVE: m_IsActive: 1)
│   ├── Right Controller [GameObject: 1670256624, Transform: 1670256625] (local pos: (0, 0, 0))
│   │   ├── Near-Far Interactor [Transform: 3954319948395782924] (Active)
│   │   ├── Poke Interactor [Transform: 8393186890254128703] (Active)
│   │   ├── Teleport Interactor [Transform: 2449787133337329436] (Active in gameplay, Inactive in Main Menu)
│   │   ├── Right Controller Visual [Transform: 6528530117482412838] (DISABLED: m_IsActive: 0)
│   │   └── Right Hand Model [Transform: 3977538770454729481] (ACTIVE: m_IsActive: 1)
│   ├── [Left InteractionAttachController] Attach [Transform: 716906830792148215]
│   └── [Right InteractionAttachController] Attach [Transform: 8718302446126152263]
└── Locomotion [Transform: 6981642495833523204]
    ├── Move (CharacterController locomotion)
    ├── Teleportation
    ├── Snap Turn
    ├── Turn
    ├── ContinuousTurn
    └── TunnelingVignette
```

---

## 2. Oculus Hand Prefab Specifications

### A. Asset Locations & GUIDs
- **Left Hand Model Prefab**: `Assets/Oculus Hands/Prefabs/Left Hand Model.prefab` (`GUID: 65847589f926b734d9eaed6e25f0046b`)
- **Right Hand Model Prefab**: `Assets/Oculus Hands/Prefabs/Right Hand Model.prefab` (`GUID: 1fd3a7c43c8190f4d8c167ebde694364`)
- **Left Hand Animator Controller**: `Assets/Oculus Hands/Animations/Left Hand Animator.controller` (`GUID: 4b5efa0c69d901449a38989140500c0d`)
- **Right Hand Animator Controller**: `Assets/Oculus Hands/Animations/Right Hand Animator.controller` (`GUID: 3338dde1ae80a3b43826deca5c238356`)
- **Hands Material**: `Assets/Oculus Hands/Materials/Hands_solid.mat` (`GUID: 2f5ff50b86a014a4ba2fc1a44c9b98ec`)
- **Input Action Asset**: `Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/XRI Default Input Actions.inputactions` (`GUID: c348712bda248c246b8c49b3db54643f`)
- **Script**: `Assets/Oculus Hands/AnimateHandOnInput.cs` (`GUID: d4dde6e94d9e27d4591597be91e15d7d`)

### B. Hierarchy of Hand Prefabs
```
<Hand Model Root> (Left: 109434732395284509 / Right: 5686134208185362930)
├── hands:hands_geom (SkinnedMeshRenderer with Hands_solid.mat)
└── hands:[l|r]_hand_world (Rig root bone)
    └── hands:b_[l|r]_wrist
        ├── hands:b_[l|r]_thumb0 -> thumb1 -> thumb2 -> thumb3 -> thumb_ignore
        ├── hands:b_[l|r]_index1 -> index2 -> index3 -> index_ignore
        ├── hands:b_[l|r]_middle1 -> middle2 -> middle3 -> middle_ignore
        ├── hands:b_[l|r]_ring1 -> ring2 -> ring3 -> ring_ignore
        └── hands:b_[l|r]_pinky0 -> pinky1 -> pinky2 -> pinky3 -> pinky_ignore
```

### C. Components on Hand Prefabs
Each prefab contains three core components on its root GameObject:
1. **Transform**: (Offsets detailed in Section 3).
2. **Animator**:
   - Left Hand: `m_Controller: {fileID: 9100000, guid: 4b5efa0c69d901449a38989140500c0d, type: 2}`, `m_Avatar: {fileID: 9000000, guid: ca20b3907ff1c794cb89c1ad2cce8282, type: 3}`
   - Right Hand: `m_Controller: {fileID: 9100000, guid: 3338dde1ae80a3b43826deca5c238356, type: 2}`, `m_Avatar: {fileID: 9000000, guid: d0c255d07fd68f14b94e55d388e7e6bb, type: 3}`
   - `cullingMode`: `CullUpdateTransforms` (1)
   - `applyRootMotion`: `false` (0)
3. **AnimateHandOnInput**:
   - `AnimateHandOnInput.cs` reads analog float values in `Update()`:
     ```csharp
     float trigger = triggerValue.action != null ? triggerValue.action.ReadValue<float>() : 0f;
     float grip = gripValue.action != null ? gripValue.action.ReadValue<float>() : 0f;
     handAnimator.SetFloat("Trigger", trigger);
     handAnimator.SetFloat("Grip", grip);
     ```
   - Input Action References:
     - **Left Hand**:
       - `triggerValue`: Reference `{fileID: -8654792680252043297, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Left Interaction / Activate Value`)
       - `gripValue`: Reference `{fileID: -7989615598897241686, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Left Interaction / Select Value`)
     - **Right Hand**:
       - `triggerValue`: Reference `{fileID: -3908803764926266157, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Right Interaction / Activate Value`)
       - `gripValue`: Reference `{fileID: 2356269739022366052, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Right Interaction / Select Value`)
   - **Float Thresholds**: None. It is an analog 0.0 to 1.0 continuous blend.
   - **Animator Parameters**:
     - `"Grip"` (Float, range `0.0` - `1.0`)
     - `"Trigger"` (Float, range `0.0` - `1.0`)
   - **Blend Tree Configuration**:
     - Type: 2D Freeform Directional (BlendType 3)
     - X Parameter: `Grip`, Y Parameter: `Trigger`
     - (0, 0): Rest / Open Hand
     - (0, 1): Point / Pinch (Trigger pulled, Grip released)
     - (1, 0): Grasp / Fist (Grip squeezed, Trigger open)
     - (1, 1): Closed Fist (Both Grip and Trigger squeezed)

---

## 3. Exact Transform Offsets in Room & Tutorial Scenes

Under `Left Controller` and `Right Controller`, the hand models have the following exact local transform offsets (as defined in `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` lines 2248-2286 and lines 3131-3170):

### Left Hand Model (under `Left Controller`):
- **Local Position**: `X: -0.001`, `Y: 0.001`, `Z: -0.035`
- **Local Rotation (Quaternion)**: `{ x: 0.0, y: 0.0, z: 0.7071068, w: 0.7071068 }`
- **Local Euler Angles Hint**: `{ x: 0.0, y: 0.0, z: 90.0 }`
- **Local Scale**: `{ x: 1.0, y: 1.0, z: 1.0 }`

### Right Hand Model (under `Right Controller`):
- **Local Position**: `X: -0.001`, `Y: 0.001`, `Z: -0.035`
- **Local Rotation (Quaternion)**: `{ x: 0.0, y: 0.0, z: -0.7071068, w: 0.7071068 }`
- **Local Euler Angles Hint**: `{ x: 0.0, y: 0.0, z: -90.0 }`
- **Local Scale**: `{ x: 1.0, y: 1.0, z: 1.0 }`

> **Key Takeaway**:
> Both hands share identical local translation `(-0.001, 0.001, -0.035)` and scale `(1, 1, 1)`.
> The rotation differs symmetrically around the Z-axis: **+90°** for the Left Hand and **-90°** for the Right Hand. Neither `Room scene.unity` nor `Tutorial scene.unity` overrides these transforms.

---

## 4. Current State of `Mainmenu scene.unity`

In `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`:
1. `XR Origin (XR Rig)` is PrefabInstance `1935650666` referencing `Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` (`f6336ac4ac8b4d34bc5072418cdc62a0`).
2. **Current Controller Models**:
   - `Left Controller Visual` (`XR Controller Left.prefab`) is **active**.
   - `Right Controller Visual` (`XR Controller Right.prefab`) is **active**.
   - These are the standard XR Interaction Toolkit generic plastic controllers.
3. **Current Hand Models**:
   - `Left Hand Model` is **absent**.
   - `Right Hand Model` is **absent**.
   - `AnimateHandOnInput.cs` is **not present**.
4. **Current Interactors**:
   - `Near-Far Interactor` (`XRNearFarInteractor` + `XRRayInteractor` + `XRDirectInteractor`) is active on both controllers.
   - `Poke Interactor` (`XRPokeInteractor`) is active on both controllers.
   - `Teleport Interactor` is disabled (`m_IsActive: 0`).
   - `Move`, `Teleportation`, `Snap Turn`, and `Turn` locomotion providers are disabled (`m_IsActive: 0`).
   - `TrackedDeviceGraphicRaycaster` on `VR_MainMenu_Canvas` listens to pointer events from the `XRRayInteractor` / `Near-Far Interactor`.

---

## 5. Step-by-Step Implementation Recommendations for 100% Parity

There are two viable execution paths. **Path A** is strongly recommended as it guarantees architectural unity across all project scenes:

### Path A (Recommended): Switch Prefab Source to Match Room & Tutorial Scenes
Because `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` is already the unified project rig:
1. In `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`, update PrefabInstance `1935650666`:
   - Change `m_SourcePrefab` to `{fileID: 100100000, guid: a2a0d931eebd6a1479e56fc82c3b78db, type: 3}`.
   - In `m_Modifications`, update target GUID from `f6336ac4ac8b4d34bc5072418cdc62a0` to `a2a0d931eebd6a1479e56fc82c3b78db`.
2. Ensure the modifications retain the disabled locomotion state for the menu:
   - `1319746308` (Left Teleport Interactor) -> `m_IsActive = 0`
   - `2449787133337329437` (Right Teleport Interactor) -> `m_IsActive = 0`
   - `58445280694286476` (Move) -> `m_IsActive = 0`
   - `2190828208922718286` (Teleportation) -> `m_IsActive = 0`
   - `2626757739553014894` (Snap Turn) -> `m_IsActive = 0`
   - `3904471525139099824` (Turn) -> `m_IsActive = 0`
   - `1470279098769358945` (ContinuousTurn) -> `m_IsActive = 0`
3. Result:
   - Oculus hands automatically instantiate with exact offsets.
   - Generic controller models are already disabled in the prefab.
   - Ray and poke interactors continue working seamlessly with `VR_MainMenu_Canvas`.
   - Zero duplicated or divergent rig definitions.

### Path B (Alternative): Direct Hierarchy Attachment in `Mainmenu scene.unity`
If done via Unity Editor / Editor script without re-pointing the prefab:
1. Under `XR Origin (XR Rig) -> Camera Offset -> Left Controller`:
   - Set `Left Controller Visual` GameObject to inactive (`gameObject.SetActive(false)`).
   - Instantiate `Assets/Oculus Hands/Prefabs/Left Hand Model.prefab` as a child.
   - Set `transform.localPosition = new Vector3(-0.001f, 0.001f, -0.035f)`.
   - Set `transform.localRotation = Quaternion.Euler(0f, 0f, 90f)`.
   - Set `transform.localScale = Vector3.one`.
2. Under `XR Origin (XR Rig) -> Camera Offset -> Right Controller`:
   - Set `Right Controller Visual` GameObject to inactive (`gameObject.SetActive(false)`).
   - Instantiate `Assets/Oculus Hands/Prefabs/Right Hand Model.prefab` as a child.
   - Set `transform.localPosition = new Vector3(-0.001f, 0.001f, -0.035f)`.
   - Set `transform.localRotation = Quaternion.Euler(0f, 0f, -90f)`.
   - Set `transform.localScale = Vector3.one`.
3. Verify that `AnimateHandOnInput` is present and enabled on both hands.
4. Save scene cleanly.

---

## 6. Audit Verification Criteria for Implementer
An automated Play Mode audit should verify:
1. `GameObject.Find("Left Hand Model")` is active in hierarchy and parented under `Left Controller`.
2. `GameObject.Find("Right Hand Model")` is active in hierarchy and parented under `Right Controller`.
3. `Left Hand Model.GetComponent<AnimateHandOnInput>() != null` and `enabled == true`.
4. `Right Hand Model.GetComponent<AnimateHandOnInput>() != null` and `enabled == true`.
5. `Left Controller Visual` and `Right Controller Visual` are inactive (`activeSelf == false`).
6. `Left Hand Model` localPosition is approximately `(-0.001, 0.001, -0.035)` and localEulerAngles Z is approximately `90`.
7. `Right Hand Model` localPosition is approximately `(-0.001, 0.001, -0.035)` and localEulerAngles Z is approximately `270` (-90).
8. Ray interactors on both controllers remain active and can raycast against `VR_MainMenu_Canvas`.
