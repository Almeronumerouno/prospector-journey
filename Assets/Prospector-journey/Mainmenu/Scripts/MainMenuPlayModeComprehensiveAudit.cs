using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProspectorJourney.UI
{
    public class MainMenuPlayModeComprehensiveAudit : MonoBehaviour
    {
        private static string LogPath => System.IO.Path.Combine(Application.dataPath, "Prospector-journey/Mainmenu/audit_log.txt");

        private static void WriteLog(string text)
        {
            try
            {
                System.IO.File.AppendAllText(LogPath, text + "\n");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AuditLog Write Error] {e.Message}");
            }
        }

        private IEnumerator Start()
        {
            try { System.IO.File.WriteAllText(LogPath, "=== START AUDIT ===\n"); } catch { }
            Debug.Log("================================================================================");
            Debug.Log("[VR PLAY MODE AUDIT] STARTING COMPREHENSIVE MAIN MENU TESTING & AUDIT SUITE");
            Debug.Log("================================================================================");

            yield return new WaitForSeconds(0.3f);

            int totalTests = 0;
            int passedTests = 0;

            // -----------------------------------------------------------------------------------------
            // TEST 1: DISPLAY SCALE, PROXIMITY & READABLE TYPOGRAPHY
            // -----------------------------------------------------------------------------------------
            totalTests++;
            var canvas = GameObject.Find("VR_MainMenu_Canvas");
            if (canvas == null)
            {
                Debug.LogError("[TEST 1 FAIL] VR_MainMenu_Canvas not found in scene!");
                WriteLog("[TEST 1 FAIL] Canvas missing\n");
            }
            else
            {
                var controller = canvas.GetComponent<MainMenuVRController>();
                float canvasScale = canvas.transform.localScale.x;
                bool scaleOk = canvasScale >= 0.0015f;

                Transform boardBg = canvas.transform.Find("Board_Background");
                Transform goldFrame = boardBg != null ? boardBg.Find("Gold_Frame") : null;
                Transform innerBoard = goldFrame != null ? goldFrame.Find("Inner_Board") : null;
                Transform titleText = innerBoard != null ? (innerBoard.Find("Header_Section/Title_Text") ?? innerBoard.Find("Title_Text")) : null;
                TextMeshProUGUI titleTMP = titleText != null ? titleText.GetComponent<TextMeshProUGUI>() : null;
                bool titleOk = titleTMP != null && titleTMP.fontSize >= 56f;

                Transform panelsContent = innerBoard != null ? innerBoard.Find("Panels_Content") : null;
                Transform mainPanel = panelsContent != null ? panelsContent.Find("Main_Buttons_Panel") : null;
                Transform btnExp = mainPanel != null ? mainPanel.Find("Btn_StartExpedition") : null;
                RectTransform btnRT = btnExp != null ? btnExp.GetComponent<RectTransform>() : null;
                TextMeshProUGUI btnTMP = btnExp != null ? btnExp.GetComponentInChildren<TextMeshProUGUI>() : null;

                bool btnSizeOk = btnRT != null && btnRT.sizeDelta.x >= 800f && btnRT.sizeDelta.y >= 85f;
                bool btnFontOk = btnTMP != null && btnTMP.fontSize >= 44f;
                bool btnAlignOk = btnTMP != null && btnTMP.alignment == TextAlignmentOptions.Center && btnTMP.margin == Vector4.zero;

                if (scaleOk && titleOk && btnSizeOk && btnFontOk && btnAlignOk)
                {
                    passedTests++;
                    string msg = $"[TEST 1 PASS] Display Scale & Readability: CanvasScale={canvasScale:F5} (>=0.0015), TitleFont={titleTMP.fontSize}pt, BtnSize={btnRT.sizeDelta.x}x{btnRT.sizeDelta.y}px, BtnFont={btnTMP.fontSize}pt, Perfectly Centered.";
                    Debug.Log(msg);
                    WriteLog(msg + "\n");
                }
                else
                {
                    string msg = $"[TEST 1 FAIL] Display Scale/Typography mismatch: ScaleOk={scaleOk}, TitleOk={titleOk}, BtnSizeOk={btnSizeOk}, BtnFontOk={btnFontOk}, AlignOk={btnAlignOk}";
                    Debug.LogError(msg);
                    WriteLog(msg + "\n");
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 2: "ADA FRAME TAPI TEMBUS" (TRANSPARENT INTERIOR & CRISP WIREFRAME BORDER)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvas != null)
            {
                Transform boardBg = canvas.transform.Find("Board_Background");
                Image bgImg = boardBg != null ? boardBg.GetComponent<Image>() : null;
                Outline outlineComp = boardBg != null ? boardBg.GetComponent<Outline>() : null;
                Transform wireframe = boardBg != null ? boardBg.Find("Wireframe_Border") : null;

                bool bgTranslucent = bgImg != null && bgImg.color.a >= 0.20f && bgImg.color.a <= 0.60f;
                bool noMilkyOutline = outlineComp == null; // No quad duplication that causes milky blur
                bool hasWireframeBorder = wireframe != null && wireframe.childCount == 4;

                Transform goldFrame = boardBg != null ? boardBg.Find("Gold_Frame") : null;
                Image gfImg = goldFrame != null ? goldFrame.GetComponent<Image>() : null;
                bool gfTransparent = gfImg != null && gfImg.color.a <= 0.05f;

                Transform innerBoard = goldFrame != null ? goldFrame.Find("Inner_Board") : null;
                Image ibImg = innerBoard != null ? innerBoard.GetComponent<Image>() : null;
                bool ibTransparent = ibImg != null && ibImg.color.a <= 0.05f;

                Transform panelsContent = innerBoard != null ? innerBoard.Find("Panels_Content") : null;
                Transform settingsPanel = panelsContent != null ? panelsContent.Find("Settings_Panel") : null;
                bool noInnerFrame = settingsPanel != null && settingsPanel.Find("Frame_GameOptions") == null;

                if (bgTranslucent && noMilkyOutline && hasWireframeBorder && gfTransparent && ibTransparent && noInnerFrame)
                {
                    passedTests++;
                    string msg = $"[TEST 2 PASS] Frame 'Tembus Pandang': Translucent glass alpha={bgImg.color.a:F2} (0.20-0.60), Wireframe Border Present, No inner frame in Game Options.";
                    Debug.Log(msg);
                    WriteLog(msg + "\n");
                }
                else
                {
                    string msg = $"[TEST 2 FAIL] Transparency/Frame issue: bgTranslucent={bgTranslucent} (a={bgImg?.color.a}), noMilkyOutline={noMilkyOutline}, hasWireframeBorder={hasWireframeBorder}, gfTransparent={gfTransparent}, ibTransparent={ibTransparent}, noInnerFrame={noInnerFrame}";
                    Debug.LogError(msg);
                    WriteLog(msg + "\n");
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 3: COMING SOON BADGE POSITIONING (CENTERED ABOVE START EXPEDITION)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvas != null)
            {
                Transform btnExp = canvas.transform.Find("Board_Background/Gold_Frame/Inner_Board/Panels_Content/Main_Buttons_Panel/Btn_StartExpedition");
                Transform badge = btnExp != null ? btnExp.Find("Badge_ComingSoon") : null;
                RectTransform badgeRT = badge != null ? badge.GetComponent<RectTransform>() : null;
                TextMeshProUGUI badgeTMP = badge != null ? badge.Find("Text")?.GetComponent<TextMeshProUGUI>() : null;

                bool badgeExists = badge != null && badgeTMP != null && badgeTMP.text == "COMING SOON";
                bool badgeAbove = badgeRT != null && badgeRT.anchoredPosition.y > 0f;
                bool badgeCenteredH = badgeRT != null && Mathf.Approximately(badgeRT.anchorMin.x, 0.5f) && Mathf.Approximately(badgeRT.anchorMax.x, 0.5f);

                // Click test
                Button expBtn = btnExp != null ? btnExp.GetComponent<Button>() : null;
                bool clickSafe = expBtn != null;
                if (expBtn != null)
                {
                    expBtn.onClick.Invoke(); // Safe invocation, should log COMING SOON
                }

                if (badgeExists && badgeAbove && badgeCenteredH && clickSafe)
                {
                    passedTests++;
                    string msg = $"[TEST 3 PASS] COMING SOON Badge: Centered Above Button (Y={badgeRT.anchoredPosition.y}px > 0, AnchorX=0.5), Text='{badgeTMP.text}', Button Click Safe.";
                    Debug.Log(msg);
                    WriteLog(msg + "\n");
                }
                else
                {
                    string msg = $"[TEST 3 FAIL] Badge placement issue: badgeExists={badgeExists}, badgeAbove={badgeAbove}, badgeCenteredH={badgeCenteredH}";
                    Debug.LogError(msg);
                    WriteLog(msg + "\n");
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 4: OCULUS HAND MODELS PARITY (MATCHING ROOM & TUTORIAL SCENES)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            GameObject rigObj = GameObject.Find("XR Origin (XR Rig)");
            if (rigObj == null)
            {
                Debug.LogError("[TEST 4 FAIL] XR Origin (XR Rig) not found!");
                WriteLog("[TEST 4 FAIL] XR Origin (XR Rig) not found!\n");
            }
            else
            {
                Transform camOffset = rigObj.transform.Find("Camera Offset");
                Transform leftCtrl = camOffset != null ? camOffset.Find("Left Controller") : null;
                Transform rightCtrl = camOffset != null ? camOffset.Find("Right Controller") : null;

                Transform leftHand = leftCtrl != null ? leftCtrl.Find("Left Hand Model") : null;
                Transform rightHand = rightCtrl != null ? rightCtrl.Find("Right Hand Model") : null;

                var animLeft = leftHand != null ? leftHand.GetComponent<AnimateHandOnInput>() : null;
                var animRight = rightHand != null ? rightHand.GetComponent<AnimateHandOnInput>() : null;

                bool leftActive = leftHand != null && leftHand.gameObject.activeSelf && animLeft != null;
                bool rightActive = rightHand != null && rightHand.gameObject.activeSelf && animRight != null;

                // Check generic meshes are inactive
                Transform leftGen = leftCtrl != null ? leftCtrl.Find("Left Controller Visual") : null;
                Transform rightGen = rightCtrl != null ? rightCtrl.Find("Right Controller Visual") : null;
                bool genericDisabled = (leftGen == null || !leftGen.gameObject.activeSelf) && (rightGen == null || !rightGen.gameObject.activeSelf);

                if (leftActive && rightActive && genericDisabled)
                {
                    passedTests++;
                    string msg = "[TEST 4 PASS] Oculus Hands Parity: Left Hand Model & Right Hand Model active with AnimateHandOnInput, generic visual meshes disabled.";
                    Debug.Log(msg);
                    WriteLog(msg + "\n");
                }
                else
                {
                    string msg = $"[TEST 4 FAIL] Oculus Hands issue: leftActive={leftActive}, rightActive={rightActive}, genericDisabled={genericDisabled}";
                    Debug.LogError(msg);
                    WriteLog(msg + "\n");
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 5: RUNTIME GAME OPTIONS INTERACTION & PERSISTENCE
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvas != null)
            {
                Transform panelsContent = canvas.transform.Find("Board_Background/Gold_Frame/Inner_Board/Panels_Content");
                Transform mainPanel = panelsContent != null ? panelsContent.Find("Main_Buttons_Panel") : null;
                Transform settingsPanel = panelsContent != null ? panelsContent.Find("Settings_Panel") : null;

                Button btnSettings = mainPanel != null ? mainPanel.Find("Btn_Settings")?.GetComponent<Button>() : null;
                if (btnSettings == null)
                {
                    Debug.LogError("[TEST 5 FAIL] Btn_Settings not found!");
                }
                else
                {
                    // Set predictable baseline for deterministic test verification
                    PlayerPrefs.SetInt("VR_TurnType", 0);
                    PlayerPrefs.SetInt("VR_LocomotionType", 0);
                    PlayerPrefs.Save();

                    btnSettings.onClick.Invoke();
                    yield return new WaitForSeconds(0.1f);

                    bool openedSettings = settingsPanel != null && settingsPanel.gameObject.activeSelf && !mainPanel.gameObject.activeSelf;

                    Transform rowTurn = settingsPanel.Find("Row_TurnType");
                    Transform rowLoco = settingsPanel.Find("Row_LocomotionType");
                    Button saveBtn = settingsPanel.Find("Btn_SaveAndExit")?.GetComponent<Button>();

                    TextMeshProUGUI turnText = rowTurn != null ? (rowTurn.Find("Selector_Group/Text_TurnValue") ?? rowTurn.Find("Selector_Group/Text_Value"))?.GetComponent<TextMeshProUGUI>() : null;
                    Button turnNext = rowTurn != null ? (rowTurn.Find("Selector_Group/Btn_TurnNext") ?? rowTurn.Find("Selector_Group/Btn_Next"))?.GetComponent<Button>() : null;
                    Button turnPrev = rowTurn != null ? (rowTurn.Find("Selector_Group/Btn_TurnPrev") ?? rowTurn.Find("Selector_Group/Btn_Prev"))?.GetComponent<Button>() : null;

                    TextMeshProUGUI locoText = rowLoco != null ? (rowLoco.Find("Selector_Group/Text_LocoValue") ?? rowLoco.Find("Selector_Group/Text_Value"))?.GetComponent<TextMeshProUGUI>() : null;
                    Button locoNext = rowLoco != null ? (rowLoco.Find("Selector_Group/Btn_LocoNext") ?? rowLoco.Find("Selector_Group/Btn_Next"))?.GetComponent<Button>() : null;
                    Button locoPrev = rowLoco != null ? (rowLoco.Find("Selector_Group/Btn_LocoPrev") ?? rowLoco.Find("Selector_Group/Btn_Prev"))?.GetComponent<Button>() : null;

                    string initialTurn = turnText != null ? turnText.text : "";
                    if (turnNext != null) turnNext.onClick.Invoke();
                    yield return null;
                    string cycledTurn = turnText != null ? turnText.text : "";
                    if (turnPrev != null) turnPrev.onClick.Invoke();
                    yield return null;

                    bool turnCycleOk = initialTurn != cycledTurn && turnText.text == initialTurn;

                    string initialLoco = locoText != null ? locoText.text : "";
                    if (locoNext != null) locoNext.onClick.Invoke();
                    yield return null;
                    string cycledLoco = locoText != null ? locoText.text : "";
                    if (locoPrev != null) locoPrev.onClick.Invoke();
                    yield return null;

                    bool locoCycleOk = initialLoco != cycledLoco && locoText.text == initialLoco;

                    // Set Continuous & Continuous, then Save
                    if (turnNext != null) turnNext.onClick.Invoke();
                    if (locoNext != null) locoNext.onClick.Invoke();
                    yield return null;

                    string beforeSaveTurn = turnText != null ? turnText.text : "";
                    string beforeSaveLoco = locoText != null ? locoText.text : "";

                    if (saveBtn != null) saveBtn.onClick.Invoke();
                    yield return new WaitForSeconds(0.15f);

                    bool closedSettings = !settingsPanel.gameObject.activeSelf && mainPanel.gameObject.activeSelf;
                    int pTurn = PlayerPrefs.GetInt("VR_TurnType", -1);
                    int pLoco = PlayerPrefs.GetInt("VR_LocomotionType", -1);
                    bool prefsSaved = (pTurn == 1 && pLoco == 1);

                    string diagMsg = $"[TEST 5 DIAG] beforeSaveTurn={beforeSaveTurn}, beforeSaveLoco={beforeSaveLoco}, pTurn={pTurn}, pLoco={pLoco}, closedSettings={closedSettings}";
                    Debug.Log(diagMsg);
                    WriteLog(diagMsg + "\n");

                    // Revert to Snap & Teleport defaults
                    btnSettings.onClick.Invoke();
                    yield return null;
                    if (turnPrev != null) turnPrev.onClick.Invoke();
                    if (locoPrev != null) locoPrev.onClick.Invoke();
                    yield return null;
                    if (saveBtn != null) saveBtn.onClick.Invoke();
                    yield return null;

                    if (openedSettings && turnCycleOk && locoCycleOk && closedSettings && prefsSaved)
                    {
                        passedTests++;
                        Debug.Log("[TEST 5 PASS] Game Options Flow: Opened panel, Cycled Turn Type & Locomotion Type, Saved to PlayerPrefs, Restored Main Menu cleanly.");
                        WriteLog("[TEST 5 PASS] Success\n");
                    }
                    else
                    {
                        string failMsg = $"[TEST 5 FAIL] Game Options failure: opened={openedSettings}, turnOk={turnCycleOk}, locoOk={locoCycleOk}, closed={closedSettings}, prefs={prefsSaved} (pTurn={pTurn}, pLoco={pLoco})";
                        Debug.LogError(failMsg);
                        WriteLog(failMsg + "\n");
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);

            // -----------------------------------------------------------------------------------------
            // SUMMARY REPORT
            // -----------------------------------------------------------------------------------------
            string summaryMsg;
            if (passedTests == totalTests)
            {
                summaryMsg = $"[VR PLAY MODE AUDIT RESULT] 100% PASSED ({passedTests}/{totalTests} CHECKS SATISFIED) - ZERO ERRORS";
                Debug.Log(summaryMsg);
            }
            else
            {
                summaryMsg = $"[VR PLAY MODE AUDIT RESULT] AUDIT FAILED ({passedTests}/{totalTests} CHECKS PASSED)";
                Debug.LogError(summaryMsg);
            }
            WriteLog(summaryMsg + "\n");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
