# Project: Main Menu Translucent UI Overhaul, VR Hands Parity & Play Mode Audit

## Architecture
- Target Project: Prospector Journey (Unity 6 / 2022.3+ VR with XR Interaction Toolkit 3.5.1, New Input System, URP)
- Target Scene: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
- Reference Scenes: `Assets/Prospector-journey/Room/Room scene.unity`, `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`
- Reference Prefabs:
  - Custom XR Rig: `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (`guid: a2a0d931eebd6a1479e56fc82c3b78db`)
  - Left Hand Model: `Assets/Oculus Hands/Prefabs/Left Hand Model.prefab` (`guid: 65847589f926b734d9eaed6e25f0046b`)
  - Right Hand Model: `Assets/Oculus Hands/Prefabs/Right Hand Model.prefab` (`guid: 1fd3a7c43c8190f4d8c167ebde694364`)
- Core Modules:
  1. Translucent UI Floating Frame & Scaled VR Typography: Dark tinted glass background (alpha 0.55), metallic outline border, removal of opaque occluders (`Gold_Frame` & `Inner_Board` alpha 0), headers 50–54pt, option/button labels 36–40pt, widened value selectors (250px).
  2. Start Expedition "COMING SOON" Badge: Child pill badge on `Btn_StartExpedition` (amber/gold, 20pt bold, raycastTarget false), click handler feedback without throwing missing scene exceptions.
  3. Oculus Hand Rig Parity: Replace Starter Assets XR Origin with project's unified `XR Origin (XR Rig).prefab` (or active Oculus hands + disabled controller visuals, local offsets `pos: (-0.001, 0.001, -0.035)`, `rot: (0, 0, 90)` / `(0, 0, -90)`), with `AnimateHandOnInput.cs` bound to grip & trigger actions.
  4. Options Cycling & Persistence: Maintain 100% functional Turn Type (`SNAP` <-> `CONTINUOUS`) and Locomotion Type (`TELEPORT` <-> `CONTINUOUS`) cycling, saving to `PlayerPrefs` (`VR_TurnType`, `VR_LocomotionType`, `VR_SnapTurn`) on `SAVE AND EXIT`.
  5. Automated Play Mode Test & Audit Suite: `MainMenuPlayModeAudit.cs` and `MainMenuPlayModeRunner.cs` asserting all 6 acceptance criteria programmatically in Play Mode with 0 console errors.

## Code Layout
- `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity` — Target Main Menu scene
- `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs` — Main menu interactive controller & options logic
- `Assets/Prospector-journey/Mainmenu/Editor/MainMenuGameOptionsBuilder.cs` — Editor script for constructing/styling options UI
- `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuPlayModeAudit.cs` — Automated Play Mode verification harness
- `Assets/Prospector-journey/Mainmenu/Editor/MainMenuPlayModeRunner.cs` — Editor Play Mode test runner menuItem

## Feature Inventory
| # | Feature | Description | Milestone | Source |
|---|---------|-------------|-----------|--------|
| 1 | Translucent Tinted Glass UI Background | Replace opaque wooden boards with dark tinted glass (alpha ~0.55) and metallic Outline border on `Board_Background`, set `Gold_Frame` & `Inner_Board` to alpha 0 | M1 | R1 |
| 2 | Preserved Hierarchy Compatibility | Keep `Board_Background/Gold_Frame/Inner_Board/Panels_Content` paths intact for existing controllers and audit scripts | M1 | R1 |
| 3 | Scaled VR Typography & Layout | Headers 50–54pt, options/button texts 36–40pt, widen `Text_Value` to 250px and `Selector_Group` to 400px, expand row heights to 92px and button heights to 76–88px | M1 | R1 |
| 4 | Options Cycling & PlayerPrefs Contract | Maintain Turn Type and Locomotion Type cycling and saving to `VR_TurnType`, `VR_LocomotionType`, `VR_SnapTurn` on SAVE AND EXIT | M1 | R1 |
| 5 | Start Expedition COMING SOON Badge | Add child `Badge_ComingSoon` to `Btn_StartExpedition` (RectTransform 160x36, amber pill, 20pt bold, raycastTarget false), non-crashing feedback | M1 | R3 |
| 6 | Oculus Hand Models Integration | Attach `Left Hand Model.prefab` and `Right Hand Model.prefab` with offsets `(-0.001, 0.001, -0.035)`, rot `90` / `-90`, scale `(1,1,1)` | M2 | R2 |
| 7 | Hand Animation via AnimateHandOnInput | Verify `AnimateHandOnInput.cs` is active and wired to `XRI Default Input Actions` (Trigger & Grip float values) | M2 | R2 |
| 8 | Controller Visuals Deactivation & Interactors | Deactivate `Left/Right Controller Visual`, ensure Ray/Poke interactors remain active while locomotion/teleport remain disabled | M2 | R2 |
| 9 | Automated Play Mode Verification Suite | Implement `MainMenuPlayModeAudit.cs` and `MainMenuPlayModeRunner.cs` testing all 6 criteria in Play Mode | M3 | R4 |
| 10| Full Acceptance Criteria Assertions | Assert translucency (alpha < 0.8), text sizes (>= 36pt), hand models + AnimateHandOnInput, COMING SOON badge, options cycling & persistence | M3 | R4 |
| 11| Independent Forensic & Victory Audit | Multi-agent gate verification (Reviewers, Challengers, Forensic Auditor) confirming 0 errors and authentic implementation | M4 | R4 |

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| M1 | Translucent Glass UI Overhaul, Scaled VR Typography & COMING SOON Badge | Update `Mainmenu scene.unity`, `MainMenuVRController.cs`, and `MainMenuGameOptionsBuilder.cs` for translucent tinted glass (alpha 0.55), metallic outline, scaled typography (headers 50-54pt, options/buttons 36-40pt), and child COMING SOON badge | none | PLANNED |
| M2 | VR Oculus Hand Models & Animation Integration | Update `XR Origin (XR Rig)` in `Mainmenu scene.unity` to use `Left Hand Model` & `Right Hand Model` with `AnimateHandOnInput.cs`, disable generic controller visuals, and maintain Ray/Poke interactors | M1 | PLANNED |
| M3 | Automated Play Mode Test Harness & Execution | Implement `MainMenuPlayModeAudit.cs` and `MainMenuPlayModeRunner.cs`, execute via Unity MCP in Play Mode, assert all 6 criteria pass with 0 errors | M2 | PLANNED |
| M4 | Final Gate Verification & Forensic Audit | Multi-reviewer, challenger stress test, and forensic integrity audit pass | M3 | PLANNED |

## Interface Contracts
### UI Hierarchy ↔ MainMenuVRController
- `Board_Background/Gold_Frame/Inner_Board/Panels_Content/Main_Buttons_Panel`:
  - `Btn_StartExpedition`, `Btn_Tutorial`, `Btn_Settings`, `Btn_Credits`, `Btn_Quit`
- `Panels_Content/Settings_Panel`:
  - `Section_Title` (TMP_Text)
  - `Frame_GameOptions/Row_TurnType`: `Text_Value` (TMP_Text), `Btn_TurnPrev`, `Btn_TurnNext`
  - `Frame_GameOptions/Row_LocomotionType`: `Text_Value` (TMP_Text), `Btn_LocoPrev`, `Btn_LocoNext`
  - `Btn_SaveAndExit`
- Persistence keys: `"VR_TurnType"`, `"VR_LocomotionType"`, `"VR_SnapTurn"`

### XR Origin ↔ Oculus Hands
- `Left Controller`: child `Left Hand Model` at local pos `(-0.001, 0.001, -0.035)`, Euler `(0, 0, 90)`, `AnimateHandOnInput` component.
- `Right Controller`: child `Right Hand Model` at local pos `(-0.001, 0.001, -0.035)`, Euler `(0, 0, -90)`, `AnimateHandOnInput` component.
- Controller Visuals: `Left Controller Visual` and `Right Controller Visual` `m_IsActive = 0`.
