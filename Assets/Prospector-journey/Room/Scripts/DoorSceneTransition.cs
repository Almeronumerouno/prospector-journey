using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace ProspectorJourney.Room
{
    public class DoorSceneTransition : MonoBehaviour
    {
        [Header("Target Scene Configuration")]
        [Tooltip("Nama scene tujuan saat pintu dibuka. Bisa diganti sewaktu-waktu di Inspector.")]
        [SerializeField] private string targetSceneName = "Village";

        [Header("Proximity Detection")]
        [Tooltip("Jarak maksimal pemain dari pintu agar interaksi aktif (meter).")]
        [SerializeField] private float interactionDistance = 1.85f;
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
            UpdatePromptLabel();
        }

        private void Update()
        {
            if (m_IsTransitioning) return;

            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera == null) return;
            }

            // 1. Check distance to camera (player's head)
            Vector3 doorPos = doorCenter.position;
            Vector3 playerPos = m_MainCamera.transform.position;
            float dist = Vector3.Distance(doorPos, playerPos);

            bool nearby = dist <= interactionDistance;
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
                if (isNearby) UpdatePromptLabel();
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

            if (!HasRequiredToolsForExit(out bool hasShovel, out bool hasPan))
            {
                UpdatePromptLabel();
                string missingMsg;
                if (!hasShovel && !hasPan)
                    missingMsg = "Belum bisa keluar! Bawa Sekop & Gold Pan dulu.";
                else if (!hasShovel)
                    missingMsg = "Belum bisa keluar! Bawa Sekop (Shovel) dulu.";
                else
                    missingMsg = "Belum bisa keluar! Bawa Gold Pan dulu.";

                if (ProspectorInventory.Instance != null)
                {
                    ProspectorInventory.Instance.PlayErrorSound();
                    ProspectorInventory.Instance.ShowToast(missingMsg);
                }

                Debug.Log($"[DoorSceneTransition] Exit ditolak: hasShovel={hasShovel}, hasPan={hasPan}");
                return;
            }

            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogWarning("[DoorSceneTransition] Target scene name belum diset!");
                return;
            }

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

            // Create temporary camera overlay canvas for clean VR fade
            GameObject fadeCanvasGo = new GameObject("[VR_Screen_Fader]");
            fadeCanvasGo.transform.SetParent(m_MainCamera.transform, false);
            fadeCanvasGo.transform.localPosition = new Vector3(0, 0, 0.2f);
            fadeCanvasGo.transform.localRotation = Quaternion.identity;

            var canvas = fadeCanvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = m_MainCamera;
            canvas.sortingOrder = 32767;

            var rectTransform = fadeCanvasGo.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(4f, 4f);

            var canvasGroup = fadeCanvasGo.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            var imgGo = new GameObject("BlackOverlay");
            imgGo.transform.SetParent(fadeCanvasGo.transform, false);
            var imgRt = imgGo.AddComponent<RectTransform>();
            imgRt.anchorMin = Vector2.zero;
            imgRt.anchorMax = Vector2.one;
            imgRt.sizeDelta = Vector2.zero;

            var img = imgGo.AddComponent<Image>();
            img.color = fadeColor;

            // Fade alpha from 0 to 1
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;

            // Keep fader persistent across load
            DontDestroyOnLoad(fadeCanvasGo);
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

            if (HasRequiredToolsForExit(out bool hasShovel, out bool hasPan))
            {
                promptText.text = "<b><size=26>[E]</size> <color=#FFFFFF>Keluar Rumah</color></b>\n<size=17><color=#7CFC00>[OK] Sekop & Gold Pan Siap (VR: [A] / Trigger)</color></size>";
            }
            else
            {
                string shovelStr = hasShovel ? "<color=#7CFC00>[OK] Sekop</color>" : "<color=#FF6B6B>[X] Sekop</color>";
                string panStr = hasPan ? "<color=#7CFC00>[OK] Gold Pan</color>" : "<color=#FF6B6B>[X] Gold Pan</color>";
                promptText.text = $"<b><size=24><color=#FF6B6B>Belum Bisa Keluar!</color></size></b>\n<size=17>Wajib Bawa: {shovelStr}  {panStr}</size>";
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = doorCenter != null ? doorCenter.position : transform.position;
            Gizmos.DrawWireSphere(center, interactionDistance);
        }
    }
}
