## 2026-10-13T10:50:53Z

You are teamwork_preview_worker (Worker M1: Translucent Glass UI Overhaul, Scaled VR Typography & COMING SOON Badge).
Your working directory is: c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_mainmenu_m1\
Project root: c:\CodeKuliah\prospector-journey

MANDATORY FIRST STEP:
Read `c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md` completely, focusing on `## 2026-10-13T10:27:16Z`.
Also read:
- `c:\CodeKuliah\prospector-journey\.agents\teamwork\orchestrator_2\PROJECT.md`
- `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\handoff.md`
- `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_3\handoff.md`

MANDATORY INTEGRITY WARNING:
DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_preview_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

EXCLUSIVE WRITE OWNERSHIP:
You own and may edit:
- `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity` (UI Canvas section)
- `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs`
- `Assets/Prospector-journey/Mainmenu/Editor/MainMenuGameOptionsBuilder.cs` (or create a dedicated editor utility in `Assets/Prospector-journey/Mainmenu/Editor/` to cleanly modify the scene uGUI components via Unity MCP or C# script)

OBJECTIVES & REQUIREMENTS FOR MILESTONE 1:
1. Translucent Tinted Glass UI Overhaul:
   - On `VR_MainMenu_Canvas`:
     - Maintain exact hierarchy paths: `Board_Background/Gold_Frame/Inner_Board/Panels_Content/Settings_Panel` (do not delete or rename these GameObjects as existing code depends on them).
     - `Board_Background`: Set Image color to dark tinted glass `Color(0.06f, 0.08f, 0.12f, 0.55f)` (alpha = 0.55). Add or configure an `Outline` component with `Color(0.80f, 0.88f, 0.96f, 0.80f)`, distance `(2.5f, -2.5f)` for a crisp light metallic border.
     - `Gold_Frame` & `Inner_Board`: Set Image fill color to `Color(0f, 0f, 0f, 0f)` (alpha = 0) so they do not block the 3D beach/sky scene.
     - `Frame_GameOptions`: Set Image color to `Color(0.08f, 0.11f, 0.16f, 0.45f)` with light metallic `Outline`.
     - Main buttons on `Main_Buttons_Panel`: Set Image color to translucent slate `Color(0.12f, 0.16f, 0.24f, 0.55f)` with `Outline`, maintaining high contrast, raycastTarget = true, and hover/click feedback.
     - Total background opacity must be between 0.50 and 0.65 (under 0.80 audit threshold), leaving the ocean/beach scene clearly visible through the UI ("tembus pandang").

2. Scaled VR Typography & Layout Widening:
   - Enlarge Header Titles (`Title_Text`, `Section_Title` "GAME OPTIONS") to 50–54pt.
   - Enlarge options and button labels (`TURN TYPE`, `LOCOMOTION TYPE`, `< SNAP >`, `< TELEPORT >`, `SAVE AND EXIT`, main buttons `START EXPEDITION`, `TUTORIAL`, `GAME OPTIONS`, `CREDITS`, `QUIT`) to 36–40pt (minimum >= 36pt).
   - Layout adjustments to prevent clipping or text wrapping:
     - In `Settings_Panel`, widen `Text_Value` container from 190px to 250px, and `Selector_Group` to 400px so "CONTINUOUS" at 38pt does not clip or wrap.
     - Increase option rows height to ~90–92px and `Frame_GameOptions` height to ~290px.
     - Increase button heights on `Main_Buttons_Panel` from 62–72px to 76–88px to fit 36–40pt fonts with comfortable vertical padding.

3. Start Expedition "COMING SOON" Badge:
   - Under `Btn_StartExpedition`, add child GameObject `Badge_ComingSoon` (RectTransform ~160x36, anchored right, pivot right).
   - Add Image component styled as amber/gold pill `Color(0.95f, 0.65f, 0.15f, 0.88f)`, `raycastTarget = false`.
   - Add child TMP_Text with bold dark bronze text "COMING SOON" (font size ~18–20pt), `raycastTarget = false`.
   - In `MainMenuVRController.cs`, update `OnClickStartExpedition()` to show feedback/log indicating expedition mode is coming soon, rather than calling `LoadSceneAsync(mainGameScene)` which throws NullReferenceException when the scene is unavailable.

4. Options Cycling & Persistence:
   - Ensure `MainMenuVRController.cs` retains 100% functional cycling for Turn Type (`SNAP` <-> `CONTINUOUS`) and Locomotion Type (`TELEPORT` <-> `CONTINUOUS`), saving to `PlayerPrefs` (`VR_TurnType`, `VR_LocomotionType`, `VR_SnapTurn`) on `SAVE AND EXIT`.

5. Verification:
   - Run compilation check / Unity MCP commands (e.g. `Unity_GetConsoleLogs` or existing menu item `ProspectorJourney -> Run Game Options Play Mode Audit`).
   - Ensure ZERO compile errors and ZERO runtime exceptions.
   - Write comprehensive findings, commands executed, and before/after verification in `c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_mainmenu_m1\handoff.md`.
