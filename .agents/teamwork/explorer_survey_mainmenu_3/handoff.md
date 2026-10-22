# 5-Component Handoff Report: Main Menu Interactive Logic, Options Persistence, Badge & Test Harness

**Agent**: Survey Explorer 3 (`explorer_survey_mainmenu_3`)  
**Mission**: Investigate Main Menu interactive logic, Game Options cycling and PlayerPrefs persistence, the Start Expedition button + "COMING SOON" badge design, and automated Play Mode test architecture.  
**Working Directory**: `c:\CodeKuliah\prospector-journey\.agents\teamwork\explorer_survey_mainmenu_3`  
**Status**: Task Complete (Hard Handoff)  

---

## 1. Observation

### 1.1 Main Menu Scripts & Interactive Handlers
- **File**: `Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs`
  - Lines 11–14: Panel references:
    ```csharp
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject creditsPanel;
    ```
  - Lines 36–42: Game Options UI references:
    ```csharp
    [SerializeField] private TextMeshProUGUI turnTypeText;
    [SerializeField] private TextMeshProUGUI locomotionTypeText;
    [SerializeField] private Button turnPrevButton;
    [SerializeField] private Button turnNextButton;
    [SerializeField] private Button locomotionPrevButton;
    [SerializeField] private Button locomotionNextButton;
    [SerializeField] private Button saveAndExitButton;
    ```
  - Lines 54–59: Options enums & defaults:
    ```csharp
    public enum TurnTypeOption { Snap = 0, Continuous = 1 }
    public enum LocomotionTypeOption { Teleport = 0, Continuous = 1 }
    private TurnTypeOption currentTurnType = TurnTypeOption.Snap;
    private LocomotionTypeOption currentLocomotionType = LocomotionTypeOption.Teleport;
    ```
  - Lines 160–165: Start Expedition click handler:
    ```csharp
    public void OnClickStartExpedition()
    {
        PlayClickSound();
        Debug.Log($"[Prospector Journey] Loading game scene: {mainGameScene}");
        SceneManager.LoadSceneAsync(mainGameScene);
    }
    ```
  - Lines 231–251: Cycling methods:
    `CycleTurnType(int dir)` cycles modulo 2 between `Snap` (0) and `Continuous` (1).
    `CycleLocomotionType(int dir)` cycles modulo 2 between `Teleport` (0) and `Continuous` (1).
  - Lines 253–264: Persistence method `OnSaveAndExitSettings()`:
    ```csharp
    PlayerPrefs.SetInt("VR_TurnType", (int)currentTurnType);
    PlayerPrefs.SetInt("VR_LocomotionType", (int)currentLocomotionType);
    PlayerPrefs.SetInt("VR_SnapTurn", currentTurnType == TurnTypeOption.Snap ? 1 : 0);
    PlayerPrefs.Save();
    ```
  - Lines 330–382: Runtime binding `BindButtonsRuntime()` automatically searches child buttons and text elements by exact name (`Btn_StartExpedition`, `Btn_Tutorial`, `Btn_Settings`, `Btn_Credits`, `Btn_Quit`, `Btn_SaveAndExit`, `Btn_TurnPrev`, `Btn_TurnNext`, `Btn_LocoPrev`, `Btn_LocoNext`, `Text_TurnValue`, `Text_LocoValue`).

### 1.2 Downstream Consumer of Saved Settings
- **File**: `Assets/Prospector-journey/Room/Scripts/VRSettingsApplier.cs`
  - Lines 26–27:
    ```csharp
    int turnType = PlayerPrefs.GetInt("VR_TurnType", PlayerPrefs.GetInt("VR_SnapTurn", 1) == 1 ? 0 : 1);
    int locoType = PlayerPrefs.GetInt("VR_LocomotionType", 0);
    ```
  - Lines 57–64: Toggles `SnapTurn` vs `ContinuousTurn`, and `ContinuousMove` vs `Teleportation` on `XR Origin (XR Rig)` in `Room scene` and `Tutorial scene`.

### 1.3 Scene Hierarchy & Layout Constraints
- **File**: `Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity`
  - Hierarchy dump from python parser:
    ```text
    VR_MainMenu_Canvas (GO: 1321170159)
      Board_Background (GO: 1687171002, Image color: r:0.12, g:0.09, b:0.07, a:0.5)
        Gold_Frame (GO: 1319634031, Image color: r:0.75, g:0.55, b:0.22, a:0.6)
          Inner_Board (GO: 1212624254, Image color: r:0.46, g:0.31, b:0.22, a:0.61)
            Panels_Content (GO: 264329703)
              Main_Buttons_Panel (GO: 1759804371, has VerticalLayoutGroup!)
                Btn_StartExpedition (GO: 1231874296, Rect: 620 x 85, color a:1.0)
                  Label (GO: 661137788, TMP_Text: "START EXPEDITION", fontSize: 32)
                Btn_Tutorial (GO: 86302643, Rect: 620 x 72, fontSize: 26)
                Btn_Settings (GO: 1251254214, Rect: 620 x 72, fontSize: 26)
                Btn_Credits (GO: 14464313, Rect: 620 x 72, fontSize: 26)
                Btn_Quit (GO: 709187463, Rect: 620 x 72, fontSize: 22)
              Settings_Panel (GO: 1398607371)
                Section_Title (GO: 63793025, TMP_Text: "GAME OPTIONS", fontSize: 42)
                Frame_GameOptions (GO: 1148180395, Image color: r:0.14, g:0.09, b:0.06, a:0.88!)
                  Row_TurnType (GO: 1031750534, Label fontSize: 28, Text_Value fontSize: 28)
                  Row_LocomotionType (GO: 1587154060, Label fontSize: 28, Text_Value fontSize: 28)
                Btn_SaveAndExit (GO: 1317975372, Label fontSize: 28)
    ```
  - **Measured Text Sizes**: All current buttons and options labels have `fontSize` between 22 and 32pt. None reach the required >= 36pt threshold.
  - **Measured Alphas**: `Frame_GameOptions` has `alpha = 0.88` (violates requirement alpha < 0.80), and buttons have `alpha = 1.0`.

### 1.4 Hand Models Parity
- **In `Mainmenu scene.unity`**:
  - `XR Origin (XR Rig)` is an instance of Starter Assets prefab (`Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab`, `guid: f6336ac4ac8b4d34bc5072418cdc62a0`), which does NOT contain Oculus Hands.
- **In `Tutorial scene.unity` & `Room scene.unity`**:
  - Both scenes instantiate `Assets/Prospector-journey/Tutorial/XR Origin (XR Rig).prefab` (`guid: a2a0d931eebd6a1479e56fc82c3b78db`).
  - Contains child prefabs:
    - `Left Hand Model.prefab` (`guid: 65847589f926b734d9eaed6e25f0046b`) at local pos `(-0.001, 0.001, -0.035)`, Euler rotation `(0, 0, 90)`.
    - `Right Hand Model.prefab` (`guid: 1fd3a7c43c8190f4d8c167ebde694364`) at local pos `(-0.001, 0.001, -0.035)`, Euler rotation `(0, 0, -90)`.
    - Both prefabs include the active component `AnimateHandOnInput.cs` (`guid: d4dde6e94d9e27d4591597be91e15d7d`).

### 1.5 Existing Play Mode Audit Harnesses & MCP Execution
- **File**: `Assets/Prospector-journey/Tutorial/Scripts/TutorialPlayModeAuditRunner.cs` & `TutorialPlayModeRunner.cs`
  - Utilizes `[DefaultExecutionOrder(30000)]`, hooks `Application.logMessageReceived`, runs multi-frame coroutines, and asserts test items.
  - Exposes `[MenuItem]` runnable via Unity MCP `Unity_ManageMenuItem` (`MenuPath: "ProspectorJourney/Run Tutorial Play Mode Audit"`).
- **File**: `Assets/Prospector-journey/Mainmenu/Editor/MainMenuOptionsAuditRunner.cs` & `Scripts/MainMenuOptionsPlayModeAudit.cs`
  - Exposes `[MenuItem("ProspectorJourney/Run Game Options Play Mode Audit")]`. Currently validates 5 basic checks (Hierarchy, Settings Open, Turn Cycle, Loco Cycle, Save & Exit persistence).

---

## 2. Logic Chain

1. **Task 1 & 2 (Interactive Logic & Options Persistence)**:
   - Observation 1.1 reveals that `MainMenuVRController` is fully functional and already handles cycling and saving to `PlayerPrefs` (`VR_TurnType`, `VR_LocomotionType`, `VR_SnapTurn`).
   - Observation 1.2 demonstrates that `VRSettingsApplier.cs` in downstream scenes relies on these exact keys.
   - Therefore, the data layer and persistence logic must remain identical. The overhaul only needs to update typography size, UI container styling, and button dimensions.

2. **Task 3 (Badge Design & Constraints)**:
   - Observation 1.3 shows that `Main_Buttons_Panel` uses a `VerticalLayoutGroup`.
   - Any sibling added to `Main_Buttons_Panel` will be treated as an additional vertical button row and pushed down.
   - Therefore, `Badge_ComingSoon` must be added as a direct child of `Btn_StartExpedition`, anchored to the right (`anchorMin=(1, 0.5), anchorMax=(1, 0.5), pivot=(1, 0.5)`).
   - Setting `raycastTarget = false` on the badge Image and TMP_Text ensures VR controller raycasts continue to hit `Btn_StartExpedition`.
   - Modifying `OnClickStartExpedition()` to provide feedback without invoking `LoadSceneAsync("Village")` fulfills R3 while preventing the simulator `NullReferenceException` seen in console logs.

3. **Task 4 & 5 (Test Harness Architecture & Checklist)**:
   - Observation 1.5 shows that `MainMenuOptionsPlayModeAudit` only tests panel toggling and option cycling.
   - Requirement R4 demands checking translucency (alpha < 0.8), typography (>= 36pt), hand models + `AnimateHandOnInput`, "COMING SOON" badge presence, and 0 console errors.
   - Therefore, expanding the audit suite into an all-encompassing `MainMenuPlayModeAudit.cs` with an Editor runner `MainMenuPlayModeRunner.cs` (or extending `MainMenuOptionsPlayModeAudit.cs`) is the cleanest, battle-tested approach that can be executed directly via `Unity_ManageMenuItem`.

---

## 3. Caveats

1. **Editor Play Mode Transition**: When entering Play Mode via `EditorApplication.isPlaying = true` or `Unity_ManageMenuItem`, Unity requires 1–2 frames for Awake/Start and XR simulator initialization to settle. Coroutines must yield before running assertions.
2. **TextMeshPro Auto-Sizing**: If `enableAutoSizing` is checked on any TMP_Text, it may downscale the rendered font size below 36pt if RectTransform height is constrained. Layout row heights must be expanded to 80–90px to ensure the font size stays >= 36pt.

---

## 4. Conclusion

1. **Interactive Logic & Options Persistence**: The existing code in `MainMenuVRController.cs` has full cycling and `PlayerPrefs` persistence logic matching the project's data contract (`VR_TurnType`, `VR_LocomotionType`, `VR_SnapTurn`).
2. **"COMING SOON" Badge**: Must be placed under `Btn_StartExpedition` as `Badge_ComingSoon` to avoid interfering with `VerticalLayoutGroup`. Styled with amber/gold pill background and bold TMP_Text, with `raycastTarget = false`.
3. **VR Hands**: Must replace/supplement the XR Rig in `Mainmenu scene` with `Left Hand Model.prefab` and `Right Hand Model.prefab` containing `AnimateHandOnInput.cs`.
4. **Test Harness Architecture**: An automated Play Mode audit suite structured with `[DefaultExecutionOrder(30000)]`, `Application.logMessageReceived` listener, and MenuItem runner will reliably verify all 6 acceptance criteria via Unity MCP.

---

## 5. Verification Method

To independently verify these findings:
1. **Inspect Scripts**:
   ```powershell
   view_file "Assets/Prospector-journey/Mainmenu/Scripts/MainMenuVRController.cs" lines 230 to 275
   view_file "Assets/Prospector-journey/Room/Scripts/VRSettingsApplier.cs" lines 20 to 65
   ```
2. **Inspect Main Menu Hierarchy**:
   Run `.agents/teamwork/explorer_survey_mainmenu_3/inspect_scene.py` to confirm `VR_MainMenu_Canvas` structure and `VerticalLayoutGroup` on `Main_Buttons_Panel`.
3. **Verify Existing Menu Items**:
   Use `Unity_ManageMenuItem` (`Action: "List", Search: "ProspectorJourney"`) to confirm runnable test runners in the Unity Editor.
