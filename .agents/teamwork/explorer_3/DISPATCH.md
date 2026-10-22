## 2026-10-22T11:57:35Z
You are Explorer 3 (Verification & Test Explorer).
Working directory: c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_3
Original user request path: c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md
Milestone scope path: c:\CodeKuliah\prospector-journey\.agents\teamwork\orchestrator_3\SCOPE.md

Your task is read-only technical exploration focusing on Requirement 4 (R4):
- Read ORIGINAL_REQUEST.md (specifically the latest request at timestamp 2026-10-22T11:53:31Z) and SCOPE.md.
- Inspect existing automated Play Mode verification scripts in the project (e.g. how previous audits for Room scene, Tutorial scene, or Mainmenu were written and executed, such as editor scripts in `Assets/Prospector-journey/Editor/` or test scripts).
- Formulate the architecture and exact implementation details for an automated Play Mode verification script in Unity that programmatically verifies:
  1. `VR_MainMenu_Canvas` remains at its fixed world position and orientation when camera turns.
  2. `XR Origin (XR Rig)` is at `Y = 0.0` (ground level) and `Camera Offset` is at `(0, 0, 0)`.
  3. Left and Right hand models are active and located at natural height (`Y` between 0.8m and 1.2m relative to floor, not >2.0m).
  4. CharacterController height is exactly 1.67m and center is (0, 0.835, 0).
  5. Unity Console has zero compiler errors, zero warnings, zero runtime exceptions.
- Identify the exact execution method (e.g., Unity batchmode command line or editor menu/script) so workers, reviewers, and challengers can run it reproducibly.
- Do NOT modify or write source code directly. Produce a detailed handoff report in `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_3\handoff.md` and send a summary back via send_message to orchestrator_3.
