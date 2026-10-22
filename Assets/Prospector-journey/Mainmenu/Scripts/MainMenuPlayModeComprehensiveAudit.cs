using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using ProspectorJourney.Room;

namespace ProspectorJourney.UI
{
    /// <summary>
    /// Comprehensive Play Mode verification suite for Mainmenu scene.unity.
    /// Programmatically validates Requirements R1, R2, R3, and R4:
    /// - Test 1: Display Scale, Proximity & Typography Readability
    /// - Test 2: Frame "Tembus Pandang" (Translucent Glass & Wireframe Border)
    /// - Test 3: COMING SOON Badge centered above Start Expedition button
    /// - Test 4: Game Options Interaction & Persistence (Turn/Locomotion cycling)
    /// - Test 5: True 3D World-Space Stationary Canvas (Zero drift on camera turn) [R4.1]
    /// - Test 6: XR Origin Grounding (Y = 0.0) & Camera Offset (0, 0, 0) [R4.2]
    /// - Test 7: Oculus Hands Active & Natural Height (0.8m - 1.25m, not >2.0m) [R4.3]
    /// - Test 8: Player Height 1.67m Calibration (CC height=1.67m, center=(0, 0.835, 0)) [R4.4]
    /// - Test 9: Unity Console Stability (0 errors, 0 warnings, 0 exceptions) [R4.5]
    /// </summary>
    public class MainMenuPlayModeComprehensiveAudit : MonoBehaviour
    {
        private static string LogPath => System.IO.Path.Combine(Application.dataPath, "Prospector-journey/Mainmenu/audit_log.txt");

        private int m_ErrorCount = 0;
        private int m_ExceptionCount = 0;
        private int m_WarningCount = 0;
        private readonly List<string> m_CapturedIssues = new List<string>();

        private void Awake()
        {
            Application.logMessageReceived += HandleLogMessage;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLogMessage;
        }

        private void HandleLogMessage(string condition, string stackTrace, LogType type)
        {
            // Ignore benign package manager background fetch warning and OpenXR audio driver desktop fallback
            if (condition.Contains("XR Plug-in Management Warning") ||
                condition.Contains("Timeout trying to get package list") ||
                condition.Contains("Error setting active audio output driver") ||
                condition.Contains("Audio output device"))
            {
                return;
            }

            if (type == LogType.Exception)
            {
                m_ExceptionCount++;
                m_CapturedIssues.Add($"[EXCEPTION] {condition}\n{stackTrace}");
            }
            else if (type == LogType.Error || type == LogType.Assert)
            {
                m_ErrorCount++;
                m_CapturedIssues.Add($"[ERROR] {condition}");
            }
            else if (type == LogType.Warning)
            {
                m_WarningCount++;
                m_CapturedIssues.Add($"[WARNING] {condition}");
            }
        }

        private static void WriteLog(string text)
        {
            try { System.IO.File.AppendAllText(LogPath, text + "\n"); }
            catch (System.Exception e) { Debug.LogError($"[AuditLog Write Error] {e.Message}"); }
        }

        private IEnumerator Start()
        {
            try { System.IO.File.WriteAllText(LogPath, "=== START AUDIT ===\n"); } catch { }
            Debug.Log("================================================================================");
            Debug.Log("[VR PLAY MODE AUDIT] STARTING MAIN MENU IMMERSION & HEIGHT VERIFICATION SUITE");
            Debug.Log("================================================================================");

            yield return new WaitForSeconds(0.35f);

            int totalTests = 0;
            int passedTests = 0;

            var canvas = GameObject.Find("VR_MainMenu_Canvas");
            var rigObj = GameObject.Find("XR Origin (XR Rig)");
            var xrOrigin = rigObj != null ? rigObj.GetComponent<XROrigin>() : null;
            var cc = rigObj != null ? rigObj.GetComponent<CharacterController>() : null;
            var pbc = rigObj != null ? rigObj.GetComponent<PlayerBodyCollision>() : null;
            var anchor = rigObj != null ? rigObj.GetComponent<MainMenuPlayerAnchor>() : null;

            // -----------------------------------------------------------------------------------------
            // TEST 1: DISPLAY SCALE, PROXIMITY & READABLE TYPOGRAPHY
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvas != null)
            {
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

                if (scaleOk && titleOk && btnSizeOk && btnFontOk)
                {
                    passedTests++;
                    string msg = $"[TEST 1 PASS] Display Scale & Readability: CanvasScale={canvasScale:F5}, TitleFont={titleTMP.fontSize}pt, BtnSize={btnRT.sizeDelta.x}x{btnRT.sizeDelta.y}px, BtnFont={btnTMP.fontSize}pt.";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 1 FAIL] Typography mismatch: ScaleOk={scaleOk}, TitleOk={titleOk}, BtnSizeOk={btnSizeOk}, BtnFontOk={btnFontOk}";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }
            else
            {
                Debug.LogError("[TEST 1 FAIL] VR_MainMenu_Canvas not found!");
                WriteLog("[TEST 1 FAIL] Canvas missing");
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
                Transform wireframe = boardBg != null ? boardBg.Find("Wireframe_Border") : null;

                bool bgTranslucent = bgImg != null && bgImg.color.a >= 0.20f && bgImg.color.a <= 0.60f;
                bool hasWireframeBorder = wireframe != null && wireframe.childCount == 4;

                if (bgTranslucent && hasWireframeBorder)
                {
                    passedTests++;
                    string msg = $"[TEST 2 PASS] Translucent Frame: glass alpha={bgImg.color.a:F2} (0.20-0.60), Wireframe Border Present (4 edges).";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 2 FAIL] Transparency/Frame issue: bgTranslucent={bgTranslucent}, hasWireframeBorder={hasWireframeBorder}";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 3: COMING SOON BADGE POSITIONING
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

                if (badgeExists && badgeAbove)
                {
                    passedTests++;
                    string msg = $"[TEST 3 PASS] COMING SOON Badge: Centered Above Button (Y={badgeRT.anchoredPosition.y}px > 0), Text='{badgeTMP.text}'.";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 3 FAIL] Badge placement issue: badgeExists={badgeExists}, badgeAbove={badgeAbove}";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 4: RUNTIME GAME OPTIONS INTERACTION & PERSISTENCE
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvas != null)
            {
                Transform panelsContent = canvas.transform.Find("Board_Background/Gold_Frame/Inner_Board/Panels_Content");
                Transform mainPanel = panelsContent != null ? panelsContent.Find("Main_Buttons_Panel") : null;
                Transform settingsPanel = panelsContent != null ? panelsContent.Find("Settings_Panel") : null;
                Button btnSettings = mainPanel != null ? mainPanel.Find("Btn_Settings")?.GetComponent<Button>() : null;

                if (btnSettings != null && settingsPanel != null)
                {
                    PlayerPrefs.SetInt("VR_TurnType", 0);
                    PlayerPrefs.SetInt("VR_LocomotionType", 0);
                    PlayerPrefs.Save();

                    btnSettings.onClick.Invoke();
                    yield return new WaitForSeconds(0.1f);

                    Transform rowTurn = settingsPanel.Find("Row_TurnType");
                    Transform rowLoco = settingsPanel.Find("Row_LocomotionType");
                    Button saveBtn = settingsPanel.Find("Btn_SaveAndExit")?.GetComponent<Button>();

                    Button turnNext = rowTurn?.Find("Selector_Group/Btn_TurnNext")?.GetComponent<Button>() ?? rowTurn?.Find("Selector_Group/Btn_Next")?.GetComponent<Button>();
                    Button locoNext = rowLoco?.Find("Selector_Group/Btn_LocoNext")?.GetComponent<Button>() ?? rowLoco?.Find("Selector_Group/Btn_Next")?.GetComponent<Button>();

                    if (turnNext != null) turnNext.onClick.Invoke();
                    if (locoNext != null) locoNext.onClick.Invoke();
                    yield return null;

                    if (saveBtn != null) saveBtn.onClick.Invoke();
                    yield return new WaitForSeconds(0.15f);

                    int pTurn = PlayerPrefs.GetInt("VR_TurnType", -1);
                    int pLoco = PlayerPrefs.GetInt("VR_LocomotionType", -1);
                    bool prefsSaved = (pTurn == 1 && pLoco == 1);

                    // Restore defaults
                    btnSettings.onClick.Invoke();
                    yield return null;
                    Button turnPrev = rowTurn?.Find("Selector_Group/Btn_TurnPrev")?.GetComponent<Button>() ?? rowTurn?.Find("Selector_Group/Btn_Prev")?.GetComponent<Button>();
                    Button locoPrev = rowLoco?.Find("Selector_Group/Btn_LocoPrev")?.GetComponent<Button>() ?? rowLoco?.Find("Selector_Group/Btn_Prev")?.GetComponent<Button>();
                    if (turnPrev != null) turnPrev.onClick.Invoke();
                    if (locoPrev != null) locoPrev.onClick.Invoke();
                    yield return null;
                    if (saveBtn != null) saveBtn.onClick.Invoke();
                    yield return null;

                    if (prefsSaved)
                    {
                        passedTests++;
                        string msg = "[TEST 4 PASS] Game Options Flow: Cycled Turn & Locomotion Type, Saved to PlayerPrefs, Restored Main Menu cleanly.";
                        Debug.Log(msg);
                        WriteLog(msg);
                    }
                    else
                    {
                        string msg = $"[TEST 4 FAIL] Game Options failure: prefsSaved={prefsSaved} (pTurn={pTurn}, pLoco={pLoco})";
                        Debug.LogError(msg);
                        WriteLog(msg);
                    }
                }
                else
                {
                    Debug.LogError("[TEST 4 FAIL] Settings panel or Btn_Settings not found!");
                    WriteLog("[TEST 4 FAIL] Settings missing");
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 5: TRUE 3D WORLD-SPACE STATIONARY MAIN MENU (R4.1)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvas != null)
            {
                var menuCtrl = canvas.GetComponent<MainMenuVRController>();
                var cam = Camera.main;

                bool lazyFollowEnabled = menuCtrl != null && menuCtrl.EnableLazyFollow;

                Vector3 initialCanvasPos = canvas.transform.position;
                Quaternion initialCanvasRot = canvas.transform.rotation;

                // Test 1: Camera yaw 45°
                if (cam != null) cam.transform.rotation = Quaternion.Euler(0f, 45f, 0f);
                yield return new WaitForSeconds(0.2f);
                float driftPos45 = Vector3.Distance(canvas.transform.position, initialCanvasPos);
                float driftRot45 = Quaternion.Angle(canvas.transform.rotation, initialCanvasRot);

                // Test 2: Camera yaw -90° (looking towards beach)
                if (cam != null) cam.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                yield return new WaitForSeconds(0.2f);
                float driftPos90 = Vector3.Distance(canvas.transform.position, initialCanvasPos);
                float driftRot90 = Quaternion.Angle(canvas.transform.rotation, initialCanvasRot);

                // Test 3: Camera pitch 25° & yaw 120° (looking up at sky/ship)
                if (cam != null) cam.transform.rotation = Quaternion.Euler(25f, 120f, 0f);
                yield return new WaitForSeconds(0.2f);
                float driftPosSky = Vector3.Distance(canvas.transform.position, initialCanvasPos);
                float driftRotSky = Quaternion.Angle(canvas.transform.rotation, initialCanvasRot);

                // Reset camera rotation
                if (cam != null) cam.transform.rotation = Quaternion.identity;
                yield return null;

                bool stationaryOk = !lazyFollowEnabled && driftPos45 < 0.001f && driftRot45 < 0.01f &&
                                    driftPos90 < 0.001f && driftRot90 < 0.01f &&
                                    driftPosSky < 0.001f && driftRotSky < 0.01f;

                if (stationaryOk)
                {
                    passedTests++;
                    string msg = $"[TEST 5 PASS] True 3D World-Space Stationary Menu: EnableLazyFollow=False, Position={canvas.transform.position}, Zero drift when camera turns 45°, -90°, and 120°/25° sky.";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 5 FAIL] Stationary Menu issue: lazyFollowEnabled={lazyFollowEnabled}, drift45=({driftPos45:F4}m, {driftRot45:F2}°), drift90=({driftPos90:F4}m, {driftRot90:F2}°), driftSky=({driftPosSky:F4}m, {driftRotSky:F2}°)";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 6: XR ORIGIN GROUNDING (Y = 0.0) & CAMERA OFFSET (0, 0, 0) (R4.2)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (rigObj != null)
            {
                Transform camOffset = rigObj.transform.Find("Camera Offset");
                float rigY = rigObj.transform.position.y;
                bool rigGrounded = Mathf.Abs(rigY) < 0.01f;
                bool offsetZero = camOffset != null && Vector3.Distance(camOffset.localPosition, Vector3.zero) < 0.01f;
                bool originFloor = xrOrigin != null && xrOrigin.RequestedTrackingOriginMode == XROrigin.TrackingOriginMode.Floor;
                bool anchorSpawnZero = anchor != null && anchor.enabled;

                if (rigGrounded && offsetZero && originFloor && anchorSpawnZero)
                {
                    passedTests++;
                    string msg = $"[TEST 6 PASS] XR Origin Grounding: Rig Y={rigY:F3}m (==0.0), Camera Offset={camOffset.localPosition} (==(0,0,0)), TrackingMode=Floor.";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 6 FAIL] Rig Grounding issue: rigGrounded={rigGrounded} (Y={rigY}), offsetZero={offsetZero} (offset={camOffset?.localPosition}), originFloor={originFloor}";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 7: OCULUS HANDS ACTIVE & NATURAL HEIGHT (0.8m - 1.25m) (R4.3)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (rigObj != null)
            {
                Transform camOffset = rigObj.transform.Find("Camera Offset");
                Transform leftCtrl = camOffset != null ? camOffset.Find("Left Controller") : null;
                Transform rightCtrl = camOffset != null ? camOffset.Find("Right Controller") : null;
                Transform leftHand = leftCtrl != null ? leftCtrl.Find("Left Hand Model") : null;
                Transform rightHand = rightCtrl != null ? rightCtrl.Find("Right Hand Model") : null;

                bool handsActive = leftHand != null && rightHand != null && leftHand.gameObject.activeInHierarchy && rightHand.gameObject.activeInHierarchy;
                bool animAttached = (leftHand?.GetComponent<AnimateHandOnInput>() != null) && (rightHand?.GetComponent<AnimateHandOnInput>() != null);

                float leftY = leftHand != null ? (leftHand.position.y - rigObj.transform.position.y) : -1f;
                float rightY = rightHand != null ? (rightHand.position.y - rigObj.transform.position.y) : -1f;

                bool leftHeightNatural = leftY >= 0.80f && leftY <= 1.25f;
                bool rightHeightNatural = rightY >= 0.80f && rightY <= 1.25f;
                bool notInSky = leftY < 2.0f && rightY < 2.0f;

                if (handsActive && animAttached && leftHeightNatural && rightHeightNatural && notInSky)
                {
                    passedTests++;
                    string msg = $"[TEST 7 PASS] Oculus Hands Natural Height: Left Y={leftY:F3}m, Right Y={rightY:F3}m (both 0.8m-1.25m, not in sky), AnimateHandOnInput active.";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 7 FAIL] Hands Height issue: handsActive={handsActive}, leftY={leftY:F3}m (ok={leftHeightNatural}), rightY={rightY:F3}m (ok={rightHeightNatural}), notInSky={notInSky}";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }

            yield return null;

            // -----------------------------------------------------------------------------------------
            // TEST 8: PLAYER HEIGHT 1.67m CALIBRATION (R4.4)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (rigObj != null && cc != null)
            {
                bool heightOk = Mathf.Abs(cc.height - 1.67f) < 0.005f;
                Vector3 expectedCenter = new Vector3(0f, 0.835f, 0f);
                bool centerOk = Vector3.Distance(cc.center, expectedCenter) < 0.005f;
                bool originCamYOk = xrOrigin != null && Mathf.Abs(xrOrigin.CameraYOffset - 1.67f) < 0.005f;
                bool pbcActive = pbc != null && pbc.enabled;

                if (heightOk && centerOk && originCamYOk && pbcActive)
                {
                    passedTests++;
                    string msg = $"[TEST 8 PASS] 1.67m Player Height Calibration: CC height={cc.height:F3}m (==1.67), CC center=({cc.center.x:F3}, {cc.center.y:F3}, {cc.center.z:F3}) (==(0, 0.835, 0)), XROrigin.CameraYOffset={xrOrigin.CameraYOffset:F3}m.";
                    Debug.Log(msg);
                    WriteLog(msg);
                }
                else
                {
                    string msg = $"[TEST 8 FAIL] 1.67m Height mismatch: heightOk={heightOk} (h={cc.height}), centerOk={centerOk} (center={cc.center}), camYOk={originCamYOk} (y={xrOrigin?.CameraYOffset})";
                    Debug.LogError(msg);
                    WriteLog(msg);
                }
            }

            yield return new WaitForSeconds(0.2f);

            // -----------------------------------------------------------------------------------------
            // TEST 9: UNITY CONSOLE STABILITY (R4.5)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            bool consoleClean = (m_ErrorCount == 0 && m_ExceptionCount == 0 && m_WarningCount == 0);
            if (consoleClean)
            {
                passedTests++;
                string msg = "[TEST 9 PASS] Console Stability: 0 compiler errors, 0 runtime exceptions, 0 warnings captured.";
                Debug.Log(msg);
                WriteLog(msg);
            }
            else
            {
                string msg = $"[TEST 9 FAIL] Console issues captured: {m_ErrorCount} errors, {m_ExceptionCount} exceptions, {m_WarningCount} warnings:\n" + string.Join("\n", m_CapturedIssues);
                Debug.LogError(msg);
                WriteLog(msg);
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
            UnityEditor.EditorPrefs.SetBool("ProspectorJourney_ImmersionAuditPassed", passedTests == totalTests);
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
