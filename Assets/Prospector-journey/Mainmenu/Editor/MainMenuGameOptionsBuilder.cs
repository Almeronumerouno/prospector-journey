using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProspectorJourney.UI.Editor
{
    public static class MainMenuGameOptionsBuilder
    {
        private const string ScenePath = "Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity";
        private const string FontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset";

        [MenuItem("ProspectorJourney/Setup Main Menu Game Options")]
        public static void BuildGameOptionsUI()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[GameOptionsBuilder] Failed to load font at {FontPath}");
                return;
            }

            var canvasObj = GameObject.Find("VR_MainMenu_Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[GameOptionsBuilder] Could not find VR_MainMenu_Canvas!");
                return;
            }

            var controller = canvasObj.GetComponent<MainMenuVRController>();
            if (controller == null)
            {
                Debug.LogError("[GameOptionsBuilder] MainMenuVRController component missing on VR_MainMenu_Canvas!");
                return;
            }

            Transform innerBoard = canvasObj.transform.Find("Board_Background/Gold_Frame/Inner_Board");
            if (innerBoard == null)
            {
                Debug.LogError("[GameOptionsBuilder] Could not find Inner_Board!");
                return;
            }

            Transform panelsContent = innerBoard.Find("Panels_Content");
            if (panelsContent == null)
            {
                Debug.LogError("[GameOptionsBuilder] Could not find Panels_Content!");
                return;
            }

            Transform settingsPanel = panelsContent.Find("Settings_Panel");
            if (settingsPanel == null)
            {
                Debug.LogError("[GameOptionsBuilder] Could not find Settings_Panel!");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(settingsPanel.gameObject, "Setup Game Options UI");

            // 1. Destroy old rows in Settings_Panel
            List<GameObject> toRemove = new List<GameObject>();
            foreach (Transform child in settingsPanel)
            {
                toRemove.Add(child.gameObject);
            }
            foreach (var go in toRemove)
            {
                Object.DestroyImmediate(go);
            }

            // Ensure Settings_Panel RectTransform stretches to parent and has no overriding layout group
            VerticalLayoutGroup vlg = settingsPanel.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Object.DestroyImmediate(vlg);
            }

            RectTransform panelRT = settingsPanel.GetComponent<RectTransform>();
            panelRT.anchorMin = Vector2.zero;
            panelRT.anchorMax = Vector2.one;
            panelRT.anchoredPosition = Vector2.zero;
            panelRT.sizeDelta = Vector2.zero;

            // Colors
            Color colHeader = new Color(1f, 0.88f, 0.55f, 1f);       // Gold Cream
            Color colCardBg = new Color(0.14f, 0.09f, 0.06f, 0.88f);   // Translucent Rustic Dark Wood
            Color colCardBorder = new Color(0.65f, 0.48f, 0.28f, 0.85f); // Amber Gold Border
            Color colRowBg = new Color(0.22f, 0.15f, 0.10f, 0.75f);    // Subtle strip background
            Color colLabel = new Color(1f, 0.95f, 0.85f, 1f);        // Warm parchment cream
            Color colValue = Color.white;
            Color colArrow = new Color(1f, 0.86f, 0.45f, 1f);        // Vibrant Gold Chevron
            Color colBtnNormal = new Color(0.26f, 0.17f, 0.12f, 1f);
            Color colBtnHighlight = new Color(0.52f, 0.36f, 0.23f, 1f);
            Color colBtnPressed = new Color(0.68f, 0.46f, 0.28f, 1f);

            // 2. Title: GAME OPTIONS
            GameObject titleObj = new GameObject("Section_Title");
            titleObj.transform.SetParent(settingsPanel, false);
            RectTransform titleRT = titleObj.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = new Vector2(0f, -50f);
            titleRT.sizeDelta = new Vector2(800f, 60f);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.font = font;
            titleTMP.text = "GAME OPTIONS";
            titleTMP.fontSize = 42;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = colHeader;
            titleTMP.characterSpacing = 3f;

            // 3. Card Container: Frame_GameOptions
            GameObject cardObj = new GameObject("Frame_GameOptions");
            cardObj.transform.SetParent(settingsPanel, false);
            RectTransform cardRT = cardObj.AddComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.5f, 1f);
            cardRT.anchorMax = new Vector2(0.5f, 1f);
            cardRT.pivot = new Vector2(0.5f, 0.5f);
            cardRT.anchoredPosition = new Vector2(0f, -235f);
            cardRT.sizeDelta = new Vector2(960f, 260f);

            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = colCardBg;

            Outline cardOutline = cardObj.AddComponent<Outline>();
            cardOutline.effectColor = colCardBorder;
            cardOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Row 1: TURN TYPE
            GameObject rowTurn = CreateOptionRow(cardObj.transform, font, "Row_TurnType", "TURN TYPE", "SNAP", 
                new Vector2(0f, 48f), colRowBg, colLabel, colValue, colArrow, colBtnNormal, colBtnHighlight, colBtnPressed,
                out TextMeshProUGUI turnValueText, out Button turnPrevBtn, out Button turnNextBtn);

            // Row 2: LOCOMOTION TYPE
            GameObject rowLoco = CreateOptionRow(cardObj.transform, font, "Row_LocomotionType", "LOCOMOTION TYPE", "TELEPORT", 
                new Vector2(0f, -48f), colRowBg, colLabel, colValue, colArrow, colBtnNormal, colBtnHighlight, colBtnPressed,
                out TextMeshProUGUI locoValueText, out Button locoPrevBtn, out Button locoNextBtn);

            // 4. SAVE AND EXIT Button
            GameObject saveBtnObj = new GameObject("Btn_SaveAndExit");
            saveBtnObj.transform.SetParent(settingsPanel, false);
            RectTransform saveBtnRT = saveBtnObj.AddComponent<RectTransform>();
            saveBtnRT.anchorMin = new Vector2(0.5f, 1f);
            saveBtnRT.anchorMax = new Vector2(0.5f, 1f);
            saveBtnRT.pivot = new Vector2(0.5f, 0.5f);
            saveBtnRT.anchoredPosition = new Vector2(0f, -440f);
            saveBtnRT.sizeDelta = new Vector2(440f, 68f);

            Image saveBtnImg = saveBtnObj.AddComponent<Image>();
            saveBtnImg.color = colBtnNormal;

            Outline saveOutline = saveBtnObj.AddComponent<Outline>();
            saveOutline.effectColor = colCardBorder;
            saveOutline.effectDistance = new Vector2(2f, -2f);

            Button saveBtn = saveBtnObj.AddComponent<Button>();
            var cb = saveBtn.colors;
            cb.normalColor = colBtnNormal;
            cb.highlightedColor = colBtnHighlight;
            cb.pressedColor = colBtnPressed;
            cb.selectedColor = colBtnHighlight;
            saveBtn.colors = cb;
            saveBtnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject saveLabelObj = new GameObject("Label");
            saveLabelObj.transform.SetParent(saveBtnObj.transform, false);
            RectTransform saveLabelRT = saveLabelObj.AddComponent<RectTransform>();
            saveLabelRT.anchorMin = Vector2.zero;
            saveLabelRT.anchorMax = Vector2.one;
            saveLabelRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI saveTMP = saveLabelObj.AddComponent<TextMeshProUGUI>();
            saveTMP.font = font;
            saveTMP.text = "SAVE AND EXIT";
            saveTMP.fontSize = 28;
            saveTMP.fontStyle = FontStyles.Bold;
            saveTMP.alignment = TextAlignmentOptions.Center;
            saveTMP.color = new Color(1f, 0.96f, 0.90f, 1f);
            saveTMP.characterSpacing = 2f;

            // 5. Connect serialized references in MainMenuVRController
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("turnTypeText").objectReferenceValue = turnValueText;
            so.FindProperty("locomotionTypeText").objectReferenceValue = locoValueText;
            so.FindProperty("turnPrevButton").objectReferenceValue = turnPrevBtn;
            so.FindProperty("turnNextButton").objectReferenceValue = turnNextBtn;
            so.FindProperty("locomotionPrevButton").objectReferenceValue = locoPrevBtn;
            so.FindProperty("locomotionNextButton").objectReferenceValue = locoNextBtn;
            so.FindProperty("saveAndExitButton").objectReferenceValue = saveBtn;

            // Navigation buttons in main panel
            Transform mainPanel = panelsContent.Find("Main_Buttons_Panel");
            if (mainPanel != null)
            {
                Button btnSettings = mainPanel.Find("Btn_Settings")?.GetComponent<Button>();
                Button btnStart = mainPanel.Find("Btn_StartExpedition")?.GetComponent<Button>();
                Button btnTut = mainPanel.Find("Btn_Tutorial")?.GetComponent<Button>();
                Button btnCred = mainPanel.Find("Btn_Credits")?.GetComponent<Button>();
                Button btnQuit = mainPanel.Find("Btn_Quit")?.GetComponent<Button>();

                if (btnSettings != null) so.FindProperty("btnSettings").objectReferenceValue = btnSettings;
                if (btnStart != null) so.FindProperty("btnStartExpedition").objectReferenceValue = btnStart;
                if (btnTut != null) so.FindProperty("btnTutorial").objectReferenceValue = btnTut;
                if (btnCred != null) so.FindProperty("btnCredits").objectReferenceValue = btnCred;
                if (btnQuit != null) so.FindProperty("btnQuit").objectReferenceValue = btnQuit;
            }

            if (mainPanel != null) mainPanel.gameObject.SetActive(true);
            settingsPanel.gameObject.SetActive(false);

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(settingsPanel.gameObject);
            if (mainPanel != null) EditorUtility.SetDirty(mainPanel.gameObject);
            EditorUtility.SetDirty(canvasObj);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[GameOptionsBuilder] Successfully built and styled Game Options UI matching Prospector Journey theme!");
        }

        private static GameObject CreateOptionRow(Transform parent, TMP_FontAsset font, string name, string labelText, string defaultValue,
            Vector2 localPos, Color colRowBg, Color colLabel, Color colValue, Color colArrow,
            Color colBtnNormal, Color colBtnHighlight, Color colBtnPressed,
            out TextMeshProUGUI valueText, out Button prevBtn, out Button nextBtn)
        {
            GameObject rowObj = new GameObject(name);
            rowObj.transform.SetParent(parent, false);
            RectTransform rowRT = rowObj.AddComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 0.5f);
            rowRT.anchorMax = new Vector2(0.5f, 0.5f);
            rowRT.pivot = new Vector2(0.5f, 0.5f);
            rowRT.anchoredPosition = localPos;
            rowRT.sizeDelta = new Vector2(880f, 76f);

            Image rowImg = rowObj.AddComponent<Image>();
            rowImg.color = colRowBg;

            // Left Label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(rowObj.transform, false);
            RectTransform labelRT = labelObj.AddComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0f, 0.5f);
            labelRT.anchorMax = new Vector2(0f, 0.5f);
            labelRT.pivot = new Vector2(0f, 0.5f);
            labelRT.anchoredPosition = new Vector2(30f, 0f);
            labelRT.sizeDelta = new Vector2(400f, 50f);

            TextMeshProUGUI labelTMP = labelObj.AddComponent<TextMeshProUGUI>();
            labelTMP.font = font;
            labelTMP.text = labelText;
            labelTMP.fontSize = 28;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = colLabel;
            labelTMP.alignment = TextAlignmentOptions.MidlineLeft;
            labelTMP.characterSpacing = 1.5f;

            // Selector Container
            GameObject selectorObj = new GameObject("Selector_Group");
            selectorObj.transform.SetParent(rowObj.transform, false);
            RectTransform selRT = selectorObj.AddComponent<RectTransform>();
            selRT.anchorMin = new Vector2(1f, 0.5f);
            selRT.anchorMax = new Vector2(1f, 0.5f);
            selRT.pivot = new Vector2(1f, 0.5f);
            selRT.anchoredPosition = new Vector2(-25f, 0f);
            selRT.sizeDelta = new Vector2(340f, 60f);

            // Prev Button (<)
            GameObject prevBtnObj = new GameObject("Btn_Prev");
            prevBtnObj.transform.SetParent(selectorObj.transform, false);
            RectTransform prevRT = prevBtnObj.AddComponent<RectTransform>();
            prevRT.anchorMin = new Vector2(0f, 0.5f);
            prevRT.anchorMax = new Vector2(0f, 0.5f);
            prevRT.pivot = new Vector2(0.5f, 0.5f);
            prevRT.anchoredPosition = new Vector2(30f, 0f);
            prevRT.sizeDelta = new Vector2(52f, 52f);

            Image prevImg = prevBtnObj.AddComponent<Image>();
            prevImg.color = colBtnNormal;

            prevBtn = prevBtnObj.AddComponent<Button>();
            var cb = prevBtn.colors;
            cb.normalColor = colBtnNormal;
            cb.highlightedColor = colBtnHighlight;
            cb.pressedColor = colBtnPressed;
            prevBtn.colors = cb;
            prevBtnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject prevLabel = new GameObject("Arrow");
            prevLabel.transform.SetParent(prevBtnObj.transform, false);
            RectTransform prevLblRT = prevLabel.AddComponent<RectTransform>();
            prevLblRT.anchorMin = Vector2.zero;
            prevLblRT.anchorMax = Vector2.one;
            prevLblRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI prevTMP = prevLabel.AddComponent<TextMeshProUGUI>();
            prevTMP.font = font;
            prevTMP.text = "<";
            prevTMP.fontSize = 32;
            prevTMP.fontStyle = FontStyles.Bold;
            prevTMP.color = colArrow;
            prevTMP.alignment = TextAlignmentOptions.Center;

            // Value Text (Center)
            GameObject valObj = new GameObject("Text_Value");
            valObj.transform.SetParent(selectorObj.transform, false);
            RectTransform valRT = valObj.AddComponent<RectTransform>();
            valRT.anchorMin = new Vector2(0.5f, 0.5f);
            valRT.anchorMax = new Vector2(0.5f, 0.5f);
            valRT.pivot = new Vector2(0.5f, 0.5f);
            valRT.anchoredPosition = Vector2.zero;
            valRT.sizeDelta = new Vector2(190f, 52f);

            valueText = valObj.AddComponent<TextMeshProUGUI>();
            valueText.font = font;
            valueText.text = defaultValue;
            valueText.fontSize = 28;
            valueText.fontStyle = FontStyles.Bold;
            valueText.color = colValue;
            valueText.alignment = TextAlignmentOptions.Center;

            // Next Button (>)
            GameObject nextBtnObj = new GameObject("Btn_Next");
            nextBtnObj.transform.SetParent(selectorObj.transform, false);
            RectTransform nextRT = nextBtnObj.AddComponent<RectTransform>();
            nextRT.anchorMin = new Vector2(1f, 0.5f);
            nextRT.anchorMax = new Vector2(1f, 0.5f);
            nextRT.pivot = new Vector2(0.5f, 0.5f);
            nextRT.anchoredPosition = new Vector2(-30f, 0f);
            nextRT.sizeDelta = new Vector2(52f, 52f);

            Image nextImg = nextBtnObj.AddComponent<Image>();
            nextImg.color = colBtnNormal;

            nextBtn = nextBtnObj.AddComponent<Button>();
            var cbNext = nextBtn.colors;
            cbNext.normalColor = colBtnNormal;
            cbNext.highlightedColor = colBtnHighlight;
            cbNext.pressedColor = colBtnPressed;
            nextBtn.colors = cbNext;
            nextBtnObj.AddComponent<VRButtonAudioFeedback>();

            GameObject nextLabel = new GameObject("Arrow");
            nextLabel.transform.SetParent(nextBtnObj.transform, false);
            RectTransform nextLblRT = nextLabel.AddComponent<RectTransform>();
            nextLblRT.anchorMin = Vector2.zero;
            nextLblRT.anchorMax = Vector2.one;
            nextLblRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI nextTMP = nextLabel.AddComponent<TextMeshProUGUI>();
            nextTMP.font = font;
            nextTMP.text = ">";
            nextTMP.fontSize = 32;
            nextTMP.fontStyle = FontStyles.Bold;
            nextTMP.color = colArrow;
            nextTMP.alignment = TextAlignmentOptions.Center;

            return rowObj;
        }
    }
}
