using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace ProspectorJourney.Room
{
    public class RoomInteractionTestHelper : MonoBehaviour
    {
        [Header("Configuration")]
        public string currentPropName = "Gold_Pan";
        public bool testRightHand = true;
        public bool autoGrabOnStart = false;
        public bool startTwoHanded = false;

        [Header("References")]
        public XRInteractionManager interactionManager;
        public NearFarInteractor rightNearFar;
        public NearFarInteractor leftNearFar;
        public Animator rightHandAnimator;
        public Animator leftHandAnimator;
        public Camera mainCamera;

        private XRGrabInteractable m_CurrentlyGrabbed;

        private void Awake()
        {
            if (interactionManager == null)
                interactionManager = FindFirstObjectByType<XRInteractionManager>();

            if (mainCamera == null)
                mainCamera = Camera.main;

            FindInteractors();
        }

        public void FindInteractors()
        {
            if (rightNearFar == null)
            {
                var rightController = GameObject.Find("Right Controller");
                if (rightController != null)
                {
                    rightNearFar = rightController.GetComponentInChildren<NearFarInteractor>(true);
                    rightHandAnimator = rightController.GetComponentInChildren<Animator>(true);
                }
            }

            if (leftNearFar == null)
            {
                var leftController = GameObject.Find("Left Controller");
                if (leftController != null)
                {
                    leftNearFar = leftController.GetComponentInChildren<NearFarInteractor>(true);
                    leftHandAnimator = leftController.GetComponentInChildren<Animator>(true);
                }
            }
        }

        private IEnumerator Start()
        {
            // Wait several frames for XR Origin & simulator initialization
            for (int i = 0; i < 10; i++) yield return null;

            FindInteractors();
            if (autoGrabOnStart)
            {
                if (startTwoHanded)
                {
                    GrabTwoHanded(currentPropName);
                }
                else
                {
                    GrabProp(currentPropName);
                }
            }
        }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.digit1Key.wasPressedThisFrame) GrabProp("tool-pickaxe");
            if (kb.digit2Key.wasPressedThisFrame) GrabProp("tool-shovel");
            if (kb.digit3Key.wasPressedThisFrame) GrabTwoHanded("Gold_Pan");
            if (kb.digit4Key.wasPressedThisFrame) GrabProp("Gold_Bar");
            if (kb.digit5Key.wasPressedThisFrame) GrabProp("Gold_Nugget_Large");
            if (kb.digit6Key.wasPressedThisFrame) GrabProp("Gold_Pouch");
            if (kb.digit7Key.wasPressedThisFrame) GrabProp("tool-hammer");
            if (kb.digit8Key.wasPressedThisFrame) GrabProp("tool-axe");
            if (kb.uKey.wasPressedThisFrame) GrabTwoHanded("tool-pickaxe");
            if (kb.iKey.wasPressedThisFrame) GrabTwoHanded("tool-shovel");
            if (kb.oKey.wasPressedThisFrame) GrabTwoHanded("tool-axe");
            if (kb.digit0Key.wasPressedThisFrame) DropCurrent();

            // Interactive test controls for two-handed manipulation
            if (m_CurrentlyGrabbed != null && m_CurrentlyGrabbed.interactorsSelecting.Count > 1)
            {
                if (kb.jKey.isPressed && leftNearFar != null && leftNearFar.transform.parent != null)
                {
                    leftNearFar.transform.parent.position += mainCamera.transform.TransformDirection(Vector3.left) * (0.3f * Time.deltaTime);
                }
                if (kb.lKey.isPressed && leftNearFar != null && leftNearFar.transform.parent != null)
                {
                    leftNearFar.transform.parent.position += mainCamera.transform.TransformDirection(Vector3.right) * (0.3f * Time.deltaTime);
                }
                if (kb.kKey.isPressed && leftNearFar != null && leftNearFar.transform.parent != null)
                {
                    leftNearFar.transform.parent.position += mainCamera.transform.TransformDirection(Vector3.up) * (0.3f * Time.deltaTime);
                }
            }
        }

        public void GrabTwoHanded(string propName)
        {
            FindInteractors();
            DropCurrent();

            GameObject propGo = GameObject.Find(propName);
            if (propGo == null)
            {
                Debug.LogWarning($"[TestHelper] Prop not found: {propName}");
                return;
            }

            XRGrabInteractable grab = propGo.GetComponent<XRGrabInteractable>();
            if (grab == null || interactionManager == null || rightNearFar == null || leftNearFar == null)
            {
                Debug.LogError("[TestHelper] Interactor or InteractionManager missing!");
                return;
            }

            bool isPan = propName.ToLowerInvariant().Contains("pan");

            if (isPan)
            {
                // Position Right Controller at right rim
                if (mainCamera != null && rightNearFar.transform.parent != null)
                {
                    Transform controllerTr = rightNearFar.transform.parent;
                    Vector3 offset = new Vector3(0.16f, -0.15f, 0.55f);
                    controllerTr.position = mainCamera.transform.TransformPoint(offset);
                    controllerTr.rotation = mainCamera.transform.rotation * Quaternion.Euler(15f, -20f, 0f);
                }

                // Position Left Controller at left rim
                if (mainCamera != null && leftNearFar.transform.parent != null)
                {
                    Transform leftTr = leftNearFar.transform.parent;
                    Vector3 offset = new Vector3(-0.16f, -0.15f, 0.55f);
                    leftTr.position = mainCamera.transform.TransformPoint(offset);
                    leftTr.rotation = mainCamera.transform.rotation * Quaternion.Euler(15f, 20f, 0f);
                }
            }
            else
            {
                // For long tools (pickaxe, shovel, axe):
                // Right hand at rear grip, Left hand forward on shaft
                if (mainCamera != null && rightNearFar.transform.parent != null)
                {
                    Transform controllerTr = rightNearFar.transform.parent;
                    Vector3 offset = new Vector3(0.08f, -0.22f, 0.50f);
                    controllerTr.position = mainCamera.transform.TransformPoint(offset);
                    controllerTr.rotation = mainCamera.transform.rotation * Quaternion.Euler(15f, -10f, 0f);
                }

                if (mainCamera != null && leftNearFar.transform.parent != null)
                {
                    Transform leftTr = leftNearFar.transform.parent;
                    Vector3 offset = new Vector3(0.06f, -0.06f, 0.56f);
                    leftTr.position = mainCamera.transform.TransformPoint(offset);
                    leftTr.rotation = mainCamera.transform.rotation * Quaternion.Euler(15f, 10f, 0f);
                }
            }

            // 1. Right Hand grabs prop (first hand)
            interactionManager.SelectEnter((IXRSelectInteractor)rightNearFar, (IXRSelectInteractable)grab);
            m_CurrentlyGrabbed = grab;
            currentPropName = propName;

            if (rightHandAnimator != null)
            {
                rightHandAnimator.SetFloat("Grip", 1f);
            }

            // 2. Left Hand grabs prop (two-handed / secondary hand)
            interactionManager.SelectEnter((IXRSelectInteractor)leftNearFar, (IXRSelectInteractable)grab);

            if (leftHandAnimator != null)
            {
                leftHandAnimator.SetFloat("Grip", 1f);
            }

            Debug.Log($"[TestHelper] Successfully grabbed two-handed: {propName}, selecting count: {grab.interactorsSelecting.Count}");
        }

        public void GrabProp(string propName)
        {
            FindInteractors();
            DropCurrent();

            GameObject propGo = GameObject.Find(propName);
            if (propGo == null)
            {
                Debug.LogWarning($"[TestHelper] Prop not found: {propName}");
                return;
            }

            XRGrabInteractable grab = propGo.GetComponent<XRGrabInteractable>();
            if (grab == null)
            {
                Debug.LogWarning($"[TestHelper] No XRGrabInteractable on: {propName}");
                return;
            }

            NearFarInteractor interactor = testRightHand ? rightNearFar : leftNearFar;
            Animator animator = testRightHand ? rightHandAnimator : leftHandAnimator;

            if (interactor == null || interactionManager == null)
            {
                Debug.LogError("[TestHelper] Interactor or InteractionManager missing!");
                return;
            }

            // Position controller right in front of camera
            if (mainCamera != null && interactor.transform.parent != null)
            {
                Transform controllerTr = interactor.transform.parent;
                // Move controller into good viewing position: slightly right, down, forward
                Vector3 offset = testRightHand ? new Vector3(0.20f, -0.15f, 0.55f) : new Vector3(-0.20f, -0.15f, 0.55f);
                controllerTr.position = mainCamera.transform.TransformPoint(offset);
                // Rotate controller so palm faces slightly up/left
                controllerTr.rotation = mainCamera.transform.rotation * Quaternion.Euler(15f, testRightHand ? -25f : 25f, 0f);
            }

            // Also ensure grab interactable can be selected
            interactionManager.SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)grab);
            m_CurrentlyGrabbed = grab;
            currentPropName = propName;

            if (animator != null)
            {
                animator.SetFloat("Grip", 1f);
            }

            Debug.Log($"[TestHelper] Successfully grabbed: {propName}");
        }

        public void DropCurrent()
        {
            if (m_CurrentlyGrabbed != null && interactionManager != null)
            {
                if (rightNearFar != null && m_CurrentlyGrabbed.interactorsSelecting.Contains((IXRSelectInteractor)rightNearFar))
                {
                    interactionManager.SelectExit((IXRSelectInteractor)rightNearFar, (IXRSelectInteractable)m_CurrentlyGrabbed);
                }
                if (leftNearFar != null && m_CurrentlyGrabbed.interactorsSelecting.Contains((IXRSelectInteractor)leftNearFar))
                {
                    interactionManager.SelectExit((IXRSelectInteractor)leftNearFar, (IXRSelectInteractable)m_CurrentlyGrabbed);
                }
                m_CurrentlyGrabbed = null;
            }

            if (rightHandAnimator != null) rightHandAnimator.SetFloat("Grip", 0f);
            if (leftHandAnimator != null) leftHandAnimator.SetFloat("Grip", 0f);
        }
    }
}
