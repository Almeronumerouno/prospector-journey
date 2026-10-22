using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Object = UnityEngine.Object;

namespace ProspectorJourney.Room
{
    public class DoorSceneTransition : MonoBehaviour
    {
        [Header("Target Scene Configuration")]
        [Tooltip("Nama scene tujuan saat pintu dibuka. Bisa diganti sewaktu-waktu di Inspector.")]
        [SerializeField] private string targetSceneName = "Village";

        [Header("Proximity Detection")]
        [Tooltip("Jarak maksimal pemain dari pintu agar interaksi aktif (meter).")]
        [SerializeField] private float interactionDistance = 2.8f;
        [Tooltip("Transform pintu atau pusat interaksi. Default: transform ini.")]
        [SerializeField] private Transform doorCenter;

        [Header("UI Interaction Prompt")]
        [SerializeField] private GameObject promptPanel;
        [SerializeField] private CanvasGroup promptCanvasGroup;
        [SerializeField] private TextMeshProUGUI promptText;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorOpenClip;
        [SerializeField] private AudioClip doorCreakClip;

        [Header("VR Screen Fade")]
        [SerializeField] private float fadeDuration = 0.75f;
        [SerializeField] private Color fadeColor = Color.black;

        private Camera m_MainCamera;
        private bool m_IsPlayerNearby = false;
        private bool m_IsTransitioning = false;
        private XRSimpleInteractable m_Interactable;

        // Controller input debounce
        private bool m_PrevRightPrimary = false;
        private bool m_PrevLeftPrimary = false;
        private bool m_PrevRightTrigger = false;
        private bool m_PrevLeftTrigger = false;

        public string TargetSceneName
        {
            get => targetSceneName;
            set => targetSceneName = value;
        }

        public bool IsPlayerNearby => m_IsPlayerNearby;

        private void Awake()
        {
            // Eradicate any stray faders from prior scene sessions
            CleanupStrayFaders();

            if (doorCenter == null) doorCenter = transform;
            m_MainCamera = Camera.main;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.spatialBlend = 1.0f; // 3D sound at door position
            audioSource.playOnAwake = false;

            // Only listen to explicit 'activated' (trigger pull), NEVER selectEntered (poke/touch)
            m_Interactable = GetComponent<XRSimpleInteractable>();
            if (m_Interactable != null)
            {
                m_Interactable.activated.AddListener(_ =>
                {
                    if (m_IsPlayerNearby) OnDoorInteracted();
                });
            }

            EnsurePromptPlacement();

            // Ensure prompt starts hidden
            if (promptCanvasGroup != null)
            {
                promptCanvasGroup.alpha = 0f;
            }
            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (m_MainCamera == null) m_MainCamera = Camera.main;
            EnsurePromptPlacement();
            EnsureCanvasCamera();
            UpdatePromptLabel();

            // Check immediate proximity on start (e.g. if spawned directly in front of door)
            if (m_MainCamera != null)
            {
                Vector3 doorPos = doorCenter != null ? doorCenter.position : transform.position;
                Vector3 playerPos = m_MainCamera.transform.position;
                float horizDist = Vector2.Distance(new Vector2(doorPos.x, doorPos.z), new Vector2(playerPos.x, playerPos.z));
                float vertDist = Mathf.Abs(doorPos.y - playerPos.y);
                float effectiveDist = Mathf.Max(interactionDistance, 2.5f);
                if (horizDist <= effectiveDist && vertDist <= 3.2f)
                {
                    m_IsPlayerNearby = true;
                    OnProximityChanged(true);
                }
            }
        }

        private void Update()
        {
            if (m_IsTransitioning) return;
            if (ProspectorJourney.UI.PauseMenuController.IsGamePaused) return;

            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera == null) return;
            }

            // 1. Check distance to camera (player's head) using horizontal distance with vertical tolerance
            Vector3 doorPos = doorCenter != null ? doorCenter.position : transform.position;
            Vector3 playerPos = m_MainCamera.transform.position;
            float horizDist = Vector2.Distance(new Vector2(doorPos.x, doorPos.z), new Vector2(playerPos.x, playerPos.z));
            float vertDist = Mathf.Abs(doorPos.y - playerPos.y);

            float effectiveDist = Mathf.Max(interactionDistance, 2.5f);
            bool nearby = (horizDist <= effectiveDist) && (vertDist <= 3.2f);
            if (nearby != m_IsPlayerNearby)
            {
                m_IsPlayerNearby = nearby;
                OnProximityChanged(nearby);
            }

            // 2. Handle input while player is nearby
            if (m_IsPlayerNearby)
            {
                FacePromptToPlayer();
                UpdatePromptLabel();
                CheckInteractionInput();
            }
        }

        public void EnsurePromptPlacement()
        {
            if (promptPanel == null) return;

            Vector3 doorPos = doorCenter != null ? doorCenter.position : transform.position;
            float dist = Vector3.Distance(promptPanel.transform.position, doorPos);

            // If prompt is placed more than 2.0m away from the door, auto-correct it right in front of the door
            if (dist > 2.0f)
            {
                Debug.LogWarning($"[DoorSceneTransition] promptPanel was placed {dist:F1}m away from door '{gameObject.name}'. Auto-correcting to door front!");
                Vector3 frontOffset = transform.forward * 0.35f;
                Vector3 targetPos = doorPos + frontOffset;
                targetPos.y = doorPos.y; // Align with door center height

                RectTransform rt = promptPanel.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(targetPos.x, targetPos.y);
                    rt.position = targetPos;
                }
                else
                {
                    promptPanel.transform.position = targetPos;
                }
                promptPanel.transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
            }
        }

        private void EnsureCanvasCamera()
        {
            if (promptPanel != null)
            {
                var canvas = promptPanel.GetComponent<Canvas>();
                if (canvas != null && canvas.worldCamera == null)
                {
                    if (m_MainCamera == null) m_MainCamera = Camera.main;
                    canvas.worldCamera = m_MainCamera;
                }
            }
        }

        public bool HasRequiredToolsForExit(out bool hasShovel, out bool hasPan)
        {
            hasShovel = false;
            hasPan = false;

            if (ProspectorInventory.Instance != null)
            {
                hasShovel = ProspectorInventory.Instance.HasItemInBagOrHand("shovel");
                hasPan = ProspectorInventory.Instance.HasItemInBagOrHand("Gold_Pan");
            }

            if (!hasShovel || !hasPan)
            {
                var grabs = FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                foreach (var grab in grabs)
                {
                    if (grab != null && grab.isSelected)
                    {
                        string n = grab.gameObject.name;
                        if (n.IndexOf("shovel", System.StringComparison.OrdinalIgnoreCase) >= 0)
                            hasShovel = true;
                        if (n.IndexOf("Gold_Pan", System.StringComparison.OrdinalIgnoreCase) >= 0)
                            hasPan = true;
                    }
                }
            }

            return hasShovel && hasPan;
        }

        private void FacePromptToPlayer()
        {
            if (promptPanel == null || m_MainCamera == null) return;
            Vector3 dirToCam = m_MainCamera.transform.position - promptPanel.transform.position;
            dirToCam.y = 0f;
            if (dirToCam.sqrMagnitude > 0.001f)
            {
                // In Unity UI Canvas, the front face looks along -forward.
                // Setting rotation to LookRotation(-dirToCam) directs the front face towards player.
                promptPanel.transform.rotation = Quaternion.LookRotation(-dirToCam.normalized, Vector3.up);
            }
        }

        private void OnProximityChanged(bool isNearby)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(isNearby);
                if (isNearby)
                {
                    EnsureCanvasCamera();
                    UpdatePromptLabel();
                    FacePromptToPlayer();
                }
                if (promptCanvasGroup != null)
                {
                    StopAllCoroutines();
                    StartCoroutine(FadeCanvasGroup(promptCanvasGroup, isNearby ? 1f : 0f, 0.15f));
                }
            }
        }

        private void CheckInteractionInput()
        {
            // Keyboard / PC Simulator: ONLY triggers when 'E' is pressed
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.eKey.wasPressedThisFrame)
                {
                    OnDoorInteracted();
                    return;
                }
            }

            // VR Controller Input: Primary button ('A' on right, 'X' on left) or Trigger click
            var rightDev = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            if (rightDev.isValid)
            {
                if (rightDev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool rightPri))
                {
                    if (rightPri && !m_PrevRightPrimary)
                    {
                        OnDoorInteracted();
                        m_PrevRightPrimary = rightPri;
                        return;
                    }
                    m_PrevRightPrimary = rightPri;
                }

                if (rightDev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool rightTrig))
                {
                    if (rightTrig && !m_PrevRightTrigger)
                    {
                        OnDoorInteracted();
                        m_PrevRightTrigger = rightTrig;
                        return;
                    }
                    m_PrevRightTrigger = rightTrig;
                }
            }

            var leftDev = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
            if (leftDev.isValid)
            {
                if (leftDev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool leftPri))
                {
                    if (leftPri && !m_PrevLeftPrimary)
                    {
                        OnDoorInteracted();
                        m_PrevLeftPrimary = leftPri;
                        return;
                    }
                    m_PrevLeftPrimary = leftPri;
                }

                if (leftDev.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool leftTrig))
                {
                    if (leftTrig && !m_PrevLeftTrigger)
                    {
                        OnDoorInteracted();
                        m_PrevLeftTrigger = leftTrig;
                        return;
                    }
                    m_PrevLeftTrigger = leftTrig;
                }
            }
        }

        public void OnDoorInteracted()
        {
            if (m_IsTransitioning) return;

            bool isEntering = !string.IsNullOrEmpty(targetSceneName) && targetSceneName.IndexOf("Room", System.StringComparison.OrdinalIgnoreCase) >= 0;
            string actionVerb = isEntering ? "masuk" : "keluar";

            if (!HasRequiredToolsForExit(out bool hasShovel, out bool hasPan))
            {
                UpdatePromptLabel();
                string missingMsg;
                if (!hasShovel && !hasPan)
                    missingMsg = $"Belum bisa {actionVerb}! Bawa Sekop & Gold Pan dulu.";
                else if (!hasShovel)
                    missingMsg = $"Belum bisa {actionVerb}! Bawa Sekop (Shovel) dulu.";
                else
                    missingMsg = $"Belum bisa {actionVerb}! Bawa Gold Pan dulu.";

                if (ProspectorInventory.Instance != null)
                {
                    ProspectorInventory.Instance.PlayErrorSound();
                    ProspectorInventory.Instance.ShowToast(missingMsg);
                }

                Debug.Log($"[DoorSceneTransition] Transisi ditolak: hasShovel={hasShovel}, hasPan={hasPan}");
                return;
            }

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("[DoorSceneTransition] Target scene name belum diset!");
                return;
            }

            StartCoroutine(ExecuteTransitionSequence());
        }

        public void TriggerTransitionDirect()
        {
            if (m_IsTransitioning) return;
            StartCoroutine(ExecuteTransitionSequence());
        }

        private IEnumerator ExecuteTransitionSequence()
        {
            m_IsTransitioning = true;

            // 1. Audio feedback (pintu berderit / dibuka)
            if (doorCreakClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(doorCreakClip);
            }
            else if (doorOpenClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(doorOpenClip);
            }

            // 2. Hide prompt
            if (promptPanel != null) promptPanel.SetActive(false);

            // 3. VR Screen Fade to Black
            yield return StartCoroutine(FadeToBlack(fadeDuration));

            // 4. Asynchronously load target scene
            Debug.Log($"[DoorSceneTransition] Berpindah ke scene: '{targetSceneName}'...");
            var asyncOp = SceneManager.LoadSceneAsync(targetSceneName);
            if (asyncOp == null)
            {
                Debug.LogError($"[DoorSceneTransition] Gagal memuat scene '{targetSceneName}'! Pastikan scene sudah ada di Build Settings.");
            }
        }

        private IEnumerator FadeToBlack(float duration)
        {
            if (m_MainCamera == null) m_MainCamera = Camera.main;
            if (m_MainCamera == null) yield break;

            CleanupStrayFaders();

            // Create persistent camera overlay canvas for clean VR fade
            GameObject fadeCanvasGo = new GameObject("[VR_Screen_Fader]");
            fadeCanvasGo.transform.position = m_MainCamera.transform.position + m_MainCamera.transform.forward * 0.2f;
            fadeCanvasGo.transform.rotation = m_MainCamera.transform.rotation;
            DontDestroyOnLoad(fadeCanvasGo);

            var canvas = fadeCanvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = m_MainCamera;
            canvas.sortingOrder = 32767;

            var rectTransform = fadeCanvasGo.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(4f, 4f);

            var canvasGroup = fadeCanvasGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            // CRITICAL: NEVER block raycasts or intercept mouse clicks in simulator
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            var imgGo = new GameObject("BlackOverlay");
            imgGo.transform.SetParent(fadeCanvasGo.transform, false);
            var imgRt = imgGo.AddComponent<RectTransform>();
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.sizeDelta = Vector2.zero;

            var img = imgGo.AddComponent<Image>();
            img.color = fadeColor;
            // CRITICAL: NEVER block mouse raycasts or clicks
            img.raycastTarget = false;

            // Attach lifecycle helper that cleanly fades back in and destroys itself in target scene
            var helper = fadeCanvasGo.AddComponent<VRScreenFaderHelper>();
            helper.fadeDuration = duration;

            // Fade alpha from 0 to 1
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                if (m_MainCamera != null)
                {
                    fadeCanvasGo.transform.position = m_MainCamera.transform.position + m_MainCamera.transform.forward * 0.2f;
                    fadeCanvasGo.transform.rotation = m_MainCamera.transform.rotation;
                }
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
        {
            float startAlpha = cg.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }
            cg.alpha = targetAlpha;
        }

        public void UpdatePromptLabel()
        {
            if (promptText == null) return;

            bool isEntering = !string.IsNullOrEmpty(targetSceneName) && targetSceneName.IndexOf("Room", System.StringComparison.OrdinalIgnoreCase) >= 0;
            string actionText = isEntering ? "Masuk Rumah" : "Keluar Rumah";
            string blockedTitle = isEntering ? "Belum Bisa Masuk!" : "Belum Bisa Keluar!";

            if (HasRequiredToolsForExit(out bool hasShovel, out bool hasPan))
            {
                promptText.text = $"<b><size=26>[E]</size> <color=#FFFFFF>{actionText}</color></b>\n<size=17><color=#7CFC00>[OK] Sekop & Gold Pan Siap (VR: [A] / Trigger)</color></size>";
            }
            else
            {
                string shovelStr = hasShovel ? "<color=#7CFC00>[OK] Sekop</color>" : "<color=#FF6B6B>[X] Sekop</color>";
                string panStr = hasPan ? "<color=#7CFC00>[OK] Gold Pan</color>" : "<color=#FF6B6B>[X] Gold Pan</color>";
                promptText.text = $"<b><size=24><color=#FF6B6B>{blockedTitle}</color></size></b>\n<size=17>Wajib Bawa: {shovelStr}  {panStr}</size>";
            }
        }

        public static void CleanupStrayFaders(GameObject exceptThis = null)
        {
            var strayCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in strayCanvases)
            {
                if (c != null && c.gameObject.name == "[VR_Screen_Fader]")
                {
                    if (exceptThis != null && c.gameObject == exceptThis) continue;
                    Object.Destroy(c.gameObject);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = doorCenter != null ? doorCenter.position : transform.position;
            Gizmos.DrawWireSphere(center, interactionDistance);
        }
    }

    /// <summary>
    /// Persistent helper attached to [VR_Screen_Fader] during scene transitions.
    /// Automatically detects scene load, re-binds to new Camera.main, re-enables
    /// input action maps and XR simulator controls, smoothly fades in from black,
    /// and destroys the fader GameObject so hands and raycasts never get blocked.
    /// </summary>
    public class VRScreenFaderHelper : MonoBehaviour
    {
        public float fadeDuration = 0.5f;

        private void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            StartCoroutine(FadeInAndDestroyRoutine(scene));
        }

        private IEnumerator FadeInAndDestroyRoutine(Scene loadedScene)
        {
            // Wait 1 frame so target scene components initialize cleanly in Start/Awake
            yield return null;

            Camera mainCam = Camera.main;
            var canvas = GetComponent<Canvas>();
            if (canvas != null && mainCam != null)
            {
                canvas.worldCamera = mainCam;
            }

            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }

            // Clean up any other stray screen faders except this one
            DoorSceneTransition.CleanupStrayFaders(this.gameObject);

            // Position player cleanly in front of the door in the loaded scene
            VRSimulatorSceneBinder.PositionPlayerAtDoorEntrance(loadedScene);

            // Re-bind XR Interaction Simulator thoroughly for the new scene
            VRSimulatorSceneBinder.RebindVRSimulator(loadedScene);

            // Smooth fade-in from black to transparent
            float elapsed = 0f;
            float dur = fadeDuration > 0.1f ? fadeDuration : 0.5f;
            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                if (cg != null)
                {
                    cg.alpha = Mathf.Clamp01(1f - (elapsed / dur));
                }
                if (mainCam != null)
                {
                    transform.position = mainCam.transform.position + mainCam.transform.forward * 0.2f;
                    transform.rotation = mainCam.transform.rotation;
                }
                yield return null;
            }

            if (cg != null) cg.alpha = 0f;

            // Destroy fader GameObject cleanly so it never lingers
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Dedicated scene binder that completely restores XR Interaction Simulator
    /// bindings, camera references, controller transforms, physics scenes, input actions,
    /// and player entrance spawn positions upon transitioning between scenes (Room <-> Tutorial).
    /// </summary>
    public static class VRSimulatorSceneBinder
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterGlobalSceneHook()
        {
            SceneManager.sceneLoaded -= OnGlobalSceneLoaded;
            SceneManager.sceneLoaded += OnGlobalSceneLoaded;
        }

        private static void OnGlobalSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Position player cleanly in front of the door
            PositionPlayerAtDoorEntrance(scene);

            // Immediate rebind on frame 0
            RebindVRSimulator(scene);

            // Auto-attach a lightweight one-shot runner to execute secondary re-bind after frame 0 (post Start())
            var runner = new GameObject("[VR_Simulator_Scene_Rebinder]").AddComponent<VRRebinderOneShot>();
            runner.targetScene = scene;
        }

        /// <summary>
        /// Positions the player (XR Origin + CharacterController) right in front of the entrance door:
        /// - Entering Room scene: Spawns at Anchor_Entrance (0, 0.01, -2.0) facing +Z forward into the cabin,
        ///   rather than spawning far away at the workbench table.
        /// - Entering Tutorial scene: Spawns on the front porch of Cabin_Door_Slab (22.8, 1.25, 2.3) facing
        ///   outward toward the clearing and pond, rather than spawning at (0, 0, 0).
        /// </summary>
        public static void PositionPlayerAtDoorEntrance(Scene scene)
        {
            if (!scene.IsValid()) scene = SceneManager.GetActiveScene();

            var xrOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (xrOrigin == null) return;

            var cc = xrOrigin.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            if (scene.name == "Room scene")
            {
                var anchorEntrance = GameObject.Find("Anchor_Entrance");
                Vector3 targetPos = anchorEntrance != null ? anchorEntrance.transform.position : new Vector3(0f, 0.01f, -2.0f);
                targetPos.y = 0.01f;
                xrOrigin.transform.position = targetPos;
                xrOrigin.transform.rotation = Quaternion.identity;
                Debug.Log($"[VRSimulatorSceneBinder] Player positioned at Room cabin entrance: {targetPos} (Anchor_Entrance)");
            }
            else if (scene.name == "Tutorial scene")
            {
                var anchorEntrance = GameObject.Find("Anchor_Entrance");
                Vector3 targetPos = anchorEntrance != null ? anchorEntrance.transform.position : new Vector3(22.8f, 1.25f, 2.3f);
                Quaternion targetRot = anchorEntrance != null ? anchorEntrance.transform.rotation : Quaternion.Euler(0f, 145f, 0f);

                xrOrigin.transform.position = targetPos;
                xrOrigin.transform.rotation = targetRot;
                Debug.Log($"[VRSimulatorSceneBinder] Player positioned at Tutorial cabin door porch: {targetPos}");
            }
            else if (scene.name == "Mainmenu scene" || scene.name == "MainMenu")
            {
                xrOrigin.transform.position = Vector3.zero;
                xrOrigin.transform.rotation = Quaternion.identity;
                if (xrOrigin.CameraFloorOffsetObject != null)
                {
                    if (PlayerBodyCollision.IsRealVRDeviceActive() || xrOrigin.RequestedTrackingOriginMode == Unity.XR.CoreUtils.XROrigin.TrackingOriginMode.Floor || xrOrigin.CurrentTrackingOriginMode == UnityEngine.XR.TrackingOriginModeFlags.Floor)
                    {
                        xrOrigin.CameraFloorOffsetObject.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        xrOrigin.CameraFloorOffsetObject.transform.localPosition = new Vector3(0f, xrOrigin.CameraYOffset > 0 ? xrOrigin.CameraYOffset : 1.67f, 0f);
                    }
                    xrOrigin.CameraFloorOffsetObject.transform.localRotation = Quaternion.identity;
                }
                if (xrOrigin.Camera != null && !PlayerBodyCollision.IsRealVRDeviceActive())
                {
                    xrOrigin.Camera.transform.localPosition = new Vector3(0f, xrOrigin.CameraYOffset > 0 ? xrOrigin.CameraYOffset : 1.67f, 0f);
                    xrOrigin.Camera.transform.localRotation = Quaternion.identity;
                }
                if (!PlayerBodyCollision.IsRealVRDeviceActive())
                {
                    ResetSimulatedInputDevices();
                    var sim = Object.FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
                    if (sim != null)
                    {
                        ResetSimulatorTrackingStates(sim);
                    }
                }
                Debug.Log($"[VRSimulatorSceneBinder] Player anchored at Main Menu spawn (0, 0, 0)");
            }

            if (cc != null) cc.enabled = true;
        }

        public static void RebindVRSimulator(Scene scene)
        {
            if (!scene.IsValid()) scene = SceneManager.GetActiveScene();

            Camera mainCam = Camera.main;
            var xrOrigin = Object.FindAnyObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (mainCam == null && xrOrigin != null)
            {
                mainCam = xrOrigin.Camera;
            }
            if (mainCam == null)
            {
                mainCam = Object.FindAnyObjectByType<Camera>();
            }

            // 1. Re-enable all InputActionManagers in target scene
            var actionManagers = Object.FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var iam in actionManagers)
            {
                if (iam != null && iam.isActiveAndEnabled)
                {
                    iam.EnableInput();
                }
            }

            // 2. Find active XR Interaction Simulator
            var sim = Object.FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
            if (sim == null)
            {
                Debug.LogWarning($"[VRSimulatorSceneBinder] XRInteractionSimulator not found in '{scene.name}'.");
                return;
            }

            // 3. Re-bind Camera transform and cached camera tuple
            if (mainCam != null)
            {
                sim.cameraTransform = mainCam.transform;
                var cachedCamField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                    .GetField("m_CachedCamera", BindingFlags.NonPublic | BindingFlags.Instance);
                if (cachedCamField != null)
                {
                    cachedCamField.SetValue(sim, (mainCam.transform, mainCam));
                }
            }

            // 4. Re-bind Left and Right Controller transforms
            Transform leftCtrl = null;
            Transform rightCtrl = null;
            if (xrOrigin != null)
            {
                leftCtrl = xrOrigin.transform.Find("Camera Offset/Left Controller");
                rightCtrl = xrOrigin.transform.Find("Camera Offset/Right Controller");
            }
            if (leftCtrl == null)
            {
                var lObj = GameObject.Find("Left Controller");
                if (lObj != null) leftCtrl = lObj.transform;
            }
            if (rightCtrl == null)
            {
                var rObj = GameObject.Find("Right Controller");
                if (rObj != null) rightCtrl = rObj.transform;
            }

            if (leftCtrl != null)
            {
                sim.leftControllerTransform = leftCtrl;
                sim.leftHandAimTransform = leftCtrl;
            }
            else if (mainCam != null)
            {
                sim.leftControllerTransform = mainCam.transform;
                sim.leftHandAimTransform = mainCam.transform;
            }

            if (rightCtrl != null)
            {
                sim.rightControllerTransform = rightCtrl;
                sim.rightHandAimTransform = rightCtrl;
            }
            else if (mainCam != null)
            {
                sim.rightControllerTransform = mainCam.transform;
                sim.rightHandAimTransform = mainCam.transform;
            }

            // Set controller & hands capability flags
            var canUseCtrlField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_CanUsePointAndClickControllers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (canUseCtrlField != null) canUseCtrlField.SetValue(sim, true);

            var canUseHandsField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_CanUsePointAndClickHands", BindingFlags.NonPublic | BindingFlags.Instance);
            if (canUseHandsField != null) canUseHandsField.SetValue(sim, true);

            var pointClickField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_PointAndClickActive", BindingFlags.NonPublic | BindingFlags.Instance);
            if (pointClickField != null) pointClickField.SetValue(sim, true);

            var usePointClickProp = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetProperty("usePointAndClick", BindingFlags.Public | BindingFlags.Instance);
            if (usePointClickProp != null) usePointClickProp.SetValue(sim, true);

            // Re-bind TargetedDevices: Ensure FPS | RightDevice is targeted so manipulatingRightDevice is true
            var targetDeviceField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_TargetedDeviceInput", BindingFlags.NonPublic | BindingFlags.Instance);
            if (targetDeviceField != null)
            {
                targetDeviceField.SetValue(sim, UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.TargetedDevices.FPS | UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.TargetedDevices.RightDevice);
            }

            // Reset previous raycast hit distance
            var prevDistField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_PreviousRaycastHitDistance", BindingFlags.NonPublic | BindingFlags.Instance);
            if (prevDistField != null) prevDistField.SetValue(sim, 0f);

            // 5. Re-bind Physics Scene so mouse raycasts hit target scene colliders
            var physicsSceneField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_LocalPhysicsScene", BindingFlags.NonPublic | BindingFlags.Instance);
            if (physicsSceneField != null)
            {
                physicsSceneField.SetValue(sim, scene.IsValid() ? scene.GetPhysicsScene() : Physics.defaultPhysicsScene);
            }

            // 6. Re-bind EventSystem & XRUIInputModule
            var uiModuleField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_UIInputModule", BindingFlags.NonPublic | BindingFlags.Instance);
            var eventDataField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_UIPointerEventData", BindingFlags.NonPublic | BindingFlags.Instance);

            var eventSys = UnityEngine.EventSystems.EventSystem.current;
            if (eventSys != null)
            {
                var xrui = eventSys.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
                if (uiModuleField != null && xrui != null) uiModuleField.SetValue(sim, xrui);
                if (eventDataField != null) eventDataField.SetValue(sim, new UnityEngine.EventSystems.PointerEventData(eventSys));
            }

            // 7. Re-bind XROrigin reference
            var xrOriginField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator)
                .GetField("m_XROrigin", BindingFlags.NonPublic | BindingFlags.Instance);
            if (xrOriginField != null && xrOrigin != null)
            {
                xrOriginField.SetValue(sim, xrOrigin);
            }

            // 8. If entering Main Menu, reset all simulated device positions/rotations
            if (scene.name == "Mainmenu scene" || scene.name == "MainMenu")
            {
                ResetSimulatorTrackingStates(sim);
                ResetSimulatedInputDevices();
            }

            Debug.Log($"[VRSimulatorSceneBinder] Simulator refreshed for '{scene.name}': Cam={mainCam != null}, LeftCtrl={leftCtrl != null}, RightCtrl={rightCtrl != null}, PhysicsScene={scene.GetPhysicsScene().IsValid()}");
        }

        public static void ResetSimulatedInputDevices()
        {
            try
            {
                foreach (var device in UnityEngine.InputSystem.InputSystem.devices)
                {
                    if (device is UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRSimulatedHMD hmd)
                    {
                        var st = new UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRSimulatedHMDState();
                        st.Reset();
                        UnityEngine.InputSystem.LowLevel.InputState.Change(hmd, st);
                    }
                    else if (device is UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRSimulatedController ctrl)
                    {
                        var st = new UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRSimulatedControllerState();
                        st.Reset();
                        bool isLeft = ctrl.name.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0;
                        st.devicePosition = isLeft ? new Vector3(-0.1f, -0.05f, 0.3f) : new Vector3(0.1f, -0.05f, 0.3f);
                        UnityEngine.InputSystem.LowLevel.InputState.Change(ctrl, st);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VRSimulatorSceneBinder] ResetSimulatedInputDevices non-fatal warning: {ex.Message}");
            }
        }

        public static void ResetSimulatorTrackingStates(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator sim)
        {
            if (sim == null) return;

            try
            {
                var simType = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator);

                // 1. Reset Euler Angles
                var centerEulerField = simType.GetField("m_CenterEyeEuler", BindingFlags.NonPublic | BindingFlags.Instance);
                if (centerEulerField != null) centerEulerField.SetValue(sim, Vector3.zero);

                var leftEulerField = simType.GetField("m_LeftControllerEuler", BindingFlags.NonPublic | BindingFlags.Instance);
                if (leftEulerField != null) leftEulerField.SetValue(sim, Vector3.zero);

                var rightEulerField = simType.GetField("m_RightControllerEuler", BindingFlags.NonPublic | BindingFlags.Instance);
                if (rightEulerField != null) rightEulerField.SetValue(sim, Vector3.zero);

                // 2. Reset HMD State
                var hmdStateField = simType.GetField("m_HMDState", BindingFlags.NonPublic | BindingFlags.Instance);
                object hmdStateObj = null;
                if (hmdStateField != null)
                {
                    var hmdStateType = hmdStateField.FieldType;
                    var resetMethod = hmdStateType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Instance);
                    hmdStateObj = hmdStateField.GetValue(sim);
                    if (hmdStateObj != null && resetMethod != null)
                    {
                        resetMethod.Invoke(hmdStateObj, null);
                    }
                    else
                    {
                        hmdStateObj = System.Activator.CreateInstance(hmdStateType);
                    }
                    hmdStateField.SetValue(sim, hmdStateObj);
                }

                // 3. Reset Controller States
                Vector3 leftDefaultPos = new Vector3(-0.1f, -0.05f, 0.3f);
                Vector3 rightDefaultPos = new Vector3(0.1f, -0.05f, 0.3f);

                var leftCtrlStateField = simType.GetField("m_LeftControllerState", BindingFlags.NonPublic | BindingFlags.Instance);
                object leftCtrlStateObj = null;
                if (leftCtrlStateField != null)
                {
                    var leftType = leftCtrlStateField.FieldType;
                    var resetMethod = leftType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Instance);
                    leftCtrlStateObj = leftCtrlStateField.GetValue(sim);
                    if (leftCtrlStateObj != null && resetMethod != null)
                    {
                        resetMethod.Invoke(leftCtrlStateObj, null);
                    }
                    else
                    {
                        leftCtrlStateObj = System.Activator.CreateInstance(leftType);
                    }
                    var posField = leftType.GetField("devicePosition", BindingFlags.Public | BindingFlags.Instance);
                    if (posField != null) posField.SetValue(leftCtrlStateObj, leftDefaultPos);
                    leftCtrlStateField.SetValue(sim, leftCtrlStateObj);
                }

                var rightCtrlStateField = simType.GetField("m_RightControllerState", BindingFlags.NonPublic | BindingFlags.Instance);
                object rightCtrlStateObj = null;
                if (rightCtrlStateField != null)
                {
                    var rightType = rightCtrlStateField.FieldType;
                    var resetMethod = rightType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Instance);
                    rightCtrlStateObj = rightCtrlStateField.GetValue(sim);
                    if (rightCtrlStateObj != null && resetMethod != null)
                    {
                        resetMethod.Invoke(rightCtrlStateObj, null);
                    }
                    else
                    {
                        rightCtrlStateObj = System.Activator.CreateInstance(rightType);
                    }
                    var posField = rightType.GetField("devicePosition", BindingFlags.Public | BindingFlags.Instance);
                    if (posField != null) posField.SetValue(rightCtrlStateObj, rightDefaultPos);
                    rightCtrlStateField.SetValue(sim, rightCtrlStateObj);
                }

                // 4. Reset Hand States
                var leftHandStateField = simType.GetField("m_LeftHandState", BindingFlags.NonPublic | BindingFlags.Instance);
                object leftHandStateObj = null;
                if (leftHandStateField != null)
                {
                    var handType = leftHandStateField.FieldType;
                    var resetMethod = handType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Instance);
                    leftHandStateObj = leftHandStateField.GetValue(sim);
                    if (leftHandStateObj != null && resetMethod != null)
                    {
                        resetMethod.Invoke(leftHandStateObj, null);
                    }
                    else
                    {
                        leftHandStateObj = System.Activator.CreateInstance(handType);
                    }
                    var posField = handType.GetField("position", BindingFlags.Public | BindingFlags.Instance);
                    if (posField != null) posField.SetValue(leftHandStateObj, leftDefaultPos);
                    leftHandStateField.SetValue(sim, leftHandStateObj);
                }

                var rightHandStateField = simType.GetField("m_RightHandState", BindingFlags.NonPublic | BindingFlags.Instance);
                object rightHandStateObj = null;
                if (rightHandStateField != null)
                {
                    var handType = rightHandStateField.FieldType;
                    var resetMethod = handType.GetMethod("Reset", BindingFlags.Public | BindingFlags.Instance);
                    rightHandStateObj = rightHandStateField.GetValue(sim);
                    if (rightHandStateObj != null && resetMethod != null)
                    {
                        resetMethod.Invoke(rightHandStateObj, null);
                    }
                    else
                    {
                        rightHandStateObj = System.Activator.CreateInstance(handType);
                    }
                    var posField = handType.GetField("position", BindingFlags.Public | BindingFlags.Instance);
                    if (posField != null) posField.SetValue(rightHandStateObj, rightDefaultPos);
                    rightHandStateField.SetValue(sim, rightHandStateObj);
                }

                // 5. Apply states to SimulatedDeviceLifecycleManager
                var dlmField = simType.GetField("m_DeviceLifecycleManager", BindingFlags.NonPublic | BindingFlags.Instance);
                object dlmObj = dlmField != null ? dlmField.GetValue(sim) : null;
                if (dlmObj != null)
                {
                    var dlmType = dlmObj.GetType();
                    if (hmdStateObj != null)
                    {
                        var applyHmd = dlmType.GetMethod("ApplyHMDState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (applyHmd != null) applyHmd.Invoke(dlmObj, new object[] { hmdStateObj });
                    }
                    if (leftCtrlStateObj != null && rightCtrlStateObj != null)
                    {
                        var applyCtrl = dlmType.GetMethod("ApplyControllerState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (applyCtrl != null) applyCtrl.Invoke(dlmObj, new object[] { leftCtrlStateObj, rightCtrlStateObj });
                    }
                    if (leftHandStateObj != null && rightHandStateObj != null)
                    {
                        var applyHand = dlmType.GetMethod("ApplyHandState", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (applyHand != null) applyHand.Invoke(dlmObj, new object[] { leftHandStateObj, rightHandStateObj });
                    }
                }

                Debug.Log("[VRSimulatorSceneBinder] Simulator HMD, Controller, and Hand states completely reset to origin (0, 0, 0).");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[VRSimulatorSceneBinder] ResetSimulatorTrackingStates non-fatal warning: {ex.Message}");
            }
        }
    }

    public class VRRebinderOneShot : MonoBehaviour
    {
        public Scene targetScene;

        private IEnumerator Start()
        {
            yield return null;
            VRSimulatorSceneBinder.PositionPlayerAtDoorEntrance(targetScene);
            VRSimulatorSceneBinder.RebindVRSimulator(targetScene);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Automated Play Mode verifier that tests bidirectional scene transitions:
    /// Tutorial scene -> Room scene -> Tutorial scene.
    /// Programmatically asserts:
    /// 1. XR Interaction Simulator references (camera, controllers, physics scene) in Tutorial scene.
    /// 2. Simulated mouse aiming / pointing responsiveness (verifying controller rotation updates dynamically without exceptions).
    /// 3. DoorSceneTransition execution from Tutorial scene to Room scene.
    /// 4. Post-transition rebinding in Room scene: camera, controllers, and physics scene valid, no stray faders, mouse aiming responsive.
    /// 5. DoorSceneTransition execution from Room scene back to Tutorial scene.
    /// 6. Post-transition rebinding in Tutorial scene: camera, controllers, and physics scene valid, mouse aiming responsive.
    /// 7. Zero compile errors and zero runtime exceptions across the entire transition cycle.
    /// </summary>
    public class DoorTransitionPlayModeVerifier : MonoBehaviour
    {
        public const string VERIFY_PREF_KEY = "ProspectorJourney_RunTransitionAudit";
        public const string VERIFY_RESULT_KEY = "ProspectorJourney_TransitionAuditPassed";
        public const string VERIFY_REPORT_KEY = "ProspectorJourney_TransitionAuditReport";

        public bool isRunning = false;
        public bool isComplete = false;
        public bool allPassed = false;

        private static DoorTransitionPlayModeVerifier s_Instance;
        private int m_ErrorCount = 0;
        private int m_ExceptionCount = 0;
        private readonly List<string> m_CapturedErrors = new List<string>();
        private readonly List<string> m_AuditLog = new List<string>();
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoStartIfAuditRequested()
        {
            if (UnityEditor.EditorPrefs.GetBool(VERIFY_PREF_KEY, false))
            {
                if (s_Instance == null)
                {
                    var go = new GameObject("[DoorTransitionPlayModeVerifier]");
                    go.AddComponent<DoorTransitionPlayModeVerifier>();
                }
            }
        }
#endif

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                DestroyImmediate(gameObject);
                return;
            }
            s_Instance = this;
            DontDestroyOnLoad(gameObject);
            Application.logMessageReceived += HandleLogMessage;
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                Application.logMessageReceived -= HandleLogMessage;
                s_Instance = null;
            }
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
                m_ErrorCount++;
                m_CapturedErrors.Add($"[ERROR] {condition}");
            }
        }

        private void Start()
        {
            StartCoroutine(RunVerificationRoutine());
        }

        private IEnumerator RunVerificationRoutine()
        {
            isRunning = true;
            LogReport("================================================================================");
            LogReport("[TRANSITION PLAY MODE AUDIT] STARTING BIDIRECTIONAL DOOR & VR SIMULATOR AUDIT");
            LogReport("================================================================================");

            // Wait 3 frames for initial scene and simulator initialization
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return null;

            // -------------------------------------------------------------------------
            // PHASE 1: Verify Initial Tutorial Scene & VR Simulator Responsiveness
            // -------------------------------------------------------------------------
            Scene tutScene = SceneManager.GetActiveScene();
            LogReport($"\n[PHASE 1] Initial Scene: '{tutScene.name}'");
            if (tutScene.name != "Tutorial scene")
            {
                FailAudit($"Initial active scene is '{tutScene.name}', expected 'Tutorial scene'!");
                yield break;
            }

            bool phase1SimOk = VerifySimulatorResponsiveness(tutScene, out string p1SimDetails);
            LogReport($"Phase 1 Simulator Status: {(phase1SimOk ? "PASSED" : "FAILED")} - {p1SimDetails}");
            if (!phase1SimOk)
            {
                FailAudit($"Phase 1 simulator verification failed: {p1SimDetails}");
                yield break;
            }

            var xrOriginP1 = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var tutDoorSlabP1 = GameObject.Find("Cabin_Door_Slab");
            float p1DistToDoor = (tutDoorSlabP1 != null && xrOriginP1 != null) ? Vector3.Distance(xrOriginP1.transform.position, tutDoorSlabP1.transform.position) : 999f;
            bool p1PosOk = p1DistToDoor < 2.5f;
            LogReport($"Phase 1 Player Door Position: {(p1PosOk ? "PASSED" : "FAILED")} (pos={xrOriginP1.transform.position}, distToDoor={p1DistToDoor:F2}m)");
            if (!p1PosOk)
            {
                FailAudit($"Phase 1 player is at {xrOriginP1.transform.position}, dist to door is {p1DistToDoor:F2}m! Expected near Cabin_Door_Slab (<2.5m).");
                yield break;
            }

            // Ensure player has required tools to pass door check
            EnsurePlayerToolsReady();

            // -------------------------------------------------------------------------
            // PHASE 2: Transition from Tutorial Scene -> Room Scene
            // -------------------------------------------------------------------------
            LogReport("\n[PHASE 2] Executing Door Transition: 'Tutorial scene' -> 'Room scene'...");
            DoorSceneTransition tutDoor = FindFirstObjectByType<DoorSceneTransition>();
            if (tutDoor == null)
            {
                FailAudit("DoorSceneTransition not found in Tutorial scene!");
                yield break;
            }

            if (tutDoor.TargetSceneName != "Room scene")
            {
                FailAudit($"Tutorial door target scene is '{tutDoor.TargetSceneName}', expected 'Room scene'!");
                yield break;
            }

            tutDoor.TriggerTransitionDirect();

            float timeout = 10f;
            float elapsed = 0f;
            while (SceneManager.GetActiveScene().name != "Room scene" && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (SceneManager.GetActiveScene().name != "Room scene")
            {
                FailAudit($"Transition timed out waiting for 'Room scene'! Active scene is '{SceneManager.GetActiveScene().name}'.");
                yield break;
            }

            // Wait for screen fader to finish fading in and destroying itself (up to 1.5s)
            float waitFader = 0f;
            int strayFaderCount = 0;
            while (waitFader < 1.5f)
            {
                strayFaderCount = 0;
                var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var c in canvases)
                {
                    if (c != null && c.gameObject.name == "[VR_Screen_Fader]") strayFaderCount++;
                }
                if (strayFaderCount == 0) break;
                waitFader += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForFixedUpdate();
            yield return null;

            // -------------------------------------------------------------------------
            // PHASE 3: Verify Room Scene State & VR Simulator Rebinding
            // -------------------------------------------------------------------------
            Scene roomScene = SceneManager.GetActiveScene();
            LogReport($"\n[PHASE 3] In Scene: '{roomScene.name}'");
            LogReport($"Screen Fader Cleanup: {(strayFaderCount == 0 ? "PASSED (0 lingering faders)" : $"FAILED ({strayFaderCount} lingering faders)")}");

            bool phase3SimOk = VerifySimulatorResponsiveness(roomScene, out string p3SimDetails);
            LogReport($"Phase 3 Simulator Status: {(phase3SimOk ? "PASSED" : "FAILED")} - {p3SimDetails}");
            if (!phase3SimOk || strayFaderCount > 0)
            {
                FailAudit($"Phase 3 verification failed! SimOk={phase3SimOk}, Faders={strayFaderCount}. Details: {p3SimDetails}");
                yield break;
            }

            var xrOriginP3 = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var anchorEntranceP3 = GameObject.Find("Anchor_Entrance");
            Vector3 entranceTarget = anchorEntranceP3 != null ? anchorEntranceP3.transform.position : new Vector3(0f, 0.01f, -2.0f);
            float p3DistToEntrance = xrOriginP3 != null ? Vector3.Distance(xrOriginP3.transform.position, entranceTarget) : 999f;
            bool p3PosOk = p3DistToEntrance < 1.0f;
            LogReport($"Phase 3 Room Entrance Position: {(p3PosOk ? "PASSED" : "FAILED")} (pos={xrOriginP3.transform.position}, distToEntrance={p3DistToEntrance:F2}m)");
            if (!p3PosOk)
            {
                FailAudit($"Phase 3 player in Room scene is at {xrOriginP3.transform.position}, dist to entrance is {p3DistToEntrance:F2}m! Expected at Anchor_Entrance (<1.0m, not at table).");
                yield break;
            }

            // -------------------------------------------------------------------------
            // PHASE 4: Transition from Room Scene -> Tutorial Scene
            // -------------------------------------------------------------------------
            LogReport("\n[PHASE 4] Executing Return Door Transition: 'Room scene' -> 'Tutorial scene'...");
            DoorSceneTransition roomDoor = FindFirstObjectByType<DoorSceneTransition>();
            if (roomDoor == null)
            {
                FailAudit("DoorSceneTransition not found in Room scene!");
                yield break;
            }

            if (roomDoor.TargetSceneName != "Tutorial scene")
            {
                FailAudit($"Room door target scene is '{roomDoor.TargetSceneName}', expected 'Tutorial scene'!");
                yield break;
            }

            roomDoor.TriggerTransitionDirect();

            elapsed = 0f;
            while (SceneManager.GetActiveScene().name != "Tutorial scene" && elapsed < timeout)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (SceneManager.GetActiveScene().name != "Tutorial scene")
            {
                FailAudit($"Transition back timed out! Active scene is '{SceneManager.GetActiveScene().name}'.");
                yield break;
            }

            // Wait for screen fader to finish fading in and destroying itself (up to 1.5s)
            waitFader = 0f;
            while (waitFader < 1.5f)
            {
                strayFaderCount = 0;
                var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var c in canvases)
                {
                    if (c != null && c.gameObject.name == "[VR_Screen_Fader]") strayFaderCount++;
                }
                if (strayFaderCount == 0) break;
                waitFader += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForFixedUpdate();
            yield return null;

            // -------------------------------------------------------------------------
            // PHASE 5: Verify Tutorial Scene Return State & VR Simulator Rebinding
            // -------------------------------------------------------------------------
            Scene finalScene = SceneManager.GetActiveScene();
            LogReport($"\n[PHASE 5] In Scene: '{finalScene.name}' (Return Complete)");
            LogReport($"Screen Fader Cleanup: {(strayFaderCount == 0 ? "PASSED (0 lingering faders)" : $"FAILED ({strayFaderCount} lingering faders)")}");

            bool phase5SimOk = VerifySimulatorResponsiveness(finalScene, out string p5SimDetails);
            LogReport($"Phase 5 Simulator Status: {(phase5SimOk ? "PASSED" : "FAILED")} - {p5SimDetails}");
            if (!phase5SimOk || strayFaderCount > 0)
            {
                FailAudit($"Phase 5 verification failed! SimOk={phase5SimOk}, Faders={strayFaderCount}. Details: {p5SimDetails}");
                yield break;
            }

            var xrOriginP5 = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            var tutDoorSlabP5 = GameObject.Find("Cabin_Door_Slab");
            float p5DistToDoor = (tutDoorSlabP5 != null && xrOriginP5 != null) ? Vector3.Distance(xrOriginP5.transform.position, tutDoorSlabP5.transform.position) : 999f;
            bool p5PosOk = p5DistToDoor < 2.5f;
            LogReport($"Phase 5 Tutorial Door Return Position: {(p5PosOk ? "PASSED" : "FAILED")} (pos={xrOriginP5.transform.position}, distToDoor={p5DistToDoor:F2}m)");
            if (!p5PosOk)
            {
                FailAudit($"Phase 5 player return position in Tutorial scene is at {xrOriginP5.transform.position}, dist to door is {p5DistToDoor:F2}m! Expected near Cabin_Door_Slab (<2.5m).");
                yield break;
            }

            // -------------------------------------------------------------------------
            // PHASE 6: Console Error and Exception Check
            // -------------------------------------------------------------------------
            LogReport("\n[PHASE 6] Exception & Error Audit");
            bool zeroErrors = (m_ErrorCount == 0 && m_ExceptionCount == 0);
            LogReport($"Console Errors: {m_ErrorCount}, Exceptions: {m_ExceptionCount}");
            if (!zeroErrors)
            {
                FailAudit($"Console errors detected during transition cycle!\n" + string.Join("\n", m_CapturedErrors));
                yield break;
            }

            // -------------------------------------------------------------------------
            // FINAL PASS SUMMARY
            // -------------------------------------------------------------------------
            allPassed = true;
            isComplete = true;
            isRunning = false;

            LogReport("\n================================================================================");
            LogReport("[TRANSITION PLAY MODE AUDIT] 100% PASSED ALL PHASES!");
            LogReport($"1. Initial Tutorial scene door porch position & simulator pointing: PASSED");
            LogReport($"2. Transition Tutorial -> Room: PASSED");
            LogReport($"3. Room scene entrance door position & simulator rebind/pointing: PASSED");
            LogReport($"4. Transition Room -> Tutorial: PASSED");
            LogReport($"5. Return Tutorial scene door porch position & simulator rebind/pointing: PASSED");
            LogReport($"6. Zero console errors & exceptions: PASSED (0 errors, 0 exceptions)");
            LogReport("================================================================================");

#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetBool(VERIFY_RESULT_KEY, true);
            UnityEditor.EditorPrefs.SetString(VERIFY_REPORT_KEY, string.Join("\n", m_AuditLog));
            yield return new WaitForSeconds(0.2f);
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private bool VerifySimulatorResponsiveness(Scene scene, out string details)
        {
            var sim = FindFirstObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
            if (sim == null)
            {
                details = "XRInteractionSimulator not found in scene!";
                return false;
            }

            if (!sim.isActiveAndEnabled)
            {
                details = "XRInteractionSimulator component is disabled!";
                return false;
            }

            if (sim.cameraTransform == null)
            {
                details = "sim.cameraTransform is null!";
                return false;
            }
            if (sim.cameraTransform.gameObject.scene != scene)
            {
                details = $"sim.cameraTransform belongs to scene '{sim.cameraTransform.gameObject.scene.name}', expected '{scene.name}'!";
                return false;
            }

            if (sim.rightControllerTransform == null)
            {
                details = "sim.rightControllerTransform is null!";
                return false;
            }
            if (sim.rightControllerTransform.gameObject.scene != scene)
            {
                details = $"sim.rightControllerTransform belongs to scene '{sim.rightControllerTransform.gameObject.scene.name}', expected '{scene.name}'!";
                return false;
            }

            var physicsSceneField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator).GetField("m_LocalPhysicsScene", BindingFlags.NonPublic | BindingFlags.Instance);
            if (physicsSceneField != null)
            {
                PhysicsScene ps = (PhysicsScene)physicsSceneField.GetValue(sim);
                if (!ps.IsValid())
                {
                    details = "sim.m_LocalPhysicsScene is invalid (dummy physics scene)!";
                    return false;
                }
                if (ps != scene.GetPhysicsScene())
                {
                    details = "sim.m_LocalPhysicsScene does not match target scene PhysicsScene!";
                    return false;
                }
            }

            var aimMethod = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator).GetMethod("ProcessAimDeviceAtWorldPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            var stateField = typeof(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator).GetField("m_RightControllerState", BindingFlags.NonPublic | BindingFlags.Instance);

            if (aimMethod != null && stateField != null)
            {
                Vector3 origin = sim.cameraTransform.position;
                Vector3 targetA = origin + sim.cameraTransform.forward * 3f + sim.cameraTransform.right * -2f;
                Vector3 targetB = origin + sim.cameraTransform.forward * 3f + sim.cameraTransform.right * 2f;

                try
                {
                    aimMethod.Invoke(sim, new object[] { targetA });
                    var stateA = (UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRSimulatedControllerState)stateField.GetValue(sim);
                    Quaternion rotA = stateA.deviceRotation;

                    aimMethod.Invoke(sim, new object[] { targetB });
                    var stateB = (UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRSimulatedControllerState)stateField.GetValue(sim);
                    Quaternion rotB = stateB.deviceRotation;

                    float deltaAngle = Quaternion.Angle(rotA, rotB);
                    if (deltaAngle < 5f)
                    {
                        details = $"Controller rotation delta too small ({deltaAngle:F2} deg), aiming appears frozen/stiff!";
                        return false;
                    }

                    details = $"Cam={sim.cameraTransform.name}, RightCtrl={sim.rightControllerTransform.name}, PhysicsScene=Valid({psToString(scene)}), AimDelta={deltaAngle:F2} deg (Mobile & Pointing OK)";
                    return true;
                }
                catch (Exception ex)
                {
                    details = $"Exception while invoking ProcessAimDeviceAtWorldPoint: {ex.Message}";
                    return false;
                }
            }

            details = $"Cam={sim.cameraTransform.name}, RightCtrl={sim.rightControllerTransform.name}, PhysicsScene=Valid";
            return true;
        }

        private string psToString(Scene s)
        {
            return s.IsValid() ? s.name : "Invalid";
        }

        private void EnsurePlayerToolsReady()
        {
            if (ProspectorInventory.Instance != null)
            {
                var shovel = GameObject.Find("tool-shovel");
                var pan = GameObject.Find("Gold_Pan");
                if (shovel != null && pan != null)
                {
                    var shovelGrab = shovel.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                    var panGrab = pan.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                    if (shovelGrab != null && !ProspectorInventory.Instance.HasItemInBagOrHand("shovel"))
                    {
                        ProspectorInventory.Instance.StoreItemDirect(shovelGrab);
                    }
                    if (panGrab != null && !ProspectorInventory.Instance.HasItemInBagOrHand("Gold_Pan"))
                    {
                        ProspectorInventory.Instance.StoreItemDirect(panGrab);
                    }
                }
            }
        }

        private void FailAudit(string reason)
        {
            allPassed = false;
            isComplete = true;
            isRunning = false;
            LogReport($"\n[AUDIT FAILED]: {reason}");
#if UNITY_EDITOR
            UnityEditor.EditorPrefs.SetBool(VERIFY_RESULT_KEY, false);
            UnityEditor.EditorPrefs.SetString(VERIFY_REPORT_KEY, string.Join("\n", m_AuditLog));
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        private void LogReport(string msg)
        {
            Debug.Log(msg);
            m_AuditLog.Add(msg);
        }
    }
}
