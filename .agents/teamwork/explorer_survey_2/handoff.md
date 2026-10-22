# Handoff Report: Explorer 2 (Tutorial Scene Surveyor)

## 1. Observation
- **Scene File**: `Assets/Prospector-journey/Tutorial/Tutorial scene.unity`
- **Unity Editor State**: Active scene is `Tutorial scene` (buildIndex: 2, rootCount: 21) verified via `Unity_ManageScene(Action="GetActive")`.
- **XR Origin Components**:
  - `XR Origin (XR Rig)` GameObject (instanceID: -1910) has 6 components:
    - `Transform` at `(0, 0, 0)`
    - `Unity.XR.CoreUtils.XROrigin`
    - `UnityEngine.CharacterController` (`radius: 0.1`, `height: 1.36144`, `center: (0, 0.76072, 0)`, `stepOffset: 0.5`, `skinWidth: 0.08`)
    - `UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager`
    - `UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager`
    - `UnityEngine.XR.Interaction.Toolkit.Gaze.XRGazeAssistance`
  - `PlayerBodyCollision` is **not present** on `XR Origin (XR Rig)` in `Tutorial scene.unity` (verified by `Unity_ManageGameObject` and regex search in scene YAML for script guid `64c4702045e23724887e7d066299cfb9`).
- **Environment & Colliders**:
  - `Environment2` has 135 children (all from `Assets/SimpleNaturePack/Prefabs/`).
  - Small foliage assets in `Assets/SimpleNaturePack/Prefabs/` have solid colliders by default:
    - `Grass_01.prefab`, `Grass_02.prefab`: `BoxCollider` (`m_IsTrigger: 0`)
    - `Flowers_01.prefab`, `Flowers_02.prefab`: `BoxCollider` (`m_IsTrigger: 0`)
    - `Mushroom_01.prefab`, `Mushroom_02.prefab`: `BoxCollider` (`m_IsTrigger: 0`)
    - `Branch_01.prefab`: `MeshCollider` (`m_IsTrigger: 0`)
    - All on Layer 0 (`Default`), tag `Untagged`.
  - Root props and buildings: `House_1`, `House_2`, `House_3`, `Bench`, `Crate`, `Fence`, `Pine Tree_1`, `Pine Tree_2`, `Lantern`, `Flower Pot` (all from `Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/`) already have active solid `MeshCollider` components.
- **Cabin & Door**:
  - `House_1` at `(19.70, -0.10, 22.10)` (rotY: 90.0°) is the primary cabin, with front yard props (`Bench`, `Crates`, `Flower Pot`, `Lanterns`, `Fence`) clustered between Z=10.0 and Z=13.4.
  - Zero door slab objects, zero `DoorSceneTransition` scripts, and zero prompt panels exist in `Tutorial scene.unity`.
- **Inventory & Tools**:
  - `[Prospector_Inventory]` is **not present** in `Tutorial scene.unity`.
  - `tool-shovel` and `Gold_Pan` are **not present** anywhere in `Tutorial scene.unity`.

## 2. Logic Chain
1. *Observation 1*: `XR Origin (XR Rig)` in `Tutorial scene.unity` currently has standard XRI locomotions and a default `CharacterController`, but lacks `PlayerBodyCollision`.
   *Inference 1*: Without `PlayerBodyCollision`, there is no HMD-CharacterController synchronizer, no counter-shifting of Camera Offset, and head movement can clip through walls and objects or drift out of sync with VR controller hands.
2. *Observation 2*: `Grass_01`, `Grass_02`, `Flowers_01`, `Flowers_02`, `Mushroom_01`, `Mushroom_02`, and `Branch_01` contain solid `BoxCollider` and `MeshCollider` components in `SimpleNaturePack`.
   *Inference 2*: If `PlayerBodyCollision` and `CharacterController` are enabled in `Tutorial scene.unity`, the player's capsule will physically hit and snag on ground grass and flowers unless `PlayerBodyCollision` explicitly calls `Physics.IgnoreCollision` on these foliage colliders.
3. *Observation 3*: `PlayerBodyCollision.EnsureFurnitureColliders()` currently only checks for `"Environment_ProspectorCabin"` which exists only in `Room scene.unity`.
   *Inference 3*: `PlayerBodyCollision` must be generalized to recognize `Environment2` and root village props in `Tutorial scene.unity`, while ensuring flat ground decals/foliage are ignored.
4. *Observation 4*: `House_1` is positioned at `(19.70, -0.10, 22.10)` facing south towards a curated front patio containing bench, crates, flower pot, lanterns, and fences.
   *Inference 4*: The entrance of `House_1` at approx `(19.0, 1.1, 16.0)` is the natural location for the cabin transition door back to `"Room scene"`.
5. *Observation 5*: `[Prospector_Inventory]`, `tool-shovel`, and `Gold_Pan` are absent in `Tutorial scene.unity`.
   *Inference 5*: The full inventory canvas, toast notification, and pre-equipped tools must be imported/ported into `Tutorial scene.unity` to satisfy requirements R2 and R3.

## 3. Caveats
- No gameplay scripts were edited during this survey (read-only investigation).
- Exact coordinates for the door slab on `House_1` should be placed near `(19.0, 1.1, 16.0)` with a trigger radius around 1.85m to match the patio access.

## 4. Conclusion
`Tutorial scene.unity` is structurally intact with environment assets and props, but requires four concrete additions to achieve parity with `Room scene.unity`:
1. Add `PlayerBodyCollision` to `XR Origin (XR Rig)` and set `CharacterController` settings (`radius=0.24`, `stepOffset=0.08`, `skinWidth=0.025`).
2. Update `PlayerBodyCollision.cs` to support `Tutorial scene.unity`: ensure colliders on environment obstacles while ignoring small foliage colliders (`Grass*`, `Flower*`, `Mushroom*`, `Branch*`).
3. Add `[Prospector_Inventory]` with `ToastNotification` and pre-stored `tool-shovel` and `Gold_Pan`.
4. Add `Cabin_Door_Slab` with `DoorSceneTransition` (target: `"Room scene"`) and `Door_Interaction_Prompt` at `House_1` entrance.

## 5. Verification Method
- **File Inspection**:
  - Detailed survey: `.agents/teamwork/explorer_survey_2/survey_report.md`
- **Unity Verification Commands**:
  - `Unity_ManageScene(Action="GetActive")` -> Active scene is `Tutorial scene`.
  - `Unity_ManageGameObject(action="get_components", target="XR Origin (XR Rig)", search_method="by_name")` -> Verify presence/absence of components.
  - Python YAML parser: `python .agents/teamwork/explorer_survey_2/inspect_colliders.py` -> Confirms colliders on foliage and root props.
