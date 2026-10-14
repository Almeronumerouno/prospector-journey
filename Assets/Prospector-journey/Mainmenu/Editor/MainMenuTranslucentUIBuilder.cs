using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ProspectorJourney.UI.Editor
{
    public static class MainMenuTranslucentUIBuilder
    {
        private const string ScenePath = "Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity";
        private const string FontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset";
        private const string LeftHandPrefabPath = "Assets/Oculus Hands/Prefabs/Left Hand Model.prefab";
        private const string RightHandPrefabPath = "Assets/Oculus Hands/Prefabs/Right Hand Model.prefab";

        // Themed Prospector Palette (Gold Rush & Maritime Nautical Tint)
        private static readonly Color ColorBoardBg = new Color(0.08f, 0.14f, 0.20f, 0.38f); // Translucent slate glass matching button opacity (not bolong)
        private static readonly Color ColorGoldBorder = new Color(0.96f, 0.82f, 0.36f, 0.88f); // Warm gold wireframe border
        private static readonly Color ColorTitleGold = new Color(1.0f, 0.92f, 0.65f, 1.0f); // Warm gold title
        private static readonly Color ColorSubtitleGold = new Color(0.88f, 0.78f, 0.52f, 0.92f); // Antique sand/parchment subtitle

        private static readonly Color ColorBtnNormal = new Color(0.10f, 0.16f, 0.22f, 0.42f); // Translucent slate glass
        private static readonly Color ColorBtnHighlight = new Color(0.24f, 0.36f, 0.48f, 0.65f);
        private static readonly Color ColorBtnPressed = new Color(0.36f, 0.50f, 0.64f, 0.85f);
        private static readonly Color ColorBtnBorder = new Color(0.92f, 0.78f, 0.35f, 0.75f); // Warm gold button border
        private static readonly Color ColorBtnText = new Color(0.98f, 0.98f, 0.95f, 1.0f);

        private static readonly Color ColorArrow = new Color(1.0f, 0.82f, 0.25f, 1.0f); // Bright gold arrow
        private static readonly Color ColorOptionValue = new Color(0.98f, 0.95f, 0.85f, 1.0f); // Warm ivory value
        private static readonly Color ColorOptionLabel = new Color(0.98f, 0.95f, 0.88f, 1.0f); // Crisp ivory label

        [MenuItem("ProspectorJourney/Apply Translucent UI & Oculus Hands Overhaul")]
        public static void ApplyOverhaul()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
                activeScene = EditorSceneManager.GetActiveScene();
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError($"[TranslucentUI] Font not found at {FontPath}");
                return;
            }

            // 1. Process Canvas and UI
            var canvasObj = GameObject.Find("VR_MainMenu_Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[TranslucentUI] VR_MainMenu_Canvas not found in scene!");
                return;
            }

            // Enlarge canvas display scale by 65% for high VR readability
            canvasObj.transform.localScale = Vector3.one * 0.00165f;

            var controller = canvasObj.GetComponent<MainMenuVRController>();
            if (controller == null)
            {
                Debug.LogError("[TranslucentUI] MainMenuVRController missing on VR_MainMenu_Canvas!");
                return;
            }

            // Tune VR follow position: closer (1.35m) and eye-height offset (+0.06m)
            SerializedObject ctrlSO = new SerializedObject(controller);
            ctrlSO.FindProperty("followDistance").floatValue = 1.35f;
            ctrlSO.FindProperty("heightOffset").floatValue = 0.06f;
            ctrlSO.ApplyModifiedProperties();

            Transform boardBg = canvasObj.transform.Find("Board_Background");
            if (boardBg == null)
            {
                Debug.LogError("[TranslucentUI] Board_Background not found!");
                return;
            }

            Transform goldFrame = boardBg.Find("Gold_Frame");
            Transform innerBoard = goldFrame != null ? goldFrame.Find("Inner_Board") : null;
            if (innerBoard == null)
            {
                Debug.LogError("[TranslucentUI] Inner_Board not found!");
                return;
            }

            // 1.1 "Ada Frame Tapi Tembus": Translucent themed glass interior + warm gold wireframe
            Image boardImg = boardBg.GetComponent<Image>();
            if (boardImg != null)
            {
                // Translucent nautical slate tint matching button level opacity (~0.38), NOT bolong 0 alpha!
                boardImg.color = ColorBoardBg;
            }

            // Remove any old Outline component that duplicated quads and created milky blur
            Outline oldOutline = boardBg.GetComponent<Outline>();
            if (oldOutline != null)
            {
                Object.DestroyImmediate(oldOutline);
            }

            // Create crisp glowing warm gold wireframe border frame
            CreateWireframeBorder(boardBg.gameObject, ColorGoldBorder, 3.5f);

            // Hide opaque stacked wooden boards
            if (goldFrame != null)
            {
                Image gfImg = goldFrame.GetComponent<Image>();
                if (gfImg != null) gfImg.color = new Color(0f, 0f, 0f, 0f);
            }
            if (innerBoard != null)
            {
                Image ibImg = innerBoard.GetComponent<Image>();
                if (ibImg != null) ibImg.color = new Color(0f, 0f, 0f, 0f);
            }

            // Typography on Main Header: enlarged and bold gold
            Transform titleObj = innerBoard.Find("Header_Section/Title_Text") ?? innerBoard.Find("Title_Text");
            if (titleObj != null)
            {
                TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
                if (titleTMP != null)
                {
                    titleTMP.font = font;
                    titleTMP.fontSize = 64f;
                    titleTMP.fontStyle = FontStyles.Bold;
                    titleTMP.color = ColorTitleGold;
                }
            }

            Transform subtitleObj = innerBoard.Find("Header_Section/Subtitle_Text") ?? innerBoard.Find("Subtitle_Text");
            if (subtitleObj != null)
            {
                TextMeshProUGUI subTMP = subtitleObj.GetComponent<TextMeshProUGUI>();
                if (subTMP != null)
                {
                    subTMP.font = font;
                    subTMP.fontSize = 26f;
                    subTMP.color = ColorSubtitleGold;
                }
            }

            // 1.2 Style Main_Buttons_Panel
            Transform panelsContent = innerBoard.Find("Panels_Content");
            if (panelsContent == null)
            {
                Debug.LogError("[TranslucentUI] Panels_Content not found!");
                return;
            }

            Transform mainPanel = panelsContent.Find("Main_Buttons_Panel");
            if (mainPanel != null)
            {
                StyleMainButtons(mainPanel, font);
            }

            // 1.3 Style Settings_Panel (Game Options)
            Transform settingsPanel = panelsContent.Find("Settings_Panel");
            if (settingsPanel != null)
            {
                BuildTranslucentGameOptions(settingsPanel, font, controller);
            }

            // 1.4 Style Credits_Panel with Group Credits
            Transform creditsPanel = panelsContent.Find("Credits_Panel");
            if (creditsPanel != null)
            {
                BuildTranslucentCredits(creditsPanel, font, controller);
            }

            // 2. Process XR Origin Hand Models Parity
            SetupOculusHands();

            // Set states: Main panel active, Settings & Credits inactive by default
            if (mainPanel != null) mainPanel.gameObject.SetActive(true);
            if (settingsPanel != null) settingsPanel.gameObject.SetActive(false);
            if (creditsPanel != null) creditsPanel.gameObject.SetActive(false);

            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(canvasObj);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);

            Debug.Log("[TranslucentUI] Successfully applied Translucent Glass UI overhaul & Oculus Hands parity!");
        }

        private static void StyleMainButtons(Transform mainPanel, TMP_FontAsset font)
        {
            string[] btnNames = { "Btn_StartExpedition", "Btn_Tutorial", "Btn_Settings", "Btn_Credits", "Btn_Quit" };

            foreach (string name in btnNames)
            {
                Transform btnTr = mainPanel.Find(name);
                if (btnTr == null) continue;

                RectTransform rt = btnTr.GetComponent<RectTransform>();
                if (rt != null)
                {
                    Vector2 sd = rt.sizeDelta;
                    sd.x = 840f; // Wide prominent VR button
                    sd.y = 92f;  // Tall readable height
                    rt.sizeDelta = sd;
                }

                Image img = btnTr.GetComponent<Image>();
                if (img != null)
                {
                    img.color = ColorBtnNormal;
                }

                Button btn = btnTr.GetComponent<Button>();
                if (btn != null)
                {
                    var cb = btn.colors;
                    cb.normalColor = ColorBtnNormal;
                    cb.highlightedColor = ColorBtnHighlight;
                    cb.pressedColor = ColorBtnPressed;
                    btn.colors = cb;
                }

                CreateWireframeBorder(btnTr.gameObject, ColorBtnBorder, 2.0f);

                TextMeshProUGUI tmp = btnTr.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.font = font;
                    tmp.fontSize = 46f;
                    tmp.fontStyle = FontStyles.Bold;
                    tmp.color = ColorBtnText;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.margin = Vector4.zero; // Always centered cleanly!
                }

                // Add COMING SOON badge ABOVE Btn_StartExpedition (not inline / not pushed to side!)
                if (name == "Btn_StartExpedition")
                {
                    SetupComingSoonBadgeAbove(btnTr, font);
                }
            }
        }

        private static void SetupComingSoonBadgeAbove(Transform btnExpedition, TMP_FontAsset font)
        {
            Transform existingBadge = btnExpedition.Find("Badge_ComingSoon");
            if (existingBadge != null)
            {
                Object.DestroyImmediate(existingBadge.gameObject);
            }

            GameObject badgeObj = new GameObject("Badge_ComingSoon", typeof(RectTransform));
            badgeObj.transform.SetParent(btnExpedition, false);

            RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
            // Position neatly above the button, centered horizontally
            badgeRT.anchorMin = new Vector2(0.5f, 1f);
            badgeRT.anchorMax = new Vector2(0.5f, 1f);
            badgeRT.pivot = new Vector2(0.5f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(0f, 8f);
            badgeRT.sizeDelta = new Vector2(170f, 32f);

            Image badgeImg = badgeObj.AddComponent<Image>();
            badgeImg.color = new Color(0.98f, 0.74f, 0.16f, 0.95f); // Amber gold pill
            badgeImg.raycastTarget = false;

            CreateWireframeBorder(badgeObj, new Color(1.0f, 0.90f, 0.50f, 0.85f), 1.5f);

            GameObject labelObj = new GameObject("Text", typeof(RectTransform));
            labelObj.transform.SetParent(badgeObj.transform, false);

            RectTransform lblRT = labelObj.GetComponent<RectTransform>();
            lblRT.anchorMin = Vector2.zero;
            lblRT.anchorMax = Vector2.one;
            lblRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI lblTMP = labelObj.AddComponent<TextMeshProUGUI>();
            lblTMP.font = font;
            lblTMP.text = "COMING SOON";
            lblTMP.fontSize = 18f;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.color = new Color(0.16f, 0.09f, 0.01f, 1.0f); // Dark bronze text on gold
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.raycastTarget = false;
        }

        private static void BuildTranslucentGameOptions(Transform settingsPanel, TMP_FontAsset font, MainMenuVRController controller)
        {
            // Clear old children in settingsPanel
            for (int i = settingsPanel.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(settingsPanel.GetChild(i).gameObject);
            }

            RectTransform spRT = settingsPanel.GetComponent<RectTransform>();
            spRT.anchorMin = Vector2.zero;
            spRT.anchorMax = Vector2.one;
            spRT.sizeDelta = Vector2.zero;
            spRT.anchoredPosition = Vector2.zero;

            // Header Title (clean, prominent title at the top matching user's photo)
            GameObject titleObj = new GameObject("Section_Title", typeof(RectTransform));
            titleObj.transform.SetParent(settingsPanel, false);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -15f);
            titleRT.sizeDelta = new Vector2(900f, 76f);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.font = font;
            titleTMP.text = "GAME OPTIONS";
            titleTMP.fontSize = 58f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = ColorTitleGold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.characterSpacing = 3f;

            // NO Frame_GameOptions! Directly position option rows inside settingsPanel matching reference photo!
            // Row 1: Turn Type
            TextMeshProUGUI turnValueText;
            Button turnPrevBtn, turnNextBtn;
            CreateOptionRow(settingsPanel, font, "Row_TurnType", "TURN TYPE", "SNAP",
                "Btn_TurnPrev", "Btn_TurnNext", "Text_TurnValue",
                new Vector2(0f, 40f),
                out turnValueText, out turnPrevBtn, out turnNextBtn);

            // Row 2: Locomotion Type
            TextMeshProUGUI locoValueText;
            Button locoPrevBtn, locoNextBtn;
            CreateOptionRow(settingsPanel, font, "Row_LocomotionType", "LOCOMOTION TYPE", "TELEPORT",
                "Btn_LocoPrev", "Btn_LocoNext", "Text_LocoValue",
                new Vector2(0f, -50f),
                out locoValueText, out locoPrevBtn, out locoNextBtn);

            // Save and Exit Button (clean bordered button at the bottom)
            GameObject saveBtnObj = new GameObject("Btn_SaveAndExit", typeof(RectTransform));
            saveBtnObj.transform.SetParent(settingsPanel, false);
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

            // Connect serialized fields in MainMenuVRController
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("turnTypeText").objectReferenceValue = turnValueText;
            so.FindProperty("locomotionTypeText").objectReferenceValue = locoValueText;
            so.FindProperty("turnPrevButton").objectReferenceValue = turnPrevBtn;
            so.FindProperty("turnNextButton").objectReferenceValue = turnNextBtn;
            so.FindProperty("locomotionPrevButton").objectReferenceValue = locoPrevBtn;
            so.FindProperty("locomotionNextButton").objectReferenceValue = locoNextBtn;
            so.FindProperty("saveAndExitButton").objectReferenceValue = saveBtn;
            so.ApplyModifiedProperties();
        }

        private static void BuildTranslucentCredits(Transform creditsPanel, TMP_FontAsset font, MainMenuVRController controller)
        {
            // Remove VerticalLayoutGroup if present so we can position elements accurately
            VerticalLayoutGroup vlg = creditsPanel.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Object.DestroyImmediate(vlg);
            }

            // Clear old children in creditsPanel
            for (int i = creditsPanel.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(creditsPanel.GetChild(i).gameObject);
            }

            RectTransform cpRT = creditsPanel.GetComponent<RectTransform>();
            cpRT.anchorMin = Vector2.zero;
            cpRT.anchorMax = Vector2.one;
            cpRT.sizeDelta = Vector2.zero;
            cpRT.anchoredPosition = Vector2.zero;

            // 1. Header Section Title: "CREDITS"
            GameObject titleObj = new GameObject("Section_Title", typeof(RectTransform));
            titleObj.transform.SetParent(creditsPanel, false);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 1f);
            titleRT.anchorMax = new Vector2(0.5f, 1f);
            titleRT.pivot = new Vector2(0.5f, 1f);
            titleRT.anchoredPosition = new Vector2(0f, -10f);
            titleRT.sizeDelta = new Vector2(900f, 55f);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            titleTMP.font = font;
            titleTMP.text = "CREDITS";
            titleTMP.fontSize = 54f;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.color = ColorTitleGold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.characterSpacing = 3f;

            // 2. Left Card: "Card_Dedication" (Institution, Lecturer & Capstone Context)
            GameObject leftCard = new GameObject("Card_Dedication", typeof(RectTransform), typeof(Image));
            leftCard.transform.SetParent(creditsPanel, false);
            RectTransform leftRT = leftCard.GetComponent<RectTransform>();
            leftRT.anchorMin = new Vector2(0.5f, 0.5f);
            leftRT.anchorMax = new Vector2(0.5f, 0.5f);
            leftRT.pivot = new Vector2(0.5f, 0.5f);
            leftRT.anchoredPosition = new Vector2(-255f, 48f);
            leftRT.sizeDelta = new Vector2(480f, 345f);

            Image leftImg = leftCard.GetComponent<Image>();
            leftImg.color = ColorBtnNormal; // Translucent slate glass tint
            CreateWireframeBorder(leftCard, ColorBtnBorder, 2.0f);

            // Left Card - Header Ribbon
            CreateCardHeaderRibbon(leftCard.transform, font, "DEDICATED TO", new Vector2(0f, -10f), new Vector2(444f, 40f));

            // Left Card - Institution Block
            CreateTextBlock(leftCard.transform, font, "Block_Institution",
                "<size=72%><color=#F5D15C><b>UNIVERSITAS</b></color></size>\n" +
                "<size=104%><color=#FFFFFF><b>Universitas Multimedia Nusantara</b></color></size>\n" +
                "<size=72%><color=#C5D6E5>Fakultas Teknologi Informasi dan Komunikasi</color></size>",
                new Vector2(0f, -56f), new Vector2(444f, 65f), 22f, TextAlignmentOptions.Center);

            // Left Card - Divider 1
            CreateHorizontalDivider(leftCard.transform, new Vector2(0f, -125f), 390f);

            // Left Card - Lecturer Block
            CreateTextBlock(leftCard.transform, font, "Block_Lecturer",
                "<size=72%><color=#F5D15C><b>DOSEN PEMBIMBING</b></color></size>\n" +
                "<size=104%><color=#FFFFFF><b>Muhamad Muhaimin, S.Kom., M.Kom.</b></color></size>\n" +
                "<size=72%><color=#C5D6E5>Capstone & Project Mentor</color></size>",
                new Vector2(0f, -133f), new Vector2(444f, 65f), 22f, TextAlignmentOptions.Center);

            // Left Card - Divider 2
            CreateHorizontalDivider(leftCard.transform, new Vector2(0f, -202f), 390f);

            // Left Card - Project Info Block
            CreateTextBlock(leftCard.transform, font, "Block_ProjectInfo",
                "<size=78%><color=#FFEAB3><b>PROSPECTOR JOURNEY VR</b></color></size>\n" +
                "<size=70%><color=#C5D6E5><i>A Realistic Gold Panning Experience</i></color></size>\n" +
                "<size=68%><color=#8FA5BC>Unity URP  •  OpenXR  •  Meta Quest</color></size>",
                new Vector2(0f, -210f), new Vector2(444f, 80f), 21f, TextAlignmentOptions.Center);

            // 3. Right Card: "Card_Team" (Team Cungpret & Roster)
            GameObject rightCard = new GameObject("Card_Team", typeof(RectTransform), typeof(Image));
            rightCard.transform.SetParent(creditsPanel, false);
            RectTransform rightRT = rightCard.GetComponent<RectTransform>();
            rightRT.anchorMin = new Vector2(0.5f, 0.5f);
            rightRT.anchorMax = new Vector2(0.5f, 0.5f);
            rightRT.pivot = new Vector2(0.5f, 0.5f);
            rightRT.anchoredPosition = new Vector2(255f, 48f);
            rightRT.sizeDelta = new Vector2(480f, 345f);

            Image rightImg = rightCard.GetComponent<Image>();
            rightImg.color = ColorBtnNormal; // Translucent slate glass tint
            CreateWireframeBorder(rightCard, ColorBtnBorder, 2.0f);

            // Right Card - Team Header Ribbon
            CreateCardHeaderRibbon(rightCard.transform, font, "NAMA TIM: <color=#FFD140><b>CUNGBRET</b></color>", new Vector2(0f, -10f), new Vector2(444f, 40f));

            // Right Card - Member Rows (Sleek slot cards with gold accent line & NIM pill badge)
            CreateMemberRowSlot(rightCard.transform, font, "Row_Almer", "Almer Pradipo Fajro", "00000087455", new Vector2(0f, -58f));
            CreateMemberRowSlot(rightCard.transform, font, "Row_Gabriel", "Gabriel Pangestu Legowo", "0000113463", new Vector2(0f, -136f));
            CreateMemberRowSlot(rightCard.transform, font, "Row_Jonathan", "Jonathan Sutandar", "00000077041", new Vector2(0f, -214f));

            // 4. Back Button: "BACK TO MAIN MENU"
            GameObject backBtnObj = new GameObject("Btn_BackFromCredits", typeof(RectTransform));
            backBtnObj.transform.SetParent(creditsPanel, false);
            RectTransform backRT = backBtnObj.GetComponent<RectTransform>();
            backRT.anchorMin = new Vector2(0.5f, 0f);
            backRT.anchorMax = new Vector2(0.5f, 0f);
            backRT.pivot = new Vector2(0.5f, 0f);
            backRT.anchoredPosition = new Vector2(0f, 75f);
            backRT.sizeDelta = new Vector2(380f, 56f);

            Image backImg = backBtnObj.AddComponent<Image>();
            backImg.color = ColorBtnNormal;

            Button backBtn = backBtnObj.AddComponent<Button>();
            var cbBack = backBtn.colors;
            cbBack.normalColor = ColorBtnNormal;
            cbBack.highlightedColor = ColorBtnHighlight;
            cbBack.pressedColor = ColorBtnPressed;
            backBtn.colors = cbBack;
            CreateWireframeBorder(backBtnObj, ColorBtnBorder, 2.0f);

            GameObject backLabel = new GameObject("Text", typeof(RectTransform));
            backLabel.transform.SetParent(backBtnObj.transform, false);
            RectTransform backLblRT = backLabel.GetComponent<RectTransform>();
            backLblRT.anchorMin = Vector2.zero;
            backLblRT.anchorMax = Vector2.one;
            backLblRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI backTMP = backLabel.AddComponent<TextMeshProUGUI>();
            backTMP.font = font;
            backTMP.text = "BACK TO MAIN MENU";
            backTMP.fontSize = 26f;
            backTMP.fontStyle = FontStyles.Bold;
            backTMP.color = ColorTitleGold;
            backTMP.alignment = TextAlignmentOptions.Center;
            backTMP.characterSpacing = 2f;

            // Connect serialized fields in MainMenuVRController
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("btnBackFromCredits").objectReferenceValue = backBtn;
            so.FindProperty("creditsPanel").objectReferenceValue = creditsPanel.gameObject;
            so.ApplyModifiedProperties();
        }

        private static void CreateCardHeaderRibbon(Transform parent, TMP_FontAsset font, string titleText, Vector2 pos, Vector2 size)
        {
            GameObject ribbonObj = new GameObject("Header_Ribbon", typeof(RectTransform), typeof(Image));
            ribbonObj.transform.SetParent(parent, false);
            RectTransform rt = ribbonObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            Image img = ribbonObj.GetComponent<Image>();
            img.color = new Color(0.11f, 0.16f, 0.20f, 0.85f);
            CreateWireframeBorder(ribbonObj, new Color(0.96f, 0.82f, 0.36f, 0.50f), 1.2f);

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(ribbonObj.transform, false);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = titleText;
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = ColorTitleGold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.characterSpacing = 2f;
        }

        private static void CreateTextBlock(Transform parent, TMP_FontAsset font, string name, string text, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform));
            textObj.transform.SetParent(parent, false);
            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.lineSpacing = 2f;
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

        private static void CreateMemberRowSlot(Transform parent, TMP_FontAsset font, string name, string memberName, string nim, Vector2 pos)
        {
            GameObject rowObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            rowObj.transform.SetParent(parent, false);
            RectTransform rowRT = rowObj.GetComponent<RectTransform>();
            rowRT.anchorMin = new Vector2(0.5f, 1f);
            rowRT.anchorMax = new Vector2(0.5f, 1f);
            rowRT.pivot = new Vector2(0.5f, 1f);
            rowRT.anchoredPosition = pos;
            rowRT.sizeDelta = new Vector2(444f, 68f);

            Image rowImg = rowObj.GetComponent<Image>();
            rowImg.color = new Color(0.10f, 0.15f, 0.20f, 0.60f);
            CreateWireframeBorder(rowObj, new Color(0.96f, 0.82f, 0.36f, 0.40f), 1.2f);

            // Left gold vertical accent bar
            GameObject barObj = new GameObject("Accent_Bar", typeof(RectTransform), typeof(Image));
            barObj.transform.SetParent(rowObj.transform, false);
            RectTransform barRT = barObj.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0f, 0f);
            barRT.anchorMax = new Vector2(0f, 1f);
            barRT.pivot = new Vector2(0f, 0.5f);
            barRT.anchoredPosition = Vector2.zero;
            barRT.sizeDelta = new Vector2(4f, 0f);

            Image barImg = barObj.GetComponent<Image>();
            barImg.color = ColorBtnBorder;
            barImg.raycastTarget = false;

            // Member Info (Tag + Full Name)
            GameObject infoObj = new GameObject("Info", typeof(RectTransform));
            infoObj.transform.SetParent(rowObj.transform, false);
            RectTransform infoRT = infoObj.GetComponent<RectTransform>();
            infoRT.anchorMin = new Vector2(0f, 0f);
            infoRT.anchorMax = new Vector2(0.68f, 1f);
            infoRT.pivot = new Vector2(0f, 0.5f);
            infoRT.offsetMin = new Vector2(16f, 4f);
            infoRT.offsetMax = new Vector2(0f, -4f);

            TextMeshProUGUI infoTMP = infoObj.AddComponent<TextMeshProUGUI>();
            infoTMP.font = font;
            infoTMP.text = $"<size=70%><color=#F5D15C><b>ANGGOTA TIM</b></color></size>\n<size=104%><color=#FFFFFF><b>{memberName}</b></color></size>";
            infoTMP.fontSize = 22f;
            infoTMP.alignment = TextAlignmentOptions.MidlineLeft;
            infoTMP.lineSpacing = 1.5f;

            // NIM Pill Badge on the right
            GameObject badgeObj = new GameObject("Badge_NIM", typeof(RectTransform), typeof(Image));
            badgeObj.transform.SetParent(rowObj.transform, false);
            RectTransform badgeRT = badgeObj.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(1f, 0.5f);
            badgeRT.anchorMax = new Vector2(1f, 0.5f);
            badgeRT.pivot = new Vector2(1f, 0.5f);
            badgeRT.anchoredPosition = new Vector2(-12f, 0f);
            badgeRT.sizeDelta = new Vector2(144f, 38f);

            Image badgeImg = badgeObj.GetComponent<Image>();
            badgeImg.color = new Color(0.14f, 0.20f, 0.26f, 0.85f);
            CreateWireframeBorder(badgeObj, new Color(0.96f, 0.82f, 0.36f, 0.45f), 1.0f);

            GameObject nimTextObj = new GameObject("Text", typeof(RectTransform));
            nimTextObj.transform.SetParent(badgeObj.transform, false);
            RectTransform nimTextRT = nimTextObj.GetComponent<RectTransform>();
            nimTextRT.anchorMin = Vector2.zero;
            nimTextRT.anchorMax = Vector2.one;
            nimTextRT.sizeDelta = Vector2.zero;

            TextMeshProUGUI nimTMP = nimTextObj.AddComponent<TextMeshProUGUI>();
            nimTMP.font = font;
            nimTMP.text = nim;
            nimTMP.fontSize = 18.5f;
            nimTMP.fontStyle = FontStyles.Bold;
            nimTMP.color = ColorTitleGold;
            nimTMP.alignment = TextAlignmentOptions.Center;
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
            rowImg.color = new Color(0f, 0f, 0f, 0f); // Completely transparent row matching reference photo

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

            // Selector Container (Clean transparent container, NO inner box/border)
            GameObject selectorObj = new GameObject("Selector_Group", typeof(RectTransform));
            selectorObj.transform.SetParent(rowObj.transform, false);
            RectTransform selRT = selectorObj.GetComponent<RectTransform>();
            selRT.anchorMin = new Vector2(1f, 0.5f);
            selRT.anchorMax = new Vector2(1f, 0.5f);
            selRT.pivot = new Vector2(1f, 0.5f);
            selRT.anchoredPosition = new Vector2(-25f, 0f);
            selRT.sizeDelta = new Vector2(480f, 80f);

            Image selImg = selectorObj.AddComponent<Image>();
            selImg.color = new Color(0f, 0f, 0f, 0f); // Completely transparent, NO inner frame!

            // Prev Button (<) - transparent button, clean arrow text
            GameObject prevBtnObj = new GameObject(prevBtnName, typeof(RectTransform));
            prevBtnObj.transform.SetParent(selectorObj.transform, false);
            RectTransform prevRT = prevBtnObj.GetComponent<RectTransform>();
            prevRT.anchorMin = new Vector2(0f, 0.5f);
            prevRT.anchorMax = new Vector2(0f, 0.5f);
            prevRT.pivot = new Vector2(0.5f, 0.5f);
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

            // Value Text (clean text without box, warm ivory)
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

            // Next Button (>) - transparent button, clean arrow text
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

        private static void SetupOculusHands()
        {
            GameObject rigObj = GameObject.Find("XR Origin (XR Rig)");
            if (rigObj == null)
            {
                Debug.LogWarning("[TranslucentUI] XR Origin (XR Rig) not found!");
                return;
            }

            GameObject leftHandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeftHandPrefabPath);
            GameObject rightHandPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RightHandPrefabPath);
            if (leftHandPrefab == null || rightHandPrefab == null)
            {
                Debug.LogError($"[TranslucentUI] Oculus Hand prefabs missing! Left: {leftHandPrefab != null}, Right: {rightHandPrefab != null}");
                return;
            }

            Transform camOffset = rigObj.transform.Find("Camera Offset");
            if (camOffset == null)
            {
                Debug.LogError("[TranslucentUI] Camera Offset not found under XR Origin!");
                return;
            }

            // Left Controller
            Transform leftCtrl = camOffset.Find("Left Controller");
            if (leftCtrl != null)
            {
                // Disable all non-hand visual models
                for (int i = 0; i < leftCtrl.childCount; i++)
                {
                    Transform child = leftCtrl.GetChild(i);
                    if (child.name.Contains("Visual") || child.name.Contains("Controller Visual") || child.name.Contains("Model") && child.name != "Left Hand Model")
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                // Check or instantiate Left Hand Model
                Transform leftHandModelTr = leftCtrl.Find("Left Hand Model");
                GameObject leftHandInstance;
                if (leftHandModelTr == null)
                {
                    leftHandInstance = (GameObject)PrefabUtility.InstantiatePrefab(leftHandPrefab, leftCtrl);
                    leftHandInstance.name = "Left Hand Model";
                }
                else
                {
                    leftHandInstance = leftHandModelTr.gameObject;
                }

                leftHandInstance.SetActive(true);
                leftHandInstance.transform.localPosition = new Vector3(-0.001f, 0.001f, -0.035f);
                leftHandInstance.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                leftHandInstance.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(leftHandInstance);
            }

            // Right Controller
            Transform rightCtrl = camOffset.Find("Right Controller");
            if (rightCtrl != null)
            {
                // Disable all non-hand visual models
                for (int i = 0; i < rightCtrl.childCount; i++)
                {
                    Transform child = rightCtrl.GetChild(i);
                    if (child.name.Contains("Visual") || child.name.Contains("Controller Visual") || child.name.Contains("Model") && child.name != "Right Hand Model")
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                // Check or instantiate Right Hand Model
                Transform rightHandModelTr = rightCtrl.Find("Right Hand Model");
                GameObject rightHandInstance;
                if (rightHandModelTr == null)
                {
                    rightHandInstance = (GameObject)PrefabUtility.InstantiatePrefab(rightHandPrefab, rightCtrl);
                    rightHandInstance.name = "Right Hand Model";
                }
                else
                {
                    rightHandInstance = rightHandModelTr.gameObject;
                }

                rightHandInstance.SetActive(true);
                rightHandInstance.transform.localPosition = new Vector3(-0.001f, 0.001f, -0.035f);
                rightHandInstance.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                rightHandInstance.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(rightHandInstance);
            }

            EditorUtility.SetDirty(rigObj);
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
