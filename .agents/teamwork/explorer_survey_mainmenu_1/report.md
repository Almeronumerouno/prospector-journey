# Comprehensive Investigation Report: Main Menu UI Canvas, Translucent Glass & Typography

**Author**: teamwork_preview_explorer (Survey Explorer 1: UI Canvas, Translucent Glass & Typography)  
**Date**: 2026-10-13  
**Working Directory**: `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_1\`  
**Target Scene**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`  
**Reference Request**: `c:\CodeKuliah\prospector-journey\.agents\teamwork\ORIGINAL_REQUEST.md` (`## 2026-10-13T10:27:16Z`)

---

## Executive Summary
This report delivers the authoritative architectural survey of the UI Canvas, translucent glass frame rendering, visual materials, hierarchy, and typography in `Mainmenu scene.unity`.

Key Findings:
1. **Opaque Wood Occlusion Root Cause Identified**: The existing UI consists of 3 nested full-canvas image fills (`Board_Background` at alpha 0.50, `Gold_Frame` at alpha 0.60, and `Inner_Board` at alpha 0.608). Their cumulative multiplication produces an effective opacity of **92.2%** solid muddy brown, and opening `Settings_Panel` (`Frame_GameOptions` at alpha 0.88) pushes total opacity to **99.1%**, completely occluding the 1,384-object beach/ocean diorama behind the canvas.
2. **Path Contract Preservation**: Both `MainMenuGameOptionsBuilder.cs` and the Play Mode audit `MainMenuOptionsPlayModeAudit.cs` hardcode the hierarchy path `Board_Background/Gold_Frame/Inner_Board/Panels_Content/Settings_Panel`. These GameObjects must be retained; translucency is achieved by setting `Board_Background` to dark tinted glass (alpha 0.55), setting `Gold_Frame` and `Inner_Board` Image fills to transparent (`alpha = 0`), and adding a crisp light metallic `Outline`.
3. **Typography Scalability**: Current typography uses `Oswald Bold SDF.asset`. Options labels and selectors are currently at 28pt, and main buttons are at 22–32pt. Scaling headers to 48–54pt and option/button texts to 36–40pt requires widening `Selector_Group` to 400px, `Text_Value` to 250px (to prevent "CONTINUOUS" clipping), increasing option row height from 76px to 92px, and increasing button heights from 62–72px to 76–88px.
4. **"COMING SOON" Badge**: Can be cleanly integrated as a child pill badge on `Btn_StartExpedition` (e.g. `Badge_ComingSoon` with amber/gold styling and 20pt bold label).

---

## Task 1: Main Menu UI Hierarchy & Scene Structure

### 1.1 Scene Location & Canvas Properties
- **Scene Path**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
- **Root Canvas**: `VR_MainMenu_Canvas` (GameObject ID: `1321170159`)
  - **Render Mode**: `WorldSpace` (`m_RenderMode: 2`)
  - **RectTransform**:
    - Anchored Position: `(0, 1.45, 0)` meters (eye height)
    - Size Delta: `1500 x 950` units
    - Local Scale: `(0.001, 0.001, 0.001)` -> Effective World Size: `1.50m wide x 0.95m tall`
  - **Key Attached Components**:
    - `Canvas` (ID: `1321170164`)
    - `CanvasScaler` (ID: `1321170163`, Dynamic Pixels Per Unit = 1.0)
    - `TrackedDeviceGraphicRaycaster` (ID: `1321170162`)
    - `MainMenuVRController` (ID: `1321170161`)
    - `AudioSource` (ID: `1321170160`)
    - `CanvasGroup` (ID: `1321170166`, `alpha = 1.0`, `interactable = 1`, `blocksRaycasts = 1`)

### 1.2 Authoritative Hierarchy Tree

```
VR_MainMenu_Canvas [WorldSpace 1500x950 @ y=1.45m]
└── Board_Background (Image: 1500x950, AnchorMin: 0,0, AnchorMax: 1,1)
    └── Gold_Frame (Image: 1468x918, SizeDelta: -32,-32, AnchorMin: 0,0, AnchorMax: 1,1)
        └── Inner_Board (Image: 1460x910, SizeDelta: -8,-8, AnchorMin: 0,0, AnchorMax: 1,1)
            ├── Header_Section (RectTransform: 1460x180, Pos: 0,0, Anchor: Top-Stretch)
            │   ├── Title_Text (TMP: "PROSPECTOR JOURNEY", 70pt, Pos: 0,-25, Size: 1460x85)
            │   ├── Subtitle_Text (TMP: "VR GOLD PANNING & RIVER CLAIM SIMULATION", 24pt, Pos: 0,-110)
            │   └── Gold_Divider (Image: 4px bar, Pos: 0,5, MinX: 0.1, MaxX: 0.9)
            └── Panels_Content (RectTransform: 1360x690, Pos: 0,-80, SizeDelta: -100,-220)
                ├── Main_Buttons_Panel [ACTIVE at Start] (VerticalLayoutGroup)
                │   ├── Btn_StartExpedition (Button + Image, Size: 620x85, Label: 32pt)
                │   ├── Btn_Tutorial (Button + Image, Size: 620x72, Label: 26pt)
                │   ├── Btn_Settings (Button + Image, Size: 620x72, Label: 26pt "GAME OPTIONS")
                │   ├── Btn_Credits (Button + Image, Size: 620x72, Label: 26pt)
                │   └── Btn_Quit (Button + Image, Size: 620x62, Label: 22pt)
                ├── Settings_Panel [INACTIVE at Start]
                │   ├── Section_Title (TMP: "GAME OPTIONS", 42pt, Pos: 0,-50, Size: 800x60)
                │   ├── Frame_GameOptions (Image + Outline, Pos: 0,-235, Size: 960x260)
                │   │   ├── Row_TurnType (Image: 880x76, Pos: 0,48)
                │   │   │   ├── Label (TMP: "TURN TYPE", 28pt, Size: 400x50)
                │   │   │   └── Selector_Group (Size: 340x60)
                │   │   │       ├── Btn_Prev (Button: 52x52, Arrow TMP: "<", 32pt)
                │   │   │       ├── Text_Value (TMP: "SNAP", 28pt, Size: 190x52)
                │   │   │       └── Btn_Next (Button: 52x52, Arrow TMP: ">", 32pt)
                │   │   └── Row_LocomotionType (Image: 880x76, Pos: 0,-48)
                │   │       ├── Label (TMP: "LOCOMOTION TYPE", 28pt, Size: 400x50)
                │   │       └── Selector_Group (Size: 340x60)
                │   │           ├── Btn_Prev (Button: 52x52, Arrow TMP: "<", 32pt)
                │   │           ├── Text_Value (TMP: "TELEPORT", 28pt, Size: 190x52)
                │   │           └── Btn_Next (Button: 52x52, Arrow TMP: ">", 32pt)
                │   └── Btn_SaveAndExit (Button + Image + Outline, Pos: 0,-440, Size: 440x68, Label: 28pt)
                ├── Tutorial_Panel [INACTIVE] (Tutorial instructional cards)
                └── Credits_Panel [INACTIVE] (Credits info card)
```

---

## Task 2: Mockup & Visual Reference Analysis

### 2.1 Workspace Asset Survey
- Comprehensive filesystem search across `Assets/`, `.agents/`, root directory, and user folders confirmed that no external bitmap mockup was deposited on disk.
- The visual specifications are strictly and unambiguously defined in the user prompt (`ORIGINAL_REQUEST.md` line 50-65):
  - **Panel Look**: "Translucent dark tinted glass (alpha ~0.50–0.65) with a crisp outline border (white / light metallic border) allowing the ocean, pirate ship, and beach environment to remain clearly visible through the UI ('tembus pandang')."
  - **Eliminate Occlusion**: "eliminating opaque wood occluders."
  - **Harmonized Buttons**: Translucent glass styling retaining high contrast and responsive hover/click feedback.
  - **Typography**: Prominent headers (46–54pt), buttons/options (36–42pt).
  - **Badge**: Distinct "COMING SOON" pill/banner tag on `Btn_StartExpedition`.

### 2.2 Environment Visibility Context
- The scene contains `Diorama_Environment` (GameObject ID: `1443230119`), which houses **1,384 child objects** featuring a beach shoreline, Caribbean pirate ship, ocean water, rocks, lanterns, campfire, and sky.
- Because `VR_MainMenu_Canvas` is floating 1.85m in front of the VR camera at eye level, making the board translucent ("tembus pandang") produces an immersive framing where the pirate cove and ocean are visible directly through the dark tinted glass.

---

## Task 3: Current Visual Components, Materials & Shaders

### 3.1 Component Inspection Matrix

| GameObject | Components Present | Current Color / Alpha | Material / Shader | Visual Impact |
|---|---|---|---|---|
| `VR_MainMenu_Canvas` | Canvas, CanvasScaler, TrackedDeviceGraphicRaycaster, MainMenuVRController, CanvasGroup | `CanvasGroup.alpha = 1.0` | Default UI | Canvas itself is fully opaque container |
| `Board_Background` | RectTransform, CanvasRenderer, Image | `RGBA(0.12, 0.09, 0.07, 0.50)` | Default (`UI/Default`) | 50% brown fill |
| `Gold_Frame` | RectTransform, CanvasRenderer, Image | `RGBA(0.75, 0.55, 0.22, 0.60)` | Default (`UI/Default`) | 60% golden brown fill |
| `Inner_Board` | RectTransform, CanvasRenderer, Image | `RGBA(0.46, 0.31, 0.22, 0.61)` | Default (`UI/Default`) | 60.8% dark wood fill |
| `Btn_StartExpedition` | RectTransform, CanvasRenderer, Image, Button, VRButtonAudioFeedback | `RGBA(0.60, 0.38, 0.12, 1.00)` | Default (`UI/Default`) | 100% solid opaque brown |
| `Btn_Tutorial` | RectTransform, CanvasRenderer, Image, Button, VRButtonAudioFeedback | `RGBA(0.24, 0.18, 0.14, 1.00)` | Default (`UI/Default`) | 100% solid opaque dark brown |
| `Btn_Settings` | RectTransform, CanvasRenderer, Image, Button, VRButtonAudioFeedback | `RGBA(0.24, 0.18, 0.14, 1.00)` | Default (`UI/Default`) | 100% solid opaque dark brown |
| `Btn_Credits` | RectTransform, CanvasRenderer, Image, Button, VRButtonAudioFeedback | `RGBA(0.24, 0.18, 0.14, 1.00)` | Default (`UI/Default`) | 100% solid opaque dark brown |
| `Btn_Quit` | RectTransform, CanvasRenderer, Image, Button, VRButtonAudioFeedback | `RGBA(0.28, 0.12, 0.12, 1.00)` | Default (`UI/Default`) | 100% solid opaque dark reddish |
| `Frame_GameOptions` | RectTransform, CanvasRenderer, Image, Outline | `RGBA(0.14, 0.09, 0.06, 0.88)` | Default (`UI/Default`) | 88% opaque dark brown wood card |
| `Row_TurnType` | RectTransform, CanvasRenderer, Image | `RGBA(0.22, 0.15, 0.10, 0.75)` | Default (`UI/Default`) | 75% dark wood strip |
| `Row_LocomotionType` | RectTransform, CanvasRenderer, Image | `RGBA(0.22, 0.15, 0.10, 0.75)` | Default (`UI/Default`) | 75% dark wood strip |
| `Btn_SaveAndExit` | RectTransform, CanvasRenderer, Image, Outline, Button, VRButtonAudioFeedback | `RGBA(0.26, 0.17, 0.12, 1.00)` | Default (`UI/Default`) | 100% solid opaque wood button |

### 3.2 Material & Shader Behavior
- All UI Image and TextMeshPro components use Unity's default UI material (`m_Material: {fileID: 0}`).
- In Unity uGUI World Space with URP, default UI graphics render with standard alpha blending (`Blend SrcAlpha OneMinusSrcAlpha`, `ZWrite Off`).
- The project also contains `Assets/Prospector-journey/Mainmenu/UI-NoZTest.mat` (`Shader "UI/NoZTest"`), which has explicit `ZTest Off`, `ZWrite Off`, `Blend SrcAlpha OneMinusSrcAlpha`.
- Standard uGUI color vertex alphas blend cleanly with no special custom shaders required. The reason the UI previously looked opaque was purely the **stacking of multiple semi-transparent and fully opaque Image fills**.

---

## Task 4: Translucent Tinted Glass ("Tembus Pandang") & Border Architecture

### 4.1 Alpha Mathematical Analysis

In the current setup:
- Behind `Panels_Content`:
  $$\text{Opacity}_{\text{board}} = 1 - (1 - 0.50) \times (1 - 0.60) \times (1 - 0.608) = 1 - (0.50 \times 0.40 \times 0.392) = 1 - 0.0784 = 92.16\%$$
- When `Frame_GameOptions` is displayed:
  $$\text{Opacity}_{\text{total}} = 1 - (1 - 0.9216) \times (1 - 0.88) = 1 - (0.0784 \times 0.12) = 99.06\%$$

### 4.2 Proposed Translucent Glass Solution

To achieve true translucency without breaking any script hierarchy references (`Board_Background/Gold_Frame/Inner_Board/Panels_Content`):

1. **`Board_Background`** (Single unified glass pane):
   - **Image Color**: `Color(0.06f, 0.08f, 0.12f, 0.55f)` (Deep slate tinted glass, alpha = 0.55).
   - **Outline Component**:
     - Effect Color: `Color(0.80f, 0.88f, 0.96f, 0.80f)` (Crisp light metallic / frosted silver border).
     - Effect Distance: `Vector2(2.5f, -2.5f)` or `Vector2(3f, -3f)`.
2. **`Gold_Frame`** (Pinstripe accent):
   - **Image Color**: `Color(0f, 0f, 0f, 0f)` (Transparent — eliminates the middle occluding layer).
   - Optional: Add `Outline` with `Color(0.90f, 0.82f, 0.60f, 0.45f)` for an elegant dual-tone metallic inner frame line.
3. **`Inner_Board`** (Layout container):
   - **Image Color**: `Color(0f, 0f, 0f, 0f)` (Transparent — completely eliminates the inner wood board occluder).
4. **`Frame_GameOptions`** (Options card glass frame):
   - **Image Color**: `Color(0.08f, 0.11f, 0.16f, 0.45f)` (Translucent dark tinted glass).
   - **Outline Component**:
     - Effect Color: `Color(0.75f, 0.85f, 0.95f, 0.80f)` (Crisp light metallic border).
     - Effect Distance: `Vector2(2.5f, -2.5f)`.
5. **Option Rows (`Row_TurnType` & `Row_LocomotionType`)**:
   - **Image Color**: `Color(0.14f, 0.18f, 0.25f, 0.45f)` (Subtle glass backing).
   - Add `Outline` with `Color(0.60f, 0.70f, 0.80f, 0.35f)` and distance `(1.5f, -1.5f)`.
6. **Main Menu Buttons (`Btn_StartExpedition`, etc.)**:
   - **Image Color**: Translucent glass backplate `Color(0.12f, 0.16f, 0.24f, 0.55f)`.
   - For `Btn_StartExpedition`: subtle golden glass tint `Color(0.30f, 0.22f, 0.10f, 0.60f)`.
   - **Outline Component** on each button: `Color(0.75f, 0.85f, 0.95f, 0.70f)`, distance `(2f, -2f)`.
   - **Button ColorBlock Transitions**:
     - Normal: `Color(0.12f, 0.16f, 0.24f, 0.55f)`
     - Highlighted: `Color(0.24f, 0.36f, 0.52f, 0.80f)` (crisp luminous hover glow)
     - Pressed: `Color(0.38f, 0.55f, 0.78f, 0.95f)` (responsive click pulse)
     - Selected: `Color(0.24f, 0.36f, 0.52f, 0.80f)`

**Resulting Net Opacity**:
- Main Canvas: Exactly **55.0%** (well within requested ~50–65% and `< 0.80` audit criteria).
- Options Frame: $$1 - (1 - 0.55) \times (1 - 0.45) = 75.25\%$$ (also `< 0.80` audit limit).
- The beach, sea, palm trees, cliffs, and sky remain clearly visible through the glass panels at all times.

---

## Task 5: Comprehensive Typography Survey

### 5.1 Font Assets
- Authoritative Font Asset: `Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset` (GUID: `0161d805a3764c089bef00bfe00793f5`).
- Texture Atlas: High-resolution Signed Distance Field (SDF), rendering sharp vector-like glyphs with zero pixelation at close and medium VR viewing distances.

### 5.2 Current Typography Inventory

| Location | GameObject | Current Text | Current Size | Font Style | Current Color | Alignment |
|---|---|---|---|---|---|---|
| Header | `Title_Text` | `PROSPECTOR JOURNEY` | 70pt | Bold | `RGBA(1.00, 0.78, 0.25, 1.00)` | Center |
| Header | `Subtitle_Text` | `VR GOLD PANNING & RIVER CLAIM SIMULATION` | 24pt | Regular | `RGBA(0.92, 0.84, 0.72, 1.00)` | Center |
| Main Menu | `Btn_StartExpedition/Label` | `START EXPEDITION` | 32pt | Bold | `RGBA(1.00, 0.96, 0.88, 1.00)` | Center |
| Main Menu | `Btn_Tutorial/Label` | `GAME TUTORIAL` | 26pt | Bold | `RGBA(0.92, 0.86, 0.78, 1.00)` | Center |
| Main Menu | `Btn_Settings/Label` | `GAME OPTIONS` | 26pt | Bold | `RGBA(0.92, 0.86, 0.78, 1.00)` | Center |
| Main Menu | `Btn_Credits/Label` | `CREDITS` | 26pt | Bold | `RGBA(0.92, 0.86, 0.78, 1.00)` | Center |
| Main Menu | `Btn_Quit/Label` | `QUIT GAME` | 22pt | Bold | `RGBA(0.92, 0.75, 0.75, 1.00)` | Center |
| Options | `Section_Title` | `GAME OPTIONS` | 42pt | Bold | `RGBA(1.00, 0.88, 0.55, 1.00)` | Center |
| Options | `Row_TurnType/Label` | `TURN TYPE` | 28pt | Bold | `RGBA(1.00, 0.95, 0.85, 1.00)` | MidlineLeft |
| Options | `Row_TurnType/Btn_Prev/Arrow` | `<` | 32pt | Bold | `RGBA(1.00, 0.86, 0.45, 1.00)` | Center |
| Options | `Row_TurnType/Text_Value` | `SNAP` / `CONTINUOUS` | 28pt | Bold | `RGBA(1.00, 1.00, 1.00, 1.00)` | Center |
| Options | `Row_TurnType/Btn_Next/Arrow` | `>` | 32pt | Bold | `RGBA(1.00, 0.86, 0.45, 1.00)` | Center |
| Options | `Row_LocoType/Label` | `LOCOMOTION TYPE` | 28pt | Bold | `RGBA(1.00, 0.95, 0.85, 1.00)` | MidlineLeft |
| Options | `Row_LocoType/Btn_Prev/Arrow` | `<` | 32pt | Bold | `RGBA(1.00, 0.86, 0.45, 1.00)` | Center |
| Options | `Row_LocoType/Text_Value` | `TELEPORT` / `CONTINUOUS` | 28pt | Bold | `RGBA(1.00, 1.00, 1.00, 1.00)` | Center |
| Options | `Row_LocoType/Btn_Next/Arrow` | `>` | 32pt | Bold | `RGBA(1.00, 0.86, 0.45, 1.00)` | Center |
| Options | `Btn_SaveAndExit/Label` | `SAVE AND EXIT` | 28pt | Bold | `RGBA(1.00, 0.96, 0.90, 1.00)` | Center |

---

## Task 6: Exact Layout Modifications & Typography Scaling Plan

To comply with the required font scaling:
- **Header titles**: Enlarged to **46–54pt**
- **Option labels, value selectors & buttons**: Scaled up to **>= 36pt** (36–42pt)
- **Zero clipping guarantee**: Accommodated with increased row heights, button sizes, and container dimensions.

### 6.1 Scaling & Dimension Comparison Table

| Element | Component / Property | Current Dimension | Target Dimension | Current Font | Target Font | Headroom / Clipping Analysis |
|---|---|---|---|---|---|---|
| `Header_Section` | RectTransform | `1460 x 180` | `1460 x 180` | — | — | Sufficient space |
| `Title_Text` | Text / RectTransform | `1460 x 85` | `1460 x 85` | 70pt | **54pt** (Bold) | Fits cleanly within 46–54pt requirement |
| `Subtitle_Text` | Text / RectTransform | `1460 x 40` | `1460 x 42` | 24pt | **28pt** | 28pt line height is ~34px, fits in 42px |
| `Btn_StartExpedition` | RectTransform | `620 x 85` | **`660 x 88`** | 32pt | **40pt** (Bold) | Fits `START EXPEDITION` (width ~350px) + Badge |
| `Btn_Tutorial` | RectTransform | `620 x 72` | **`640 x 80`** | 26pt | **38pt** (Bold) | Height 80px easily accommodates 38pt glyphs |
| `Btn_Settings` | RectTransform | `620 x 72` | **`640 x 80`** | 26pt | **38pt** (Bold) | Height 80px easily accommodates 38pt glyphs |
| `Btn_Credits` | RectTransform | `620 x 72` | **`640 x 80`** | 26pt | **38pt** (Bold) | Height 80px easily accommodates 38pt glyphs |
| `Btn_Quit` | RectTransform | `620 x 62` | **`640 x 76`** | 22pt | **36pt** (Bold) | Increased from 62px to 76px to prevent clipping |
| `Section_Title` (Options) | RectTransform | `800 x 60` | **`900 x 70`** | 42pt | **50pt** (Bold) | Satisfies 46–54pt header requirement |
| `Frame_GameOptions` | RectTransform | `960 x 260` | **`1000 x 290`** | — | — | Expands card to house taller 92px rows |
| `Row_TurnType` | RectTransform | `880 x 76` | **`920 x 92`** | — | — | +16px height for VR readability |
| `Row_TurnType/Label` | RectTransform | `400 x 50` | **`450 x 64`** | 28pt | **38pt** (Bold) | Text width ~210px in 450px container |
| `Row_TurnType/Selector` | RectTransform | `340 x 60` | **`400 x 72`** | — | — | Expanded to fit 38pt values & larger arrows |
| `Row_TurnType/Btn_Prev` | RectTransform | `52 x 52` | **`60 x 60`** | 32pt | **38pt** (`<`) | Larger touch target for VR ray/poke |
| `Row_TurnType/Text_Value`| RectTransform | `190 x 52` | **`250 x 60`** | 28pt | **38pt** (Bold) | **CRITICAL**: 250px width fits "CONTINUOUS" |
| `Row_TurnType/Btn_Next` | RectTransform | `52 x 52` | **`60 x 60`** | 32pt | **38pt** (`>`) | Larger touch target for VR ray/poke |
| `Row_LocoType` | RectTransform | `880 x 76` | **`920 x 92`** | — | — | +16px height for VR readability |
| `Row_LocoType/Label` | RectTransform | `400 x 50` | **`450 x 64`** | 28pt | **38pt** (Bold) | Text width ~345px in 450px container |
| `Row_LocoType/Selector` | RectTransform | `340 x 60` | **`400 x 72`** | — | — | Expanded width |
| `Row_LocoType/Btn_Prev` | RectTransform | `52 x 52` | **`60 x 60`** | 32pt | **38pt** (`<`) | Touch target enlarged |
| `Row_LocoType/Text_Value`| RectTransform | `190 x 52` | **`250 x 60`** | 28pt | **38pt** (Bold) | **CRITICAL**: 250px width fits "CONTINUOUS" |
| `Row_LocoType/Btn_Next` | RectTransform | `52 x 52` | **`60 x 60`** | 32pt | **38pt** (`>`) | Touch target enlarged |
| `Btn_SaveAndExit` | RectTransform | `440 x 68` | **`500 x 78`** | 28pt | **38pt** (Bold) | Expanded to fit 38pt text without wrapping |

### 6.2 "COMING SOON" Badge Specifications
- **Attachment Point**: Child of `Btn_StartExpedition` (named `Badge_ComingSoon`).
- **RectTransform**:
  - AnchorMin: `(1.0, 0.5)`
  - AnchorMax: `(1.0, 0.5)`
  - Pivot: `(1.0, 0.5)`
  - AnchoredPosition: `(-18, 0)`
  - SizeDelta: `(160, 36)`
- **Visuals**:
  - `Image`: Pill/rounded banner with vibrant amber/gold styling: `Color(0.95f, 0.65f, 0.15f, 0.88f)`.
  - `Outline`: `Color(1.0f, 0.88f, 0.50f, 0.95f)`, distance: `(1.5f, -1.5f)`.
- **Label (`Badge_Text`)**:
  - Text: `"COMING SOON"`
  - Font: `Oswald Bold SDF`
  - Font Size: `20pt` (Bold)
  - Color: `Color(0.12f, 0.08f, 0.02f, 1.0f)` (Dark bronze on bright gold pill, maximum contrast).
  - Alignment: `Center`.

---

## Conclusion & Implementation Blueprint

1. **Hierarchy Preservation**: Keep the exact path `Board_Background/Gold_Frame/Inner_Board/Panels_Content/Settings_Panel` intact.
2. **Glass Translucency**:
   - Set `Board_Background.color = Color(0.06f, 0.08f, 0.12f, 0.55f)` with light metallic `Outline` `Color(0.80f, 0.88f, 0.96f, 0.80f)`.
   - Set `Gold_Frame.color = Color(0, 0, 0, 0)` and `Inner_Board.color = Color(0, 0, 0, 0)`.
   - Set `Frame_GameOptions.color = Color(0.08f, 0.11f, 0.16f, 0.45f)` with `Outline`.
   - Set button backplates to translucent slate `Color(0.12f, 0.16f, 0.24f, 0.55f)` with outlines.
3. **Typography Standards**:
   - Headers: 50–54pt.
   - Buttons and options: 36–40pt.
   - Expand selector groups and value text containers to 250px to ensure `CONTINUOUS` fits cleanly.
4. **Tooling & Audit Integration**:
   - The implementer can update or execute `MainMenuGameOptionsBuilder.cs` (or an equivalent builder script) using these exact coordinates and properties.
   - Extend `MainMenuOptionsPlayModeAudit.cs` to assert:
     - `background.color.a < 0.8f`
     - all option texts and button texts `>= 36pt`
     - `Badge_ComingSoon` active on `Btn_StartExpedition`
