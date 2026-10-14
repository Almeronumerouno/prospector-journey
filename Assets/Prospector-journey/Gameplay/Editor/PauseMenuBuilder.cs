using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace ProspectorJourney.UI.Editor
{
    public static class PauseMenuBuilder
    {
        private const string RoomScenePath = "Assets/Prospector-journey/Room/Room scene.unity";
        private const string TutorialScenePath = "Assets/Prospector-journey/Tutorial/Tutorial scene.unity";
        private const string FontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset";
        private const string HoverClipPath = "Assets/Assets-meet3/_Course Library/Audio/FX/SFX_UI_Click_1.ogg";
        private const string ClickClipPath = "Assets/Assets-meet3/_Course Library/Audio/FX/SFX_UI_Button_1.ogg";

        // Exact palette from Main Menu (Translucent slate glass & warm gold wireframe)
        private static readonly Color ColorBoardBg = new Color(0.08f, 0.14f, 0.20f, 0.38f);
        private static readonly Color ColorGoldBorder = new Color(0.96f, 0.82f, 0.36f, 0.88f);
        private static readonly Color ColorTitleGold = new Color(1.0f, 0.92f, 0.65f, 1.0f);
        private static readonly Color ColorSubtitleGold = new Color(0.88f, 0.78f, 0.52f, 0.92f);

        private static readonly Color ColorBtnNormal = new Color(0.10f, 0.16f, 0.22f, 0.42f);
        private static readonly Color ColorBtnHighlight = new Color(0.24f, 0.36f, 0.48f, 0.65f);
        private static readonly Color ColorBtnPressed = new Color(0.36f, 0.50f, 0.64f, 0.85f);
        private static readonly Color ColorBtnBorder = new Color(0.92f, 0.78f, 0.35f, 0.75f);
        private static readonly Color ColorBtnText = new Color(0.98f, 0.98f, 0.95f, 1.0f);

        private static readonly Color ColorArrow = new Color(1.0f, 0.82f, 0.25f, 1.0f);
        private static readonly Color ColorOptionValue = new Color(0.98f, 0.95f, 0.85f, 1.0f);
        private static readonly Color ColorOptionLabel = new Color(0.98f, 0.95f, 0.88f, 1.0f);

        [MenuItem("ProspectorJourney/Build Pause Menu in Room and Tutorial Scenes")]
        public static void BuildInAllScenes()
        {
            BuildInScene(RoomScenePath);
            BuildInScene(TutorialScenePath);
            Debug.Log("[PauseMenuBuilder] Successfully built Pause UI in Room scene and Tutorial scene!");
        }

        [MenuItem("ProspectorJourney/Build Pause Menu in Room Scene")]
        public static void BuildInRoomScene()
        {
            BuildInScene(RoomScenePath);
        }

        [MenuItem("ProspectorJourney/Build Pause Menu in Tutorial Scene")]
        public static void BuildInTutorialScene()
        {
            BuildInScene(TutorialScenePath);
        }

        public static void BuildInScene(string scenePath)
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != scenePath)
            {
                EditorSceneManager.OpenScene(scenePath);
                activeScene = EditorSceneManager.GetActiveScene();
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[PauseMenuBuilder] Font not found at {FontPath}");
                return;
            }

            AudioClip hoverClip = AssetDatabase.LoadAssetAtPath<AudioClip>(HoverClipPath);
            AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClickClipPath);

            // 1. Check or create root [Pause_Menu]
            GameObject pauseRoot = GameObject.Find("[Pause_Menu]");
            if (pauseRoot == null)
            {
                pauseRoot = new GameObject("[Pause_Menu]");
            }

            // Remove existing children for clean rebuild
            for (int i = pauseRoot.transform.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(pauseRoot.transform.GetChild(i).gameObject);
            }

            // Add PauseMenuController & AudioSource
            PauseMenuController controller = pauseRoot.GetComponent<PauseMenuController>();
            if (controller == null)
            {
                controller = pauseRoot.AddComponent<PauseMenuController>();
            }

            AudioSource audioSource = pauseRoot.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = pauseRoot.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.ignoreListenerPause = true;

            // 2. Create Canvas: Pause_Canvas
            GameObject canvasObj = new GameObject("Pause_Canvas", typeof(RectTransform));
            canvasObj.transform.SetParent(pauseRoot.transform, false);
            RectTransform canvasRT = canvasObj.GetComponent<RectTransform>();
            canvasRT.sizeDelta = new Vector2(1500f, 950f);
            canvasRT.localScale = Vector3.one * 0.00165f;

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3f;
            scaler.referencePixelsPerUnit = 100f;

            GraphicRaycaster gr = canvasObj.AddComponent<GraphicRaycaster>();
            TrackedDeviceGraphicRaycaster tdgr = canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();
            tdgr.checkFor2DOcclusion = false;

            // 3. Board Background (Translucent slate glass + warm gold wireframe)
            GameObject boardBg = new GameObject("Board_Background", typeof(RectTransform), typeof(Image));
            boardBg.transform.SetParent(canvasObj.transform, false);
            RectTransform boardRT = boardBg.GetComponent<RectTransform>();
            boardRT.anchorMin = Vector2.zero;
            boardRT.anchorMax = Vector2.one;
            boardRT.sizeDelta = Vector2.zero;
            boardRT.anchoredPosition = Vector2.zero;

            Image boardImg = boardBg.GetComponent<Image>();
            boardImg.color = ColorBoardBg;

            CreateWireframeBorder(boardBg, ColorGoldBorder, 3.5f);

            // 4. Header Section
            GameObject headerSec = new GameObject("Header_Section", typeof(RectTransform));
            headerSec.transform.SetParent(boardBg.transform, false);
            RectTransform headerRT = headerSec.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0f, 1f);
            headerRT.anchorMax = new Vector2(1f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.anchoredPosition = new Vector2(0f, -25f);
            headerRT.sizeDelta = new Vector2(0f, 140f);

            // Title Text: "PROSPECTOR JOURNEY"
            GameObject titleObj = new GameObject("Title_Text", typeof(RectTransform));
            titleObj.transform.SetParent(headerSec.transform, false);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, 0f);
            titleRT.sizeDelta = new Vector2(1300f, 80f);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.font = font;
            titleTMP.text = "PROSPECTOR JOURNEY";
            titleTMP.fontSize = 64f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = ColorTitleGold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.characterSpacing = 3f;

            // Subtitle Text: "VR GOLD PANNING & RIVER CLAIM SIMULATION"
            GameObject subObj = new GameObject("Subtitle_Text", typeof(RectTransform));
            subObj.transform.SetParent(headerSec.transform, false);
            RectTransform subRT = subObj.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.5f, 1f);
            subRT.anchorMax = new Vector2(0.5f, 1f);
            subRT.pivot = new Vector2(0.5f, 1f);
            subRT.anchoredPosition = new Vector2(0f, -74f);
            subRT.sizeDelta = new Vector2(1300f, 40f);

            TextMeshProUGUI subTMP = subObj.AddComponent<TextMeshProUGUI>();
            subTMP.font = font;
            subTMP.text = "VR GOLD PANNING & RIVER CLAIM SIMULATION";
            subTMP.fontSize = 24f;
            subTMP.fontStyle = FontStyles.Bold;
            subTMP.color = ColorSubtitleGold;
            subTMP.alignment = TextAlignmentOptions.Center;
            subTMP.characterSpacing = 2f;

            // Horizontal Gold Divider Line
            CreateHorizontalDivider(headerSec.transform, new Vector2(0f, -125f), 1200f);

            // 5. Panels Content Container
            GameObject panelsContent = new GameObject("Panels_Content", typeof(RectTransform));
            panelsContent.transform.SetParent(boardBg.transform, false);
            RectTransform panelsRT = panelsContent.GetComponent<RectTransform>();
            panelsRT.anchorMin = Vector2.zero;
            panelsRT.anchorMax = Vector2.one;
            panelsRT.pivot = new Vector2(0.5f, 0.5f);
            panelsRT.anchoredPosition = new Vector2(0f, -65f);
            panelsRT.sizeDelta = new Vector2(-100f, -200f);

            // 6. Pause Main Panel (Resume, Game Options, Quit)
            GameObject pauseMainPanel = new GameObject("Pause_Main_Panel", typeof(RectTransform));
            pauseMainPanel.transform.SetParent(panelsContent.transform, false);
            RectTransform pmpRT = pauseMainPanel.GetComponent<RectTransform>();
            pmpRT.anchorMin = Vector2.zero;
            pmpRT.anchorMax = Vector2.one;
            pmpRT.anchoredPosition = Vector2.zero;
            pmpRT.sizeDelta = Vector2.zero;

            // Section Title: "GAME PAUSED"
            GameObject pmpTitleObj = new GameObject("Section_Title", typeof(RectTransform));
            pmpTitleObj.transform.SetParent(pauseMainPanel.transform, false);
            RectTransform pmpTitleRT = pmpTitleObj.GetComponent<RectTransform>();
            pmpTitleRT.anchorMin = new Vector2(0.5f, 1f);
            pmpTitleRT.anchorMax = new Vector2(0.5f, 1f);
            pmpTitleRT.pivot = new Vector2(0.5f, 1f);
            pmpTitleRT.anchoredPosition = new Vector2(0f, -15f);
            pmpTitleRT.sizeDelta = new Vector2(900f, 65f);

            TextMeshProUGUI pmpTitleTMP = pmpTitleObj.AddComponent<TextMeshProUGUI>();
            pmpTitleTMP.font = font;
            pmpTitleTMP.text = "GAME PAUSED";
            pmpTitleTMP.fontSize = 54f;
            pmpTitleTMP.fontStyle = FontStyles.Bold;
            pmpTitleTMP.color = ColorTitleGold;
            pmpTitleTMP.alignment = TextAlignmentOptions.Center;
            pmpTitleTMP.characterSpacing = 3f;

            // 3 Buttons: Resume, Game Options, Quit
            Button btnResume = CreateStyledButton(pauseMainPanel.transform, font, "Btn_Resume", "RESUME", new Vector2(0f, 55f));
            Button btnOptions = CreateStyledButton(pauseMainPanel.transform, font, "Btn_GameOptions", "GAME OPTIONS", new Vector2(0f, -60f));
            Button btnQuit = CreateStyledButton(pauseMainPanel.transform, font, "Btn_Quit", "QUIT TO MAIN MENU", new Vector2(0f, -175f));

            // 7. Settings Panel (Game Options - identik, plek ketiplek dengan Main Menu)
            GameObject settingsPanel = new GameObject("Settings_Panel", typeof(RectTransform));
            settingsPanel.transform.SetParent(panelsContent.transform, false);
            RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
            spRT.anchorMin = Vector2.zero;
            spRT.anchorMax = Vector2.one;
            spRT.anchoredPosition = Vector2.zero;
            spRT.sizeDelta = Vector2.zero;

            // Section Title: "GAME OPTIONS"
            GameObject optTitleObj = new GameObject("Section_Title", typeof(RectTransform));
            optTitleObj.transform.SetParent(settingsPanel.transform, false);
            RectTransform optTitleRT = optTitleObj.GetComponent<RectTransform>();
            optTitleRT.anchorMin = new Vector2(0.5f, 1f);
            optTitleRT.anchorMax = new Vector2(0.5f, 1f);
            optTitleRT.pivot = new Vector2(0.5f, 1f);
            optTitleRT.anchoredPosition = new Vector2(0f, -15f);
            optTitleRT.sizeDelta = new Vector2(900f, 76f);

            TextMeshProUGUI optTitleTMP = optTitleObj.AddComponent<TextMeshProUGUI>();
            optTitleTMP.font = font;
            optTitleTMP.text = "GAME OPTIONS";
            optTitleTMP.fontSize = 58f;
            optTitleTMP.fontStyle = FontStyles.Bold;
            optTitleTMP.color = ColorTitleGold;
            optTitleTMP.alignment = TextAlignmentOptions.Center;
            optTitleTMP.characterSpacing = 3f;

            // Row 1: Turn Type
            TextMeshProUGUI turnValueText;
            Button turnPrevBtn, turnNextBtn;
            CreateOptionRow(settingsPanel.transform, font, "Row_TurnType", "TURN TYPE", "SNAP",
                "Btn_TurnPrev", "Btn_TurnNext", "Text_TurnValue",
                new Vector2(0f, 40f),
                out turnValueText, out turnPrevBtn, out turnNextBtn);

            // Row 2: Locomotion Type
            TextMeshProUGUI locoValueText;
            Button locoPrevBtn, locoNextBtn;
            CreateOptionRow(settingsPanel.transform, font, "Row_LocomotionType", "LOCOMOTION TYPE", "TELEPORT",
                "Btn_LocoPrev", "Btn_LocoNext", "Text_LocoValue",
                new Vector2(0f, -50f),
                out locoValueText, out locoPrevBtn, out locoNextBtn);

            // Save and Exit Button
            GameObject saveBtnObj = new GameObject("Btn_SaveAndExit", typeof(RectTransform));
            saveBtnObj.transform.SetParent(settingsPanel.transform, false);
            RectTransform saveRT = saveBtnObj.GetComponent<RectTransform>();
            saveRT.anchorMin = new Vector2(0.5f, 0f);
            saveRT.anchorMax = new Vector2(0.5f, 0f);
            saveRT.pivot = new Vector2(0.5f, 0f);
            saveRT.anchoredPosition = new Vector2(0f, 22f);
            saveRT.sizeDelta = new Vector2(520f, 84f);

            Image saveImg = saveBtnObj.AddComponent<Image>();
            saveImg.color = ColorBtnNormal;

            Button saveBtn = saveBtnObj.AddComponent<Button>();
            var cbSave = saveBtn.colors;
            cbSave.normalColor = ColorBtnNormal;
            cbSave.highlightedColor = ColorBtnHighlight;
            cbSave.pressedColor = ColorBtnPressed;
            saveBtn.colors = cbSave;
            CreateWireframeBorder(saveBtnObj, ColorBtnBorder, 2.5f);
            saveBtnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject saveLabel = new GameObject("Text", typeof(RectTransform));
            saveLabel.transform.SetParent(saveBtnObj.transform, false);
            RectTransform saveLblRT = saveLabel.GetComponent<RectTransform>();
            saveLblRT.anchorMin = Vector2.zero;
            saveLblRT.anchorMax = Vector2.one;
            saveLblRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI saveTMP = saveLabel.AddComponent<TextMeshProUGUI>();
            saveTMP.font = font;
            saveTMP.text = "SAVE AND EXIT";
            saveTMP.fontSize = 42f;
            saveTMP.fontStyle = FontStyles.Bold;
            saveTMP.color = ColorTitleGold;
            saveTMP.alignment = TextAlignmentOptions.Center;
            saveTMP.characterSpacing = 2f;

            // 8. Connect Serialized Properties in PauseMenuController
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("pauseMenuCanvas").objectReferenceValue = canvasObj;
            so.FindProperty("pausePanel").objectReferenceValue = pauseMainPanel;
            so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;

            so.FindProperty("btnResume").objectReferenceValue = btnResume;
            so.FindProperty("btnGameOptions").objectReferenceValue = btnOptions;
            so.FindProperty("btnQuit").objectReferenceValue = btnQuit;

            so.FindProperty("turnTypeText").objectReferenceValue = turnValueText;
            so.FindProperty("locomotionTypeText").objectReferenceValue = locoValueText;
            so.FindProperty("turnPrevButton").objectReferenceValue = turnPrevBtn;
            so.FindProperty("turnNextButton").objectReferenceValue = turnNextBtn;
            so.FindProperty("locomotionPrevButton").objectReferenceValue = locoPrevBtn;
            so.FindProperty("locomotionNextButton").objectReferenceValue = locoNextBtn;
            so.FindProperty("saveAndExitButton").objectReferenceValue = saveBtn;

            so.FindProperty("audioSource").objectReferenceValue = audioSource;
            so.FindProperty("hoverClip").objectReferenceValue = hoverClip;
            so.FindProperty("clickClip").objectReferenceValue = clickClip;

            so.FindProperty("followDistance").floatValue = 1.35f;
            so.FindProperty("heightOffset").floatValue = 0.04f;
            so.FindProperty("mainMenuScene").stringValue = "Mainmenu scene";

            so.ApplyModifiedProperties();

            // Default State: canvas inactive, pause panel active, settings panel inactive
            pauseMainPanel.SetActive(true);
            settingsPanel.SetActive(false);
            canvasObj.SetActive(false);

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(pauseRoot);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log($"[PauseMenuBuilder] Successfully built and saved [Pause_Menu] in '{scenePath}'");
        }

        private static Button CreateStyledButton(Transform parent, TMP_FontAsset font, string name, string labelText, Vector2 pos)
        {
            GameObject btnObj = new GameObject(name, typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(840f, 92f);

            Image img = btnObj.AddComponent<Image>();
            img.color = ColorBtnNormal;

            Button btn = btnObj.AddComponent<Button>();
            var cb = btn.colors;
            cb.normalColor = ColorBtnNormal;
            cb.highlightedColor = ColorBtnHighlight;
            cb.pressedColor = ColorBtnPressed;
            btn.colors = cb;

            CreateWireframeBorder(btnObj, ColorBtnBorder, 2.0f);
            btnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject labelObj = new GameObject("Text", typeof(RectTransform));
            labelObj.transform.SetParent(btnObj.transform, false);
            RectTransform lblRT = labelObj.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = labelText;
            tmp.fontSize = 46f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColorBtnText;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.characterSpacing = 2f;

            return btn;
        }

        private static void CreateOptionRow(Transform parent, TMP_FontAsset font, string name, string labelText, string defaultValue,
            string prevBtnName, string nextBtnName, string valTextName,
            Vector2 localPos, out TextMeshProUGUI valueText, out Button prevBtn, out Button nextBtn)
        {
            GameObject rowObj = new GameObject(name, typeof(RectTransform));
            rowObj.transform.SetParent(parent, false);
            RectTransform rowRT = rowObj.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 0.5f);
            rowRT.anchorMax = new Vector2(0.5f, 0.5f);
            rowRT.pivot = new Vector2(0.5f, 0.5f);
            rowRT.anchoredPosition = localPos;
            rowRT.sizeDelta = new Vector2(1040f, 90f);

            Image rowImg = rowObj.AddComponent<Image>();
            rowImg.color = new Color(0f, 0f, 0f, 0f);

            // Left Label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(rowObj.transform, false);
            RectTransform labelRT = labelObj.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0.5f);
            labelRT.anchorMax = new Vector2(0f, 0.5f);
            labelRT.pivot = new Vector2(0f, 0.5f);
            labelRT.anchoredPosition = new Vector2(35f, 0f);
            labelRT.sizeDelta = new Vector2(480f, 70f);

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.font = font;
            labelTMP.text = labelText;
            labelTMP.fontSize = 46f;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = ColorOptionLabel;
            labelTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Selector Container
            GameObject selectorObj = new GameObject("Selector_Group", typeof(RectTransform));
            selectorObj.transform.SetParent(rowObj.transform, false);
            RectTransform selRT = selectorObj.GetComponent<RectTransform>();
            selRT.anchorMin = new Vector2(1f, 0.5f);
            selRT.anchorMax = new Vector2(1f, 0.5f);
            selRT.pivot = new Vector2(1f, 0.5f);
            selRT.anchoredPosition = new Vector2(-25f, 0f);
            selRT.sizeDelta = new Vector2(480f, 80f);

            Image selImg = selectorObj.AddComponent<Image>();
            selImg.color = new Color(0f, 0f, 0f, 0f);

            // Prev Button (<)
            GameObject prevBtnObj = new GameObject(prevBtnName, typeof(RectTransform));
            prevBtnObj.transform.SetParent(selectorObj.transform, false);
            RectTransform prevRT = prevBtnObj.GetComponent<RectTransform>();
            prevRT.anchorMin = new Vector2(0f, 0.5f);
            prevRT.anchorMax = new Vector2(0f, 0.5f);
            prevRT.pivot = new Vector2(0f, 0.5f);
            prevRT.anchoredPosition = new Vector2(36f, 0f);
            prevRT.sizeDelta = new Vector2(64f, 64f);

            Image prevImg = prevBtnObj.AddComponent<Image>();
            prevImg.color = new Color(0f, 0f, 0f, 0f);

            prevBtn = prevBtnObj.AddComponent<Button>();
            var cbPrev = prevBtn.colors;
            cbPrev.normalColor = new Color(0f, 0f, 0f, 0f);
            cbPrev.highlightedColor = new Color(1f, 1f, 1f, 0.15f);
            cbPrev.pressedColor = new Color(1f, 1f, 1f, 0.30f);
            prevBtn.colors = cbPrev;
            prevBtnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject prevLabel = new GameObject("Arrow", typeof(RectTransform));
            prevLabel.transform.SetParent(prevBtnObj.transform, false);
            RectTransform prevLblRT = prevLabel.GetComponent<RectTransform>();
            prevLblRT.anchorMin = Vector2.zero;
            prevLblRT.anchorMax = Vector2.one;
            prevLblRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI prevTMP = prevLabel.AddComponent<TextMeshProUGUI>();
            prevTMP.font = font;
            prevTMP.text = "<";
            prevTMP.fontSize = 50f;
            prevTMP.fontStyle = FontStyles.Bold;
            prevTMP.color = ColorArrow;
            prevTMP.alignment = TextAlignmentOptions.Center;

            // Value Text
            GameObject valObj = new GameObject(valTextName, typeof(RectTransform));
            valObj.transform.SetParent(selectorObj.transform, false);
            RectTransform valRT = valObj.GetComponent<RectTransform>();
            valRT.anchorMin = new Vector2(0.5f, 0.5f);
            valRT.anchorMax = new Vector2(0.5f, 0.5f);
            valRT.pivot = new Vector2(0.5f, 0.5f);
            valRT.anchoredPosition = Vector2.zero;
            valRT.sizeDelta = new Vector2(320f, 70f);

            valueText = valObj.AddComponent<TextMeshProUGUI>();
            valueText.font = font;
            valueText.text = defaultValue;
            valueText.fontSize = 44f;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = ColorOptionValue;
            valueText.alignment = TextAlignmentOptions.Center;

            // Next Button (>)
            GameObject nextBtnObj = new GameObject(nextBtnName, typeof(RectTransform));
            nextBtnObj.transform.SetParent(selectorObj.transform, false);
            RectTransform nextRT = nextBtnObj.GetComponent<RectTransform>();
            nextRT.anchorMin = new Vector2(1f, 0.5f);
            nextRT.anchorMax = new Vector2(1f, 0.5f);
            nextRT.pivot = new Vector2(1f, 0.5f);
            nextRT.anchoredPosition = new Vector2(-36f, 0f);
            nextRT.sizeDelta = new Vector2(64f, 64f);

            Image nextImg = nextBtnObj.AddComponent<Image>();
            nextImg.color = new Color(0f, 0f, 0f, 0f);

            nextBtn = nextBtnObj.AddComponent<Button>();
            var cbNext = nextBtn.colors;
            cbNext.normalColor = new Color(0f, 0f, 0f, 0f);
            cbNext.highlightedColor = new Color(1f, 1f, 1f, 0.15f);
            cbNext.pressedColor = new Color(1f, 1f, 1f, 0.30f);
            nextBtn.colors = cbNext;
            nextBtnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject nextLabel = new GameObject("Arrow", typeof(RectTransform));
            nextLabel.transform.SetParent(nextBtnObj.transform, false);
            RectTransform nextLblRT = nextLabel.GetComponent<RectTransform>();
            nextLblRT.anchorMin = Vector2.zero;
            nextLblRT.anchorMax = Vector2.one;
            nextLblRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI nextTMP = nextLabel.AddComponent<TextMeshProUGUI>();
            nextTMP.font = font;
            nextTMP.text = ">";
            nextTMP.fontSize = 50f;
            nextTMP.fontStyle = FontStyles.Bold;
            nextTMP.color = ColorArrow;
            nextTMP.alignment = TextAlignmentOptions.Center;
        }

        private static void CreateHorizontalDivider(Transform parent, Vector2 pos, float width)
        {
            GameObject div = new GameObject("Line_Divider", typeof(RectTransform), typeof(Image));
            div.transform.SetParent(parent, false);
            RectTransform rt = div.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(width, 1.5f);

            Image img = div.GetComponent<Image>();
            img.color = new Color(0.96f, 0.82f, 0.36f, 0.35f);
            img.raycastTarget = false;
        }

        private static void CreateWireframeBorder(GameObject parent, Color borderColor, float thickness = 3.5f)
        {
            Transform existing = parent.transform.Find("Wireframe_Border");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            GameObject borderRoot = new GameObject("Wireframe_Border", typeof(RectTransform));
            borderRoot.transform.SetParent(parent.transform, false);
            RectTransform rootRT = borderRoot.GetComponent<RectTransform>();
            rootRT.anchorMin = Vector2.zero;
            rootRT.anchorMax = Vector2.one;
            rootRT.sizeDelta = Vector2.zero;
            rootRT.anchoredPosition = Vector2.zero;

            // Top border line
            CreateBorderLine(borderRoot.transform, borderColor,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, 0f), new Vector2(0f, thickness));

            // Bottom border line
            CreateBorderLine(borderRoot.transform, borderColor,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 0f), new Vector2(0f, thickness));

            // Left border line
            CreateBorderLine(borderRoot.transform, borderColor,
                new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f),
                new Vector2(0f, 0f), new Vector2(thickness, 0f));

            // Right border line
            CreateBorderLine(borderRoot.transform, borderColor,
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(1f, 0.5f),
                new Vector2(0f, 0f), new Vector2(thickness, 0f));
        }

        private static void CreateBorderLine(Transform parent, Color col, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            GameObject line = new GameObject("Line", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(parent, false);
            RectTransform rt = line.GetComponent<RectTransform>();
            rt.anchorMin = aMin;
            rt.anchorMax = aMax;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            Image img = line.GetComponent<Image>();
            img.color = col;
            img.raycastTarget = false;
        }
    }
}
