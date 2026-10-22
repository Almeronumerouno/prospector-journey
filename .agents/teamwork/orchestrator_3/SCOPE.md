# SCOPE: Main Menu VR Immersion, Grounding & 1.67m Height Calibration

## Architecture & Boundaries
- Scene: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
- Scripts:
  - `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs` (or similar controller for menu canvas positioning)
  - `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuPlayerAnchor.cs` (or player positioning/anchoring logic)
  - `PlayerBodyCollision.cs` / `XROrigin` / `CharacterController`
- Test/Verification: Automated Play Mode script executing in Unity via editor or CLI.

## Requirements Breakdown
### R1. True 3D World-Space Stationary Main Menu (Natural 360° Immersion)
- Disable head-locked / lazy-follow translation and rotation in `MainMenuVRController` so `VR_MainMenu_Canvas` behaves as a stationary 3D object firmly planted in front of player's spawn point (`(0, 1.45, 1.85)` facing spawn).
- Player can freely look 360° around (ocean, pirate ship, rocks, beach camp, sky) without canvas chasing gaze.
- Ray and poke interactions on UI buttons with `XRUIInputModule` remain functional.

### R2. XR Origin Grounding & Meta Quest 3 Hand Height Calibration
- Set root `XR Origin (XR Rig)` position in `Mainmenu scene.unity` to ground level `(0, 0, 0)` matching `Camp_Ground` at `Y = 0.0`.
- Update `MainMenuPlayerAnchor` so `spawnPosition` is `(0, 0, 0)` and does not lift `XR Origin` to `Y = 1.38m`.
- Ensure `Camera Offset` stays at `(0, 0, 0)` under `Floor` tracking mode so Meta Quest 3 controllers track at natural waist/chest level (~1.0m above floor) and NEVER fly 2.38m in the sky.

### R3. Player Height Calibration to 167 cm (1.67m)
- Configure `CameraYOffset = 1.67f` on `XROrigin`.
- Configure `CharacterController` on `XR Origin (XR Rig)` with `height = 1.67f` and `center = (0, 0.835, 0)`.
- Configure `PlayerBodyCollision` with default head height `1.67f`.

### R4. Automated Verification & Parity Audit
- Play Mode verification script in Unity to verify:
  1. `VR_MainMenu_Canvas` remains at fixed world pos/rot when camera turns.
  2. `XR Origin (XR Rig)` is at `Y = 0.0` and `Camera Offset` is at `(0, 0, 0)`.
  3. Left and Right hand models active and at natural height (`Y` between 0.8m and 1.2m relative to floor).
  4. CharacterController height is 1.67m.
  5. Zero compiler errors, warnings, runtime exceptions in Unity Console.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | Main Menu VR Immersion & Height Calibration | Full R1-R4 implementation and verification | none | IN_PROGRESS |
