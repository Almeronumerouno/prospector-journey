using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.XR.CoreUtils;
using ProspectorJourney.Room;
using ProspectorJourney.UI;

namespace ProspectorJourney.UI
{
    public class MainMenuReturnPlayModeAudit : MonoBehaviour
    {
        private static string LogPath => Path.Combine(Application.dataPath, "Prospector-journey/Gameplay/return_audit_log.txt");

        private static void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(LogPath, text + "\n");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[ReturnAuditLog Error] {e.Message}");
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private IEnumerator Start()
        {
            try { File.WriteAllText(LogPath, "=== START MAIN MENU RETURN & COLLISION AUDIT ===\n"); } catch { }
            Debug.Log("================================================================================");
            Debug.Log("[RETURN AUDIT] STARTING RETURN TO MAIN MENU & COLLISION VERIFICATION");
            Debug.Log("================================================================================");

            int totalTests = 0;
            int passedTests = 0;

            // Wait 0.5s for initial scene load to settle
            yield return new WaitForSeconds(0.5f);

            // -----------------------------------------------------------------------------------------
            // TEST 1: INITIAL TUTORIAL SCENE STATE & SPAWN POSITION
            // -----------------------------------------------------------------------------------------
            totalTests++;
            string initialScene = SceneManager.GetActiveScene().name;
            var initialRig = FindAnyObjectByType<XROrigin>();
            Vector3 rigInitialPos = initialRig != null ? initialRig.transform.position : Vector3.zero;
            bool test1 = initialScene == "Tutorial scene" && initialRig != null;

            if (test1)
            {
                passedTests++;
                Debug.Log($"[AUDIT TEST 1 PASS] Started in Tutorial scene at rig position: {rigInitialPos}");
                WriteLog($"[TEST 1 PASS] Started in Tutorial scene, rig pos={rigInitialPos}");
            }
            else
            {
                Debug.LogError($"[AUDIT TEST 1 FAIL] Expected 'Tutorial scene', got '{initialScene}'");
                WriteLog($"[TEST 1 FAIL] Scene={initialScene}");
            }

            // -----------------------------------------------------------------------------------------
            // TEST 2: TRIGGER PAUSE MENU & QUIT TO MAIN MENU
            // -----------------------------------------------------------------------------------------
            totalTests++;
            var pauseCtrl = PauseMenuController.Instance;
            if (pauseCtrl == null)
            {
                pauseCtrl = FindAnyObjectByType<PauseMenuController>();
            }

            if (pauseCtrl != null)
            {
                pauseCtrl.PauseGame();
                yield return new WaitForSecondsRealtime(0.2f);

                if (PauseMenuController.IsGamePaused)
                {
                    Debug.Log("[AUDIT TEST 2] Pause menu successfully activated. Clicking Quit to Main Menu...");
                    pauseCtrl.OnClickQuitToMainMenu();
                    passedTests++;
                    Debug.Log("[AUDIT TEST 2 PASS] Quit to Main Menu invoked.");
                    WriteLog("[TEST 2 PASS] Pause toggled and OnClickQuitToMainMenu invoked.");
                }
                else
                {
                    Debug.LogError("[AUDIT TEST 2 FAIL] Pause menu failed to pause game.");
                    WriteLog("[TEST 2 FAIL] Game not paused.");
                }
            }
            else
            {
                Debug.LogError("[AUDIT TEST 2 FAIL] PauseMenuController instance not found in Tutorial scene.");
                WriteLog("[TEST 2 FAIL] PauseMenuController missing.");
            }

            // Wait for Mainmenu scene to load (up to 5 seconds)
            float waitTimer = 0f;
            while (SceneManager.GetActiveScene().name != "Mainmenu scene" && waitTimer < 5.0f)
            {
                waitTimer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Wait 0.5s for rebinding, simulator reset, and anchor to settle
            yield return new WaitForSecondsRealtime(0.5f);

            // -----------------------------------------------------------------------------------------
            // TEST 3: MAIN MENU LOADED & SPAWN ANCHOR POSITION (0, 0, 0)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            string loadedScene = SceneManager.GetActiveScene().name;
            var menuRig = FindAnyObjectByType<XROrigin>();

            if (loadedScene == "Mainmenu scene" && menuRig != null)
            {
                Vector3 rigPos = menuRig.transform.position;
                Vector3 camLocalPos = menuRig.Camera != null ? menuRig.Camera.transform.localPosition : Vector3.zero;
                Vector3 camWorldPos = menuRig.Camera != null ? menuRig.Camera.transform.position : Vector3.zero;

                // Rig should be at (0, 0, 0)
                bool rigAtOrigin = Vector3.Distance(rigPos, Vector3.zero) < 0.1f;
                // Camera world position horizontal coordinates should be within 0.35m of (0, 0), NOT at (22.8, 2.3)
                bool camNotAtCliffs = Mathf.Abs(camWorldPos.x) < 0.5f && Mathf.Abs(camWorldPos.z) < 0.5f;

                if (rigAtOrigin && camNotAtCliffs)
                {
                    passedTests++;
                    Debug.Log($"[AUDIT TEST 3 PASS] Player successfully anchored at Main Menu spawn: Rig={rigPos}, CamWorld={camWorldPos}");
                    WriteLog($"[TEST 3 PASS] Player anchored at (0, 0, 0): Rig={rigPos}, Cam={camWorldPos}");
                }
                else
                {
                    Debug.LogError($"[AUDIT TEST 3 FAIL] Player spawned incorrectly! Rig={rigPos}, CamWorld={camWorldPos} (rigAtOrigin={rigAtOrigin}, camNotAtCliffs={camNotAtCliffs})");
                    WriteLog($"[TEST 3 FAIL] Rig={rigPos}, Cam={camWorldPos}");
                }
            }
            else
            {
                Debug.LogError($"[AUDIT TEST 3 FAIL] Mainmenu scene not loaded or XROrigin missing. Scene={loadedScene}");
                WriteLog($"[TEST 3 FAIL] Scene={loadedScene}");
            }

            // -----------------------------------------------------------------------------------------
            // TEST 4: MAIN MENU REQUIRED COMPONENTS (PlayerBodyCollision & MainMenuPlayerAnchor)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (menuRig != null)
            {
                var pbc = menuRig.GetComponent<PlayerBodyCollision>();
                var anchor = menuRig.GetComponent<MainMenuPlayerAnchor>();
                var cc = menuRig.GetComponent<CharacterController>();

                bool pbcOk = pbc != null && pbc.enabled;
                bool anchorOk = anchor != null && anchor.enabled;
                bool ccOk = cc != null && cc.enabled;

                if (pbcOk && anchorOk && ccOk)
                {
                    passedTests++;
                    Debug.Log("[AUDIT TEST 4 PASS] PlayerBodyCollision, MainMenuPlayerAnchor, and CharacterController all active on XR Origin.");
                    WriteLog("[TEST 4 PASS] Collision & Anchor components active.");
                }
                else
                {
                    Debug.LogError($"[AUDIT TEST 4 FAIL] Components missing: PBC={pbcOk}, Anchor={anchorOk}, CC={ccOk}");
                    WriteLog($"[TEST 4 FAIL] PBC={pbcOk}, Anchor={anchorOk}, CC={ccOk}");
                }
            }

            // -----------------------------------------------------------------------------------------
            // TEST 5: NO INVISIBLE WALLS & CLEAN FLOATING CANVAS COLLIDER
            // -----------------------------------------------------------------------------------------
            totalTests++;
            var boundariesRoot = GameObject.Find("[Menu_Collision_Boundaries]");
            var canvasObj = GameObject.Find("VR_MainMenu_Canvas");
            var canvasCollider = canvasObj != null ? canvasObj.GetComponent<Collider>() : null;

            bool noInvisibleWalls = boundariesRoot == null;
            bool canvasNotBlocking = canvasCollider == null || canvasCollider.isTrigger;

            if (noInvisibleWalls && canvasNotBlocking)
            {
                passedTests++;
                Debug.Log("[AUDIT TEST 5 PASS] Invisible boundaries removed (free roaming enabled) and Canvas has no blocking collider.");
                WriteLog("[TEST 5 PASS] No invisible walls & canvas does not block locomotion.");
            }
            else
            {
                Debug.LogError($"[AUDIT TEST 5 FAIL] noInvisibleWalls={noInvisibleWalls}, canvasNotBlocking={canvasNotBlocking}");
                WriteLog($"[TEST 5 FAIL] noInvisibleWalls={noInvisibleWalls}, canvasNotBlocking={canvasNotBlocking}");
            }

            // -----------------------------------------------------------------------------------------
            // TEST 6: FREE LOCOMOTION ROAMING
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (menuRig != null)
            {
                var cc = menuRig.GetComponent<CharacterController>();
                if (cc != null)
                {
                    // Move 3 meters forward to verify free locomotion without artificial invisible box
                    Vector3 startPos = menuRig.transform.position;
                    cc.Move(Vector3.forward * 3.0f);
                    float movedDist = menuRig.transform.position.z - startPos.z;

                    bool playerMovedFreely = movedDist > 1.5f;

                    if (playerMovedFreely)
                    {
                        passedTests++;
                        Debug.Log($"[AUDIT TEST 6 PASS] Player roams freely without artificial boundaries! Moved Z: {movedDist:F2}m");
                        WriteLog($"[TEST 6 PASS] Free locomotion: Moved Z={movedDist:F2}m");
                    }
                    else
                    {
                        Debug.LogError($"[AUDIT TEST 6 FAIL] Player blocked from free movement: Moved Z={movedDist:F2}m");
                        WriteLog($"[TEST 6 FAIL] Player blocked: Moved Z={movedDist:F2}m");
                    }
                }
            }

            // -----------------------------------------------------------------------------------------
            // TEST 7: RESPONSIVE FOLLOW (CANVAS FOLLOWS PLAYER & STOPS WHEN PLAYER STOPS)
            // -----------------------------------------------------------------------------------------
            totalTests++;
            if (canvasObj != null && menuRig != null)
            {
                var controller = canvasObj.GetComponent<MainMenuVRController>();
                // Wait 0.4s for follow to settle after player moved
                yield return new WaitForSecondsRealtime(0.4f);

                Vector3 canvasPos = canvasObj.transform.position;
                Vector3 camPos = menuRig.Camera != null ? menuRig.Camera.transform.position : menuRig.transform.position;
                float distToCam = Vector3.Distance(new Vector3(canvasPos.x, 0, canvasPos.z), new Vector3(camPos.x, 0, camPos.z));

                // Canvas should be close to followDistance (~1.6m) in front of the new player position
                bool canvasFollowedPlayer = distToCam > 0.8f && distToCam < 2.5f;

                // Check that when player stays still, the canvas also stops (no ghost drifting)
                Vector3 pos1 = canvasObj.transform.position;
                yield return new WaitForSecondsRealtime(0.25f);
                Vector3 pos2 = canvasObj.transform.position;
                float drift = Vector3.Distance(pos1, pos2);
                bool isStationaryWhenStopped = drift < 0.015f;

                if (canvasFollowedPlayer && isStationaryWhenStopped)
                {
                    passedTests++;
                    Debug.Log($"[AUDIT TEST 7 PASS] Responsive follow verified: Canvas followed player (dist={distToCam:F2}m) and stopped completely when player stopped (drift={drift:F4}m).");
                    WriteLog($"[TEST 7 PASS] Canvas follows player (dist={distToCam:F2}m) and stops cleanly when player stops (drift={drift:F4}m).");
                }
                else
                {
                    Debug.LogError($"[AUDIT TEST 7 FAIL] canvasFollowedPlayer={canvasFollowedPlayer} (dist={distToCam:F2}m), isStationaryWhenStopped={isStationaryWhenStopped} (drift={drift:F4}m)");
                    WriteLog($"[TEST 7 FAIL] Followed={canvasFollowedPlayer} (dist={distToCam:F2}m), Stationary={isStationaryWhenStopped} (drift={drift:F4}m)");
                }
            }

            // -----------------------------------------------------------------------------------------
            // FINAL REPORT
            // -----------------------------------------------------------------------------------------
            Debug.Log("================================================================================");
            Debug.Log($"[RETURN AUDIT COMPLETE] Passed {passedTests}/{totalTests} tests ({(passedTests == totalTests ? "100% SUCCESS" : "FAILED")})");
            Debug.Log("================================================================================");
            WriteLog($"=== AUDIT RESULT: {passedTests}/{totalTests} PASSED ===");

            yield return new WaitForSecondsRealtime(0.5f);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
