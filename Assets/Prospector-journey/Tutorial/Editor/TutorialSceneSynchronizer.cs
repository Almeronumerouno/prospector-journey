using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using ProspectorJourney.Room;
using Object = UnityEngine.Object;

namespace ProspectorJourney.Tutorial.Editor
{
    public static class TutorialSceneSynchronizer
    {
        private const string TUTORIAL_SCENE_PATH = "Assets/Prospector-journey/Tutorial/Tutorial scene.unity";
        private const string ROOM_SCENE_PATH = "Assets/Prospector-journey/Room/Room scene.unity";

        [MenuItem("ProspectorJourney/Sync Tutorial Scene M2")]
        public static void SyncTutorialScene()
        {
            Debug.Log("[TutorialSceneSynchronizer] Starting Milestone M2 Scene Synchronization...");

            // 1. Open Tutorial scene
            Scene tutorialScene = EditorSceneManager.OpenScene(TUTORIAL_SCENE_PATH, OpenSceneMode.Single);
            if (!tutorialScene.IsValid() || !tutorialScene.isLoaded)
            {
                Debug.LogError($"[TutorialSceneSynchronizer] Failed to open {TUTORIAL_SCENE_PATH}");
                return;
            }
            SceneManager.SetActiveScene(tutorialScene);

            // 2. Configure XR Origin (XR Rig) position and body collision
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                Debug.LogError("[TutorialSceneSynchronizer] 'XR Origin (XR Rig)' not found in Tutorial scene!");
                return;
            }

            xrOrigin.transform.position = new Vector3(22.8f, 1.25f, 2.3f);
            xrOrigin.transform.rotation = Quaternion.Euler(0f, 145f, 0f);
            EditorUtility.SetDirty(xrOrigin);
            Debug.Log($"[TutorialSceneSynchronizer] XR Origin (XR Rig) positioned at Tutorial cabin door porch: {xrOrigin.transform.position}");

            CharacterController cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null)
            {
                SerializedObject ccSo = new SerializedObject(cc);
                ccSo.FindProperty("m_Radius").floatValue = 0.24f;
                ccSo.FindProperty("m_StepOffset").floatValue = 0.08f;
                ccSo.FindProperty("m_SkinWidth").floatValue = 0.025f;
                ccSo.FindProperty("m_MinMoveDistance").floatValue = 0.0001f;
                ccSo.FindProperty("m_SlopeLimit").floatValue = 45f;
                ccSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(cc);
                Debug.Log("[TutorialSceneSynchronizer] CharacterController configured on XR Origin (XR Rig).");
            }
            else
            {
                Debug.LogError("[TutorialSceneSynchronizer] CharacterController not found on XR Origin (XR Rig)!");
            }

            PlayerBodyCollision pbc = xrOrigin.GetComponent<PlayerBodyCollision>();
            if (pbc == null)
            {
                pbc = Undo.AddComponent<PlayerBodyCollision>(xrOrigin);
                Debug.Log("[TutorialSceneSynchronizer] Added PlayerBodyCollision to XR Origin (XR Rig).");
            }
            if (pbc != null)
            {
                SerializedObject pbcSo = new SerializedObject(pbc);
                pbcSo.FindProperty("bodyRadius").floatValue = 0.24f;
                pbcSo.FindProperty("stepOffset").floatValue = 0.08f;
                pbcSo.FindProperty("skinWidth").floatValue = 0.025f;
                pbcSo.FindProperty("minHeadHeight").floatValue = 0.85f;
                pbcSo.FindProperty("maxHeadHeight").floatValue = 2.05f;
                pbcSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(pbc);
                Debug.Log("[TutorialSceneSynchronizer] PlayerBodyCollision properties configured.");
            }

            // 3. Open Room scene additively to port references & update its default spawn
            Scene roomScene = EditorSceneManager.OpenScene(ROOM_SCENE_PATH, OpenSceneMode.Additive);
            if (!roomScene.IsValid() || !roomScene.isLoaded)
            {
                Debug.LogError($"[TutorialSceneSynchronizer] Failed to additively open {ROOM_SCENE_PATH}");
                return;
            }

            GameObject roomInventory = null;
            GameObject roomShovel = null;
            GameObject roomPan = null;
            GameObject roomDoorSlab = null;
            GameObject roomPrompt = null;
            GameObject roomOrigin = null;
            GameObject roomAnchorEntrance = null;

            foreach (var rootGo in roomScene.GetRootGameObjects())
            {
                if (rootGo.name == "[Prospector_Inventory]")
                {
                    roomInventory = rootGo;
                }
                else if (rootGo.name == "Door_Interaction_Prompt")
                {
                    roomPrompt = rootGo;
                }
                else if (rootGo.name == "XR Origin (XR Rig)")
                {
                    roomOrigin = rootGo;
                }
                else if (rootGo.name == "Teleport_Anchors")
                {
                    var ent = rootGo.transform.Find("Anchor_Entrance");
                    if (ent != null) roomAnchorEntrance = ent.gameObject;
                }
            }

            if (roomAnchorEntrance == null) roomAnchorEntrance = GameObject.Find("Anchor_Entrance");

            // Reposition Room scene XR Origin right to Anchor_Entrance (in front of the door)
            if (roomOrigin != null)
            {
                Vector3 roomEntPos = roomAnchorEntrance != null ? roomAnchorEntrance.transform.position : new Vector3(0f, 0.01f, -2.0f);
                roomOrigin.transform.position = roomEntPos;
                roomOrigin.transform.rotation = Quaternion.identity;
                EditorUtility.SetDirty(roomOrigin);
                EditorSceneManager.MarkSceneDirty(roomScene);
                EditorSceneManager.SaveScene(roomScene);
                Debug.Log($"[TutorialSceneSynchronizer] Room scene XR Origin (XR Rig) repositioned to cabin door entrance: {roomEntPos}");
            }

            // Create or ensure Anchor_Entrance in Tutorial scene at door porch
            GameObject tutAnchor = null;
            foreach (var rootGo in tutorialScene.GetRootGameObjects())
            {
                if (rootGo.name == "Anchor_Entrance")
                {
                    tutAnchor = rootGo;
                    break;
                }
            }
            if (tutAnchor == null && roomAnchorEntrance != null)
            {
                tutAnchor = Object.Instantiate(roomAnchorEntrance);
                tutAnchor.name = "Anchor_Entrance";
                SceneManager.MoveGameObjectToScene(tutAnchor, tutorialScene);
                tutAnchor.transform.SetParent(null);
                tutAnchor.transform.position = new Vector3(22.8f, 1.25f, 2.3f);
                tutAnchor.transform.rotation = Quaternion.Euler(0f, 145f, 0f);
                EditorUtility.SetDirty(tutAnchor);
                Debug.Log("[TutorialSceneSynchronizer] Anchor_Entrance created at Tutorial cabin door porch (22.8, 1.25, 2.3).");
            }

            // Find tools under Workbench or in Room scene
            var allRoomGrabs = Object.FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var grab in allRoomGrabs)
            {
                if (grab.gameObject.scene == roomScene)
                {
                    if (grab.gameObject.name == "tool-shovel" || grab.gameObject.name.Contains("shovel"))
                        roomShovel = grab.gameObject;
                    else if (grab.gameObject.name == "Gold_Pan" || grab.gameObject.name.Contains("Gold_Pan"))
                        roomPan = grab.gameObject;
                }
            }

            // Find door slab in Room scene
            var allRoomTransitions = Object.FindObjectsByType<DoorSceneTransition>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var dst in allRoomTransitions)
            {
                if (dst.gameObject.scene == roomScene)
                {
                    roomDoorSlab = dst.gameObject;
                    break;
                }
            }

            if (roomInventory == null) Debug.LogError("[TutorialSceneSynchronizer] roomInventory not found in Room scene!");
            if (roomShovel == null) Debug.LogError("[TutorialSceneSynchronizer] roomShovel not found in Room scene!");
            if (roomPan == null) Debug.LogError("[TutorialSceneSynchronizer] roomPan not found in Room scene!");
            if (roomDoorSlab == null) Debug.LogError("[TutorialSceneSynchronizer] roomDoorSlab not found in Room scene!");
            if (roomPrompt == null) Debug.LogError("[TutorialSceneSynchronizer] roomPrompt not found in Room scene!");

            // 4. Port [Prospector_Inventory] and Pre-stored Tools into Tutorial scene
            GameObject existingTutInv = null;
            foreach (var rootGo in tutorialScene.GetRootGameObjects())
            {
                if (rootGo.name == "[Prospector_Inventory]")
                {
                    existingTutInv = rootGo;
                    break;
                }
            }
            if (existingTutInv != null)
            {
                Object.DestroyImmediate(existingTutInv);
            }

            GameObject tutInventory = Object.Instantiate(roomInventory);
            tutInventory.name = "[Prospector_Inventory]";
            SceneManager.MoveGameObjectToScene(tutInventory, tutorialScene);
            tutInventory.transform.position = Vector3.zero;
            tutInventory.transform.rotation = Quaternion.identity;
            tutInventory.transform.localScale = Vector3.one;

            Transform tutContainer = tutInventory.transform.Find("Stored_Items_Container");
            if (tutContainer == null) tutContainer = tutInventory.transform.Find("[Stored_Items_Container]");
            if (tutContainer == null)
            {
                GameObject cg = new GameObject("Stored_Items_Container");
                cg.transform.SetParent(tutInventory.transform);
                tutContainer = cg.transform;
            }
            tutContainer.name = "Stored_Items_Container";
            tutContainer.gameObject.SetActive(false);

            // Clear any old items in container
            for (int i = tutContainer.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(tutContainer.GetChild(i).gameObject);
            }

            // Clone shovel and gold pan into Stored_Items_Container
            if (roomShovel != null)
            {
                GameObject shovelClone = Object.Instantiate(roomShovel, tutContainer);
                shovelClone.name = "tool-shovel";
                shovelClone.SetActive(false);
                shovelClone.transform.localPosition = Vector3.zero;
                shovelClone.transform.localRotation = Quaternion.identity;
                Debug.Log("[TutorialSceneSynchronizer] tool-shovel pre-stored in Stored_Items_Container.");
            }

            if (roomPan != null)
            {
                GameObject panClone = Object.Instantiate(roomPan, tutContainer);
                panClone.name = "Gold_Pan";
                panClone.SetActive(false);
                panClone.transform.localPosition = Vector3.zero;
                panClone.transform.localRotation = Quaternion.identity;
                panClone.transform.localScale = new Vector3(0.82f, 0.82f, 0.82f);
                Debug.Log("[TutorialSceneSynchronizer] Gold_Pan pre-stored in Stored_Items_Container.");
            }

            // Ensure ProspectorInventory component references on tutInventory
            ProspectorInventory pi = tutInventory.GetComponent<ProspectorInventory>();
            if (pi != null)
            {
                SerializedObject piSo = new SerializedObject(pi);
                piSo.FindProperty("storedItemsContainer").objectReferenceValue = tutContainer;
                Transform tutCanvas = tutInventory.transform.Find("InventoryCanvas");
                if (tutCanvas != null)
                {
                    piSo.FindProperty("inventoryPanel").objectReferenceValue = tutCanvas.gameObject;
                    tutCanvas.gameObject.SetActive(false);
                }
                Transform tutToast = tutInventory.transform.Find("ToastNotification");
                if (tutToast != null)
                {
                    piSo.FindProperty("toastPanel").objectReferenceValue = tutToast.gameObject;
                    var txt = tutToast.GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null)
                        piSo.FindProperty("toastText").objectReferenceValue = txt;
                    tutToast.gameObject.SetActive(false);
                }
                piSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(pi);
                Debug.Log("[TutorialSceneSynchronizer] ProspectorInventory wired cleanly.");
            }

            // 5. Port Cabin Door & Interaction Prompt (Preserving Almer's door position at House_2)
            GameObject existingDoor = null;
            GameObject existingPrompt = null;
            foreach (var rootGo in tutorialScene.GetRootGameObjects())
            {
                if (rootGo.name == "Cabin_Door_Slab") existingDoor = rootGo;
                if (rootGo.name == "Door_Interaction_Prompt") existingPrompt = rootGo;
            }
            if (existingPrompt != null) Object.DestroyImmediate(existingPrompt);

            GameObject tutDoor = existingDoor;
            if (tutDoor == null)
            {
                tutDoor = Object.Instantiate(roomDoorSlab);
                tutDoor.name = "Cabin_Door_Slab";
                SceneManager.MoveGameObjectToScene(tutDoor, tutorialScene);
                tutDoor.transform.SetParent(null);
                tutDoor.transform.position = new Vector3(23.94f, 2.36f, 1.89f);
                tutDoor.transform.rotation = Quaternion.Euler(0f, -22.524f, 0f);
                tutDoor.transform.localScale = new Vector3(1.2f, 2.24f, 0.08f);
            }

            // Solid collider on door
            BoxCollider doorCol = tutDoor.GetComponent<BoxCollider>();
            if (doorCol != null)
            {
                doorCol.isTrigger = false;
                EditorUtility.SetDirty(doorCol);
            }

            // Clone Door_Interaction_Prompt and place directly at Cabin_Door_Slab at eye height
            GameObject tutPrompt = Object.Instantiate(roomPrompt);
            tutPrompt.name = "Door_Interaction_Prompt";
            SceneManager.MoveGameObjectToScene(tutPrompt, tutorialScene);
            // Position 0.35m outward in front of door slab towards player approach
            Vector3 promptPos = new Vector3(23.80f, 2.20f, 2.21f);
            tutPrompt.transform.position = promptPos;
            RectTransform tutPromptRt = tutPrompt.GetComponent<RectTransform>();
            if (tutPromptRt != null)
            {
                tutPromptRt.anchoredPosition = new Vector2(promptPos.x, promptPos.y);
                tutPromptRt.localPosition = new Vector3(0f, 0f, promptPos.z);
                tutPromptRt.position = promptPos;
            }
            tutPrompt.transform.rotation = Quaternion.Euler(0f, 157.476f, 0f);
            tutPrompt.transform.localScale = new Vector3(0.0018f, 0.0018f, 0.0018f);
            tutPrompt.SetActive(false);

            Canvas promptCanvas = tutPrompt.GetComponent<Canvas>();
            if (promptCanvas != null)
            {
                Camera cam = xrOrigin.GetComponentInChildren<Camera>(true);
                promptCanvas.worldCamera = cam;
                EditorUtility.SetDirty(promptCanvas);
            }

            // Wire DoorSceneTransition
            DoorSceneTransition dstComp = tutDoor.GetComponent<DoorSceneTransition>();
            if (dstComp == null) dstComp = tutDoor.AddComponent<DoorSceneTransition>();
            if (dstComp != null)
            {
                SerializedObject dstSo = new SerializedObject(dstComp);
                dstSo.FindProperty("targetSceneName").stringValue = "Room scene";
                dstSo.FindProperty("interactionDistance").floatValue = 2.8f;
                dstSo.FindProperty("doorCenter").objectReferenceValue = tutDoor.transform;
                dstSo.FindProperty("promptPanel").objectReferenceValue = tutPrompt;
                dstSo.FindProperty("promptCanvasGroup").objectReferenceValue = tutPrompt.GetComponent<CanvasGroup>();

                TextMeshProUGUI promptTmp = null;
                Transform pTextTransform = tutPrompt.transform.Find("PromptText");
                if (pTextTransform != null) promptTmp = pTextTransform.GetComponent<TextMeshProUGUI>();
                if (promptTmp == null) promptTmp = tutPrompt.GetComponentInChildren<TextMeshProUGUI>(true);
                if (promptTmp != null)
                {
                    promptTmp.text = "<b><size=28>[E]</size> <color=#FFFFFF>Masuk Rumah</color></b>\n\n<size=18><color=#7CFC00>[OK] Sekop & Gold Pan Siap (VR: [A] / Trigger)</color></size>";
                    EditorUtility.SetDirty(promptTmp);
                }
                dstSo.FindProperty("promptText").objectReferenceValue = promptTmp;

                // SFX clips from roomDoorSlab
                if (roomDoorSlab != null)
                {
                    var roomDst = roomDoorSlab.GetComponent<DoorSceneTransition>();
                    if (roomDst != null)
                    {
                        var roomDstSo = new SerializedObject(roomDst);
                        dstSo.FindProperty("doorOpenClip").objectReferenceValue = roomDstSo.FindProperty("doorOpenClip").objectReferenceValue;
                        dstSo.FindProperty("doorCreakClip").objectReferenceValue = roomDstSo.FindProperty("doorCreakClip").objectReferenceValue;
                    }
                }

                dstSo.FindProperty("fadeDuration").floatValue = 0.75f;
                dstSo.FindProperty("fadeColor").colorValue = Color.black;
                dstSo.ApplyModifiedProperties();
                EditorUtility.SetDirty(dstComp);
                Debug.Log($"[TutorialSceneSynchronizer] Cabin_Door_Slab configured with targetSceneName = 'Room scene' and UI prompt placed at {tutPrompt.transform.position}.");
            }

            // 6. Silence Pond Ambient Audio as requested by user
            var pondRoot = GameObject.Find("[Tutorial_Pond]");
            if (pondRoot != null)
            {
                var allAudio = pondRoot.GetComponentsInChildren<AudioSource>(true);
                foreach (var a in allAudio)
                {
                    a.volume = 0f;
                    a.mute = true;
                    a.playOnAwake = false;
                    a.enabled = false;
                    EditorUtility.SetDirty(a);
                }
                var pondAudioChild = pondRoot.transform.Find("Pond_Audio");
                if (pondAudioChild != null)
                {
                    pondAudioChild.gameObject.SetActive(false);
                    EditorUtility.SetDirty(pondAudioChild.gameObject);
                }
                Debug.Log("[TutorialSceneSynchronizer] Pond ambient audio silenced cleanly (AudioSources disabled & volume set to 0).");
            }

            // 7. Close Room scene without saving
            EditorSceneManager.CloseScene(roomScene, true);

            // 8. Save Tutorial scene cleanly
            EditorSceneManager.MarkSceneDirty(tutorialScene);
            bool saved = EditorSceneManager.SaveScene(tutorialScene);
            Debug.Log($"[TutorialSceneSynchronizer] Tutorial scene saved cleanly: {saved}");

            // Run Verification
            VerifyTutorialScene();
        }

        [MenuItem("ProspectorJourney/Verify Tutorial Scene M2")]
        public static bool VerifyTutorialScene()
        {
            Debug.Log("==================== [TUTORIAL SCENE M2 VERIFICATION] ====================");
            bool allPassed = true;

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TUTORIAL_SCENE_PATH)
            {
                activeScene = EditorSceneManager.OpenScene(TUTORIAL_SCENE_PATH, OpenSceneMode.Single);
            }

            // 1. XR Origin checks
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin == null)
            {
                Debug.LogError("[FAIL] XR Origin (XR Rig) not found!");
                allPassed = false;
            }
            else
            {
                var cc = xrOrigin.GetComponent<CharacterController>();
                if (cc == null)
                {
                    Debug.LogError("[FAIL] CharacterController not found on XR Origin!");
                    allPassed = false;
                }
                else
                {
                    bool ccRadiusOk = Mathf.Approximately(cc.radius, 0.24f);
                    bool ccStepOk = Mathf.Approximately(cc.stepOffset, 0.08f);
                    bool ccSkinOk = Mathf.Approximately(cc.skinWidth, 0.025f);
                    bool ccMinMoveOk = Mathf.Approximately(cc.minMoveDistance, 0.0001f);
                    Debug.Log($"[CHECK] CharacterController: radius={cc.radius} (ok={ccRadiusOk}), stepOffset={cc.stepOffset} (ok={ccStepOk}), skinWidth={cc.skinWidth} (ok={ccSkinOk}), minMove={cc.minMoveDistance} (ok={ccMinMoveOk})");
                    if (!ccRadiusOk || !ccStepOk || !ccSkinOk || !ccMinMoveOk) allPassed = false;
                }

                var pbc = xrOrigin.GetComponent<PlayerBodyCollision>();
                if (pbc == null)
                {
                    Debug.LogError("[FAIL] PlayerBodyCollision not found on XR Origin!");
                    allPassed = false;
                }
                else
                {
                    SerializedObject pbcSo = new SerializedObject(pbc);
                    float r = pbcSo.FindProperty("bodyRadius").floatValue;
                    float s = pbcSo.FindProperty("stepOffset").floatValue;
                    float sk = pbcSo.FindProperty("skinWidth").floatValue;
                    float minH = pbcSo.FindProperty("minHeadHeight").floatValue;
                    float maxH = pbcSo.FindProperty("maxHeadHeight").floatValue;
                    bool pbcOk = Mathf.Approximately(r, 0.24f) && Mathf.Approximately(s, 0.08f) && Mathf.Approximately(sk, 0.025f) && Mathf.Approximately(minH, 0.85f) && Mathf.Approximately(maxH, 2.05f);
                    Debug.Log($"[CHECK] PlayerBodyCollision: radius={r}, step={s}, skin={sk}, minH={minH}, maxH={maxH} (all ok={pbcOk})");
                    if (!pbcOk) allPassed = false;
                }

                var doorSlabCheck = GameObject.Find("Cabin_Door_Slab");
                float distToDoor = doorSlabCheck != null ? Vector3.Distance(xrOrigin.transform.position, doorSlabCheck.transform.position) : 999f;
                bool posAtDoorOk = distToDoor < 2.5f;
                Debug.Log($"[CHECK] XR Origin positioned at Cabin Door: pos={xrOrigin.transform.position}, distToDoor={distToDoor:F2}m (ok={posAtDoorOk})");
                if (!posAtDoorOk) allPassed = false;
            }

            // 2. Inventory and Pre-Stored Tools checks
            GameObject invGo = GameObject.Find("[Prospector_Inventory]");
            if (invGo == null)
            {
                Debug.LogError("[FAIL] [Prospector_Inventory] not found in scene!");
                allPassed = false;
            }
            else
            {
                var pi = invGo.GetComponent<ProspectorInventory>();
                var audio = invGo.GetComponent<AudioSource>();
                if (pi == null || audio == null)
                {
                    Debug.LogError("[FAIL] ProspectorInventory or AudioSource missing on [Prospector_Inventory]!");
                    allPassed = false;
                }

                Transform container = invGo.transform.Find("Stored_Items_Container");
                if (container == null) container = invGo.transform.Find("[Stored_Items_Container]");
                if (container == null)
                {
                    Debug.LogError("[FAIL] Stored_Items_Container not found under [Prospector_Inventory]!");
                    allPassed = false;
                }
                else
                {
                    bool containerInactive = !container.gameObject.activeSelf;
                    Transform shovel = container.Find("tool-shovel");
                    Transform pan = container.Find("Gold_Pan");

                    bool shovelOk = shovel != null && !shovel.gameObject.activeSelf && shovel.GetComponent<Rigidbody>() != null && shovel.GetComponent<XRGrabInteractable>() != null && shovel.GetComponent<XRRestingProp>() != null && shovel.Find("attach1") != null && shovel.Find("attach2") != null;
                    bool panOk = pan != null && !pan.gameObject.activeSelf && pan.GetComponent<Rigidbody>() != null && pan.GetComponent<XRGrabInteractable>() != null && pan.GetComponent<XRRestingProp>() != null && pan.Find("attach1") != null && pan.Find("attach2") != null;

                    Debug.Log($"[CHECK] Stored_Items_Container inactive: {containerInactive}, tool-shovel valid: {shovelOk}, Gold_Pan valid: {panOk}");
                    if (!containerInactive || !shovelOk || !panOk) allPassed = false;
                }

                Transform invCanvas = invGo.transform.Find("InventoryCanvas");
                Transform toast = invGo.transform.Find("ToastNotification");
                bool canvasOk = invCanvas != null && invCanvas.GetComponent<Canvas>() != null && invCanvas.GetComponent<CanvasGroup>() != null;
                bool toastOk = toast != null && toast.GetComponent<Canvas>() != null;
                Debug.Log($"[CHECK] InventoryCanvas valid: {canvasOk}, ToastNotification valid: {toastOk}");
                if (!canvasOk || !toastOk) allPassed = false;
            }

            // 3. Cabin Door & Prompt checks
            GameObject doorGo = null;
            GameObject promptGo = null;
            foreach (var rootGo in activeScene.GetRootGameObjects())
            {
                if (rootGo.name == "Cabin_Door_Slab") doorGo = rootGo;
                if (rootGo.name == "Door_Interaction_Prompt") promptGo = rootGo;
            }
            if (doorGo == null)
            {
                Debug.LogError("[FAIL] Cabin_Door_Slab not found in scene!");
                allPassed = false;
            }
            else
            {
                var col = doorGo.GetComponent<BoxCollider>();
                var simple = doorGo.GetComponent<XRSimpleInteractable>();
                var dst = doorGo.GetComponent<DoorSceneTransition>();

                bool colOk = col != null && !col.isTrigger;
                bool simpleOk = simple != null;
                bool dstOk = false;

                if (dst != null)
                {
                    SerializedObject dstSo = new SerializedObject(dst);
                    string target = dstSo.FindProperty("targetSceneName").stringValue;
                    float dist = dstSo.FindProperty("interactionDistance").floatValue;
                    var center = dstSo.FindProperty("doorCenter").objectReferenceValue;
                    var pPanel = dstSo.FindProperty("promptPanel").objectReferenceValue;
                    var openSfx = dstSo.FindProperty("doorOpenClip").objectReferenceValue;
                    var creakSfx = dstSo.FindProperty("doorCreakClip").objectReferenceValue;

                    dstOk = target == "Room scene" && dist >= 2.0f && center != null && pPanel != null && openSfx != null && creakSfx != null;
                    Debug.Log($"[CHECK] DoorSceneTransition: target='{target}', dist={dist}, center={center != null}, pPanel={pPanel != null}, SFX={openSfx != null && creakSfx != null} (ok={dstOk})");
                }

                Debug.Log($"[CHECK] Cabin_Door_Slab: pos={doorGo.transform.position}, solidCol={colOk}, simpleInteractable={simpleOk}, transition={dstOk}");
                if (!colOk || !simpleOk || !dstOk) allPassed = false;
            }

            if (promptGo == null)
            {
                Debug.LogError("[FAIL] Door_Interaction_Prompt not found in scene!");
                allPassed = false;
            }
            else
            {
                var canvas = promptGo.GetComponent<Canvas>();
                var cg = promptGo.GetComponent<CanvasGroup>();
                var tmp = promptGo.GetComponentInChildren<TextMeshProUGUI>(true);
                bool promptOk = canvas != null && cg != null && tmp != null;
                float distToDoor = doorGo != null ? Vector3.Distance(doorGo.transform.position, promptGo.transform.position) : 999f;
                bool posOk = distToDoor < 1.5f; // Must be placed at the door, not 14m away
                Debug.Log($"[CHECK] Door_Interaction_Prompt: pos={promptGo.transform.position}, distToDoor={distToDoor:F2}m (ok={posOk}), canvas={canvas != null}, cg={cg != null}, tmp={tmp != null} (ok={promptOk})");
                if (!promptOk || !posOk) allPassed = false;
            }

            // 4. Pond Audio Silence Check
            var pondGo = GameObject.Find("[Tutorial_Pond]");
            if (pondGo != null)
            {
                var pondAudios = pondGo.GetComponentsInChildren<AudioSource>(true);
                bool anyAudible = false;
                foreach (var a in pondAudios)
                {
                    if (a.enabled && a.volume > 0.001f && !a.mute) anyAudible = true;
                }
                var pondAudioGo = pondGo.transform.Find("Pond_Audio");
                bool audioChildActive = pondAudioGo != null && pondAudioGo.gameObject.activeSelf;
                bool pondSilent = !anyAudible && !audioChildActive;
                Debug.Log($"[CHECK] Pond ambient audio silent: {pondSilent} (anyAudible={anyAudible}, audioChildActive={audioChildActive})");
                if (!pondSilent) allPassed = false;
            }

            Debug.Log($"==================== [VERIFICATION RESULT: {(allPassed ? "PASSED ALL" : "FAILED")}] ====================");
            return allPassed;
        }

        [InitializeOnLoadMethod]
        private static void RegisterTransitionAuditHook()
        {
            EditorApplication.playModeStateChanged -= OnTransitionPlayModeChanged;
            EditorApplication.playModeStateChanged += OnTransitionPlayModeChanged;
        }

        private static void OnTransitionPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Clean up any test runner in scene
                var stray = GameObject.Find("[DoorTransitionPlayModeVerifier]");
                if (stray != null)
                {
                    Object.DestroyImmediate(stray);
                    var activeScene = SceneManager.GetActiveScene();
                    if (activeScene.isDirty) EditorSceneManager.SaveScene(activeScene);
                }

                if (EditorPrefs.GetBool(DoorTransitionPlayModeVerifier.VERIFY_PREF_KEY, false))
                {
                    EditorPrefs.DeleteKey(DoorTransitionPlayModeVerifier.VERIFY_PREF_KEY);
                    bool passed = EditorPrefs.GetBool(DoorTransitionPlayModeVerifier.VERIFY_RESULT_KEY, false);
                    string report = EditorPrefs.GetString(DoorTransitionPlayModeVerifier.VERIFY_REPORT_KEY, "");
                    if (passed)
                    {
                        Debug.Log(">>> [BIDIRECTIONAL TRANSITION AUDIT: 100% PASSED] <<<\n" + report);
                    }
                    else
                    {
                        Debug.LogWarning(">>> [BIDIRECTIONAL TRANSITION AUDIT FAILED] <<<\n" + report);
                    }
                }
            }
        }

        [MenuItem("ProspectorJourney/Run Bidirectional Door Transition Audit")]
        public static void RunBidirectionalDoorTransitionAudit()
        {
            Debug.Log("[TutorialSceneSynchronizer] Initiating Bidirectional Door Transition Audit...");

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TUTORIAL_SCENE_PATH)
            {
                Debug.Log($"[TutorialSceneSynchronizer] Opening {TUTORIAL_SCENE_PATH}...");
                activeScene = EditorSceneManager.OpenScene(TUTORIAL_SCENE_PATH, OpenSceneMode.Single);
            }

            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                Debug.LogError($"[TutorialSceneSynchronizer] Failed to load {TUTORIAL_SCENE_PATH}");
                return;
            }

            EditorPrefs.SetBool(DoorTransitionPlayModeVerifier.VERIFY_PREF_KEY, true);
            EditorPrefs.SetBool(DoorTransitionPlayModeVerifier.VERIFY_RESULT_KEY, false);
            EditorPrefs.DeleteKey(DoorTransitionPlayModeVerifier.VERIFY_REPORT_KEY);

            // Verifier will automatically instantiate on AfterSceneLoad via [RuntimeInitializeOnLoadMethod]
            Debug.Log("[TutorialSceneSynchronizer] Entering Play Mode to execute DoorTransitionPlayModeVerifier...");
            EditorApplication.isPlaying = true;
        }
    }
}
