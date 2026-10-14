using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProspectorJourney.UI
{
    public class MainMenuOptionsPlayModeAudit : MonoBehaviour
    {
        private IEnumerator Start()
        {
            Debug.Log("[MAIN MENU AUDIT] ================================================================");
            Debug.Log("[MAIN MENU AUDIT] STARTING GAME OPTIONS INTERACTION & VERIFICATION TEST PASS");
            Debug.Log("[MAIN MENU AUDIT] ================================================================");

            yield return new WaitForSeconds(0.2f);

            var canvas = GameObject.Find("VR_MainMenu_Canvas");
            if (canvas == null)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: VR_MainMenu_Canvas not found in scene!");
                yield break;
            }

            var controller = canvas.GetComponent<MainMenuVRController>();
            if (controller == null)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: MainMenuVRController component not found!");
                yield break;
            }

            Transform innerBoard = canvas.transform.Find("Board_Background/Gold_Frame/Inner_Board");
            Transform panelsContent = innerBoard.Find("Panels_Content");
            Transform mainPanel = panelsContent.Find("Main_Buttons_Panel");
            Transform settingsPanel = panelsContent.Find("Settings_Panel");

            // Check 1: Hierarchy and Initial Panel State
            if (mainPanel == null || settingsPanel == null)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Main_Buttons_Panel or Settings_Panel missing!");
                yield break;
            }

            if (!mainPanel.gameObject.activeSelf)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Main_Buttons_Panel should be active at start!");
                yield break;
            }
            if (settingsPanel.gameObject.activeSelf)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Settings_Panel should be inactive at start!");
                yield break;
            }
            Debug.Log("[PASS] Check 1: Main menu initialized with Main_Buttons_Panel active and Settings_Panel inactive.");

            // Check 2: Click 'GAME OPTIONS' (Btn_Settings)
            Button btnSettings = mainPanel.Find("Btn_Settings")?.GetComponent<Button>();
            if (btnSettings == null)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Btn_Settings not found!");
                yield break;
            }

            btnSettings.onClick.Invoke();
            yield return null;

            if (!settingsPanel.gameObject.activeSelf)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Clicking Btn_Settings did not activate Settings_Panel!");
                yield break;
            }
            if (mainPanel.gameObject.activeSelf)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Main_Buttons_Panel should be hidden when Settings_Panel is active!");
                yield break;
            }
            Debug.Log("[PASS] Check 2: Clicking 'GAME OPTIONS' opened Settings_Panel and hid main panel.");

            // Find UI Elements inside Settings_Panel
            Transform frame = settingsPanel.Find("Frame_GameOptions");
            Transform rowTurn = frame.Find("Row_TurnType");
            Transform rowLoco = frame.Find("Row_LocomotionType");
            Button saveBtn = settingsPanel.Find("Btn_SaveAndExit")?.GetComponent<Button>();

            TextMeshProUGUI turnText = rowTurn.Find("Selector_Group/Text_Value")?.GetComponent<TextMeshProUGUI>();
            Button turnPrev = rowTurn.Find("Selector_Group/Btn_Prev")?.GetComponent<Button>();
            Button turnNext = rowTurn.Find("Selector_Group/Btn_Next")?.GetComponent<Button>();

            TextMeshProUGUI locoText = rowLoco.Find("Selector_Group/Text_Value")?.GetComponent<TextMeshProUGUI>();
            Button locoPrev = rowLoco.Find("Selector_Group/Btn_Prev")?.GetComponent<Button>();
            Button locoNext = rowLoco.Find("Selector_Group/Btn_Next")?.GetComponent<Button>();

            if (turnText == null || turnPrev == null || turnNext == null ||
                locoText == null || locoPrev == null || locoNext == null || saveBtn == null)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Game Options components or selector buttons missing!");
                yield break;
            }

            // Check 3: Turn Type Cycling
            string initialTurn = turnText.text;
            turnNext.onClick.Invoke();
            yield return null;
            string cycledTurn = turnText.text;

            if (initialTurn == cycledTurn)
            {
                Debug.LogError($"[MAIN MENU AUDIT] FAIL: Turn Type did not change on next click! Text={turnText.text}");
                yield break;
            }

            turnPrev.onClick.Invoke();
            yield return null;
            if (turnText.text != initialTurn)
            {
                Debug.LogError($"[MAIN MENU AUDIT] FAIL: Turn Type did not return to initial on prev click! Text={turnText.text}");
                yield break;
            }
            Debug.Log($"[PASS] Check 3: Turn Type cycle functional ('{initialTurn}' -> '{cycledTurn}' -> '{turnText.text}').");

            // Check 4: Locomotion Type Cycling
            string initialLoco = locoText.text;
            locoNext.onClick.Invoke();
            yield return null;
            string cycledLoco = locoText.text;

            if (initialLoco == cycledLoco)
            {
                Debug.LogError($"[MAIN MENU AUDIT] FAIL: Locomotion Type did not change on next click! Text={locoText.text}");
                yield break;
            }

            locoPrev.onClick.Invoke();
            yield return null;
            if (locoText.text != initialLoco)
            {
                Debug.LogError($"[MAIN MENU AUDIT] FAIL: Locomotion Type did not return to initial on prev click! Text={locoText.text}");
                yield break;
            }
            Debug.Log($"[PASS] Check 4: Locomotion Type cycle functional ('{initialLoco}' -> '{cycledLoco}' -> '{locoText.text}').");

            // Check 5: Save And Exit
            // Cycle Turn to CONTINUOUS and Locomotion to CONTINUOUS
            turnNext.onClick.Invoke(); // Set to Continuous
            locoNext.onClick.Invoke(); // Set to Continuous
            yield return null;

            saveBtn.onClick.Invoke();
            yield return null;

            if (settingsPanel.gameObject.activeSelf)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Save and Exit did not hide Settings_Panel!");
                yield break;
            }
            if (!mainPanel.gameObject.activeSelf)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Save and Exit did not restore Main_Buttons_Panel!");
                yield break;
            }

            int savedTurn = PlayerPrefs.GetInt("VR_TurnType", -1);
            int savedLoco = PlayerPrefs.GetInt("VR_LocomotionType", -1);
            if (savedTurn != 1 || savedLoco != 1)
            {
                Debug.LogError($"[MAIN MENU AUDIT] FAIL: PlayerPrefs did not store expected values! savedTurn={savedTurn}, savedLoco={savedLoco}");
                yield break;
            }

            // Restore defaults (Snap = 0, Teleport = 0)
            btnSettings.onClick.Invoke();
            yield return null;
            turnPrev.onClick.Invoke(); // Back to Snap
            locoPrev.onClick.Invoke(); // Back to Teleport
            yield return null;
            saveBtn.onClick.Invoke();
            yield return null;

            if (PlayerPrefs.GetInt("VR_TurnType", -1) != 0 || PlayerPrefs.GetInt("VR_LocomotionType", -1) != 0)
            {
                Debug.LogError("[MAIN MENU AUDIT] FAIL: Restoring defaults failed!");
                yield break;
            }
            Debug.Log("[PASS] Check 5: 'SAVE AND EXIT' successfully persisted PlayerPrefs (VR_TurnType & VR_LocomotionType) and cleanly returned to Main Menu.");

            Debug.Log("[MAIN MENU AUDIT] ================================================================");
            Debug.Log("[MAIN MENU AUDIT] COMPLETE REPORT: 100% PASS (5/5 CHECKS SATISFIED)");
            Debug.Log("[MAIN MENU AUDIT] ================================================================");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
