using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ProspectorJourney.Room
{
    [System.Serializable]
    public class InventorySlotData
    {
        public bool isOccupied;
        public string itemName;
        public string itemDisplayName;
        public GameObject itemGameObject;
        public XRGrabInteractable grabInteractable;
    }

    public class ProspectorInventory : MonoBehaviour
    {
        public static ProspectorInventory Instance { get; private set; }

        [Header("Inventory Setup")]
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private int maxSlots = 4;
        [SerializeField] private Transform storedItemsContainer;

        [Header("VR Lazy Follow Settings (Floating Logic like MainMenu)")]
        [Tooltip("Enable smooth VR follow that stays at eye height and respects horizontal gaze.")]
        [SerializeField] private bool enableLazyFollow = true;
        [Tooltip("Reference to the VR camera. Defaults to Camera.main.")]
        [SerializeField] private Transform targetCamera;
        [Tooltip("Distance from the camera in meters.")]
        [SerializeField] private float followDistance = 0.85f;
        [Tooltip("Height offset relative to camera eye level.")]
        [SerializeField] private float heightOffset = -0.12f;
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

        [Header("XR Interactors")]
        [SerializeField] private XRInteractionManager interactionManager;
        [SerializeField] private NearFarInteractor rightNearFar;
        [SerializeField] private NearFarInteractor leftNearFar;

        [Header("UI References")]
        [SerializeField] private Button[] slotButtons;
        [SerializeField] private TextMeshProUGUI[] slotNameTexts;
        [SerializeField] private TextMeshProUGUI[] slotActionTexts;
        [SerializeField] private Image[] slotBackgrounds;
        [SerializeField] private Button storeHeldButton;
        [SerializeField] private TextMeshProUGUI storeHeldButtonText;
        [SerializeField] private Button closeButton;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Toast Notification (Direct Store Feedback)")]
        [SerializeField] private GameObject toastPanel;
        [SerializeField] private TextMeshProUGUI toastText;

        [Header("Thematic Colors")]
        [SerializeField] private Color emptySlotBg = new Color(0.12f, 0.08f, 0.05f, 0.95f);     // Dark aged parchment/leather
        [SerializeField] private Color occupiedSlotBg = new Color(0.24f, 0.15f, 0.09f, 0.95f);  // Warm rustic leather
        [SerializeField] private Color goldAccentColor = new Color(0.85f, 0.65f, 0.25f, 1.0f);  // Burnished gold
        [SerializeField] private Color emptyTextColor = new Color(0.55f, 0.50f, 0.45f, 0.7f);   // Muted stone gray

        [Header("Audio SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private AudioClip closeClip;
        [SerializeField] private AudioClip storeClip;
        [SerializeField] private AudioClip retrieveClip;
        [SerializeField] private AudioClip errorClip;
        [SerializeField] private AudioClip hoverClip;
        [SerializeField] private AudioClip clickClip;

        private InventorySlotData[] m_Slots;
        private Camera m_MainCamera;
        private bool m_IsOpen = false;

        public bool IsOpen => m_IsOpen;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            if (interactionManager == null)
                interactionManager = FindFirstObjectByType<XRInteractionManager>();

            if (storedItemsContainer == null)
            {
                var existing = transform.Find("[Stored_Items_Container]") ?? transform.Find("Stored_Items_Container");
                if (existing != null)
                {
                    storedItemsContainer = existing;
                }
                else
                {
                    var containerGo = new GameObject("[Stored_Items_Container]");
                    containerGo.transform.SetParent(transform);
                    containerGo.SetActive(false);
                    storedItemsContainer = containerGo.transform;
                }
            }

            if (toastPanel == null)
            {
                var tp = transform.Find("ToastNotification") ?? transform.Find("ToastPanel");
                if (tp == null)
                {
                    var toastGo = GameObject.Find("ToastNotification");
                    if (toastGo != null) tp = toastGo.transform;
                }
                if (tp != null)
                {
                    toastPanel = tp.gameObject;
                    if (toastText == null)
                        toastText = toastPanel.GetComponentInChildren<TextMeshProUGUI>(true);
                }
            }

            m_MainCamera = Camera.main;
            InitializeSlots();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public Transform StoredItemsContainer => storedItemsContainer;
        public InventorySlotData[] Slots => m_Slots;

        private void Start()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            FindInteractors();
            SetupUIEvents();
            InitializePreStoredItems();
            UpdateUI();

            // Start closed
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            m_IsOpen = false;
        }

        public void InitializePreStoredItems()
        {
            if (storedItemsContainer == null)
            {
                var existing = transform.Find("[Stored_Items_Container]") ?? transform.Find("Stored_Items_Container");
                if (existing != null)
                {
                    storedItemsContainer = existing;
                }
            }

            if (storedItemsContainer == null || m_Slots == null) return;

            int slotIdx = 0;
            int childCount = storedItemsContainer.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = storedItemsContainer.GetChild(i);
                if (child == null) continue;

                // Check if already registered in any slot
                bool alreadyRegistered = false;
                for (int s = 0; s < maxSlots; s++)
                {
                    if (m_Slots[s] != null && m_Slots[s].isOccupied && m_Slots[s].itemGameObject == child.gameObject)
                    {
                        alreadyRegistered = true;
                        break;
                    }
                }
                if (alreadyRegistered) continue;

                // Find next free slot
                while (slotIdx < maxSlots && m_Slots[slotIdx].isOccupied)
                {
                    slotIdx++;
                }
                if (slotIdx >= maxSlots) break;

                GameObject itemGo = child.gameObject;
                itemGo.SetActive(false);

                var grab = itemGo.GetComponent<XRGrabInteractable>() ?? itemGo.GetComponentInChildren<XRGrabInteractable>(true);
                var rb = itemGo.GetComponent<Rigidbody>() ?? itemGo.GetComponentInChildren<Rigidbody>(true);
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                m_Slots[slotIdx].isOccupied = true;
                m_Slots[slotIdx].itemName = itemGo.name;
                m_Slots[slotIdx].itemDisplayName = FormatDisplayName(itemGo.name);
                m_Slots[slotIdx].itemGameObject = itemGo;
                m_Slots[slotIdx].grabInteractable = grab;

                slotIdx++;
            }

            UpdateUI();
        }

        private void InitializeSlots()
        {
            m_Slots = new InventorySlotData[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                m_Slots[i] = new InventorySlotData();
            }
        }

        public void FindInteractors()
        {
            if (rightNearFar == null)
            {
                var rc = GameObject.Find("Right Controller");
                if (rc != null) rightNearFar = rc.GetComponentInChildren<NearFarInteractor>(true);
            }
            if (leftNearFar == null)
            {
                var lc = GameObject.Find("Left Controller");
                if (lc != null) leftNearFar = lc.GetComponentInChildren<NearFarInteractor>(true);
            }
        }

        private void SetupUIEvents()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
            }

            if (storeHeldButton != null)
            {
                storeHeldButton.onClick.AddListener(() => StoreHeldItem());
            }

            if (slotButtons != null)
            {
                for (int i = 0; i < slotButtons.Length; i++)
                {
                    int index = i;
                    slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
                }
            }
        }

        private bool m_PrevLeftSecondary = false;
        private bool m_PrevRightSecondary = false;
        private bool m_PrevLeftPrimary = false;
        private bool m_PrevRightPrimary = false;
        private Coroutine m_ToastCoroutine;

        private DoorSceneTransition m_CachedDoor;

        private bool IsPlayerNearDoor()
        {
            if (m_CachedDoor == null)
                m_CachedDoor = FindFirstObjectByType<DoorSceneTransition>();
            return m_CachedDoor != null && m_CachedDoor.IsPlayerNearby;
        }

        public void PlayErrorSound()
        {
            PlaySound(errorClip);
        }

        private void CheckVRInput()
        {
            bool nearDoor = IsPlayerNearDoor();

            // Left Controller (X = Primary, Y = Secondary)
            var leftDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);
            if (leftDevice.isValid)
            {
                if (leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool leftPri))
                {
                    if (leftPri && !m_PrevLeftPrimary && !nearDoor) QuickStoreHeldItem(leftNearFar);
                    m_PrevLeftPrimary = leftPri;
                }

                if (leftDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool leftSec))
                {
                    if (leftSec && !m_PrevLeftSecondary)
                    {
                        if (IsHandHolding(leftNearFar)) QuickStoreHeldItem(leftNearFar);
                        else ToggleInventory();
                    }
                    m_PrevLeftSecondary = leftSec;
                }
            }

            // Right Controller (A = Primary, B = Secondary)
            var rightDevice = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand);
            if (rightDevice.isValid)
            {
                if (rightDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool rightPri))
                {
                    if (rightPri && !m_PrevRightPrimary && !nearDoor) QuickStoreHeldItem(rightNearFar);
                    m_PrevRightPrimary = rightPri;
                }

                if (rightDevice.TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out bool rightSec))
                {
                    if (rightSec && !m_PrevRightSecondary)
                    {
                        if (IsHandHolding(rightNearFar)) QuickStoreHeldItem(rightNearFar);
                        else ToggleInventory();
                    }
                    m_PrevRightSecondary = rightSec;
                }
            }
        }

        private void Update()
        {
            if (ProspectorJourney.UI.PauseMenuController.IsGamePaused)
            {
                return;
            }

            var kb = Keyboard.current;
            if (kb != null)
            {
                // One-button direct store without opening inventory!
                // 'C' or 'X' immediately sends the held tool straight into the bag
                if (kb.cKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame)
                {
                    QuickStoreHeldItem();
                }

                // 'I' or 'Tab' toggles the satchel UI to view or retrieve items
                if (kb.iKey.wasPressedThisFrame || kb.tabKey.wasPressedThisFrame)
                {
                    ToggleInventory();
                }
            }

            CheckVRInput();

            // Update store button label dynamically while open
            if (m_IsOpen)
            {
                UpdateStoreButtonState();
            }
        }

        public bool IsHandHolding(NearFarInteractor interactor)
        {
            if (interactor == null) return false;
            if (interactor.hasSelection)
            {
                foreach (var sel in interactor.interactablesSelected)
                {
                    if (sel is XRGrabInteractable) return true;
                }
            }
            return false;
        }

        public bool QuickStoreHeldItem(NearFarInteractor preferredHand = null)
        {
            FindInteractors();
            XRGrabInteractable grabToStore = null;

            if (preferredHand != null && preferredHand.hasSelection)
            {
                foreach (var sel in preferredHand.interactablesSelected)
                {
                    if (sel is XRGrabInteractable grab)
                    {
                        grabToStore = grab;
                        break;
                    }
                }
            }

            if (grabToStore == null)
            {
                grabToStore = GetHeldInteractable(out _);
            }

            if (grabToStore == null)
            {
                PlaySound(errorClip);
                ShowToast("Tidak ada alat yang sedang dipegang!");
                return false;
            }

            int emptySlot = GetFirstEmptySlot();
            if (emptySlot == -1)
            {
                PlaySound(errorClip);
                ShowToast("Tas Penuh! Kosongkan slot dulu.");
                return false;
            }

            string cleanName = FormatDisplayName(grabToStore.gameObject.name);
            bool success = StoreItemInternal(grabToStore, emptySlot);
            if (success)
            {
                ShowToast($"[+] {cleanName} masuk ke Tas");
            }
            return success;
        }

        public void ShowToast(string message)
        {
            if (toastPanel != null && toastText != null)
            {
                if (m_ToastCoroutine != null) StopCoroutine(m_ToastCoroutine);
                m_ToastCoroutine = StartCoroutine(ToastRoutine(message));
            }
            else
            {
                SetStatus(message);
            }
        }

        private IEnumerator ToastRoutine(string message)
        {
            if (m_MainCamera == null) m_MainCamera = Camera.main;
            if (m_MainCamera != null)
            {
                Vector3 fwd = m_MainCamera.transform.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
                fwd.Normalize();

                toastPanel.transform.position = m_MainCamera.transform.position + fwd * 0.75f + Vector3.up * -0.15f;
                toastPanel.transform.rotation = Quaternion.LookRotation(fwd);
            }

            toastText.text = message;
            toastPanel.SetActive(true);
            yield return new WaitForSeconds(1.8f);
            toastPanel.SetActive(false);
            m_ToastCoroutine = null;
        }

        public void ToggleInventory()
        {
            if (m_IsOpen) CloseInventory();
            else OpenInventory();
        }

        public void SnapToFront()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            if (targetCamera == null || inventoryPanel == null) return;

            Vector3 camFwd = targetCamera.forward;
            camFwd.y = 0f;
            currentHorizontalFacing = camFwd.sqrMagnitude > 0.001f ? camFwd.normalized : Vector3.forward;

            Vector3 snapPos = targetCamera.position + currentHorizontalFacing * followDistance;
            snapPos.y = targetCamera.position.y + heightOffset;
            inventoryPanel.transform.position = snapPos;
            inventoryPanel.transform.rotation = ComputeTargetRotation(snapPos, currentHorizontalFacing);
            lastPosition = snapPos;
            isCatchingUp = false;
            IsMenuMoving = false;
        }

        public void OpenInventory()
        {
            if (targetCamera == null)
            {
                var cam = Camera.main;
                if (cam != null) targetCamera = cam.transform;
            }

            if (inventoryPanel != null)
            {
                var canvas = inventoryPanel.GetComponent<Canvas>();
                if (canvas != null && canvas.worldCamera == null && Camera.main != null)
                {
                    canvas.worldCamera = Camera.main;
                }

                SnapToFront();
                inventoryPanel.SetActive(true);
            }

            m_IsOpen = true;
            PlaySound(openClip);
            UpdateUI();
        }

        public void CloseInventory()
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.SetActive(false);
            }
            m_IsOpen = false;
            IsMenuMoving = false;
            PlaySound(closeClip);
        }

        private void LateUpdate()
        {
            if (!m_IsOpen || !enableLazyFollow || inventoryPanel == null)
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
            float newY = Mathf.Lerp(inventoryPanel.transform.position.y, desiredY, heightSpeed * Time.deltaTime);
            targetPos.y = newY;

            inventoryPanel.transform.position = Vector3.Lerp(inventoryPanel.transform.position, targetPos, followSpeed * Time.deltaTime);

            // Target rotation
            Quaternion targetRot = ComputeTargetRotation(inventoryPanel.transform.position, currentHorizontalFacing);
            inventoryPanel.transform.rotation = Quaternion.Slerp(inventoryPanel.transform.rotation, targetRot, followSpeed * Time.deltaTime);

            // Determine if the menu is considered in motion
            float moveDelta = (inventoryPanel.transform.position - lastPosition).sqrMagnitude;
            lastPosition = inventoryPanel.transform.position;
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

        private void OnDisable()
        {
            IsMenuMoving = false;
        }

        private void OnSlotClicked(int index)
        {
            if (index < 0 || index >= maxSlots) return;

            if (m_Slots[index].isOccupied)
            {
                // Retrieve item
                RetrieveItem(index);
            }
            else
            {
                // Slot is empty -> Try to store held item into this slot
                StoreHeldItem(index);
            }
        }

        private XRGrabInteractable GetHeldInteractable(out NearFarInteractor holdingInteractor)
        {
            FindInteractors();
            holdingInteractor = null;

            // Check Right Hand first
            if (rightNearFar != null && rightNearFar.hasSelection)
            {
                foreach (var sel in rightNearFar.interactablesSelected)
                {
                    if (sel is XRGrabInteractable grab)
                    {
                        holdingInteractor = rightNearFar;
                        return grab;
                    }
                }
            }

            // Check Left Hand
            if (leftNearFar != null && leftNearFar.hasSelection)
            {
                foreach (var sel in leftNearFar.interactablesSelected)
                {
                    if (sel is XRGrabInteractable grab)
                    {
                        holdingInteractor = leftNearFar;
                        return grab;
                    }
                }
            }

            return null;
        }

        private void UpdateStoreButtonState()
        {
            if (storeHeldButton == null || storeHeldButtonText == null) return;

            var held = GetHeldInteractable(out var interactor);
            if (held != null)
            {
                string handName = interactor == rightNearFar ? "Kanan" : "Kiri";
                string cleanName = FormatDisplayName(held.gameObject.name);
                storeHeldButtonText.text = $"Simpan {cleanName} ({handName})";
                storeHeldButton.interactable = HasEmptySlot();
            }
            else
            {
                storeHeldButtonText.text = "Tangan Kosong (Pegang alat dulu)";
                storeHeldButton.interactable = false;
            }
        }

        public bool HasEmptySlot()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (!m_Slots[i].isOccupied) return true;
            }
            return false;
        }

        public bool HasItemInBagOrHand(string keyword)
        {
            if (string.IsNullOrEmpty(keyword)) return false;

            if (m_Slots != null)
            {
                for (int i = 0; i < m_Slots.Length; i++)
                {
                    if (m_Slots[i] != null && m_Slots[i].isOccupied)
                    {
                        if (!string.IsNullOrEmpty(m_Slots[i].itemName) && m_Slots[i].itemName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                        if (!string.IsNullOrEmpty(m_Slots[i].itemDisplayName) && m_Slots[i].itemDisplayName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }
            }

            FindInteractors();
            if (rightNearFar != null && rightNearFar.hasSelection)
            {
                foreach (var sel in rightNearFar.interactablesSelected)
                {
                    if (sel is Component comp)
                    {
                        if (comp.gameObject.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                        if (comp.transform.root != null && comp.transform.root.gameObject.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                    }
                }
            }

            if (leftNearFar != null && leftNearFar.hasSelection)
            {
                foreach (var sel in leftNearFar.interactablesSelected)
                {
                    if (sel is Component comp)
                    {
                        if (comp.gameObject.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                        if (comp.transform.root != null && comp.transform.root.gameObject.name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                    }
                }
            }

            return false;
        }

        private int GetFirstEmptySlot()
        {
            for (int i = 0; i < maxSlots; i++)
            {
                if (!m_Slots[i].isOccupied) return i;
            }
            return -1;
        }

        public bool StoreHeldItem(int targetSlot = -1)
        {
            var grab = GetHeldInteractable(out var interactor);
            if (grab == null)
            {
                SetStatus("Tidak ada alat yang sedang dipegang!");
                PlaySound(errorClip);
                return false;
            }

            int slotIndex = targetSlot;
            if (slotIndex < 0 || slotIndex >= maxSlots || m_Slots[slotIndex].isOccupied)
            {
                slotIndex = GetFirstEmptySlot();
            }

            if (slotIndex == -1)
            {
                SetStatus("Tas Penuh! Kosongkan slot terlebih dahulu.");
                PlaySound(errorClip);
                return false;
            }

            return StoreItemInternal(grab, slotIndex);
        }

        public bool StoreItemDirect(XRGrabInteractable grab)
        {
            if (grab == null) return false;
            int emptySlot = GetFirstEmptySlot();
            if (emptySlot == -1)
            {
                SetStatus("Tas Penuh!");
                PlaySound(errorClip);
                return false;
            }
            return StoreItemInternal(grab, emptySlot);
        }

        private bool StoreItemInternal(XRGrabInteractable grab, int slotIndex)
        {
            if (grab == null || slotIndex < 0 || slotIndex >= maxSlots) return false;

            // 1. Force drop from all interactors currently selecting it
            if (interactionManager != null && grab.interactorsSelecting.Count > 0)
            {
                var interactorsCopy = new List<IXRSelectInteractor>(grab.interactorsSelecting);
                foreach (var inter in interactorsCopy)
                {
                    interactionManager.SelectExit(inter, (IXRSelectInteractable)grab);
                }
            }

            // 2. Parent to storage container and deactivate physics
            GameObject itemGo = grab.gameObject;
            var rb = itemGo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            itemGo.transform.SetParent(storedItemsContainer);
            itemGo.SetActive(false);

            // 3. Register to slot
            m_Slots[slotIndex].isOccupied = true;
            m_Slots[slotIndex].itemName = itemGo.name;
            m_Slots[slotIndex].itemDisplayName = FormatDisplayName(itemGo.name);
            m_Slots[slotIndex].itemGameObject = itemGo;
            m_Slots[slotIndex].grabInteractable = grab;

            PlaySound(storeClip);
            SetStatus($"Berhasil menyimpan {m_Slots[slotIndex].itemDisplayName} ke Tas.");
            UpdateUI();
            return true;
        }

        public bool RetrieveItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return false;
            if (!m_Slots[slotIndex].isOccupied) return false;

            var slot = m_Slots[slotIndex];
            GameObject itemGo = slot.itemGameObject;
            XRGrabInteractable grab = slot.grabInteractable;

            if (itemGo == null)
            {
                slot.isOccupied = false;
                UpdateUI();
                return false;
            }

            if (grab == null)
            {
                grab = itemGo.GetComponent<XRGrabInteractable>() ?? itemGo.GetComponentInChildren<XRGrabInteractable>(true);
                slot.grabInteractable = grab;
            }

            // 1. Reactivate GameObject
            itemGo.transform.SetParent(null);
            itemGo.SetActive(true);

            // 2. Position near player's hand or in front of inventory slot
            FindInteractors();
            NearFarInteractor targetHand = null;
            if (rightNearFar != null && !rightNearFar.hasSelection) targetHand = rightNearFar;
            else if (leftNearFar != null && !leftNearFar.hasSelection) targetHand = leftNearFar;

            Vector3 spawnPos;
            Quaternion spawnRot;

            if (targetHand != null && targetHand.transform.parent != null)
            {
                spawnPos = targetHand.transform.parent.position + targetHand.transform.parent.forward * 0.10f;
                spawnRot = targetHand.transform.parent.rotation;
            }
            else if (inventoryPanel != null)
            {
                spawnPos = inventoryPanel.transform.position + inventoryPanel.transform.forward * -0.25f + Vector3.up * 0.05f;
                spawnRot = inventoryPanel.transform.rotation;
            }
            else if (targetCamera != null)
            {
                spawnPos = targetCamera.position + targetCamera.forward * 0.5f;
                spawnRot = targetCamera.rotation;
            }
            else
            {
                spawnPos = transform.position + transform.forward * 0.5f;
                spawnRot = transform.rotation;
            }

            itemGo.transform.position = spawnPos;
            itemGo.transform.rotation = spawnRot;

            var rb = itemGo.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            // 3. Attempt to auto-grab if hand is free
            if (grab != null && targetHand != null && interactionManager != null)
            {
                interactionManager.SelectEnter((IXRSelectInteractor)targetHand, (IXRSelectInteractable)grab);
            }

            string retrievedName = !string.IsNullOrEmpty(slot.itemDisplayName) ? slot.itemDisplayName : FormatDisplayName(slot.itemName);
            slot.isOccupied = false;
            slot.itemGameObject = null;
            slot.grabInteractable = null;
            slot.itemName = "";
            slot.itemDisplayName = "";

            PlaySound(retrieveClip);
            SetStatus($"Mengeluarkan {retrievedName} dari Tas.");
            ShowToast($"[-] {retrievedName} keluar dari Tas");
            UpdateUI();
            return true;
        }

        private void UpdateUI()
        {
            if (m_Slots == null) return;

            for (int i = 0; i < maxSlots; i++)
            {
                if (i >= slotButtons.Length) break;

                bool occupied = m_Slots[i].isOccupied;

                if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
                {
                    slotBackgrounds[i].color = occupied ? occupiedSlotBg : emptySlotBg;
                }

                if (slotNameTexts != null && i < slotNameTexts.Length && slotNameTexts[i] != null)
                {
                    if (occupied)
                    {
                        slotNameTexts[i].text = m_Slots[i].itemDisplayName;
                        slotNameTexts[i].color = goldAccentColor;
                    }
                    else
                    {
                        slotNameTexts[i].text = $"[ Slot {i + 1} Kosong ]";
                        slotNameTexts[i].color = emptyTextColor;
                    }
                }

                if (slotActionTexts != null && i < slotActionTexts.Length && slotActionTexts[i] != null)
                {
                    slotActionTexts[i].text = occupied ? "Klik untuk Ambil" : "Klik untuk Simpan";
                    slotActionTexts[i].color = occupied ? Color.white : emptyTextColor;
                }
            }

            UpdateStoreButtonState();
        }

        public void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        public static string FormatDisplayName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "Item";
            string s = rawName.Replace("(Clone)", "").Trim();

            if (s.IndexOf("pan", StringComparison.OrdinalIgnoreCase) >= 0) return "Gold Pan";
            if (s.IndexOf("pickaxe-upgraded", StringComparison.OrdinalIgnoreCase) >= 0) return "Beliung Emas";
            if (s.IndexOf("pickaxe", StringComparison.OrdinalIgnoreCase) >= 0) return "Beliung (Pickaxe)";
            if (s.IndexOf("shovel-upgraded", StringComparison.OrdinalIgnoreCase) >= 0) return "Sekop Baja";
            if (s.IndexOf("shovel", StringComparison.OrdinalIgnoreCase) >= 0) return "Sekop (Shovel)";
            if (s.IndexOf("axe", StringComparison.OrdinalIgnoreCase) >= 0) return "Kapak (Axe)";
            if (s.IndexOf("hammer", StringComparison.OrdinalIgnoreCase) >= 0) return "Palu (Hammer)";
            if (s.IndexOf("bar", StringComparison.OrdinalIgnoreCase) >= 0) return "Batangan Emas (Gold Bar)";
            if (s.IndexOf("pouch", StringComparison.OrdinalIgnoreCase) >= 0) return "Kantong Emas (Gold Pouch)";
            if (s.IndexOf("nugget_large", StringComparison.OrdinalIgnoreCase) >= 0) return "Bongkahan Emas Besar";
            if (s.IndexOf("nugget_medium", StringComparison.OrdinalIgnoreCase) >= 0) return "Bongkahan Emas Sedang";
            if (s.IndexOf("nugget_small", StringComparison.OrdinalIgnoreCase) >= 0) return "Bongkahan Emas Kecil";
            if (s.IndexOf("bucket", StringComparison.OrdinalIgnoreCase) >= 0) return "Ember (Bucket)";
            if (s.IndexOf("bottle", StringComparison.OrdinalIgnoreCase) >= 0) return "Botol Air (Bottle)";

            return s.Replace("tool-", "").Replace("Gold_", "Gold ");
        }
    }
}
