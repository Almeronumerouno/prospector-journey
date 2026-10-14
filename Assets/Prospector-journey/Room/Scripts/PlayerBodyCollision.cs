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

            // Apply turn and locomotion preferences selected in Main Menu Game Options
            ProspectorJourney.UI.VRSettingsApplier.ApplySettingsToRig(gameObject);

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
                if (m_InitialOffsetY < 0.1f)
                    m_InitialOffsetY = 1.36144f;
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

        public void EnsureFurnitureColliders()
        {
            // 1. Support Room scene (Environment_ProspectorCabin)
            var cabinEnv = GameObject.Find("Environment_ProspectorCabin");
            if (cabinEnv != null)
            {
                var meshFilters = cabinEnv.GetComponentsInChildren<MeshFilter>(true);
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

            // 2. Support Tutorial scene (Environment2: trees, rocks, stumps, ground)
            var env2 = GameObject.Find("Environment2");
            if (env2 != null)
            {
                var meshFilters = env2.GetComponentsInChildren<MeshFilter>(true);
                foreach (var mf in meshFilters)
                {
                    if (mf == null || mf.sharedMesh == null) continue;
                    var go = mf.gameObject;

                    if (go.GetComponentInParent<XRGrabInteractable>() != null) continue;
                    if (go.GetComponentInParent<TeleportationAnchor>() != null) continue;

                    // Skip foliage decals so we don't attach new colliders to grass/flowers
                    if (IsFoliageDecal(go.name) || (go.transform.parent != null && IsFoliageDecal(go.transform.parent.name)))
                        continue;

                    // Ensure solid obstacle (tree, rock, stump, ground) has a collider
                    if (go.GetComponent<Collider>() == null && go.GetComponentInParent<Collider>() == null && go.GetComponentInChildren<Collider>() == null)
                    {
                        var mc = go.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                    }
                }
            }

            // 3. Support root obstacle props in Tutorial scene:
            // House_1, House_2, House_3, Bench, Crate, Fence, Pine Tree_1, Pine Tree_2, Lantern, Flower Pot, [Tutorial_Pond], Pond
            string[] obstaclePropNames = new string[]
            {
                "House_1", "House_2", "House_3", "Bench", "Crate", "Fence",
                "Pine Tree_1", "Pine Tree_2", "Lantern", "Flower Pot",
                "[Tutorial_Pond]", "Pond", "Rock_"
            };

            var rootObjects = gameObject.scene.IsValid() ? gameObject.scene.GetRootGameObjects() : null;
            if (rootObjects != null)
            {
                foreach (var rootGo in rootObjects)
                {
                    if (rootGo == null) continue;
                    bool isTargetObstacle = false;
                    for (int i = 0; i < obstaclePropNames.Length; i++)
                    {
                        if (rootGo.name.StartsWith(obstaclePropNames[i], System.StringComparison.OrdinalIgnoreCase))
                        {
                            isTargetObstacle = true;
                            break;
                        }
                    }

                    if (isTargetObstacle)
                    {
                        EnsureObstacleColliders(rootGo);
                    }
                }
            }

            // Also check direct GameObject.Find in case rootObjects was unavailable
            for (int i = 0; i < obstaclePropNames.Length; i++)
            {
                var propGo = GameObject.Find(obstaclePropNames[i]);
                if (propGo != null)
                {
                    EnsureObstacleColliders(propGo);
                }
            }

            // 4. Support Mainmenu scene (Diorama_Environment, campsite props, boundaries)
            var dioramaEnv = GameObject.Find("Diorama_Environment");
            if (dioramaEnv != null)
            {
                var meshFilters = dioramaEnv.GetComponentsInChildren<MeshFilter>(true);
                foreach (var mf in meshFilters)
                {
                    if (mf == null || mf.sharedMesh == null) continue;
                    var go = mf.gameObject;

                    if (go.GetComponentInParent<XRGrabInteractable>() != null) continue;
                    if (go.GetComponentInParent<TeleportationAnchor>() != null) continue;

                    // Skip foliage decals
                    if (IsFoliageDecal(go.name) || (go.transform.parent != null && IsFoliageDecal(go.transform.parent.name)))
                        continue;

                    // Ensure solid obstacle (cliff, rock, crate, bench, ground) has a collider
                    if (go.GetComponent<Collider>() == null && go.GetComponentInParent<Collider>() == null && go.GetComponentInChildren<Collider>() == null)
                    {
                        var mc = go.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                    }
                }
            }

            // Also ensure colliders on root props in Mainmenu: fence, fence (1), bench
            string[] mainMenuProps = new string[] { "fence", "fence (1)", "bench" };
            foreach (var propName in mainMenuProps)
            {
                var propGo = GameObject.Find(propName);
                if (propGo != null)
                {
                    EnsureObstacleColliders(propGo);
                }
            }
        }

        private void EnsureObstacleColliders(GameObject obstacleRoot)
        {
            if (obstacleRoot == null) return;
            var meshFilters = obstacleRoot.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in meshFilters)
            {
                if (mf == null || mf.sharedMesh == null) continue;
                var go = mf.gameObject;
                if (IsFoliageDecal(go.name) || (go.transform.parent != null && IsFoliageDecal(go.transform.parent.name)))
                    continue;

                if (go.GetComponent<Collider>() == null && go.GetComponentInParent<Collider>() == null && go.GetComponentInChildren<Collider>() == null)
                {
                    var mc = go.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh;
                }
            }
        }

        public static bool IsFoliageDecal(string objectName)
        {
            if (string.IsNullOrEmpty(objectName)) return false;
            // Exclude Flower Pot (which is an obstacle prop)
            if (objectName.IndexOf("Flower Pot", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return objectName.IndexOf("Grass", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Flowers", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Flower", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Mushroom", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Branch", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Water", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Lily", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("LilyPad", System.StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Leaf", System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public void IgnoreGrabbableAndTeleportCollisions()
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

            // Ignore flat ground/foliage decals (Grass, Flowers, Mushroom, Branch) in Environment2 so player never snags
            var env2 = GameObject.Find("Environment2");
            if (env2 != null)
            {
                var foliageColliders = env2.GetComponentsInChildren<Collider>(true);
                foreach (var col in foliageColliders)
                {
                    if (col == null || col == m_CharacterController) continue;
                    if (IsFoliageDecal(col.gameObject.name) || (col.transform.parent != null && IsFoliageDecal(col.transform.parent.name)))
                    {
                        Physics.IgnoreCollision(m_CharacterController, col, true);
                    }
                }
            }

            // Global foliage collider ignore pass
            var allColliders = FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var col in allColliders)
            {
                if (col == null || col == m_CharacterController) continue;
                if (IsFoliageDecal(col.gameObject.name) || (col.transform.parent != null && IsFoliageDecal(col.transform.parent.name)))
                {
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
