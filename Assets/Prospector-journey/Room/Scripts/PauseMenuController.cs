using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR;

namespace ProspectorJourney.UI
{
    /// <summary>
    /// Controls the in-game Pause UI for Room and Tutorial scenes.
    /// Provides Resume, Game Options (identik plek ketiplek dengan Main Menu), and Quit to Main Menu.
    /// Freezes gameplay physics/locomotion using Time.timeScale and spatial VR positioning.
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }
        public static bool IsGamePaused { get; private set; }

        public enum TurnTypeOption { Snap = 0, Continuous = 1 }
        public enum LocomotionTypeOption { Teleport = 0, Continuous = 1 }

        [Header("UI Panels")]
        [Tooltip("Root canvas GameObject that appears when paused.")]
        [SerializeField] private GameObject pauseMenuCanvas;
        [Tooltip("Main pause menu panel (Resume, Options, Quit).")]
        [SerializeField] private GameObject pausePanel;
        [Tooltip("Game Options panel (Turn Type, Locomotion Type, Save & Exit).")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Main Pause Buttons")]
        [SerializeField] private Button btnResume;
        [SerializeField] private Button btnGameOptions;
        [SerializeField] private Button btnQuit;

        [Header("Game Options UI (Turn & Locomotion)")]
        [SerializeField] private TextMeshProUGUI turnTypeText;
        [SerializeField] private TextMeshProUGUI locomotionTypeText;
        [SerializeField] private Button turnPrevButton;
        [SerializeField] private Button turnNextButton;
        [SerializeField] private Button locomotionPrevButton;
        [SerializeField] private Button locomotionNextButton;
        [SerializeField] private Button saveAndExitButton;

        [Header("Spatial VR Follow Settings")]
        [Tooltip("Reference to the VR camera. Defaults to Camera.main.")]
        [SerializeField] private Transform targetCamera;
        [Tooltip("Distance in front of the camera when spawned.")]
        [SerializeField] private float followDistance = 1.45f;
        [Tooltip("Height offset relative to camera eye level.")]
        [SerializeField] private float heightOffset = 0.0f;
        [Tooltip("Enable smooth VR follow when player turns their head.")]
        [SerializeField] private bool enableLazyFollow = true;
        [SerializeField] private float deadzoneAngle = 22f;
        [SerializeField] private float catchUpThreshold = 2.0f;
        [SerializeField] private float followSpeed = 4.0f;
        [SerializeField] private float heightSpeed = 3.5f;

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;

        [Header("Scene Navigation")]
        [SerializeField] private string mainMenuScene = "Mainmenu scene";

        public bool IsMenuMoving { get; private set; }

        private TurnTypeOption currentTurnType = TurnTypeOption.Snap;
        private LocomotionTypeOption currentLocomotionType = LocomotionTypeOption.Teleport;

        private Vector3 currentHorizontalFacing = Vector3.forward;
        private bool isCatchingUp = false;
        private Vector3 lastPosition;
        private bool m_PrevMenuPressed = false;

        private void Awake()
        {
            Instance = this;
            IsGamePaused = false;
            Time.timeScale = 1.0f;

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D clean stereo UI audio
            audioSource.ignoreListenerPause = true;

            LoadSettings();
            BindButtons();

            // Default state: canvas inactive, pause panel ready
            if (pausePanel != null) pausePanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
        }

        private void Start()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            var canvas = pauseMenuCanvas != null ? pauseMenuCanvas.GetComponent<Canvas>() : GetComponentInChildren<Canvas>(true);
            if (canvas != null && canvas.worldCamera == null && targetCamera != null)
            {
                canvas.worldCamera = targetCamera.GetComponent<Camera>();
            }

            UpdateOptionsUI();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            // Always ensure timeScale is restored if scene changes
            Time.timeScale = 1.0f;
            IsGamePaused = false;
        }

        private void Update()
        {
            CheckPauseInput();
        }

        private void CheckPauseInput()
        {
            bool toggleRequested = false;

            // 1. Keyboard Input: Escape or P
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.escapeKey.wasPressedThisFrame || kb.pKey.wasPressedThisFrame)
                {
                    toggleRequested = true;
                }
            }

            // 2. VR Controller Input: Menu button on Left Controller
            var leftDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            if (leftDevice.isValid)
            {
                if (leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out bool menuPressed))
                {
                    if (menuPressed && !m_PrevMenuPressed)
                    {
                        toggleRequested = true;
                    }
                    m_PrevMenuPressed = menuPressed;
                }
            }

            if (toggleRequested)
            {
                if (!IsGamePaused)
                {
                    PauseGame();
                }
                else
                {
                    // If in Settings panel, Escape / Menu returns to Pause panel
                    if (settingsPanel != null && settingsPanel.activeSelf)
                    {
                        OnBackFromSettings();
                    }
                    else
                    {
                        ResumeGame();
                    }
                }
            }
        }

        public void PauseGame()
        {
            if (IsGamePaused) return;

            IsGamePaused = true;
            Time.timeScale = 0f;

            // Close inventory if open
            var inv = FindFirstObjectByType<ProspectorJourney.Room.ProspectorInventory>();
            if (inv != null && inv.IsOpen)
            {
                inv.CloseInventory();
            }

            // Temporarily disable locomotion so player doesn't move while paused
            SetRigLocomotionEnabled(false);

            // Position UI right in front of camera at eye height
            SnapToFront();

            if (pausePanel != null) pausePanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(true);

            PlayClickSound();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ResumeGame()
        {
            if (!IsGamePaused) return;

            IsGamePaused = false;
            Time.timeScale = 1.0f;

            // Re-enable locomotion and apply settings
            SetRigLocomotionEnabled(true);
            var rig = GameObject.Find("XR Origin (XR Rig)");
            if (rig != null)
            {
                VRSettingsApplier.ApplySettingsToRig(rig);
            }

            if (pauseMenuCanvas != null) pauseMenuCanvas.SetActive(false);
            IsMenuMoving = false;

            PlayClickSound();
        }

        public void OnClickGameOptions()
        {
            PlayClickSound();
            LoadSettings();
            UpdateOptionsUI();
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnBackFromSettings()
        {
            PlayClickSound();
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void OnSaveAndExitSettings()
        {
            PlayerPrefs.SetInt("VR_TurnType", (int)currentTurnType);
            PlayerPrefs.SetInt("VR_LocomotionType", (int)currentLocomotionType);
            PlayerPrefs.SetInt("VR_SnapTurn", currentTurnType == TurnTypeOption.Snap ? 1 : 0);
            PlayerPrefs.Save();

            PlayClickSound();
            Debug.Log($"[PauseMenuController] Saved Game Options: Turn={currentTurnType}, Locomotion={currentLocomotionType}");

            // Apply immediately to rig
            var rig = GameObject.Find("XR Origin (XR Rig)");
            if (rig != null)
            {
                VRSettingsApplier.ApplySettingsToRig(rig);
            }

            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void OnClickQuitToMainMenu()
        {
            PlayClickSound();
            Time.timeScale = 1.0f;
            IsGamePaused = false;
            Debug.Log($"[PauseMenuController] Quitting to Main Menu: {mainMenuScene}");

            // Reset simulated input devices and simulator tracking state before transitioning
            ProspectorJourney.Room.VRSimulatorSceneBinder.ResetSimulatedInputDevices();
            var sim = FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
            if (sim != null)
            {
                ProspectorJourney.Room.VRSimulatorSceneBinder.ResetSimulatorTrackingStates(sim);
            }

            SceneManager.LoadSceneAsync(mainMenuScene);
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

        private void LoadSettings()
        {
            currentTurnType = (TurnTypeOption)PlayerPrefs.GetInt("VR_TurnType", PlayerPrefs.GetInt("VR_SnapTurn", 1) == 1 ? 0 : 1);
            currentLocomotionType = (LocomotionTypeOption)PlayerPrefs.GetInt("VR_LocomotionType", 0);
            UpdateOptionsUI();
        }
        #endregion

        #region Locomotion Safety
        private void SetRigLocomotionEnabled(bool enabled)
        {
            var rig = GameObject.Find("XR Origin (XR Rig)");
            if (rig == null) return;

            var locomotionTr = rig.transform.Find("Locomotion");
            if (locomotionTr != null)
            {
                locomotionTr.gameObject.SetActive(enabled);
            }

            var providers = rig.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Locomotion.LocomotionProvider>(true);
            foreach (var p in providers)
            {
                p.enabled = enabled;
            }
        }
        #endregion

        #region Spatial Lazy Follow
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
            transform.rotation = Quaternion.LookRotation(currentHorizontalFacing, Vector3.up);
            lastPosition = snapPos;
            isCatchingUp = false;
            IsMenuMoving = false;
        }

        private void LateUpdate()
        {
            if (!IsGamePaused || !enableLazyFollow)
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

            float dt = Time.unscaledDeltaTime; // Must use unscaledDeltaTime when timeScale == 0!
            Vector3 camPos = targetCamera.position;
            Vector3 camFwd = targetCamera.forward;
            camFwd.y = 0f;

            if (camFwd.sqrMagnitude < 0.001f) return;
            camFwd.Normalize();

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
                currentHorizontalFacing = Vector3.Slerp(currentHorizontalFacing, camFwd, followSpeed * dt).normalized;
            }

            Vector3 targetPos = camPos + currentHorizontalFacing * followDistance;
            float desiredY = camPos.y + heightOffset;
            float newY = Mathf.Lerp(transform.position.y, desiredY, heightSpeed * dt);
            targetPos.y = newY;

            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * dt);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(currentHorizontalFacing, Vector3.up), followSpeed * dt);

            float moveDelta = (transform.position - lastPosition).sqrMagnitude;
            lastPosition = transform.position;
            IsMenuMoving = isCatchingUp || (moveDelta > 0.0001f);
        }
        #endregion

        #region Audio Feedback
        public void PlayHoverSound()
        {
            if (IsMenuMoving) return;
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
        #endregion

        #region Button Binding
        private void BindButtons()
        {
            BindClick(btnResume, ResumeGame);
            BindClick(btnGameOptions, OnClickGameOptions);
            BindClick(btnQuit, OnClickQuitToMainMenu);

            BindClick(turnPrevButton, OnTurnTypePrev);
            BindClick(turnNextButton, OnTurnTypeNext);
            BindClick(locomotionPrevButton, OnLocomotionTypePrev);
            BindClick(locomotionNextButton, OnLocomotionTypeNext);
            BindClick(saveAndExitButton, OnSaveAndExitSettings);
        }

        private static void BindClick(Button btn, UnityEngine.Events.UnityAction action)
        {
            if (btn == null || action == null) return;
            btn.onClick.RemoveListener(action);
            btn.onClick.AddListener(action);
        }
        #endregion
    }
}
