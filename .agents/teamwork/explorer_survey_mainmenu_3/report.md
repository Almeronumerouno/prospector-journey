# Main Menu Interactive Logic, Options Persistence, Badge & Test Harness Survey Report

**Survey Explorer**: Survey Explorer 3 (Interactive Logic, Options Persistence, Badge & Test Harness)  
**Date**: 2026-10-13  
**Working Directory**: `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_3`  
**Target Scene**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`  
**Status**: READ-ONLY INVESTIGATION COMPLETE  

---

## Executive Summary

This investigation analyzed the script architecture driving the Main Menu UI, the exact logic flow for Game Options cycling and `PlayerPrefs` persistence, the structure and constraints for adding the "COMING SOON" badge to `Btn_StartExpedition`, existing automated Play Mode test patterns across the project, and the test specification needed to assert all criteria in Milestone M1/M2/M3.

---

## 1. Main Menu Script Architecture & Control Flow (Task 1)

### 1.1 Key Driving Scripts
1. **`Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs`**:
   - Primary controller component attached to `VR_MainMenu_Canvas` (GameObject ID: 1321170159).
   - Manages:
     - **Panel Navigation**: `ShowPanel(GameObject)` toggles `mainPanel` (`Main_Buttons_Panel`), `settingsPanel` (`Settings_Panel`), `tutorialPanel` (`Tutorial_Panel`), and `creditsPanel` (`Credits_Panel`).
     - **Button Runtime Binding**: `BindButtonsRuntime()` automatically searches child buttons by name (`Btn_StartExpedition`, `Btn_Tutorial`, `Btn_Settings`, `Btn_Credits`, `Btn_Quit`, `Btn_SaveAndExit`, `Btn_TurnPrev`, `Btn_TurnNext`, `Btn_LocoPrev`, `Btn_LocoNext`, etc.) and binds `UnityAction` callbacks using `BindClick()`.
     - **Audio Feedback**: Contains `AudioSource` references for SFX (`hoverClip`, `clickClip`) and stereo background music (`BGM_MusicPlayer`, `bgmVolume = 0.45f`).
     - **VR Lazy Follow**: `LateUpdate()` provides smooth horizontal yaw alignment (deadzone: 18°, catch-up speed: 4.0, distance: 1.85m) and gentle pitch tilt clamped to ±12° so the floating canvas stays perpendicular to the player's gaze at eye level without flipping flat.
     - **Moving Detection**: Exposes `public bool IsMenuMoving { get; private set; }` to suppress hover sound spam while the canvas is floating/re-orienting.

2. **`Assets/Prospector-journey/Mainmenu/Scripts/VRButtonAudioFeedback.cs`**:
   - Attached to individual interactive UI buttons (`Selectable`).
   - Implements `IPointerEnterHandler` and `IPointerClickHandler`.
   - Debounces pointer hover events (min interval: 0.12s) and checks `controller.IsMenuMoving` before playing audio to prevent audio stutter during player head rotations.

3. **`Assets/Prospector-journey/Mainmenu/Editor/MainMenuGameOptionsBuilder.cs`**:
   - Editor tool script previously used to generate the Game Options sub-panel.
   - Sets up fonts, containers (`Frame_GameOptions`, `Row_TurnType`, `Row_LocomotionType`), and connects serialized properties on `MainMenuVRController`.

---

## 2. Game Options Cycling & PlayerPrefs Persistence Logic (Task 2)

### 2.1 State Representation & Enums
Defined in `MainMenuVRController.cs` (lines 54–59):
```csharp
public enum TurnTypeOption { Snap = 0, Continuous = 1 }
public enum LocomotionTypeOption { Teleport = 0, Continuous = 1 }

private TurnTypeOption currentTurnType = TurnTypeOption.Snap;
private LocomotionTypeOption currentLocomotionType = LocomotionTypeOption.Teleport;
```

### 2.2 Cycling Logic
Located in `MainMenuVRController.cs` (lines 231–251):
```csharp
private void CycleTurnType(int dir)
{
    int total = 2; // Snap (0), Continuous (1)
    int val = (int)currentTurnType + dir;
    if (val < 0) val = total - 1;
    else if (val >= total) val = 0;
    currentTurnType = (TurnTypeOption)val;
    UpdateOptionsUI();
    PlayClickSound();
}

private void CycleLocomotionType(int dir)
{
    int total = 2; // Teleport (0), Continuous (1)
    int val = (int)currentLocomotionType + dir;
    if (val < 0) val = total - 1;
    else if (val >= total) val = 0;
    currentLocomotionType = (LocomotionTypeOption)val;
    UpdateOptionsUI();
    PlayClickSound();
}
```

### 2.3 Display Text Formatting
Located in `MainMenuVRController.cs` (lines 265–276):
- Turn Type:
  - If `Snap` (0) -> `"SNAP"`
  - If `Continuous` (1) -> `"CONTINUOUS"`
- Locomotion Type:
  - If `Teleport` (0) -> `"TELEPORT"`
  - If `Continuous` (1) -> `"CONTINUOUS"`

### 2.4 Persistence Specification (`PlayerPrefs`)
Executed when clicking `Btn_SaveAndExit` via `OnSaveAndExitSettings()` (lines 253–264):
```csharp
PlayerPrefs.SetInt("VR_TurnType", (int)currentTurnType);
PlayerPrefs.SetInt("VR_LocomotionType", (int)currentLocomotionType);
PlayerPrefs.SetInt("VR_SnapTurn", currentTurnType == TurnTypeOption.Snap ? 1 : 0);
PlayerPrefs.Save();
```

| Key | Type | Possible Values | Meaning |
|---|---|---|---|
| `"VR_TurnType"` | `int` | `0` or `1` | `0` = Snap Turn, `1` = Continuous Turn |
| `"VR_LocomotionType"` | `int` | `0` or `1` | `0` = Teleportation, `1` = Continuous Move |
| `"VR_SnapTurn"` | `int` | `1` or `0` | Legacy compatibility key (`1` = Snap, `0` = Continuous) |

### 2.5 Cross-Scene Consumption (`Room` & `Tutorial` Scenes)
In `Assets/Prospector-journey/Room/Scripts/VRSettingsApplier.cs` (lines 26–65):
- `VRSettingsApplier.ApplySettingsToRig(GameObject)` executes at `Awake` and `Start` (`DefaultExecutionOrder(-100)`).
- It reads:
  ```csharp
  int turnType = PlayerPrefs.GetInt("VR_TurnType", PlayerPrefs.GetInt("VR_SnapTurn", 1) == 1 ? 0 : 1);
  int locoType = PlayerPrefs.GetInt("VR_LocomotionType", 0);
  ```
- It automatically locates `Locomotion/SnapTurn`, `Locomotion/ContinuousTurn`, `Locomotion/ContinuousMove`, and `Locomotion/Teleportation` on `XR Origin (XR Rig)` and toggles their active state.
- **Conclusion**: The persistence keys and data contract are 100% synchronized with the downstream gameplay scenes.

---

## 3. Start Expedition Button & "COMING SOON" Badge Design (Task 3)

### 3.1 Current Hierarchy & Constraints
In `Mainmenu scene.unity`:
```text
VR_MainMenu_Canvas
└── Board_Background
    └── Gold_Frame
        └── Inner_Board
            └── Panels_Content
                └── Main_Buttons_Panel (has VerticalLayoutGroup!)
                    ├── Btn_StartExpedition (RectTransform: 620 x 85)
                    │   └── Label (TMP_Text: "START EXPEDITION", fontSize: 32)
                    ├── Btn_Tutorial (RectTransform: 620 x 72)
                    ├── Btn_Settings (RectTransform: 620 x 72)
                    ├── Btn_Credits (RectTransform: 620 x 72)
                    └── Btn_Quit (RectTransform: 620 x 72)
```

**Crucial Constraint**:
- `Main_Buttons_Panel` contains a `VerticalLayoutGroup` (`padding`: Top=30, Bottom=20, `spacing`: 22).
- **If the badge is added as a sibling in `Main_Buttons_Panel`, it will be treated as an independent menu row, disrupting button layout and alignment.**
- **Therefore, the "COMING SOON" badge MUST be attached as a direct child GameObject of `Btn_StartExpedition` (e.g. `Btn_StartExpedition/Badge_ComingSoon`).**

### 3.2 Recommended Badge Structure & Visual Specification
- **GameObject Name**: `Badge_ComingSoon`
- **Parent**: `Btn_StartExpedition.transform`
- **RectTransform Configuration**:
  - `anchorMin`: `(1.0, 0.5)`
  - `anchorMax`: `(1.0, 0.5)`
  - `pivot`: `(1.0, 0.5)`
  - `anchoredPosition`: `(-20, 0)` (or `(-16, 12)` for a corner pill badge)
  - `sizeDelta`: `(148, 32)`
- **Background `Image`**:
  - Pill background color: Amber Gold `Color(0.92f, 0.68f, 0.18f, 0.95f)` or dark amber `Color(0.20f, 0.14f, 0.08f, 0.88f)`.
  - Border: `Outline` component with `effectColor = Color(1.0f, 0.82f, 0.35f, 1f)`, `effectDistance = (2f, -2f)`.
  - `raycastTarget = false` (critical so ray interactor clicks pass through to `Btn_StartExpedition`).
- **Badge Text (`TextMeshProUGUI`)**:
  - Font: `Oswald Bold SDF`
  - Text: `"COMING SOON"`
  - `fontSize`: `18`–`20` pt (Bold)
  - Alignment: `Center`
  - Character Spacing: `2f`
  - Color: `Color(0.12f, 0.08f, 0.04f, 1f)` (dark text on bright gold) or `Color(1f, 0.92f, 0.65f, 1f)` (gold on dark pill).
  - `raycastTarget = false`.
- **Button Label Adjustment**:
  - `Btn_StartExpedition/Label`:
  - Enlarge font size to >= 36pt (e.g. 38pt).
  - Adjust anchored position or padding so "START EXPEDITION" text does not overlap the right-aligned badge pill.

### 3.3 Non-Blocking / Expedition Launch Behavior
In `MainMenuVRController.cs`:
- Currently, `OnClickStartExpedition()` invokes `SceneManager.LoadSceneAsync(mainGameScene)` (`"Village"`).
- In Unity Console logs, loading `"Village"` when the simulator is active triggers a `NullReferenceException` in `XRInteractionSimulator` because controller instances are missing in `"Village"`.
- Under Requirement R3:
  - "Keep the button responsive or disable expedition launch while clearly showing that the mode is not yet available."
  - Recommended implementation: `OnClickStartExpedition()` plays click sound, logs `"[Prospector Journey] Expedition mode is COMING SOON - scene launch disabled."`, but **does NOT** trigger scene loading. This keeps the button responsive and feedback-rich without crashing the simulator or loading an unfinished scene.

---

## 4. Existing Automated Play Mode Test Architecture (Task 4)

### 4.1 Proven Test Runner Architecture
The project employs a standardized 2-class Play Mode test runner pattern:

```
[Editor Script]                             [Runtime Component]
MainMenuOptionsAuditRunner.cs       <----->  MainMenuOptionsPlayModeAudit.cs
(in Editor/ folder)                         (in Scripts/ folder)
- [MenuItem] entry point                    - [DefaultExecutionOrder(30000)]
- EditorPrefs flag set                      - Hooks Application.logMessageReceived
- EditorApplication.isPlaying = true        - Multi-frame coroutine
- Listens to playModeStateChanged           - Step-by-step assertions
- Clean teardown & scene save in EditMode   - EditorApplication.isPlaying = false
```

### 4.2 Key Lessons from `TutorialPlayModeAuditRunner.cs`:
1. **Execution Order**: `[DefaultExecutionOrder(30000)]` ensures all scene Awake/Start methods finish before the test begins.
2. **Log Listener**: Traps exceptions and unexpected errors via:
   ```csharp
   Application.logMessageReceived += HandleLogMessage;
   ```
   Ensuring Check 6 (Zero Console Errors/Exceptions) is mathematically verified.
3. **Multi-Frame Coroutines**:
   - `yield return null;`
   - `yield return new WaitForFixedUpdate();`
   Allows UI LayoutGroups and Physics frames to settle before asserting rect bounds or hierarchy states.
4. **Clean Exit**:
   - Programmatically stops play mode via `#if UNITY_EDITOR UnityEditor.EditorApplication.isPlaying = false; #endif`.
   - The editor counterpart verifies `EditorPrefs.GetBool(AUDIT_PASSED_KEY)`, clears keys, and saves scene cleanly.
5. **Tool Execution**:
   - Executable directly from Editor menu item via Unity MCP `Unity_ManageMenuItem` (`MenuPath: "..."`).

---

## 5. Verification Checklist & Test Structure for New Requirements (Task 5)

An upgraded Play Mode test harness (e.g. `MainMenuPlayModeAudit.cs` and `MainMenuPlayModeAuditRunner.cs`) must assert all 6 checks:

### Check 1: Translucency ("Tembus Pandang") Verification
- [ ] Locate `Board_Background`, `Inner_Board`, and `Frame_GameOptions`.
- [ ] Assert that `Image.color.a` on background panels is strictly `< 0.80` (target: 0.50–0.65).
  - *Current scene observation*: `Frame_GameOptions` has `alpha = 0.88` (will fail until updated), and `Inner_Board` has opaque wood styling.
- [ ] Verify that panels allow the 3D beach/sky environment to be visible behind the UI.

### Check 2: Typography & Scaled VR Readability (Font Sizes >= 36pt)
- [ ] Assert all main interactive button labels have `m_fontSize >= 36`:
  - `Btn_StartExpedition/Label`: >= 36pt (currently 32pt)
  - `Btn_Tutorial/Label`: >= 36pt (currently 26pt)
  - `Btn_Settings/Label`: >= 36pt (currently 26pt)
- [ ] Assert all Game Options typography has `m_fontSize >= 36`:
  - `Row_TurnType/Label`: >= 36pt (currently 28pt)
  - `Row_TurnType/Selector_Group/Text_Value`: >= 36pt (currently 28pt)
  - `Row_LocomotionType/Label`: >= 36pt (currently 28pt)
  - `Row_LocomotionType/Selector_Group/Text_Value`: >= 36pt (currently 28pt)
  - `Btn_SaveAndExit/Label`: >= 36pt (currently 28pt)
- [ ] Assert header titles have `m_fontSize >= 46`:
  - `Section_Title` (GAME OPTIONS): >= 46pt (currently 42pt)
  - `Title_Text` (PROSPECTOR JOURNEY): >= 48pt (currently 70pt)

### Check 3: Hand Model Parity with Room & Tutorial Scenes
- [ ] Assert `Left Controller` has an active child named `"Left Hand Model"`.
- [ ] Assert `Right Controller` has an active child named `"Right Hand Model"`.
- [ ] Assert `AnimateHandOnInput` component is present and enabled on both hand models.
- [ ] Assert local position offsets match `(-0.001, 0.001, -0.035)` and Euler rotation matches `(0, 0, 90)` for Left, `(0, 0, -90)` for Right.
  - *Current scene observation*: `Mainmenu scene` currently instantiates Starter Assets XR Origin (`f6336ac4ac8b4d34bc5072418cdc62a0`) without Oculus Hand prefabs. Upgrading to `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (`a2a0d931eebd6a1479e56fc82c3b78db`) or attaching `Left Hand Model.prefab` / `Right Hand Model.prefab` will satisfy this check.

### Check 4: Start Expedition "COMING SOON" Badge
- [ ] Assert a child GameObject named `Badge_ComingSoon` exists under `Btn_StartExpedition` and is active in the hierarchy.
- [ ] Assert `Badge_ComingSoon` contains a `TextMeshProUGUI` component with text containing `"COMING SOON"`.
- [ ] Assert `Badge_ComingSoon` has an `Image` component with gold/amber styling.
- [ ] Assert clicking `Btn_StartExpedition` does not crash or leave the Main Menu.

### Check 5: Game Options Cycling & PlayerPrefs Persistence
- [ ] Programmatically invoke `Btn_Settings.onClick`: assert `Settings_Panel` becomes active and `Main_Buttons_Panel` becomes inactive.
- [ ] Assert Turn Type cycles (`SNAP` -> `CONTINUOUS` -> `SNAP`) on next/prev clicks.
- [ ] Assert Locomotion Type cycles (`TELEPORT` -> `CONTINUOUS` -> `TELEPORT`) on next/prev clicks.
- [ ] Set Turn to `CONTINUOUS` (1) and Locomotion to `CONTINUOUS` (1), invoke `Btn_SaveAndExit.onClick`:
  - Assert `Settings_Panel` closes and `Main_Buttons_Panel` restores.
  - Assert `PlayerPrefs.GetInt("VR_TurnType") == 1`.
  - Assert `PlayerPrefs.GetInt("VR_LocomotionType") == 1`.
  - Assert `PlayerPrefs.GetInt("VR_SnapTurn") == 0`.
- [ ] Reopen settings, restore to `SNAP` (0) and `TELEPORT` (0), save and exit:
  - Assert `PlayerPrefs.GetInt("VR_TurnType") == 0`.
  - Assert `PlayerPrefs.GetInt("VR_LocomotionType") == 0`.
  - Assert `PlayerPrefs.GetInt("VR_SnapTurn") == 1`.

### Check 6: Console Error & Runtime Exception Audit
- [ ] Verify `m_ErrorCount == 0` and `m_ExceptionCount == 0` across all execution frames via `Application.logMessageReceived`.
