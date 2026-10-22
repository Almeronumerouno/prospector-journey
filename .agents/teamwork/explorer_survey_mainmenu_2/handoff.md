# Handoff Report — VR Hand Models & Parity Survey
**Agent**: `teamwork_preview_explorer` (Survey Explorer 2)  
**Date**: 2026-10-13  
**Type**: Hard Handoff (Investigation Complete)

---

## 1. Observation
1. **Room Scene XR Origin**:
   - File: `Assets/Prospector-journey/Room/Room scene.unity`
   - Line 8847:
     ```yaml
     m_SourcePrefab: {fileID: 100100000, guid: a2a0d931eebd6a1479e56fc82c3b78db, type: 3}
     ```
   - No overrides or modifications exist on `Left Hand Model` or `Right Hand Model` (lines 8760–8846).

2. **Tutorial Scene XR Origin**:
   - File: `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`
   - Line 5810:
     ```yaml
     m_SourcePrefab: {fileID: 100100000, guid: a2a0d931eebd6a1479e56fc82c3b78db, type: 3}
     ```
   - No overrides or modifications exist on `Left Hand Model` or `Right Hand Model` (lines 5727–5809).

3. **Unified Prefab Architecture**:
   - File: `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (`guid: a2a0d931eebd6a1479e56fc82c3b78db`)
   - Left Controller (`Transform: 202364688`):
     - Line 2073: `Left Controller Visual` (`guid: e99967c67ba19b14fa1224bd0eb55157`) property `m_IsActive: 0` (disabled).
     - Line 2241: PrefabInstance `933366740352012232` parented under `202364688`, source `m_SourcePrefab: {fileID: 100100000, guid: 65847589f926b734d9eaed6e25f0046b, type: 3}` (`Left Hand Model.prefab`).
     - Lines 2248-2286:
       - `m_LocalPosition`: `(-0.001, 0.001, -0.035)`
       - `m_LocalRotation`: `{x: 0, y: 0, z: 0.7071068, w: 0.7071068}`
       - `m_LocalEulerAnglesHint`: `{x: 0, y: 0, z: 90}`
       - `m_LocalScale`: `(1, 1, 1)`
   - Right Controller (`Transform: 1670256625`):
     - Line 3021: `Right Controller Visual` (`guid: c41d667b8ddcd024e95a87014b0ee01b`) property `m_IsActive: 0` (disabled).
     - Line 3129: PrefabInstance `8780394880458172059` parented under `1670256625`, source `m_SourcePrefab: {fileID: 100100000, guid: 1fd3a7c43c8190f4d8c167ebde694364, type: 3}` (`Right Hand Model.prefab`).
     - Lines 3131-3170:
       - `m_LocalPosition`: `(-0.001, 0.001, -0.035)`
       - `m_LocalRotation`: `{x: 0, y: 0, z: -0.7071068, w: 0.7071068}`
       - `m_LocalEulerAnglesHint`: `{x: 0, y: 0, z: -90}`
       - `m_LocalScale`: `(1, 1, 1)`

4. **Hand Prefabs & Animation Components**:
   - `Assets/Oculus Hands/Prefabs/Left Hand Model.prefab` (`guid: 65847589f926b734d9eaed6e25f0046b`):
     - Line 366: Animator controller `guid: 4b5efa0c69d901449a38989140500c0d` (`Left Hand Animator.controller`).
     - Line 389: `m_EditorClassIdentifier: Assembly-CSharp::AnimateHandOnInput`
     - Line 402: `triggerValue` -> `{fileID: -8654792680252043297, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Left Interaction / Activate Value`).
     - Line 415: `gripValue` -> `{fileID: -7989615598897241686, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Left Interaction / Select Value`).
   - `Assets/Oculus Hands/Prefabs/Right Hand Model.prefab` (`guid: 1fd3a7c43c8190f4d8c167ebde694364`):
     - Line 932: Animator controller `guid: 3338dde1ae80a3b43826deca5c238356` (`Right Hand Animator.controller`).
     - Line 955: `m_EditorClassIdentifier: Assembly-CSharp::AnimateHandOnInput`
     - Line 968: `triggerValue` -> `{fileID: -3908803764926266157, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Right Interaction / Activate Value`).
     - Line 981: `gripValue` -> `{fileID: 2356269739022366052, guid: c348712bda248c246b8c49b3db54643f, type: 3}` (`XRI Right Interaction / Select Value`).
   - `Assets/Oculus Hands/AnimateHandOnInput.cs`:
     - Lines 48-49: `handAnimator.SetFloat("Trigger", trigger);` and `handAnimator.SetFloat("Grip", grip);`.
     - 2D Blend Tree (BlendType 3) parameters: `"Grip"` (X axis, 0-1) and `"Trigger"` (Y axis, 0-1).

5. **Current State in Mainmenu Scene**:
   - File: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
   - Lines 11551-11631:
     ```yaml
     m_SourcePrefab: {fileID: 100100000, guid: f6336ac4ac8b4d34bc5072418cdc62a0, type: 3}
     ```
   - Searches for `Left Hand Model`, `Right Hand Model`, `65847589f926b734d9eaed6e25f0046b`, and `1fd3a7c43c8190f4d8c167ebde694364` return 0 matches.
   - `Left Controller Visual` and `Right Controller Visual` are currently active.

---

## 2. Logic Chain
1. From **Observation 1 & 2**, both `Room scene` and `Tutorial scene` instantiate the exact same custom prefab `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (`guid: a2a0d931eebd6a1479e56fc82c3b78db`).
2. From **Observation 3**, that custom prefab already has:
   - Generic controller models disabled (`m_IsActive: 0`).
   - Animated Oculus hands attached with exact local position `(-0.001, 0.001, -0.035)`, scale `(1, 1, 1)`, and rotation `+90°` (Left) / `-90°` (Right) on the Z axis.
3. From **Observation 4**, the Oculus hand prefabs already bundle `AnimateHandOnInput.cs` wired to the standard XRI input actions (`c348712bda248c246b8c49b3db54643f`) for both Trigger and Grip float values.
4. From **Observation 5**, `Mainmenu scene.unity` diverges solely because it still points to the raw Unity starter assets prefab (`f6336ac4ac8b4d34bc5072418cdc62a0`) instead of the project's unified rig prefab.
5. Therefore, switching `Mainmenu scene.unity`'s XR Origin prefab source to `a2a0d931eebd6a1479e56fc82c3b78db` (or instantiating the hand prefabs and disabling the controller visuals) immediately achieves 100% parity across all three scenes with zero code duplication.

---

## 3. Caveats
- `Mainmenu scene.unity` requires locomotion and teleportation interactors to remain disabled (`m_IsActive: 0`) so the player cannot accidentally walk or teleport away while interacting with the menu UI.
- `Near-Far Interactor` (`XRRayInteractor` and `XRPokeInteractor`) must remain active on both hands to allow pointer raycasting against `VR_MainMenu_Canvas`.
- No source code or scenes were modified during this investigation (strict read-only).

---

## 4. Conclusion
100% VR hand model and animation parity in `Mainmenu scene.unity` requires:
1. Disabling `Left Controller Visual` and `Right Controller Visual` (`m_IsActive = 0`).
2. Attaching `Left Hand Model.prefab` under `Left Controller` with localPos `(-0.001, 0.001, -0.035)` and localEuler `(0, 0, 90)`.
3. Attaching `Right Hand Model.prefab` under `Right Controller` with localPos `(-0.001, 0.001, -0.035)` and localEuler `(0, 0, -90)`.
4. Ensuring `AnimateHandOnInput.cs` is enabled on both hands.
5. The cleanest architectural method to achieve this is updating `Mainmenu scene.unity`'s PrefabInstance `1935650666` `m_SourcePrefab` to `{fileID: 100100000, guid: a2a0d931eebd6a1479e56fc82c3b78db, type: 3}`, matching Room and Tutorial scenes 1:1.

---

## 5. Verification Method
1. **Scene YAML Inspection**:
   ```powershell
   Select-String -Path "Assets\Prospector-journey\Mainmenu\Mainmenu scene.unity" -Pattern "Left Hand Model|Right Hand Model|AnimateHandOnInput|a2a0d931eebd6a1479e56fc82c3b78db"
   ```
2. **Unity Play Mode Hierarchy Verification**:
   In Play Mode on `Mainmenu scene.unity`:
   - `GameObject.Find("Left Hand Model")` is active and has component `AnimateHandOnInput`.
   - `GameObject.Find("Right Hand Model")` is active and has component `AnimateHandOnInput`.
   - `GameObject.Find("Left Controller Visual")` and `GameObject.Find("Right Controller Visual")` are inactive.
3. **Console Stability**:
   Ensure zero errors in Unity Editor console (`Unity_GetConsoleLogs` or console window).
