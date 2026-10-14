using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ProspectorJourney.UI
{
    public class MainMenuVRController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Scene Names")]
        [SerializeField] private string mainGameScene = "Village";
        [SerializeField] private string tutorialScene = "Tutorial scene";

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;

        [Header("Background Music")]
        [SerializeField] private AudioSource bgmAudioSource;
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] [Range(0f, 1f)] private float bgmVolume = 0.45f;

        [Header("Settings Controls (Legacy)")]
        [SerializeField] private Toggle snapTurnToggle;
        [SerializeField] private Toggle vignetteToggle;
        [SerializeField] private Slider volumeSlider;

        [Header("Game Options UI (Turn & Locomotion)")]
        [SerializeField] private TextMeshProUGUI turnTypeText;
        [SerializeField] private TextMeshProUGUI locomotionTypeText;
        [SerializeField] private Button turnPrevButton;
        [SerializeField] private Button turnNextButton;
        [SerializeField] private Button locomotionPrevButton;
        [SerializeField] private Button locomotionNextButton;
        [SerializeField] private Button saveAndExitButton;

        [Header("Main Menu Navigation Buttons (Auto-Bound)")]
        [SerializeField] private Button btnStartExpedition;
        [SerializeField] private Button btnTutorial;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnCredits;
        [SerializeField] private Button btnQuit;
        [SerializeField] private Button btnLaunchTutorialScene;
        [SerializeField] private Button btnBackFromTutorial;
        [SerializeField] private Button btnBackFromCredits;

        public enum TurnTypeOption { Snap = 0, Continuous = 1 }
        public enum LocomotionTypeOption { Teleport = 0, Continuous = 1 }

        private TurnTypeOption currentTurnType = TurnTypeOption.Snap;
        private LocomotionTypeOption currentLocomotionType = LocomotionTypeOption.Teleport;

        [Header("VR Lazy Follow Settings")]
        [Tooltip("Enable smooth VR follow that stays at eye height and respects horizontal gaze.")]
        [SerializeField] private bool enableLazyFollow = true;
        [Tooltip("Reference to the VR camera. Defaults to Camera.main.")]
        [SerializeField] private Transform targetCamera;
        [Tooltip("Distance from the camera in meters.")]
        [SerializeField] private float followDistance = 1.85f;
        [Tooltip("Height offset relative to camera eye level.")]
        [SerializeField] private float heightOffset = 0.0f;
        [Tooltip("Horizontal angle deadzone in degrees before menu catches up.")]
        [SerializeField] private float deadzoneAngle = 18f;
        [Tooltip("Angle at which catch-up concludes.")]
        [SerializeField] private float catchUpThreshold = 2.0f;
        [Tooltip("Horizontal smoothing speed.")]
        [SerializeField] private float followSpeed = 4.0f;
        [Tooltip("Vertical height smoothing speed.")]
        [SerializeField] private float heightSpeed = 3.5f;

        [Header("Pitch Tilt Settings")]
        [Tooltip("Allow gentle pitch tilt towards camera without flying into ceiling/floor.")]
        [SerializeField] private bool enableSubtlePitch = true;
        [Tooltip("Max pitch angle in degrees (strictly clamped so menu never flips flat).")]
        [SerializeField] private float maxPitchAngle = 12f;

        public bool IsMenuMoving { get; private set; }

        private Vector3 currentHorizontalFacing = Vector3.forward;
        private bool isCatchingUp = false;
        private Vector3 lastPosition;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D clean UI audio
            }

            InitializeBGM();
            LoadSettings();
            ShowPanel(mainPanel);
        }

        private void InitializeBGM()
        {
            if (bgmAudioSource == null)
            {
                var bgmObj = new GameObject("BGM_MusicPlayer");
                bgmObj.transform.SetParent(transform, false);
                bgmAudioSource = bgmObj.AddComponent<AudioSource>();
            }

            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;
            bgmAudioSource.spatialBlend = 0f; // 2D clean stereo audio
            bgmAudioSource.volume = bgmVolume;

            if (bgmClip != null && !bgmAudioSource.isPlaying)
            {
                bgmAudioSource.clip = bgmClip;
                bgmAudioSource.Play();
            }
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            var canvas = GetComponent<Canvas>();
            if (canvas != null && canvas.worldCamera == null && targetCamera != null)
            {
                canvas.worldCamera = targetCamera.GetComponent<Camera>();
            }

            BindButtonsRuntime();
            UpdateOptionsUI();
            SnapToFront();
        }

        public void PlayHoverSound()
        {
            // Jangan bunyikan suara hover kalau menu sedang bergeser/nengok
            if (IsMenuMoving)
            {
                return;
            }

            if (audioSource != null && hoverClip != null)
            {
                audioSource.PlayOneShot(hoverClip, 0.4f);
            }
        }

        public void PlayClickSound()
        {
            if (audioSource != null && clickClip != null)
            {
                audioSource.PlayOneShot(clickClip, 0.8f);
            }
        }

        public void OnClickStartExpedition()
        {
            PlayClickSound();
            Debug.Log("[Prospector Journey] Start Expedition is COMING SOON!");
        }

        public void OnClickTutorial()
        {
            PlayClickSound();
            ShowPanel(tutorialPanel);
        }

        public void OnClickLaunchTutorialScene()
        {
            PlayClickSound();
            Debug.Log($"[Prospector Journey] Loading tutorial scene: {tutorialScene}");
            SceneManager.LoadSceneAsync(tutorialScene);
        }

        public void OnClickSettings()
        {
            PlayClickSound();
            LoadSettings();
            UpdateOptionsUI();
            ShowPanel(settingsPanel);
        }

        public void OnClickCredits()
        {
            PlayClickSound();
            ShowPanel(creditsPanel);
        }

        public void OnClickBack()
        {
            PlayClickSound();
            ShowPanel(mainPanel);
        }

        public void OnClickQuit()
        {
            PlayClickSound();
            Debug.Log("[Prospector Journey] Quitting application.");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        #region Game Options Controls (Turn Type & Locomotion Type)
        public void OnTurnTypePrev()
        {
            CycleTurnType(-1);
        }

        public void OnTurnTypeNext()
        {
            CycleTurnType(1);
        }

        public void OnLocomotionTypePrev()
        {
            CycleLocomotionType(-1);
        }

        public void OnLocomotionTypeNext()
        {
            CycleLocomotionType(1);
        }

        private void CycleTurnType(int dir)
        {
            int total = 2; // Snap (0), Continuous (1)
            int val = (int)currentTurnType + dir;
            if (val < 0) val = total - 1;
            else if (val >= total) val = 0;
            currentTurnType = (TurnTypeOption)val;
            UpdateOptionsUI();
            PlayClickSound();
        }

        private void CycleLocomotionType(int dir)
        {
            int total = 2; // Teleport (0), Continuous (1)
            int val = (int)currentLocomotionType + dir;
            if (val < 0) val = total - 1;
            else if (val >= total) val = 0;
            currentLocomotionType = (LocomotionTypeOption)val;
            UpdateOptionsUI();
            PlayClickSound();
        }

        public void OnSaveAndExitSettings()
        {
            PlayerPrefs.SetInt("VR_TurnType", (int)currentTurnType);
            PlayerPrefs.SetInt("VR_LocomotionType", (int)currentLocomotionType);
            PlayerPrefs.SetInt("VR_SnapTurn", currentTurnType == TurnTypeOption.Snap ? 1 : 0);
            PlayerPrefs.Save();

            PlayClickSound();
            Debug.Log($"[Prospector Journey] Saved Game Options: Turn={currentTurnType}, Locomotion={currentLocomotionType}");
            ShowPanel(mainPanel);
        }

        private void UpdateOptionsUI()
        {
            if (turnTypeText != null)
            {
                turnTypeText.text = currentTurnType == TurnTypeOption.Snap ? "SNAP" : "CONTINUOUS";
            }

            if (locomotionTypeText != null)
            {
                locomotionTypeText.text = currentLocomotionType == LocomotionTypeOption.Teleport ? "TELEPORT" : "CONTINUOUS";
            }
        }
        #endregion

        #region Legacy Settings Toggles
        public void OnToggleSnapTurn(bool isSnap)
        {
            currentTurnType = isSnap ? TurnTypeOption.Snap : TurnTypeOption.Continuous;
            PlayerPrefs.SetInt("VR_SnapTurn", isSnap ? 1 : 0);
            PlayerPrefs.SetInt("VR_TurnType", (int)currentTurnType);
            PlayerPrefs.Save();
            UpdateOptionsUI();
            PlayClickSound();
        }

        public void OnToggleVignette(bool enabled)
        {
            PlayerPrefs.SetInt("VR_Vignette", enabled ? 1 : 0);
            PlayerPrefs.Save();
            PlayClickSound();
        }

        public void OnVolumeChanged(float volume)
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("Audio_Volume", volume);
        }
        #endregion

        private void LoadSettings()
        {
            // Game Options
            currentTurnType = (TurnTypeOption)PlayerPrefs.GetInt("VR_TurnType", PlayerPrefs.GetInt("VR_SnapTurn", 1) == 1 ? 0 : 1);
            currentLocomotionType = (LocomotionTypeOption)PlayerPrefs.GetInt("VR_LocomotionType", 0);

            if (snapTurnToggle != null)
            {
                snapTurnToggle.isOn = currentTurnType == TurnTypeOption.Snap;
            }

            if (vignetteToggle != null)
            {
                vignetteToggle.isOn = PlayerPrefs.GetInt("VR_Vignette", 1) == 1;
            }

            if (volumeSlider != null)
            {
                float vol = PlayerPrefs.GetFloat("Audio_Volume", 1f);
                volumeSlider.value = vol;
                AudioListener.volume = vol;
            }

            UpdateOptionsUI();
        }

        private void BindButtonsRuntime()
        {
            // Auto-locate navigation buttons if unassigned
            if (mainPanel != null)
            {
                if (btnStartExpedition == null) btnStartExpedition = FindButtonByName(mainPanel.transform, "Btn_StartExpedition");
                if (btnTutorial == null) btnTutorial = FindButtonByName(mainPanel.transform, "Btn_Tutorial");
                if (btnSettings == null) btnSettings = FindButtonByName(mainPanel.transform, "Btn_Settings");
                if (btnCredits == null) btnCredits = FindButtonByName(mainPanel.transform, "Btn_Credits");
                if (btnQuit == null) btnQuit = FindButtonByName(mainPanel.transform, "Btn_Quit");
            }

            if (tutorialPanel != null)
            {
                if (btnLaunchTutorialScene == null) btnLaunchTutorialScene = FindButtonByName(tutorialPanel.transform, "Btn_LaunchTutorialScene");
                if (btnBackFromTutorial == null) btnBackFromTutorial = FindButtonByName(tutorialPanel.transform, "Btn_BackFromTutorial");
            }

            if (creditsPanel != null)
            {
                if (btnBackFromCredits == null) btnBackFromCredits = FindButtonByName(creditsPanel.transform, "Btn_BackFromCredits");
            }

            if (settingsPanel != null)
            {
                if (turnPrevButton == null) turnPrevButton = FindButtonByName(settingsPanel.transform, "Btn_TurnPrev");
                if (turnNextButton == null) turnNextButton = FindButtonByName(settingsPanel.transform, "Btn_TurnNext");
                if (locomotionPrevButton == null) locomotionPrevButton = FindButtonByName(settingsPanel.transform, "Btn_LocoPrev");
                if (locomotionNextButton == null) locomotionNextButton = FindButtonByName(settingsPanel.transform, "Btn_LocoNext");
                if (saveAndExitButton == null) saveAndExitButton = FindButtonByName(settingsPanel.transform, "Btn_SaveAndExit");
                if (saveAndExitButton == null) saveAndExitButton = FindButtonByName(settingsPanel.transform, "Btn_BackFromSettings");

                if (turnTypeText == null) turnTypeText = FindTextByName(settingsPanel.transform, "Text_TurnValue");
                if (locomotionTypeText == null) locomotionTypeText = FindTextByName(settingsPanel.transform, "Text_LocoValue");
            }

            // Hook up listeners reliably
            BindClick(btnStartExpedition, OnClickStartExpedition);
            BindClick(btnTutorial, OnClickTutorial);
            BindClick(btnSettings, OnClickSettings);
            BindClick(btnCredits, OnClickCredits);
            BindClick(btnQuit, OnClickQuit);

            BindClick(btnLaunchTutorialScene, OnClickLaunchTutorialScene);
            BindClick(btnBackFromTutorial, OnClickBack);
            BindClick(btnBackFromCredits, OnClickBack);

            BindClick(turnPrevButton, OnTurnTypePrev);
            BindClick(turnNextButton, OnTurnTypeNext);
            BindClick(locomotionPrevButton, OnLocomotionTypePrev);
            BindClick(locomotionNextButton, OnLocomotionTypeNext);
            BindClick(saveAndExitButton, OnSaveAndExitSettings);
        }

        private static Button FindButtonByName(Transform root, string buttonName)
        {
            if (root == null) return null;
            var buttons = root.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b.gameObject.name == buttonName) return b;
            }
            return null;
        }

        private static TextMeshProUGUI FindTextByName(Transform root, string textName)
        {
            if (root == null) return null;
            var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t.gameObject.name == textName) return t;
            }
            return null;
        }

        private static void BindClick(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null || action == null) return;
            btn.onClick.RemoveListener(action);
            btn.onClick.AddListener(action);
        }

        private void ShowPanel(GameObject panelToShow)
        {
            if (mainPanel != null) mainPanel.SetActive(mainPanel == panelToShow);
            if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == panelToShow);
            if (tutorialPanel != null) tutorialPanel.SetActive(tutorialPanel == panelToShow);
            if (creditsPanel != null) creditsPanel.SetActive(creditsPanel == panelToShow);
        }

        public void SnapToFront()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            if (targetCamera == null) return;

            Vector3 camFwd = targetCamera.forward;
            camFwd.y = 0f;
            currentHorizontalFacing = camFwd.sqrMagnitude > 0.001f ? camFwd.normalized : Vector3.forward;

            Vector3 snapPos = targetCamera.position + currentHorizontalFacing * followDistance;
            snapPos.y = targetCamera.position.y + heightOffset;
            transform.position = snapPos;
            transform.rotation = ComputeTargetRotation(snapPos, currentHorizontalFacing);
            lastPosition = snapPos;
            isCatchingUp = false;
            IsMenuMoving = false;
        }

        private void LateUpdate()
        {
            if (!enableLazyFollow)
            {
                IsMenuMoving = false;
                return;
            }

            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
                else return;
            }

            Vector3 camPos = targetCamera.position;
            Vector3 camFwd = targetCamera.forward;
            camFwd.y = 0f;

            if (camFwd.sqrMagnitude < 0.001f) return;
            camFwd.Normalize();

            // Calculate horizontal angle between current menu facing and camera forward
            float angle = Vector3.Angle(currentHorizontalFacing, camFwd);

            if (!isCatchingUp && angle > deadzoneAngle)
            {
                isCatchingUp = true;
            }
            else if (isCatchingUp && angle < catchUpThreshold)
            {
                isCatchingUp = false;
            }

            if (isCatchingUp)
            {
                currentHorizontalFacing = Vector3.Slerp(currentHorizontalFacing, camFwd, followSpeed * Time.deltaTime).normalized;
            }

            // Target position: strictly along horizontal direction at fixed distance, with smoothed Y height
            Vector3 targetPos = camPos + currentHorizontalFacing * followDistance;
            float desiredY = camPos.y + heightOffset;
            float newY = Mathf.Lerp(transform.position.y, desiredY, heightSpeed * Time.deltaTime);
            targetPos.y = newY;

            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

            // Target rotation
            Quaternion targetRot = ComputeTargetRotation(transform.position, currentHorizontalFacing);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, followSpeed * Time.deltaTime);

            // Determine if the menu is considered in motion
            float moveDelta = (transform.position - lastPosition).sqrMagnitude;
            lastPosition = transform.position;
            IsMenuMoving = isCatchingUp || (moveDelta > 0.0001f);
        }

        private Quaternion ComputeTargetRotation(Vector3 menuPos, Vector3 forwardDir)
        {
            if (!enableSubtlePitch || targetCamera == null)
            {
                return Quaternion.LookRotation(forwardDir, Vector3.up);
            }

            Vector3 toCamera = targetCamera.position - menuPos;
            if (toCamera.sqrMagnitude < 0.001f)
            {
                return Quaternion.LookRotation(forwardDir, Vector3.up);
            }

            // Canvas forward points away from camera so front face is visible
            Vector3 desiredFwd = -toCamera.normalized;
            float pitch = Mathf.Asin(Mathf.Clamp(desiredFwd.y, -1f, 1f)) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, -maxPitchAngle, maxPitchAngle);

            Vector3 horizontalDir = forwardDir.normalized;
            Quaternion yawRot = Quaternion.LookRotation(horizontalDir, Vector3.up);
            Quaternion pitchRot = Quaternion.Euler(pitch, 0f, 0f);

            return yawRot * pitchRot;
        }

        private void OnDisable()
        {
            IsMenuMoving = false;
        }
    }
}
