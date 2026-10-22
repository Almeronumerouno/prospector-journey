## 2026-10-12T16:32:16Z
You are Worker M1 (VR Script Implementer).
Working directory: c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m1
Read c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md and c:\CodeKuliah\prospector-journey\PROJECT.md first.

DO NOT CHEAT. All implementations must be genuine. DO NOT hardcode test results, create dummy/facade implementations, or circumvent the intended task. A teamwork_preview_auditor will independently verify your work. Integrity violations WILL be detected and your work WILL be rejected.

Your exclusive write files:
- Assets/Prospector-journey/Room/Scripts/PlayerBodyCollision.cs
- Assets/Prospector-journey/Room/Scripts/ProspectorInventory.cs

Task for Milestone M1:
1. Extend `PlayerBodyCollision.cs`:
   - In `EnsureFurnitureColliders()`: Support `Tutorial scene.unity` by scanning for `Environment2` (trees, rocks, stumps) and root obstacle props (`House_1`, `House_2`, `House_3`, `Bench`, `Crate`, `Fence`, `Pine Tree_1`, `Pine Tree_2`, `Lantern`, `Flower Pot`), ensuring solid colliders (`MeshCollider` or `BoxCollider`) are attached to solid obstacles so the player never clips through them.
   - In `IgnoreGrabbableAndTeleportCollisions()` / `EnsureFurnitureColliders()`: Discover all colliders on flat ground/foliage decals (any GameObject whose name starts with or contains `Grass`, `Flowers`, `Mushroom`, `Branch`) in `Environment2` and call `Physics.IgnoreCollision(m_CharacterController, col, true)` on all of them, so the player never snags on grass or flowers.
   - Maintain 1:1 speed synchronization and camera Y eye-height lock when pressing E/Q.
2. Extend `ProspectorInventory.cs`:
   - In `Start()` or after `InitializeSlots()`, add pre-stored items initialization: scan `storedItemsContainer` children. If items (like `tool-shovel` and `Gold_Pan`) exist in `storedItemsContainer`, register them into `m_Slots` (isOccupied=true, itemGameObject, grabInteractable, itemName, itemDisplayName), deactivate their GameObjects, and call `UpdateUI()`.
   - Ensure quick-storing (C/X or VR buttons) and slot retrieval work smoothly for these pre-stored tools with active ToastNotification display.
3. Verification:
   - Verify C# syntax and compilation (e.g. via Unity_GetConsoleLogs, dotnet build or checking compiler diagnostics).
   - Document changes and verification in `c:\CodeKuliah\prospector-journey\.agents\teamwork\worker_m1\handoff.md`.
Send a message back when complete.
