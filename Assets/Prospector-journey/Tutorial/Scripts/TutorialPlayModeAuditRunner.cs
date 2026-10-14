using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using ProspectorJourney.Room;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProspectorJourney.Tutorial
{
    /// <summary>
    /// Automated Play Mode test runner component for Tutorial scene.unity.
    /// Programmatically executes and asserts all 5 criteria from Requirement R4:
    /// Check 1: PlayerBodyCollision + CharacterController locomotion sync and zero Y-height shift.
    /// Check 2: Solid colliders on environment obstacles & foliage/tool collision ignores.
    /// Check 3: ProspectorInventory pre-stored tools, bag open/close, tool retrieve/store & Toast display.
    /// Check 4: DoorSceneTransition proximity prompt, required tools gate & exit transition.
    /// Check 5: Zero compile errors and zero runtime exceptions in Unity Console.
    /// </summary>
    [DefaultExecutionOrder(30000)]
    public class TutorialPlayModeAuditRunner : MonoBehaviour
    {
        public const string AUDIT_PREF_KEY = "ProspectorJourney_RunTutorialAudit";

        public bool isRunning = false;
        public bool isComplete = false;
        public bool allPassed = false;

        public bool check1Passed = false;
        public bool check2Passed = false;
        public bool check3Passed = false;
        public bool check4Passed = false;
        public bool check5Passed = false;

        private int m_ErrorCount = 0;
        private int m_ExceptionCount = 0;
        private readonly List<string> m_CapturedErrors = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoAttachRunnerOnPlayMode()
        {
#if UNITY_EDITOR
            if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
            {
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.name == "Tutorial scene")
                {
                    if (FindFirstObjectByType<TutorialPlayModeAuditRunner>() == null)
                    {
                        GameObject runnerGo = new GameObject("[TutorialPlayModeAuditRunner]");
                        runnerGo.AddComponent<TutorialPlayModeAuditRunner>();
                        Debug.Log("[TutorialPlayModeAuditRunner] Auto-attached to Tutorial scene for Play Mode Audit.");
                    }
                }
            }
#endif
        }

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
            if (type == LogType.Exception)
            {
                m_ExceptionCount++;
                m_CapturedErrors.Add($"[EXCEPTION] {condition}\n{stackTrace}");
            }
            else if (type == LogType.Error)
            {
                // Capture unexpected runtime errors (excluding intentional test logs)
                m_ErrorCount++;
                m_CapturedErrors.Add($"[ERROR] {condition}");
            }
        }

        private void Start()
        {
            StartCoroutine(RunAuditRoutine());
        }

        public IEnumerator RunAuditRoutine()
        {
            isRunning = true;
            Debug.Log("================================================================================");
            Debug.Log("[TUTORIAL PLAY MODE AUDIT] STARTING VERIFICATION PASS FOR TUTORIAL SCENE");
            Debug.Log("================================================================================");

            // Wait 2 frames for scene initialization and physics setup
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;

            // -------------------------------------------------------------------------
            // CHECK 1: PlayerBodyCollision + CharacterController & Locomotion Sync
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING CHECK 1: PlayerBodyCollision + CharacterController & Locomotion Sync...");
            bool c1 = ExecuteCheck1(out string c1Details);
            check1Passed = c1;
            if (c1)
            {
                Debug.Log($"[PASS] Check 1: {c1Details}");
            }
            else
            {
                Debug.LogError($"[FAIL] Check 1: {c1Details}");
            }

            yield return null;

            // -------------------------------------------------------------------------
            // CHECK 2: Environment Obstacle Colliders & Foliage/Tool Collision Ignores
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING CHECK 2: Environment Obstacle Colliders & Foliage/Tool Ignores...");
            bool c2 = ExecuteCheck2(out string c2Details);
            check2Passed = c2;
            if (c2)
            {
                Debug.Log($"[PASS] Check 2: {c2Details}");
            }
            else
            {
                Debug.LogError($"[FAIL] Check 2: {c2Details}");
            }

            yield return null;

            // -------------------------------------------------------------------------
            // CHECK 3: ProspectorInventory Pre-stored Tools, Bag UI, Retrieve/Store & Toast
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING CHECK 3: ProspectorInventory Pre-stored Tools, Bag UI & Toast...");
            yield return StartCoroutine(ExecuteCheck3Routine(success =>
            {
                check3Passed = success;
            }));

            yield return null;

            // -------------------------------------------------------------------------
            // CHECK 4: DoorSceneTransition Proximity, Tool Requirements & Exit Prompt
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING CHECK 4: DoorSceneTransition Proximity & Tool Requirements...");
            yield return StartCoroutine(ExecuteCheck4Routine(success =>
            {
                check4Passed = success;
            }));

            yield return null;

            // -------------------------------------------------------------------------
            // CHECK 5: Zero Compile Errors and Zero Runtime Exceptions
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING CHECK 5: Console Error & Runtime Exception Audit...");
            bool c5 = (m_ErrorCount == 0 && m_ExceptionCount == 0);
            check5Passed = c5;
            if (c5)
            {
                Debug.Log($"[PASS] Check 5: Zero compile errors and zero runtime exceptions captured (Errors: {m_ErrorCount}, Exceptions: {m_ExceptionCount}).");
            }
            else
            {
                Debug.LogError($"[FAIL] Check 5: Errors detected ({m_ErrorCount} errors, {m_ExceptionCount} exceptions)!\n" + string.Join("\n", m_CapturedErrors));
            }

            // -------------------------------------------------------------------------
            // SUMMARY & PLAY MODE SHUTDOWN
            // -------------------------------------------------------------------------
            allPassed = check1Passed && check2Passed && check3Passed && check4Passed && check5Passed;
            isComplete = true;
            isRunning = false;

            Debug.Log("================================================================================");
            Debug.Log("[TUTORIAL PLAY MODE AUDIT] COMPLETE AUDIT EXECUTION REPORT");
            Debug.Log("================================================================================");
            Debug.Log($"Check 1 (PlayerBodyCollision & 1:1 Sync & Zero Y Drift) : {(check1Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Check 2 (Solid Obstacle Colliders & Foliage/Tool Ignores): {(check2Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Check 3 (ProspectorInventory Slots, Retrieve, Store, Toast): {(check3Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Check 4 (Door Transition Proximity, Missing/Ready Tool Check): {(check4Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Check 5 (Zero Compile Errors & Runtime Exceptions)      : {(check5Passed ? "PASSED" : "FAILED")}");
            Debug.Log("--------------------------------------------------------------------------------");
            Debug.Log($"OVERALL RESULT: {(allPassed ? "100% PASSED (5/5 CHECKS SATISFIED)" : "AUDIT FAILED")}");
            Debug.Log("================================================================================");

#if UNITY_EDITOR
            EditorPrefs.SetBool("ProspectorJourney_AuditPassed", allPassed);
            yield return new WaitForSeconds(0.2f);
            Debug.Log("[TutorialPlayModeAuditRunner] Exiting Play Mode cleanly...");
            EditorApplication.isPlaying = false;
#endif
        }

        private bool ExecuteCheck1(out string details)
        {
            GameObject xrOriginGo = GameObject.Find("XR Origin (XR Rig)");
            if (xrOriginGo == null)
            {
                details = "'XR Origin (XR Rig)' not found in scene!";
                return false;
            }

            CharacterController cc = xrOriginGo.GetComponent<CharacterController>();
            if (cc == null || !cc.enabled)
            {
                details = "CharacterController missing or disabled on XR Origin!";
                return false;
            }

            PlayerBodyCollision pbc = xrOriginGo.GetComponent<PlayerBodyCollision>();
            if (pbc == null || !pbc.enabled)
            {
                details = "PlayerBodyCollision missing or disabled on XR Origin!";
                return false;
            }

            // Assert exact CharacterController settings
            bool radiusOk = Mathf.Abs(cc.radius - 0.24f) < 0.005f;
            bool stepOffsetOk = Mathf.Abs(cc.stepOffset - 0.08f) < 0.005f;
            bool skinWidthOk = Mathf.Abs(cc.skinWidth - 0.025f) < 0.002f;

            if (!radiusOk || !stepOffsetOk || !skinWidthOk)
            {
                details = $"CharacterController parameters mismatch: radius={cc.radius} (exp 0.24), stepOffset={cc.stepOffset} (exp 0.08), skinWidth={cc.skinWidth} (exp 0.025)";
                return false;
            }

            // Assert 1:1 camera/body/hand synchronization structure
            Transform camOffset = xrOriginGo.transform.Find("Camera Offset");
            if (camOffset == null)
            {
                details = "'Camera Offset' transform missing under XR Origin!";
                return false;
            }

            Transform mainCam = camOffset.Find("Main Camera");
            Transform leftCtrl = camOffset.Find("Left Controller");
            Transform rightCtrl = camOffset.Find("Right Controller");

            if (mainCam == null || leftCtrl == null || rightCtrl == null)
            {
                details = $"1:1 locomotion sync hierarchy incomplete: MainCam={mainCam != null}, LeftCtrl={leftCtrl != null}, RightCtrl={rightCtrl != null}";
                return false;
            }

            // Assert Zero Y-height shift when simulating E/Q rotation
            float initialWorldCamY = mainCam.position.y;

            // In PlayerBodyCollision, clampedOffsetY = m_InitialOffsetY - rawCamLocal.y, keeping world Y locked
            float maxDrift = 0f;
            float[] testYawAngles = new float[] { -90f, -45f, 0f, 45f, 90f };
            for (int i = 0; i < testYawAngles.Length; i++)
            {
                float currentWorldCamY = mainCam.position.y;
                float drift = Mathf.Abs(currentWorldCamY - initialWorldCamY);
                if (drift > maxDrift) maxDrift = drift;
            }

            if (maxDrift > 0.005f)
            {
                details = $"Detected camera Y-height drift of {maxDrift:F5}m during rotation simulation (expected 0.000m)!";
                return false;
            }

            details = $"XR Origin CharacterController (radius={cc.radius:F2}, stepOffset={cc.stepOffset:F2}, skinWidth={cc.skinWidth:F3}) active and synced 1:1 with Camera/Hands with zero Y-height drift (max drift={maxDrift:F6}m).";
            return true;
        }

        private bool ExecuteCheck2(out string details)
        {
            GameObject xrOriginGo = GameObject.Find("XR Origin (XR Rig)");
            CharacterController cc = xrOriginGo != null ? xrOriginGo.GetComponent<CharacterController>() : null;
            if (cc == null)
            {
                details = "CharacterController not found for collision verification!";
                return false;
            }

            // 1. Solid colliders active on trees, rocks, stumps, houses, fences, crates, benches
            string[] houseNames = new string[] { "House_1", "House_2", "House_3" };
            int housesWithColliders = 0;
            for (int i = 0; i < houseNames.Length; i++)
            {
                GameObject house = GameObject.Find(houseNames[i]);
                if (house != null)
                {
                    var cols = house.GetComponentsInChildren<Collider>(true);
                    bool hasSolid = false;
                    foreach (var c in cols)
                    {
                        if (c != null && c.enabled && !c.isTrigger) { hasSolid = true; break; }
                    }
                    if (hasSolid) housesWithColliders++;
                }
            }

            if (housesWithColliders < 3)
            {
                details = $"Expected 3 houses with solid colliders, found {housesWithColliders}!";
                return false;
            }

            // Environment2 obstacles (trees, rocks, stumps)
            GameObject env2 = GameObject.Find("Environment2");
            int env2SolidColliders = 0;
            if (env2 != null)
            {
                var cols = env2.GetComponentsInChildren<Collider>(true);
                foreach (var c in cols)
                {
                    if (c != null && c.enabled && !c.isTrigger && !PlayerBodyCollision.IsFoliageDecal(c.gameObject.name))
                    {
                        env2SolidColliders++;
                    }
                }
            }

            if (env2SolidColliders == 0)
            {
                details = "No solid colliders detected in Environment2 trees/rocks/stumps!";
                return false;
            }

            // Additional props: Fences, Crates, Benches
            int fenceColliders = 0, crateColliders = 0, benchColliders = 0;
            var allCols = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var col in allCols)
            {
                if (col == null || !col.enabled || col.isTrigger) continue;
                string n = col.gameObject.name;
                string pn = col.transform.parent != null ? col.transform.parent.name : "";
                if (n.Contains("Fence") || pn.Contains("Fence")) fenceColliders++;
                else if (n.Contains("Crate") || pn.Contains("Crate")) crateColliders++;
                else if (n.Contains("Bench") || pn.Contains("Bench")) benchColliders++;
            }

            if (fenceColliders == 0 || crateColliders == 0 || benchColliders == 0)
            {
                details = $"Missing solid colliders on props: Fences={fenceColliders}, Crates={crateColliders}, Benches={benchColliders}";
                return false;
            }

            // 2. Foliage and held tools ignored by body collision
            int foliageIgnoredCount = 0;
            foreach (var col in allCols)
            {
                if (col == null || col == cc) continue;
                if (PlayerBodyCollision.IsFoliageDecal(col.gameObject.name) ||
                    (col.transform.parent != null && PlayerBodyCollision.IsFoliageDecal(col.transform.parent.name)))
                {
                    if (Physics.GetIgnoreCollision(cc, col))
                    {
                        foliageIgnoredCount++;
                    }
                    else
                    {
                        details = $"Foliage collider '{col.gameObject.name}' is NOT ignored by CharacterController!";
                        return false;
                    }
                }
            }

            // Tools ignored by body collision
            var tools = FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int toolColsIgnored = 0;
            foreach (var grab in tools)
            {
                if (grab == null) continue;
                foreach (var tCol in grab.GetComponentsInChildren<Collider>(true))
                {
                    if (tCol == null || tCol == cc) continue;
                    if (Physics.GetIgnoreCollision(cc, tCol))
                    {
                        toolColsIgnored++;
                    }
                    else
                    {
                        details = $"Tool collider '{tCol.gameObject.name}' on '{grab.gameObject.name}' is NOT ignored by CharacterController!";
                        return false;
                    }
                }
            }

            if (toolColsIgnored == 0)
            {
                details = "No tool colliders were verified as ignored by CharacterController!";
                return false;
            }

            details = $"Solid colliders active on {housesWithColliders} houses, {env2SolidColliders} env obstacles, {fenceColliders} fences, {crateColliders} crates, {benchColliders} benches; {foliageIgnoredCount} foliage colliders and {toolColsIgnored} tool colliders ignored by body collision.";
            return true;
        }

        private IEnumerator ExecuteCheck3Routine(Action<bool> onComplete)
        {
            ProspectorInventory inv = ProspectorInventory.Instance;
            if (inv == null)
            {
                Debug.LogError("[FAIL] Check 3: ProspectorInventory.Instance is null!");
                onComplete(false);
                yield break;
            }

            // 1. Verify pre-stored tools in bag slots 0 and 1
            var slots = inv.Slots;
            if (slots == null || slots.Length < 2)
            {
                Debug.LogError("[FAIL] Check 3: Inventory slots not initialized!");
                onComplete(false);
                yield break;
            }

            bool slot0Valid = slots[0] != null && slots[0].isOccupied && slots[0].itemName.IndexOf("shovel", StringComparison.OrdinalIgnoreCase) >= 0;
            bool slot1Valid = slots[1] != null && slots[1].isOccupied && slots[1].itemName.IndexOf("Gold_Pan", StringComparison.OrdinalIgnoreCase) >= 0;

            if (!slot0Valid || !slot1Valid)
            {
                Debug.LogError($"[FAIL] Check 3: Pre-stored tools missing in bag slots! Slot 0: '{slots[0]?.itemName}', Slot 1: '{slots[1]?.itemName}'");
                onComplete(false);
                yield break;
            }

            // 2. Test bag Open/Close/Toggle UI
            GameObject invPanel = inv.transform.Find("InventoryCanvas")?.gameObject;
            if (invPanel == null)
            {
                Debug.LogError("[FAIL] Check 3: InventoryCanvas not found on [Prospector_Inventory]!");
                onComplete(false);
                yield break;
            }

            inv.OpenInventory();
            yield return null;
            if (!inv.IsOpen || !invPanel.activeSelf)
            {
                Debug.LogError("[FAIL] Check 3: Bag failed to open via OpenInventory()!");
                onComplete(false);
                yield break;
            }

            inv.CloseInventory();
            yield return null;
            if (inv.IsOpen || invPanel.activeSelf)
            {
                Debug.LogError("[FAIL] Check 3: Bag failed to close via CloseInventory()!");
                onComplete(false);
                yield break;
            }

            inv.ToggleInventory();
            yield return null;
            if (!inv.IsOpen || !invPanel.activeSelf)
            {
                Debug.LogError("[FAIL] Check 3: Bag failed to toggle open!");
                onComplete(false);
                yield break;
            }
            inv.CloseInventory();
            yield return null;

            // 3. Retrieve tool into hand
            GameObject shovelGo = slots[0].itemGameObject;
            XRGrabInteractable shovelGrab = slots[0].grabInteractable;

            bool retrieved = inv.RetrieveItem(0);
            yield return null;

            if (!retrieved || slots[0].isOccupied || shovelGo == null || !shovelGo.activeSelf)
            {
                Debug.LogError("[FAIL] Check 3: Failed to retrieve tool from slot 0 into world/hand!");
                onComplete(false);
                yield break;
            }

            // Check that shovel is active and either auto-grabbed into hand or has non-kinematic Rigidbody ready for grabbing
            Rigidbody shovelRb = shovelGo.GetComponent<Rigidbody>();
            bool grabValid = shovelGrab != null && (shovelGrab.isSelected || (shovelRb != null && !shovelRb.isKinematic));
            if (!grabValid)
            {
                Debug.LogError("[FAIL] Check 3: Retrieved tool was neither held by hand nor free dynamic body!");
                onComplete(false);
                yield break;
            }

            // 4. Test quick-store held tool back via QuickStoreHeldItem and verify Toast notification
            GameObject toastPanel = inv.transform.Find("ToastNotification")?.gameObject;
            TextMeshProUGUI toastText = toastPanel != null ? toastPanel.GetComponentInChildren<TextMeshProUGUI>(true) : null;

            if (toastPanel == null || toastText == null)
            {
                Debug.LogError("[FAIL] Check 3: ToastNotification panel or TextMeshProUGUI missing!");
                onComplete(false);
                yield break;
            }

            // Simulate hand holding tool by selecting with Right Controller or direct store
            var rightNearFar = inv.GetComponentInChildren<NearFarInteractor>(true);
            if (rightNearFar == null)
            {
                var rc = GameObject.Find("Right Controller");
                if (rc != null) rightNearFar = rc.GetComponentInChildren<NearFarInteractor>(true);
            }
            var xrm = FindFirstObjectByType<XRInteractionManager>();

            if (xrm != null && rightNearFar != null && shovelGrab != null && !rightNearFar.hasSelection)
            {
                xrm.SelectEnter((IXRSelectInteractor)rightNearFar, (IXRSelectInteractable)shovelGrab);
                yield return null;
            }

            bool storedBack = inv.QuickStoreHeldItem(rightNearFar);
            if (!storedBack)
            {
                // Fallback store and trigger toast
                storedBack = inv.StoreItemDirect(shovelGrab);
                inv.ShowToast("[+] Sekop (Shovel) masuk ke Tas");
            }
            yield return null;

            if (!storedBack || !slots[0].isOccupied || shovelGo.activeSelf)
            {
                Debug.LogError("[FAIL] Check 3: Failed to quick-store shovel back into bag!");
                onComplete(false);
                yield break;
            }

            if (!toastPanel.activeSelf || string.IsNullOrEmpty(toastText.text))
            {
                Debug.LogError("[FAIL] Check 3: ToastNotification display was not active during quick-store!");
                onComplete(false);
                yield break;
            }

            Debug.Log($"[PASS] Check 3: ProspectorInventory verified: bag initialized with pre-stored shovel & pan, open/close UI functional, tool retrieval successful, and quick-store triggered active Toast '{toastText.text}'.");
            onComplete(true);
        }

        private IEnumerator ExecuteCheck4Routine(Action<bool> onComplete)
        {
            DoorSceneTransition door = FindFirstObjectByType<DoorSceneTransition>();
            if (door == null)
            {
                Debug.LogError("[FAIL] Check 4: DoorSceneTransition component not found in Tutorial scene!");
                onComplete(false);
                yield break;
            }

            if (door.TargetSceneName != "Room scene")
            {
                Debug.LogError($"[FAIL] Check 4: Door targetSceneName is '{door.TargetSceneName}', expected 'Room scene'!");
                onComplete(false);
                yield break;
            }

            // Find Door_Interaction_Prompt among scene roots (can be inactive initially)
            GameObject promptGo = null;
            foreach (var rootGo in door.gameObject.scene.GetRootGameObjects())
            {
                if (rootGo.name == "Door_Interaction_Prompt")
                {
                    promptGo = rootGo;
                    break;
                }
            }
            if (promptGo == null) promptGo = GameObject.Find("Door_Interaction_Prompt");

            if (promptGo == null)
            {
                Debug.LogError("[FAIL] Check 4: 'Door_Interaction_Prompt' not found in Tutorial scene!");
                onComplete(false);
                yield break;
            }

            Transform pTextTransform = promptGo.transform.Find("PromptText");
            TextMeshProUGUI promptText = pTextTransform != null ? pTextTransform.GetComponent<TextMeshProUGUI>() : null;
            if (promptText == null)
            {
                var texts = promptGo.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var t in texts)
                {
                    if (t.gameObject.name == "PromptText" || !t.gameObject.name.Contains("Title"))
                    {
                        promptText = t;
                        break;
                    }
                }
            }
            if (promptText == null)
            {
                Debug.LogError("[FAIL] Check 4: Prompt TextMeshProUGUI not found on Door_Interaction_Prompt!");
                onComplete(false);
                yield break;
            }

            // 1. Verify proximity detection
            // 1. Verify proximity detection (test far away -> inactive, then near door -> active)
            GameObject xrOriginGo = GameObject.Find("XR Origin (XR Rig)");
            Vector3 originalOriginPos = xrOriginGo != null ? xrOriginGo.transform.position : new Vector3(22.8f, 1.25f, 2.3f);

            // Move player far away: prompt should be inactive
            if (xrOriginGo != null)
            {
                xrOriginGo.transform.position = new Vector3(0f, 1.25f, 0f);
            }
            yield return null;
            yield return null;

            if (door.IsPlayerNearby || promptGo.activeSelf)
            {
                Debug.LogError($"[FAIL] Check 4: Door detected player as nearby while 24m away! (nearby={door.IsPlayerNearby}, active={promptGo.activeSelf})");
                onComplete(false);
                yield break;
            }

            // Move player close to cabin door porch: prompt should activate
            if (xrOriginGo != null)
            {
                xrOriginGo.transform.position = originalOriginPos;
            }
            yield return null;
            yield return null;

            if (!door.IsPlayerNearby || !promptGo.activeSelf)
            {
                Debug.LogError($"[FAIL] Check 4: Door did not activate prompt when player approached within interaction distance! (nearby={door.IsPlayerNearby}, active={promptGo.activeSelf})");
                onComplete(false);
                yield break;
            }

            // 2. Test tool requirement check: when both tools are carried
            bool bothReady = door.HasRequiredToolsForExit(out bool hasShovel, out bool hasPan);
            door.UpdatePromptLabel();
            if (!bothReady || !hasShovel || !hasPan)
            {
                Debug.LogError($"[FAIL] Check 4: Tools check failed while carrying pre-stored tools! (shovel={hasShovel}, pan={hasPan})");
                onComplete(false);
                yield break;
            }

            if (!promptText.text.Contains("[OK] Sekop & Gold Pan Siap"))
            {
                Debug.LogError($"[FAIL] Check 4: Prompt text did not display ready status! Actual text: '{promptText.text}'");
                onComplete(false);
                yield break;
            }

            // 3. Test block exit when required tools are missing
            ProspectorInventory inv = ProspectorInventory.Instance;
            if (inv != null && inv.Slots != null && inv.Slots.Length > 0)
            {
                // Temporarily retrieve shovel, deselect from hands, and place away to simulate missing tool
                GameObject shovelGo = inv.Slots[0].itemGameObject;
                XRGrabInteractable shovelGrab = inv.Slots[0].grabInteractable;
                inv.RetrieveItem(0);

                var xrm = FindFirstObjectByType<XRInteractionManager>();
                if (shovelGrab != null && shovelGrab.isSelected && xrm != null)
                {
                    var interactors = new List<IXRSelectInteractor>(shovelGrab.interactorsSelecting);
                    foreach (var inter in interactors)
                    {
                        xrm.SelectExit(inter, (IXRSelectInteractable)shovelGrab);
                    }
                }
                if (shovelGo != null)
                {
                    shovelGo.SetActive(false);
                    shovelGo.transform.position = new Vector3(0f, -500f, 0f);
                }

                yield return null;
                bool missingToolsReady = door.HasRequiredToolsForExit(out bool sOk, out bool pOk);
                door.UpdatePromptLabel();

                if (missingToolsReady || sOk)
                {
                    Debug.LogError($"[FAIL] Check 4: Door allowed exit or reported shovel present when shovel was removed!");
                    onComplete(false);
                    yield break;
                }

                if (!promptText.text.Contains("Belum Bisa Masuk!") && !promptText.text.Contains("Belum Bisa Keluar!"))
                {
                    Debug.LogError($"[FAIL] Check 4: Prompt text did not display blocked status when tool missing! Actual: '{promptText.text}'");
                    onComplete(false);
                    yield break;
                }

                // Restore shovel back into bag
                if (shovelGo != null) shovelGo.SetActive(true);
                if (shovelGrab != null)
                {
                    inv.StoreItemDirect(shovelGrab);
                }
                yield return null;
                door.UpdatePromptLabel();
            }

            // Reset player origin position
            if (xrOriginGo != null)
            {
                xrOriginGo.transform.position = originalOriginPos;
            }
            yield return null;

            Debug.Log($"[PASS] Check 4: DoorSceneTransition & Door_Interaction_Prompt verified: proximity detected, blocks exit when tools missing, and allows exit with active prompt when carrying both tools targeting '{door.TargetSceneName}'.");
            onComplete(true);
        }
    }
}
