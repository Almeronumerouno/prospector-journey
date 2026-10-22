## 2026-10-13T10:32:01Z
You are teamwork_preview_explorer (Survey Explorer 3: Interactive Logic, Options Persistence, Badge & Test Harness).
Your working directory is: c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_3\
Project root: c:\CodeKuliah\prospector-journey

MANDATORY FIRST STEP:
Read `c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md` completely, focusing on the latest section `## 2026-10-13T10:27:16Z`.

OBJECTIVE:
Investigate Main Menu interactive logic, Game Options cycling and PlayerPrefs persistence, the Start Expedition button + "COMING SOON" badge design, and automated Play Mode test architecture.

TASKS TO INVESTIGATE:
1. Find and inspect the scripts driving the Main Menu UI (e.g. `MainMenuManager.cs`, `MenuController.cs`, or whatever scripts exist in `Assets/` or scene).
2. Trace the logic for Game Options: Turn Type (`SNAP` <-> `CONTINUOUS`) and Locomotion Type (`TELEPORT` <-> `CONTINUOUS`), how values are displayed, cycled, and saved to `PlayerPrefs` on `SAVE AND EXIT`.
3. Inspect `Btn_StartExpedition`. Determine where and how to attach the "COMING SOON" badge (e.g. child GameObject with Image + TMP_Text pill badge, gold/amber color styling, non-blocking / disabling expedition launch).
4. Inspect existing automated Play Mode test harnesses in the project (e.g., `TutorialPlayModeAuditRunner.cs`, `RoomPlayModeAuditRunner.cs`, or Editor test runners) to understand how automated Play Mode tests are structured, executed, and validated via Unity command line or editor batch scripts with zero console errors.
5. Formulate the verification checklist and test structure for the new Main Menu requirements (translucency verification, text size assertions >= 36pt, hand model presence & `AnimateHandOnInput`, COMING SOON badge, options cycling & persistence).

BOUNDARIES:
- READ-ONLY exploration. DO NOT modify any code, scenes, or assets.
- Write your comprehensive findings to `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_3\report.md` and `handoff.md`.
- Send a summary message back to parent when done.
