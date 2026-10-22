## 2026-10-22T11:56:05Z
From: f4778799-4da6-485e-bc17-1d99d0ab74b5 (Sentinel)
Subject: Project Orchestrator Milestone Assignment

Requirements:
1. R1. True 3D World-Space Stationary Main Menu (Natural 360° Immersion)
- Disable head-locked/lazy-follow translation and rotation in `MainMenuVRController` so the menu board (`VR_MainMenu_Canvas`) behaves as a physical, stationary 3D object firmly planted in front of the player's spawn point (approx `(0, 1.45, 1.85)` facing spawn).
- When the player looks left, right, up, or down in Meta Quest 3, the player can freely view the ocean, pirate ship, rocks, beach camp, and sky without the menu chasing or sticking to their gaze.
- Retain ray and poke interactions on UI buttons with `XRUIInputModule`.

2. R2. XR Origin Grounding & Meta Quest 3 Hand Height Calibration
- Set the root `XR Origin (XR Rig)` position in `Mainmenu scene.unity` to ground level `(0, 0, 0)` so it matches the ground collider `Camp_Ground` at `Y = 0.0`.
- Update `MainMenuPlayerAnchor` so `spawnPosition` is `(0, 0, 0)` and ensure it does not lift `XR Origin` to `Y = 1.38m`.
- Ensure `Camera Offset` stays strictly at `(0, 0, 0)` under `Floor` tracking mode so Meta Quest 3 controllers track naturally at waist/chest level (~1.0m above floor) and NEVER fly 2.38m in the sky.

3. R3. Player Height Calibration to 167 cm (1.67m)
- Configure `CameraYOffset = 1.67f` on `XROrigin`.
- Configure `CharacterController` on `XR Origin (XR Rig)` with `height = 1.67f` and `center = (0, 0.835, 0)`.
- Configure `PlayerBodyCollision` (and any related controllers) with default head height `1.67f`, ensuring real VR floor tracking respects the player's 1.67m stature.

4. R4. Automated Verification & Parity Audit
- Run an automated Play Mode verification script in Unity to verify:
  1. `VR_MainMenu_Canvas` remains at its fixed world position and orientation when camera turns.
  2. `XR Origin (XR Rig)` is at `Y = 0.0` (ground level) and `Camera Offset` is at `(0, 0, 0)`.
  3. Left and Right hand models are active and located at natural height (`Y` between 0.8m and 1.2m relative to floor, not >2.0m).
  4. CharacterController height is exactly 1.67m.
  5. Zero compiler errors, zero warnings, and zero runtime exceptions in Unity Console.
