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
                m_GrabInteractable.attachEaseInTime = 0f;
                m_GrabInteractable.farAttachMode = InteractableFarAttachMode.Near;

                SetupAttachTransform();
            }
        }

        private void SetupAttachTransform()
        {
            m_AttachPrimary = m_GrabInteractable.attachTransform;
            m_AttachSecondary = m_GrabInteractable.secondaryAttachTransform ?? transform.Find("attach2");

            if (m_AttachPrimary != null)
            {
                m_AttachPoint = m_AttachPrimary;
                return;
            }

            // Find existing child or create new
            Transform existing = transform.Find("attach1") ?? transform.Find("Attach_Point");
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
                attach.localPosition = new Vector3(0f, 0.04f, -0.22f);
                attach.localRotation = Quaternion.Euler(15f, 0f, 0f);
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
                m_GrabInteractable.hoverEntered.AddListener(OnHoverEntered);
                m_GrabInteractable.firstSelectEntered.AddListener(OnGrabbed);
                m_GrabInteractable.lastSelectExited.AddListener(OnDropped);

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
            if (m_GrabInteractable != null)
            {
                m_GrabInteractable.hoverEntered.RemoveListener(OnHoverEntered);
                m_GrabInteractable.firstSelectEntered.RemoveListener(OnGrabbed);
                m_GrabInteractable.lastSelectExited.RemoveListener(OnDropped);

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
            if (m_DirectGrabOnly && interactor is NearFarInteractor nearFar)
            {
                if (nearFar.selectionRegion.Value == NearFarInteractor.Region.Far)
                    return false;
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

        private void OnHoverEntered(HoverEnterEventArgs args)
        {
            // If this interactable has two attach points (mid & bottom)
            if (m_AttachPrimary != null && m_AttachSecondary != null)
            {
                // Only dynamically route if not already held
                if (m_GrabInteractable.interactorsSelecting.Count == 0)
                {
                    bool isLeft = IsLeftHand(args.interactorObject);
                    if (isLeft)
                    {
                        // Left hand approaching first: Left gets bottom, Right will get mid
                        m_GrabInteractable.attachTransform = m_AttachSecondary;
                        m_GrabInteractable.secondaryAttachTransform = m_AttachPrimary;
                    }
                    else
                    {
                        // Right hand approaching first: Right gets mid, Left will get bottom
                        m_GrabInteractable.attachTransform = m_AttachPrimary;
                        m_GrabInteractable.secondaryAttachTransform = m_AttachSecondary;
                    }
                }
            }
        }

        private bool IsLeftHand(IXRInteractor interactor)
        {
            if (interactor == null) return false;
            string n = interactor.transform.name;
            string pn = interactor.transform.parent != null ? interactor.transform.parent.name : "";
            return n.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                   pn.IndexOf("Left", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.constraints = RigidbodyConstraints.None;
                // Do NOT force isKinematic = false or useGravity = true while held!
                // XRI keeps it kinematic during grab so it tracks smoothly with zero physics glitching.
            }
        }

        private void OnDropped(SelectExitEventArgs args)
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.constraints = RigidbodyConstraints.None;
                m_Rigidbody.isKinematic = false;
                m_Rigidbody.useGravity = true;
            }

            // Restore default attach transforms
            if (m_AttachPrimary != null && m_AttachSecondary != null)
            {
                m_GrabInteractable.attachTransform = m_AttachPrimary;
                m_GrabInteractable.secondaryAttachTransform = m_AttachSecondary;
            }
        }
    }
}
