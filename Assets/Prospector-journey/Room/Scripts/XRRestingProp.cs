using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Attachment;

namespace ProspectorJourney.Room
{
    /// <summary>
    /// Handles realistic physical grabbing and resting stability for room props:
    /// 1. Direct Grab Only: Rejects ray/distance grab so tools must be physically grabbed by hand.
    /// 2. Hand-snapping: Ensures props snap accurately into the palm via dedicated AttachTransform.
    /// 3. Physics & Gravity: Freezes until grabbed; when released/dropped, dynamic gravity and throw physics take over.
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class XRRestingProp : MonoBehaviour, IXRSelectFilter, IXRHoverFilter
    {
        [Tooltip("If true, object remains frozen until grabbed by VR interactor.")]
        [SerializeField] private bool m_FreezeUntilGrabbed = true;

        [Tooltip("If true, requires hand proximity (Near interaction) and rejects ray/laser grab from afar.")]
        [SerializeField] private bool m_DirectGrabOnly = true;

        [Tooltip("Custom attach offset position inside the hand.")]
        [SerializeField] private Vector3 m_AttachLocalPosition = Vector3.zero;

        [Tooltip("Custom attach orientation angles inside the hand.")]
        [SerializeField] private Vector3 m_AttachLocalEulerAngles = Vector3.zero;

        [Tooltip("Distance in meters at which secondary hand releases grip if pulled away.")]
        [SerializeField] private float m_SecondaryBreakDistance = 0.85f;

        private Rigidbody m_Rigidbody;
        private XRGrabInteractable m_GrabInteractable;
        private Transform m_AttachPoint;
        private Transform m_AttachPrimary;
        private Transform m_AttachSecondary;

        public bool canProcess => true;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_GrabInteractable = GetComponent<XRGrabInteractable>();

            if (m_Rigidbody != null)
            {
                // Must be non-kinematic with gravity so XRI caches wasKinematic = false!
                m_Rigidbody.isKinematic = false;
                m_Rigidbody.useGravity = true;

                if (m_FreezeUntilGrabbed)
                {
                    m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                }
            }

            if (m_GrabInteractable != null)
            {
                m_GrabInteractable.throwOnDetach = true;
                m_GrabInteractable.forceGravityOnDetach = true;

                // Snap into hand instead of floating at ray intersection
                m_GrabInteractable.useDynamicAttach = false;
                m_GrabInteractable.matchAttachPosition = true;
                m_GrabInteractable.matchAttachRotation = true;
                m_GrabInteractable.snapToColliderVolume = false;
                m_GrabInteractable.attachEaseInTime = 0f;
                m_GrabInteractable.farAttachMode = InteractableFarAttachMode.Near;

                SetupAttachTransform();
            }
        }

        private void SetupAttachTransform()
        {
            Transform attach1 = transform.Find("attach1");
            Transform attach2 = transform.Find("attach2");

            if (attach1 != null)
            {
                m_AttachPrimary = attach1;
                m_AttachPoint = attach1;
                m_GrabInteractable.attachTransform = attach1;
            }
            else if (m_GrabInteractable.attachTransform != null)
            {
                m_AttachPrimary = m_GrabInteractable.attachTransform;
                m_AttachPoint = m_AttachPrimary;
            }
            else
            {
                Transform existing = transform.Find("Attach_Point");
                if (existing != null)
                {
                    m_AttachPoint = existing;
                }
                else
                {
                    GameObject attachGo = new GameObject("Attach_Point");
                    attachGo.transform.SetParent(transform, false);
                    m_AttachPoint = attachGo.transform;
                    ApplyDefaultAttachTransform(m_AttachPoint);
                }
                m_GrabInteractable.attachTransform = m_AttachPoint;
                m_AttachPrimary = m_AttachPoint;
            }

            if (attach2 != null)
            {
                m_AttachSecondary = attach2;
                m_GrabInteractable.secondaryAttachTransform = attach2;
            }
            else
            {
                m_AttachSecondary = m_GrabInteractable.secondaryAttachTransform;
            }
        }

        private void ApplyDefaultAttachTransform(Transform attach)
        {
            if (m_AttachLocalPosition != Vector3.zero || m_AttachLocalEulerAngles != Vector3.zero)
            {
                attach.localPosition = m_AttachLocalPosition;
                attach.localRotation = Quaternion.Euler(m_AttachLocalEulerAngles);
                return;
            }

            string n = gameObject.name.ToLowerInvariant();
            if (n.Contains("shovel"))
            {
                attach.localPosition = new Vector3(0f, 0.11f, 0f);
                attach.localRotation = Quaternion.Euler(60f, 0f, 0f);
            }
            else if (n.Contains("pickaxe"))
            {
                attach.localPosition = new Vector3(0f, 0.08f, 0f);
                attach.localRotation = Quaternion.Euler(-25f, -90f, 0f);
            }
            else if (n.Contains("axe"))
            {
                attach.localPosition = new Vector3(0.01f, 0.08f, 0f);
                attach.localRotation = Quaternion.Euler(-20f, -90f, 0f);
            }
            else if (n.Contains("hammer"))
            {
                attach.localPosition = new Vector3(0f, 0.05f, 0f);
                attach.localRotation = Quaternion.Euler(-15f, -90f, 0f);
            }
            else if (n.Contains("pan"))
            {
                attach.localPosition = new Vector3(0.20f, 0.05f, 0f);
                attach.localRotation = Quaternion.identity;
            }
            else if (n.Contains("bottle"))
            {
                attach.localPosition = new Vector3(0f, 0.07f, 0f);
                attach.localRotation = Quaternion.Euler(15f, 0f, 0f);
            }
            else if (n.Contains("bucket"))
            {
                attach.localPosition = new Vector3(0f, 0.18f, -0.10f);
                attach.localRotation = Quaternion.Euler(10f, 0f, 0f);
            }
            else if (n.Contains("bar"))
            {
                attach.localPosition = new Vector3(0f, 0.02f, 0f);
                attach.localRotation = Quaternion.identity;
            }
            else if (n.Contains("pouch"))
            {
                attach.localPosition = new Vector3(0f, 0.08f, 0f);
                attach.localRotation = Quaternion.identity;
            }
            else
            {
                attach.localPosition = Vector3.zero;
                attach.localRotation = Quaternion.identity;
            }
        }

        private bool m_FiltersRegistered = false;

        private void OnEnable()
        {
            if (m_GrabInteractable != null)
            {
                m_GrabInteractable.selectEntered.AddListener(OnSelectEntered);
                m_GrabInteractable.selectExited.AddListener(OnSelectExited);

                if (!m_FiltersRegistered)
                {
                    m_GrabInteractable.selectFilters.Add(this);
                    m_GrabInteractable.hoverFilters.Add(this);
                    m_FiltersRegistered = true;
                }
            }
        }

        private void OnDisable()
        {
            if (m_IsSecondaryHandSnapped)
            {
                ReleaseSecondaryHand();
            }

            if (m_GrabInteractable != null)
            {
                m_GrabInteractable.selectEntered.RemoveListener(OnSelectEntered);
                m_GrabInteractable.selectExited.RemoveListener(OnSelectExited);

                if (m_FiltersRegistered)
                {
                    m_GrabInteractable.selectFilters.Remove(this);
                    m_GrabInteractable.hoverFilters.Remove(this);
                    m_FiltersRegistered = false;
                }
            }
        }

        public bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
        {
            // Only filter far grab when attempting to initiate a new grab (prevent laser beam grab from distance)
            if (m_DirectGrabOnly && m_GrabInteractable != null && !m_GrabInteractable.interactorsSelecting.Contains(interactor) && interactor is NearFarInteractor nearFar)
            {
                if (nearFar.selectionRegion.Value == NearFarInteractor.Region.Far)
                    return false;
            }

            // Proximity & hand-aware attach routing on initial grab
            if (m_AttachPrimary != null && m_AttachSecondary != null && m_GrabInteractable != null)
            {
                if (m_GrabInteractable.interactorsSelecting.Count == 0)
                {
                    bool isPan = gameObject.name.ToLowerInvariant().Contains("pan");
                    if (isPan)
                    {
                        bool isLeft = IsLeftHand(interactor);
                        if (isLeft)
                        {
                            m_GrabInteractable.attachTransform = m_AttachSecondary;
                            m_GrabInteractable.secondaryAttachTransform = m_AttachPrimary;
                        }
                        else
                        {
                            m_GrabInteractable.attachTransform = m_AttachPrimary;
                            m_GrabInteractable.secondaryAttachTransform = m_AttachSecondary;
                        }
                    }
                    else
                    {
                        Vector3 interactorPos = interactor.transform.position;
                        float distPrimary = Vector3.Distance(interactorPos, m_AttachPrimary.position);
                        float distSecondary = Vector3.Distance(interactorPos, m_AttachSecondary.position);

                        if (distSecondary < distPrimary && distSecondary < 0.20f)
                        {
                            m_GrabInteractable.attachTransform = m_AttachSecondary;
                            m_GrabInteractable.secondaryAttachTransform = m_AttachPrimary;
                        }
                        else
                        {
                            m_GrabInteractable.attachTransform = m_AttachPrimary;
                            m_GrabInteractable.secondaryAttachTransform = m_AttachSecondary;
                        }
                    }
                }
            }

            return true;
        }

        public bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
        {
            if (m_DirectGrabOnly && interactor is NearFarInteractor nearFar)
            {
                if (nearFar.selectionRegion.Value == NearFarInteractor.Region.Far)
                    return false;
            }
            return true;
        }

        private IXRInteractor m_PrimaryInteractor;
        private Animator m_PrimaryAnimator;

        private IXRInteractor m_SecondaryInteractor;
        private Transform m_SecondaryHandModel;
        private Vector3 m_SavedSecondaryLocalPos;
        private Quaternion m_SavedSecondaryLocalRot;
        private bool m_IsSecondaryHandSnapped = false;
        private Animator m_SecondaryAnimator;

        private bool IsLeftHand(IXRInteractor interactor)
        {
            if (interactor == null) return false;
            string n = interactor.transform.name;
            string pn = interactor.transform.parent != null ? interactor.transform.parent.name : "";
            return n.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   pn.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.constraints = RigidbodyConstraints.None;
            }

            if (m_GrabInteractable != null)
            {
                if (m_GrabInteractable.interactorsSelecting.Count == 1)
                {
                    m_PrimaryInteractor = args.interactorObject;
                    Transform controller = m_PrimaryInteractor.transform.parent != null ? m_PrimaryInteractor.transform.parent : m_PrimaryInteractor.transform;
                    m_PrimaryAnimator = controller.GetComponentInChildren<Animator>();
                }
                else if (m_GrabInteractable.interactorsSelecting.Count > 1)
                {
                    SnapSecondaryHand(args.interactorObject);
                }
            }
        }

        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactorObject == m_SecondaryInteractor)
            {
                ReleaseSecondaryHand();
            }

            if (args.interactorObject == m_PrimaryInteractor)
            {
                if (m_PrimaryAnimator != null)
                {
                    m_PrimaryAnimator.SetFloat("Grip", 0f);
                }
                m_PrimaryAnimator = null;
                m_PrimaryInteractor = null;

                // If secondary interactor is still selecting, promote it to primary
                if (m_GrabInteractable != null && m_GrabInteractable.interactorsSelecting.Count > 0)
                {
                    m_PrimaryInteractor = m_GrabInteractable.interactorsSelecting[0];
                    Transform controller = m_PrimaryInteractor.transform.parent != null ? m_PrimaryInteractor.transform.parent : m_PrimaryInteractor.transform;
                    m_PrimaryAnimator = controller.GetComponentInChildren<Animator>();
                    if (m_PrimaryAnimator != null) m_PrimaryAnimator.SetFloat("Grip", 1.0f);
                    ReleaseSecondaryHand();
                }
            }

            if (m_Rigidbody != null && (m_GrabInteractable == null || m_GrabInteractable.interactorsSelecting.Count == 0))
            {
                ReleaseSecondaryHand();
                if (m_PrimaryAnimator != null) m_PrimaryAnimator.SetFloat("Grip", 0f);
                m_PrimaryAnimator = null;
                m_PrimaryInteractor = null;
                m_Rigidbody.constraints = RigidbodyConstraints.None;
                m_Rigidbody.isKinematic = false;
                m_Rigidbody.useGravity = true;

                if (m_AttachPrimary != null && m_AttachSecondary != null)
                {
                    m_GrabInteractable.attachTransform = m_AttachPrimary;
                    m_GrabInteractable.secondaryAttachTransform = m_AttachSecondary;
                }
            }
        }

        private int m_TwoHandedFrames = 0;

        private void SnapSecondaryHand(IXRInteractor secInteractor)
        {
            if (secInteractor == null) return;
            m_SecondaryInteractor = secInteractor;
            m_TwoHandedFrames = 0;

            bool secIsLeft = IsLeftHand(secInteractor);
            Transform controller = secInteractor.transform.parent != null ? secInteractor.transform.parent : secInteractor.transform;

            m_SecondaryHandModel = controller.Find(secIsLeft ? "Left Hand Model" : "Right Hand Model");
            if (m_SecondaryHandModel == null)
            {
                var animOnCtrl = controller.GetComponentInChildren<Animator>();
                if (animOnCtrl != null) m_SecondaryHandModel = animOnCtrl.transform;
            }

            if (m_SecondaryHandModel != null)
            {
                m_SavedSecondaryLocalPos = m_SecondaryHandModel.localPosition;
                m_SavedSecondaryLocalRot = m_SecondaryHandModel.localRotation;
                m_SecondaryAnimator = m_SecondaryHandModel.GetComponent<Animator>();
            }

            if (m_SecondaryAnimator == null)
            {
                m_SecondaryAnimator = controller.GetComponentInChildren<Animator>();
            }

            if (m_SecondaryAnimator != null)
            {
                m_SecondaryAnimator.SetFloat("Grip", 1.0f);
            }

            m_IsSecondaryHandSnapped = true;
            Application.onBeforeRender += OnBeforeRenderCallback;
        }

        private void ReleaseSecondaryHand()
        {
            Application.onBeforeRender -= OnBeforeRenderCallback;

            if (m_SecondaryHandModel != null)
            {
                m_SecondaryHandModel.localPosition = m_SavedSecondaryLocalPos;
                m_SecondaryHandModel.localRotation = m_SavedSecondaryLocalRot;
            }

            if (m_SecondaryAnimator != null)
            {
                m_SecondaryAnimator.SetFloat("Grip", 0f);
            }

            m_IsSecondaryHandSnapped = false;
            m_SecondaryHandModel = null;
            m_SecondaryAnimator = null;
            m_SecondaryInteractor = null;
            m_TwoHandedFrames = 0;
        }

        private void OnBeforeRenderCallback()
        {
            ApplySecondaryHandTransform();
        }

        private void ApplySecondaryHandTransform()
        {
            if (!m_IsSecondaryHandSnapped || m_SecondaryHandModel == null || m_GrabInteractable == null || m_GrabInteractable.interactorsSelecting.Count <= 1)
                return;

            Transform targetSecAttach = (m_GrabInteractable.attachTransform == m_AttachPrimary) ? m_AttachSecondary : m_AttachPrimary;
            if (targetSecAttach == null) targetSecAttach = m_AttachSecondary ?? transform;

            bool secIsLeft = IsLeftHand(m_SecondaryInteractor);
            m_SecondaryHandModel.position = targetSecAttach.position;
            m_SecondaryHandModel.rotation = targetSecAttach.rotation * Quaternion.Euler(0f, 0f, secIsLeft ? 90f : 270f);
        }

        private void LateUpdate()
        {
            if (m_GrabInteractable == null) return;

            if (m_GrabInteractable.interactorsSelecting.Count > 0)
            {
                if (m_PrimaryAnimator != null)
                {
                    m_PrimaryAnimator.SetFloat("Grip", 1.0f);
                }
            }

            if (m_GrabInteractable.interactorsSelecting.Count > 1)
            {
                if (!m_IsSecondaryHandSnapped)
                {
                    SnapSecondaryHand(m_GrabInteractable.interactorsSelecting[1]);
                }
                UpdateSecondaryHandPose();
            }
            else if (m_IsSecondaryHandSnapped)
            {
                ReleaseSecondaryHand();
            }
        }

        private void UpdateSecondaryHandPose()
        {
            if (!m_IsSecondaryHandSnapped || m_SecondaryInteractor == null) return;

            m_TwoHandedFrames++;

            if (m_SecondaryAnimator != null)
            {
                m_SecondaryAnimator.SetFloat("Grip", 1.0f);
            }

            ApplySecondaryHandTransform();

            // Optional breakaway: only trigger if physical controller is pulled excessively far from the tool handle (> 0.75m)
            if (m_SecondaryBreakDistance > 0f)
            {
                Transform targetSecAttach = (m_GrabInteractable.attachTransform == m_AttachPrimary) ? m_AttachSecondary : m_AttachPrimary;
                if (targetSecAttach == null) targetSecAttach = m_AttachSecondary ?? transform;

                float controllerDist = Vector3.Distance(m_SecondaryInteractor.transform.position, targetSecAttach.position);
                float maxAllowed = Mathf.Max(m_SecondaryBreakDistance, 0.75f);
                if (m_TwoHandedFrames > 15 && controllerDist > maxAllowed)
                {
                    if (m_GrabInteractable.interactionManager != null && m_SecondaryInteractor is IXRSelectInteractor secSelect)
                    {
                        m_GrabInteractable.interactionManager.SelectExit(secSelect, m_GrabInteractable);
                    }
                    ReleaseSecondaryHand();
                }
            }
        }

        private void OnDestroy()
        {
            ReleaseSecondaryHand();
        }
    }
}
