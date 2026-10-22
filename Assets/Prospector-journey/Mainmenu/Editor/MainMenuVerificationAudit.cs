using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using ProspectorJourney.Room;

namespace ProspectorJourney.UI.Editor
{
    public static class MainMenuVerificationAudit
    {
        [MenuItem("ProspectorJourney/Verify Main Menu Overhaul")]
        public static void RunVerification()
        {
            Debug.Log("==================================================");
            Debug.Log("[AUDIT] Starting Main Menu Overhaul Verification");
            Debug.Log("==================================================");

            bool allPassed = true;

            // 1. Verify Canvas & Panels
            GameObject canvasObj = GameObject.Find("VR_MainMenu_Canvas");
            if (canvasObj == null)
            {
                Debug.LogError("[AUDIT FAIL] VR_MainMenu_Canvas not found!");
                return;
            }

            // Verify Scale >= 0.0015
            bool scalePass = canvasObj.transform.localScale.x >= 0.0015f;
            Debug.Log($"[AUDIT 0.1] Canvas localScale: {canvasObj.transform.localScale.x} (>= 0.0015) -> {(scalePass ? "PASS" : "FAIL")}");
            if (!scalePass) allPassed = false;

            Transform boardBg = canvasObj.transform.Find("Board_Background");
            if (boardBg != null)
            {
                Image bgImg = boardBg.GetComponent<Image>();
                Transform wireframe = boardBg.Find("Wireframe_Border");
                bool bgTransparent = bgImg != null && bgImg.color.a >= 0.20f && bgImg.color.a <= 0.60f;
                bool borderPass = wireframe != null && wireframe.childCount == 4;

                Debug.Log($"[AUDIT 1.1] Board_Background Image color: {bgImg?.color} (alpha 0.20-0.60), Wireframe_Border: {borderPass} -> {(bgTransparent && borderPass ? "PASS" : "FAIL")}");
                if (!bgTransparent || !borderPass) allPassed = false;
            }
            else
            {
                Debug.LogError("[AUDIT FAIL] Board_Background not found!");
                allPassed = false;
            }

            Transform goldFrame = boardBg?.Find("Gold_Frame");
            if (goldFrame != null)
            {
                Image gfImg = goldFrame.GetComponent<Image>();
                bool gfZeroAlpha = gfImg != null && Mathf.Approximately(gfImg.color.a, 0f);
                Debug.Log($"[AUDIT 1.2] Gold_Frame alpha: {gfImg?.color.a} -> {(gfZeroAlpha ? "PASS" : "FAIL")}");
                if (!gfZeroAlpha) allPassed = false;
            }
            else
            {
                Debug.LogError("[AUDIT FAIL] Gold_Frame not found under Board_Background!");
                allPassed = false;
            }

            Transform innerBoard = goldFrame?.Find("Inner_Board");
            if (innerBoard != null)
            {
                Image ibImg = innerBoard.GetComponent<Image>();
                bool ibZeroAlpha = ibImg != null && Mathf.Approximately(ibImg.color.a, 0f);
                Debug.Log($"[AUDIT 1.3] Inner_Board alpha: {ibImg?.color.a} -> {(ibZeroAlpha ? "PASS" : "FAIL")}");
                if (!ibZeroAlpha) allPassed = false;
            }
            else
            {
                Debug.LogError("[AUDIT FAIL] Inner_Board not found under Gold_Frame!");
                allPassed = false;
            }

            // 2. Main Buttons & Typography
            Transform panelsContent = innerBoard?.Find("Panels_Content");
            Transform mainPanel = panelsContent?.Find("Main_Buttons_Panel");
            if (mainPanel != null)
            {
                Transform btnExp = mainPanel.Find("Btn_StartExpedition");
                if (btnExp != null)
                {
                    RectTransform rt = btnExp.GetComponent<RectTransform>();
                    TextMeshProUGUI tmp = btnExp.GetComponentInChildren<TextMeshProUGUI>();
                    Transform badge = btnExp.Find("Badge_ComingSoon");
                    TextMeshProUGUI badgeTMP = badge?.Find("Text")?.GetComponent<TextMeshProUGUI>();

                    bool btnPass = rt != null && rt.sizeDelta.y >= 85f && tmp != null && tmp.fontSize >= 42f;
                    bool badgePass = badge != null && badgeTMP != null && badgeTMP.text == "COMING SOON";
                    bool badgeAbove = badge != null && badge.GetComponent<RectTransform>().anchoredPosition.y > 0f;

                    Debug.Log($"[AUDIT 2.1] Btn_StartExpedition height: {rt?.sizeDelta.y}, fontSize: {tmp?.fontSize} -> {(btnPass ? "PASS" : "FAIL")}");
                    Debug.Log($"[AUDIT 2.2] Badge_ComingSoon present: {badge != null}, text: '{badgeTMP?.text}', positioned above: {badgeAbove} -> {(badgePass && badgeAbove ? "PASS" : "FAIL")}");

                    if (!btnPass || !badgePass || !badgeAbove) allPassed = false;
                }
                else
                {
                    Debug.LogError("[AUDIT FAIL] Btn_StartExpedition not found!");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[AUDIT FAIL] Main_Buttons_Panel not found!");
                allPassed = false;
            }

            // 3. Settings Panel (Game Options)
            Transform settingsPanel = panelsContent?.Find("Settings_Panel");
            if (settingsPanel != null)
            {
                Transform frame = settingsPanel.Find("Frame_GameOptions");
                Transform rowTurn = frame?.Find("Row_TurnType") ?? settingsPanel.Find("Row_TurnType");
                Transform rowLoco = frame?.Find("Row_LocomotionType") ?? settingsPanel.Find("Row_LocomotionType");
                Transform saveBtn = settingsPanel.Find("Btn_SaveAndExit");

                TextMeshProUGUI turnLabel = rowTurn?.Find("Label")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI turnVal = (rowTurn?.Find("Selector_Group/Text_TurnValue") ?? rowTurn?.Find("Selector_Group/Text_Value"))?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI locoLabel = rowLoco?.Find("Label")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI locoVal = (rowLoco?.Find("Selector_Group/Text_LocoValue") ?? rowLoco?.Find("Selector_Group/Text_Value"))?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI saveTMP = (saveBtn?.Find("Text") ?? saveBtn?.Find("Btn_SaveAndExit_Text"))?.GetComponent<TextMeshProUGUI>() ?? saveBtn?.GetComponentInChildren<TextMeshProUGUI>();

                bool settingsPass = rowTurn != null && rowLoco != null && saveBtn != null &&
                                     turnLabel?.fontSize >= 36f && turnVal?.fontSize >= 36f &&
                                     locoLabel?.fontSize >= 36f && locoVal?.fontSize >= 36f &&
                                     saveTMP?.fontSize >= 36f;

                Debug.Log($"[AUDIT 3.1] Settings Panel layout & font sizes -> {(settingsPass ? "PASS" : "FAIL")} (TurnLabel: {turnLabel?.fontSize}, TurnVal: {turnVal?.fontSize}, SaveText: {saveTMP?.fontSize})");
                if (!settingsPass) allPassed = false;
            }
            else
            {
                Debug.LogError("[AUDIT FAIL] Settings_Panel not found!");
                allPassed = false;
            }

            // 4. VR Hands Parity
            GameObject rigObj = GameObject.Find("XR Origin (XR Rig)");
            if (rigObj != null)
            {
                Transform leftCtrl = rigObj.transform.Find("Camera Offset/Left Controller");
                Transform rightCtrl = rigObj.transform.Find("Camera Offset/Right Controller");

                Transform leftHand = leftCtrl?.Find("Left Hand Model");
                Transform rightHand = rightCtrl?.Find("Right Hand Model");

                var animLeft = leftHand?.GetComponent<AnimateHandOnInput>();
                var animRight = rightHand?.GetComponent<AnimateHandOnInput>();

                bool handsPass = leftHand != null && rightHand != null && animLeft != null && animRight != null;

                Debug.Log($"[AUDIT 4.1] Oculus Hands Parity: LeftHand: {leftHand != null}, RightHand: {rightHand != null}, AnimateHandOnInput: {animLeft != null && animRight != null} -> {(handsPass ? "PASS" : "FAIL")}");
                if (!handsPass) allPassed = false;

                // 5. Quest 3 Tracking Offset & Collision Parity
                Transform camOffset = rigObj.transform.Find("Camera Offset");
                var xrOrigin = rigObj.GetComponent<Unity.XR.CoreUtils.XROrigin>();
                var pbc = rigObj.GetComponent<PlayerBodyCollision>();
                var anchor = rigObj.GetComponent<MainMenuPlayerAnchor>();
                var cc = rigObj.GetComponent<CharacterController>();

                bool pbcPass = pbc != null && pbc.enabled;
                bool anchorPass = anchor != null && anchor.enabled;
                bool ccPass = cc != null && cc.enabled && Mathf.Abs(cc.height - 1.67f) < 0.01f && Vector3.Distance(cc.center, new Vector3(0f, 0.835f, 0f)) < 0.01f;
                bool originPass = xrOrigin != null && xrOrigin.RequestedTrackingOriginMode == Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Floor && Mathf.Abs(xrOrigin.CameraYOffset - 1.67f) < 0.01f;
                bool rigGrounded = Mathf.Abs(rigObj.transform.position.y) < 0.01f;
                bool offsetZero = camOffset != null && Vector3.Distance(camOffset.localPosition, Vector3.zero) < 0.01f;

                Debug.Log($"[AUDIT 5.1] XR Calibration: RigGrounded={rigGrounded}, OffsetZero={offsetZero}, CC height={cc?.height} center={cc?.center} -> {(rigGrounded && offsetZero && ccPass && originPass ? "PASS" : "FAIL")}");
                if (!pbcPass || !anchorPass || !ccPass || !originPass || !rigGrounded || !offsetZero) allPassed = false;

                // 6. MainMenuVRController Stationary Canvas Logic
                var vrCtrl = canvasObj.GetComponent<MainMenuVRController>();
                bool vrCtrlPass = vrCtrl != null && !vrCtrl.EnableLazyFollow;
                Debug.Log($"[AUDIT 6.1] MainMenuVRController Stationary (EnableLazyFollow=False) -> {(vrCtrlPass ? "PASS" : "FAIL")}");
                if (!vrCtrlPass) allPassed = false;
            }
            else
            {
                Debug.LogError("[AUDIT FAIL] XR Origin (XR Rig) not found!");
                allPassed = false;
            }

            Debug.Log("==================================================");
            Debug.Log($"[AUDIT RESULT] OVERALL VERIFICATION: {(allPassed ? "100% PASSED" : "FAILED")}");
            Debug.Log("==================================================");
        }
    }
}
