# BRIEFING — 2026-10-13T10:52:00Z

## Mission
Deliver Milestone 1: Translucent Tinted Glass UI Overhaul, Scaled VR Typography & COMING SOON Badge for Main Menu Canvas.

## 🔒 My Identity
- Archetype: teamwork_preview_worker
- Roles: implementer, qa, specialist
- Working directory: c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_mainmenu_m1
- Original parent: ad1bed17-6e27-451b-9055-5f4b5ba65726
- Milestone: Milestone 1 (Translucent Glass UI Overhaul, Scaled VR Typography & COMING SOON Badge)

## 🔒 Key Constraints
- Maintain exact hierarchy paths: Board_Background/Gold_Frame/Inner_Board/Panels_Content/Settings_Panel
- Board_Background opacity between 0.50 and 0.65 (under 0.80 audit threshold)
- Gold_Frame & Inner_Board Image fill alpha = 0 (translucent glass effect to see 3D beach/sky scene)
- Typography: Header Titles 50-54pt, Options & Button labels 36-40pt
- Text_Value container widened to 250px, Selector_Group 400px, option row height 90-92px, Frame_GameOptions height ~290px
- Btn_StartExpedition badge: Badge_ComingSoon pill with "COMING SOON" text
- MainMenuVRController OnClickStartExpedition() prevents NullReferenceException when scene unavailable
- Options cycling & persistence preserved (PlayerPrefs: VR_TurnType, VR_LocomotionType, VR_SnapTurn)
- ZERO compile errors, ZERO runtime exceptions
- Ownership: Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity, MainMenuVRController.cs, MainMenuGameOptionsBuilder.cs (or dedicated editor script in Assets/Prospector-journey/Mainmenu/Editor/)

## Current Parent
- Conversation ID: ad1bed17-6e27-451b-9055-5f4b5ba65726
- Updated: not yet

## Task Summary
- **What to build**: Translucent tinted glass UI aesthetic for VR Main Menu Canvas, enlarged VR-readable typography, COMING SOON badge on Start Expedition, and robust start expedition handling.
- **Success criteria**: All styling, layout metrics, typography sizes, badge specs, and functional persistence pass verification with zero compile/runtime errors.
- **Interface contracts**: PROJECT.md
- **Code layout**: Unity UI uGUI + TextMeshPro, C# scripts in Assets/Prospector-journey/Mainmenu/

## Change Tracker
- **Files modified**: None yet
- **Build status**: Pending inspection
- **Pending issues**: None

## Quality Status
- **Build/test result**: Pending
- **Lint status**: Clean
- **Tests added/modified**: Pending

## Loaded Skills
- **Source**: C:\Users\almer\.gemini\config\skills\unity-official-ui-ugui\SKILL.md
- **Core methodology**: Unity uGUI canvas hierarchies, RectTransforms, Layout Groups, TextMeshPro styling and interaction.

## Key Decisions Made
- Will inspect required files before making any modifications.

## Artifact Index
- DISPATCH.md — Assignment instructions
- BRIEFING.md — Persistent context & situational awareness
