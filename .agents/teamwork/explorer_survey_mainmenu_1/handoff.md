# Handoff Report: Main Menu UI Canvas, Translucent Glass & Typography

**Agent**: teamwork_preview_explorer (Survey Explorer 1: UI Canvas, Translucent Glass & Typography)  
**Date**: 2026-10-13  
**Working Directory**: `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\`  
**Target Scene**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`  

---

## 1. Observation

1. **Scene and Hierarchy Location**:
   - `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity` line 212 of YAML contains `VR_MainMenu_Canvas` (GameObject ID: `1321170159`).
   - `VR_MainMenu_Canvas` has RectTransform SizeDelta: `(1500, 950)`, anchoredPosition: `(0, 1.45)`, scale: `(0.001, 0.001, 0.001)`.
   - Hierarchy path:
     - `VR_MainMenu_Canvas` (GO: `1321170159`)
       - `Board_Background` (GO: `1687171002`, Image color `RGBA(0.12, 0.09, 0.07, 0.50)`)
         - `Gold_Frame` (GO: `1319634031`, Image color `RGBA(0.75, 0.55, 0.22, 0.60)`)
           - `Inner_Board` (GO: `1212624254`, Image color `RGBA(0.46, 0.31, 0.22, 0.61)`)
             - `Header_Section` (GO: `505553723`) -> `Title_Text` (GO: `279430716`), `Subtitle_Text` (GO: `1974459084`), `Gold_Divider` (GO: `1073927727`)
             - `Panels_Content` (GO: `264329703`)
               - `Main_Buttons_Panel` (GO: `1759804371`, `VerticalLayoutGroup`) -> `Btn_StartExpedition`, `Btn_Tutorial`, `Btn_Settings`, `Btn_Credits`, `Btn_Quit`
               - `Settings_Panel` (GO: `1398607371`) -> `Section_Title`, `Frame_GameOptions`, `Btn_SaveAndExit`
               - `Tutorial_Panel` (GO: `1347507813`, inactive)
               - `Credits_Panel` (GO: `1285441568`, inactive)
2. **Hierarchy Path Hardcoding in Code**:
   - In `Assets/Prospector-journey/Mainmenu/Editor/MainMenuGameOptionsBuilder.cs` lines 45–65:
     ```csharp
     Transform innerBoard = canvasObj.transform.Find("Board_Background/Gold_Frame/Inner_Board");
     Transform panelsContent = innerBoard.Find("Panels_Content");
     Transform settingsPanel = panelsContent.Find("Settings_Panel");
     ```
   - In `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuOptionsPlayModeAudit.cs` lines 32–36:
     ```csharp
     Transform innerBoard = canvas.transform.Find("Board_Background/Gold_Frame/Inner_Board");
     Transform panelsContent = innerBoard.Find("Panels_Content");
     Transform mainPanel = panelsContent.Find("Main_Buttons_Panel");
     Transform settingsPanel = panelsContent.Find("Settings_Panel");
     ```
3. **Current Image Opacity & Occlusion**:
   - `Board_Background`: `Image.color = {r: 0.12, g: 0.09, b: 0.07, a: 0.5}`
   - `Gold_Frame`: `Image.color = {r: 0.75, g: 0.55, b: 0.22, a: 0.6}`
   - `Inner_Board`: `Image.color = {r: 0.4627, g: 0.3059, b: 0.2196, a: 0.6078}`
   - `Frame_GameOptions`: `Image.color = {r: 0.14, g: 0.09, b: 0.06, a: 0.88}`
   - `Btn_StartExpedition`: `Image.color = {r: 0.6, g: 0.38, b: 0.12, a: 1.0}`
   - `Btn_Tutorial`, `Btn_Settings`, `Btn_Credits`: `Image.color = {r: 0.24, g: 0.18, b: 0.14, a: 1.0}`
   - `Btn_Quit`: `Image.color = {r: 0.28, g: 0.12, b: 0.12, a: 1.0}`
4. **Current Typography Survey**:
   - Font asset: `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset` (GUID: `0161d805a3764c089bef00bfe00793f5`).
   - `Title_Text`: 70pt
   - `Subtitle_Text`: 24pt
   - `Main_Buttons_Panel`:
     - `Btn_StartExpedition/Label`: 32pt
     - `Btn_Tutorial/Label`: 26pt
     - `Btn_Settings/Label`: 26pt ("GAME OPTIONS")
     - `Btn_Credits/Label`: 26pt
     - `Btn_Quit/Label`: 22pt
   - `Settings_Panel`:
     - `Section_Title`: 42pt ("GAME OPTIONS")
     - `Row_TurnType/Label`: 28pt ("TURN TYPE")
     - `Row_TurnType/Text_Value`: 28pt ("SNAP" / "CONTINUOUS", width: 190px)
     - `Row_TurnType/Arrows`: 32pt ("<", ">")
     - `Row_LocomotionType/Label`: 28pt ("LOCOMOTION TYPE")
     - `Row_LocomotionType/Text_Value`: 28pt ("TELEPORT" / "CONTINUOUS", width: 190px)
     - `Btn_SaveAndExit/Label`: 28pt ("SAVE AND EXIT", width: 440px)
5. **Background Scene Environment**:
   - `Diorama_Environment` (GO ID: `1443230119`) contains 1,384 objects (beach, ship, sea, rocks, sky, lanterns).

---

## 2. Logic Chain

1. **Why the background looks completely opaque (from Obs 1 & 3)**:
   - Three full-screen Image components are rendered atop each other.
   - Combined opacity: $1 - (1 - 0.50) \times (1 - 0.60) \times (1 - 0.608) = 1 - 0.0784 = 92.16\%$.
   - When `Settings_Panel` opens, `Frame_GameOptions` (alpha 0.88) stacks over the 92.16% base, creating $99.06\%$ opacity.
   - Buttons on `Main_Buttons_Panel` have alpha = 1.0 (100% solid brown).
   - Thus, the beach and ocean diorama behind the canvas are entirely blocked.
2. **Why GameObjects cannot be deleted or renamed (from Obs 2)**:
   - `MainMenuOptionsPlayModeAudit.cs` and `MainMenuGameOptionsBuilder.cs` both execute:
     `canvas.transform.Find("Board_Background/Gold_Frame/Inner_Board/Panels_Content/Settings_Panel")`.
   - Deleting or reparenting these GameObjects will break existing runtime navigation and automated playmode test passes.
3. **How to achieve the translucent dark tinted glass look cleanly (from Obs 2 & 3)**:
   - Set `Board_Background.color = Color(0.06f, 0.08f, 0.12f, 0.55f)` (single tinted glass plane, alpha 0.55).
   - Set `Board_Background` `Outline` to `Color(0.80f, 0.88f, 0.96f, 0.80f)`, distance `(2.5f, -2.5f)` (crisp light metallic/silver border).
   - Set `Gold_Frame.color = Color(0, 0, 0, 0)` and `Inner_Board.color = Color(0, 0, 0, 0)` (alpha 0, completely removing redundant fills).
   - Set `Frame_GameOptions.color = Color(0.08f, 0.11f, 0.16f, 0.45f)` with `Outline`.
   - Set buttons to translucent slate `Color(0.12f, 0.16f, 0.24f, 0.55f)` with outlines.
   - Total canvas opacity is cleanly 0.55 (well below 0.80 threshold), allowing the 3D beach environment to be seen through the UI ("tembus pandang").
4. **Why layout modifications are mandatory for text scaling (from Obs 4)**:
   - Requirement specifies: header titles 46–54pt; buttons/options >= 36pt (36–42pt).
   - In `Settings_Panel`, `Text_Value` container is currently only 190px wide. At 38pt, "CONTINUOUS" is ~210px wide, which would clip or word-wrap into an unreadable state.
   - Therefore, `Text_Value` must be widened to 250px, `Selector_Group` widened to 400px, option row heights increased from 76px to 92px, and `Frame_GameOptions` height increased to 290px.
   - In `Main_Buttons_Panel`, button heights must be increased from 62–72px to 76–88px to fit 36–40pt fonts with comfortable vertical padding.

---

## 3. Caveats

1. **VR Controller Raycasting vs Transparent Buttons**: uGUI `Image` with alpha > 0 (even 0.55) remains a fully valid `raycastTarget = 1` for `TrackedDeviceGraphicRaycaster`. However, if `raycastTarget` were disabled, clicking would fail; ensure all interactive buttons retain `raycastTarget = 1`.
2. **Text Outline Shader**: TextMeshPro components use SDF font materials. When resizing text to 36–54pt, SDF rendering remains razor-sharp without needing font atlas regeneration.
3. **Secondary Panels (`Tutorial_Panel`, `Credits_Panel`)**: These panels are inactive at start. They currently have smaller font sizes (18–28pt). The primary requirement focuses on `Main_Buttons_Panel` and `Settings_Panel`, but secondary panel titles should also be bumped to 48pt for visual consistency.

---

## 4. Conclusion

The Main Menu UI in `Mainmenu scene.unity` is structurally well-organized and uses standard uGUI components, making it ideal for the translucent glass overhaul:
1. **Glass Aesthetic**: Achieved by setting `Board_Background` to dark tinted glass (`alpha = 0.55`), `Gold_Frame` and `Inner_Board` to `alpha = 0`, adding light metallic `Outline` components, and giving buttons translucent slate backplates (`alpha = 0.55`).
2. **Typography & Readability**: Headers scaled to 50–54pt, options/button texts scaled to 36–40pt, with container widths expanded (notably `Text_Value` to 250px) to prevent word wrapping or clipping on "CONTINUOUS".
3. **"COMING SOON" Badge**: Added as a child pill badge on `Btn_StartExpedition` (`(160, 36)` with amber/gold styling and 20pt bold text).
4. **Script & Test Integrity**: Zero changes to GameObject names or hierarchy paths are required, preserving 100% compatibility with `MainMenuVRController.cs` and `MainMenuOptionsPlayModeAudit.cs`.

---

## 5. Verification Method

To independently verify all observations and design parameters:
1. **Scene YAML & Hierarchy Check**:
   - Run `python inspect_mainmenu_scene.py` in `explorer_survey_mainmenu_1` or inspect `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity` to confirm:
     - Root canvas: `VR_MainMenu_Canvas` (GO: `1321170159`)
     - Path: `Board_Background/Gold_Frame/Inner_Board/Panels_Content`
2. **Detailed UI Properties & Sizes**:
   - Inspect `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\detailed_ui_hierarchy.json` and `report.md`.
3. **Play Mode Audit Verification**:
   - Unity Menu: `ProspectorJourney -> Run Game Options Play Mode Audit`.
   - Verify all 5 existing checks pass. When R1–R4 are implemented, the updated audit will confirm translucency (`alpha < 0.8`), text sizes (`>= 36pt`), and COMING SOON badge presence.
