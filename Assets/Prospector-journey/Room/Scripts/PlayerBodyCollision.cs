using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace ProspectorJourney.Room
{
    /// <summary>
    /// Synchronizes the XR Origin CharacterController with the Main Camera (HMD)
    /// by counter-shifting Camera Offset in XZ and routing horizontal translation
    /// through CharacterController.Move, keeping head, body, and both VR hands
    /// moving at identical 1:1 speed and stopping together at walls/props.
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    [RequireComponent(typeof(CharacterController))]
    [DefaultExecutionOrder(20000)]
    public class PlayerBodyCollision : MonoBehaviour
    {
        [Header("Body Capsule Settings")]
        [SerializeField] private float bodyRadius = 0.24f;
        [SerializeField] private float stepOffset = 0.08f;
        [SerializeField] private float skinWidth = 0.025f;
        [SerializeField] private float minHeadHeight = 0.85f;
        [SerializeField] private float maxHeadHeight = 2.05f;

        private XROrigin m_XROrigin;
        private CharacterController m_CharacterController;
        private Transform m_CameraTransform;
        private Transform m_CameraOffsetTransform;
        private TrackedPoseDriver m_TrackedPoseDriver;

        private Vector3 m_PrevRawCamLocal;
        private Vector3 m_CurrentOffsetLocalPos;
        private float m_InitialOffsetY = 1.36144f;
        private float m_VerticalVelocity;
        private bool m_Initialized;
        private bool m_BeforeRenderRegistered;

        private void Awake()
        {
            m_XROrigin = GetComponent<XROrigin>();
            m_CharacterController = GetComponent<CharacterController>();

            if (m_XROrigin != null)
            {
                if (m_XROrigin.Camera != null)
                    m_CameraTransform = m_XROrigin.Camera.transform;
                if (m_XROrigin.CameraFloorOffsetObject != null)
                    m_CameraOffsetTransform = m_XROrigin.CameraFloorOffsetObject.transform;
            }

            if (m_CameraTransform == null && Camera.main != null)
                m_CameraTransform = Camera.main.transform;

            if (m_CameraTransform != null)
                m_TrackedPoseDriver = m_CameraTransform.GetComponent<TrackedPoseDriver>();

            if (m_CameraOffsetTransform == null && m_CameraTransform != null)
                m_CameraOffsetTransform = m_CameraTransform.parent;

            if (m_CameraOffsetTransform != null)
            {
                m_InitialOffsetY = m_CameraOffsetTransform.localPosition.y;
                m_CurrentOffsetLocalPos = new Vector3(0f, m_InitialOffsetY, 0f);
            }

            ConfigureCharacterController();
            EnsureFurnitureColliders();
            IgnoreGrabbableAndTeleportCollisions();
        }

        private void Start()
        {
            if (!m_BeforeRenderRegistered)
            {
                Application.onBeforeRender += OnBeforeRender;
                RenderPipelineManager.beginContextRendering += OnBeginContextRendering;
                m_BeforeRenderRegistered = true;
            }

            IgnoreGrabbableAndTeleportCollisions();

            m_PrevRawCamLocal = ReadRawHmdLocalPosition();
            m_Initialized = true;
        }

        private void OnDisable()
        {
            if (m_BeforeRenderRegistered)
            {
                Application.onBeforeRender -= OnBeforeRender;
                RenderPipelineManager.beginContextRendering -= OnBeginContextRendering;
                m_BeforeRenderRegistered = false;
            }
        }

        private Vector3 ReadRawHmdLocalPosition()
        {
            if (m_TrackedPoseDriver != null && m_TrackedPoseDriver.positionInput.action != null)
            {
                return m_TrackedPoseDriver.positionInput.action.ReadValue<Vector3>();
            }
            return m_CameraTransform != null ? m_CameraTransform.localPosition : Vector3.zero;
        }

        private void ConfigureCharacterController()
        {
            if (m_CharacterController == null) return;

            m_CharacterController.radius = bodyRadius;
            m_CharacterController.stepOffset = stepOffset;
            m_CharacterController.skinWidth = skinWidth;
            m_CharacterController.minMoveDistance = 0.0001f;
            m_CharacterController.slopeLimit = 45f;

            float h = Mathf.Clamp(m_InitialOffsetY, minHeadHeight, maxHeadHeight);
            m_CharacterController.height = h;
            m_CharacterController.center = new Vector3(0f, h * 0.5f + skinWidth, 0f);
        }

        private void EnsureFurnitureColliders()
        {
            var env = GameObject.Find("Environment_ProspectorCabin");
            if (env == null) return;

            var meshFilters = env.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in meshFilters)
            {
                if (mf == null || mf.sharedMesh == null) continue;
                var go = mf.gameObject;

                // Skip grabbable props, teleport anchors, rugs, and workbench (which uses Tabletop_Surface_Collider)
                if (go.GetComponentInParent<XRGrabInteractable>() != null) continue;
                if (go.GetComponentInParent<TeleportationAnchor>() != null) continue;
                if (go.name == "workbench" || go.name == "rugRound" || go.name.Contains("Visuals") || go.name.Contains("Anchor"))
                    continue;

                if (go.GetComponent<Collider>() == null)
                {
                    var mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
            }
        }

        private void IgnoreGrabbableAndTeleportCollisions()
        {
            if (m_CharacterController == null) return;

            // Ignore handheld grabbable props so holding an object never pushes the player's own body
            var grabbables = FindObjectsByType<XRGrabInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var grab in grabbables)
            {
                if (grab == null) continue;
                foreach (var col in grab.GetComponentsInChildren<Collider>(true))
                {
                    if (col != null && col != m_CharacterController)
                        Physics.IgnoreCollision(m_CharacterController, col, true);
                }
            }

            // Ignore flat floor teleport anchors and rugs so feet don't snag on floor decals
            var anchors = FindObjectsByType<TeleportationAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var anchor in anchors)
            {
                if (anchor == null) continue;
                foreach (var col in anchor.GetComponentsInChildren<Collider>(true))
                {
                    if (col != null && col != m_CharacterController)
                        Physics.IgnoreCollision(m_CharacterController, col, true);
                }
            }

            var rug = GameObject.Find("rugRound");
            if (rug != null)
            {
                foreach (var col in rug.GetComponentsInChildren<Collider>(true))
                {
                    if (col != null && col != m_CharacterController)
                        Physics.IgnoreCollision(m_CharacterController, col, true);
                }
            }
        }

        private void LateUpdate()
        {
            if (m_CharacterController == null || !m_CharacterController.enabled || m_CameraTransform == null || m_CameraOffsetTransform == null)
                return;

            Vector3 rawCamLocal = ReadRawHmdLocalPosition();
            if (!m_Initialized)
            {
                m_PrevRawCamLocal = rawCamLocal;
                m_Initialized = true;
            }

            Vector3 localDelta = rawCamLocal - m_PrevRawCamLocal;
            m_PrevRawCamLocal = rawCamLocal;

            float headHeight = m_InitialOffsetY;
            float clampedOffsetY = headHeight - rawCamLocal.y;

            // Counter-shift Camera Offset in XYZ so Main Camera AND both Left/Right Controllers
            // stay locked to the XR Origin CharacterController at 1:1 speed and pressing E/Q
            // in XR Interaction Simulator does not shift the display height up or down
            m_CurrentOffsetLocalPos = new Vector3(-rawCamLocal.x, clampedOffsetY, -rawCamLocal.z);
            m_CameraOffsetTransform.localPosition = m_CurrentOffsetLocalPos;

            m_CharacterController.height = headHeight;
            m_CharacterController.center = new Vector3(0f, headHeight * 0.5f + skinWidth, 0f);

            // Convert HMD horizontal movement delta into CharacterController.Move on XR Origin
            Vector3 worldMove = Vector3.zero;
            Vector3 localXZDelta = new Vector3(localDelta.x, 0f, localDelta.z);
            if (localXZDelta.sqrMagnitude > 0.00000001f)
            {
                worldMove = m_CameraOffsetTransform.TransformVector(localXZDelta);
                worldMove.y = 0f;
            }

            if (Time.deltaTime > 0f)
            {
                if (m_CharacterController.isGrounded && m_VerticalVelocity < 0f)
                    m_VerticalVelocity = -1f;
                else
                    m_VerticalVelocity += Physics.gravity.y * Time.deltaTime;

                worldMove.y += m_VerticalVelocity * Time.deltaTime;
            }

            if (worldMove.sqrMagnitude > 0f)
            {
                m_CharacterController.Move(worldMove);
            }
        }

        private void OnBeforeRender()
        {
            if (m_CameraOffsetTransform != null && m_Initialized)
            {
                Vector3 rawCamLocal = m_CameraTransform != null ? m_CameraTransform.localPosition : m_PrevRawCamLocal;
                float clampedOffsetY = m_InitialOffsetY - rawCamLocal.y;
                m_CameraOffsetTransform.localPosition = new Vector3(-rawCamLocal.x, clampedOffsetY, -rawCamLocal.z);
            }
        }

        private void OnBeginContextRendering(ScriptableRenderContext context, System.Collections.Generic.List<Camera> cameras)
        {
            OnBeforeRender();
        }
    }
}
