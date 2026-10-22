## 2026-10-12T16:44:32Z
You are Worker M2 (Tutorial Scene Synchronizer).
Working directory: c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m2
Read c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md, c:\CodeKuliah\prospector-journey\PROJECT.md, and the survey reports:
- c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_1_gen2\survey_report.md
- c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_2\survey_report.md
first.

DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_preview_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Your exclusive target file:
- `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`
(You can also create a temporary or permanent Editor utility script under `Assets/Prospector-journey/Tutorial/Editor/` to inspect, build, or verify the scene via Unity MCP / Editor if needed).

Task for Milestone M2:
1. `XR Origin (XR Rig)` in `Tutorial scene.unity`:
   - Attach and configure `PlayerBodyCollision` (`bodyRadius = 0.24`, `stepOffset = 0.08`, `skinWidth = 0.025`, `minHeadHeight = 0.85`, `maxHeadHeight = 2.05`).
   - Configure `CharacterController` on `XR Origin (XR Rig)` with `radius = 0.24`, `stepOffset = 0.08`, `skinWidth = 0.025`, `minMoveDistance = 0.0001`.
2. `[Prospector_Inventory]` and Pre-equipped Tools in `Tutorial scene.unity`:
   - Port `[Prospector_Inventory]` hierarchy matching `Room scene.unity`:
     - Root `[Prospector_Inventory]` with `ProspectorInventory.cs`, `AudioSource`, and UI SFX audio clip references.
     - Child `Stored_Items_Container` (active = false).
     - Child `InventoryCanvas` (WorldSpace canvas, CanvasGroup, GraphicRaycasters, PanelBackground, Header, StoreHeldButton, 4 Slot buttons with TMP texts, backgrounds, and VRButtonAudioFeedback, StatusBar).
     - Child `ToastNotification` (WorldSpace canvas with text and border).
   - Pre-store `tool-shovel` and `Gold_Pan` inside `Stored_Items_Container`:
     - Fully configured instances matching Room scene: Rigidbody, BoxCollider/MeshCollider, `XRGrabInteractable`, `XRRestingProp`, `XRGeneralGrabTransformer`, `attach1` and `attach2` points.
     - When scene starts, `ProspectorInventory.InitializePreStoredItems()` will register them into slots 1 and 2.
3. Cabin Door Interaction Display (`Door_Interaction_Prompt` & `DoorSceneTransition`):
   - In `Tutorial scene.unity`, at cabin `House_1` front doorway (around `(19.64, 1.2, 15.60)`):
     - Create `Cabin_Door_Slab` with `BoxCollider` (solid), `MeshRenderer`, `XRSimpleInteractable`, and `DoorSceneTransition` configured with:
       - `targetSceneName = "Room scene"`
       - `interactionDistance = 1.85`
       - `doorCenter = Cabin_Door_Slab.transform`
       - `doorOpenClip` and `doorCreakClip` SFX
     - Create `Door_Interaction_Prompt` (WorldSpace canvas with `CanvasGroup`, `TitleText`, and `PromptText`) facing the front yard towards the player.
4. Verification:
   - Ensure the scene is saved cleanly in Edit Mode without corruption or missing references.
   - Verify zero console errors and clean compilation using `Unity_GetConsoleLogs` or Unity MCP tools.
   - Document changes and verification in `c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m2\handoff.md`.
Send a message back when complete.
