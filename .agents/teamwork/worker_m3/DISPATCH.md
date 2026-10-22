## 2026-10-12T16:54:27Z
You are Worker M3 (Play Mode Test Engineer).
Working directory: c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m3
Read c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md, c:\CodeKuliah\prospector-journey\PROJECT.md, and:
- c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m1\handoff.md
- c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m2\handoff.md
first.

DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_preview_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Your exclusive files:
- Assets/Prospector-journey/Tutorial/Scripts/TutorialPlayModeAuditRunner.cs
- Assets/Prospector-journey/Tutorial/Editor/TutorialPlayModeRunner.cs

Task for Milestone M3:
1. Implement a comprehensive Play Mode test runner component `TutorialPlayModeAuditRunner.cs` in `Assets/Prospector-journey/Tutorial/Scripts/` (and Editor runner `TutorialPlayModeRunner.cs` with MenuItem "ProspectorJourney/Run Tutorial Play Mode Audit"):
   It must execute in Unity Play Mode on `Tutorial scene.unity` and programmatically assert all 5 criteria from Requirement R4:
   - Check 1: `PlayerBodyCollision` + `CharacterController` active on `XR Origin (XR Rig)`, `radius == 0.24`, `stepOffset == 0.08`, `skinWidth == 0.025`, 1:1 camera/body/hand sync, and zero Y-height shift when simulating E/Q rotation.
   - Check 2: Solid colliders active on trees, rocks, stumps, houses (`House_1`..`3`), fences, crates, and benches; foliage (`Grass`, `Flowers`, `Mushroom`, `Branch`) and held tools are ignored by body collision (`Physics.GetIgnoreCollision(m_CharacterController, col) == true`).
   - Check 3: `[Prospector_Inventory]` starts with `tool-shovel` and `Gold_Pan` in the bag (slots 0 and 1 occupied), bag opens/closes via `I`/`Tab`, retrieves tools into hand, and quick-stores held tools back via `C`/`X` with active `ToastNotification` display.
   - Check 4: `DoorSceneTransition` and `Door_Interaction_Prompt` in `Tutorial scene.unity` display status, block exit when required tools are missing, and allow exit when carrying `tool-shovel` + `Gold_Pan`.
   - Check 5: Zero compile errors and zero runtime exceptions in Unity Console.
2. Execute the Play Mode audit in Unity via Unity MCP (`Unity_ManageMenuItem` / `Unity_GetConsoleLogs`) or Editor API.
3. Verify that 100% of test assertions pass with zero console errors. Ensure Play Mode stops and Edit Mode scene is saved cleanly.
4. Document all test results, verbatim console output, and assertions in `c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m3\handoff.md`.
Send a message back when complete.
