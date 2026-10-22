using UnityEngine;
using Unity.XR.CoreUtils;
using ProspectorJourney.Room;

namespace ProspectorJourney.UI
{
    /// <summary>
    /// Ensures the player starts cleanly at the Main Menu spawn point (0, 0, 0)
    /// in front of the menu board whenever entering or returning from another scene.
    /// Resets simulator tracking offsets and preserves free player locomotion
    /// while relying on PlayerBodyCollision + CharacterController to prevent walking through walls.
    /// </summary>
    [RequireComponent(typeof(XROrigin))]
    [DefaultExecutionOrder(15000)]
    public class MainMenuPlayerAnchor : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private Vector3 spawnPosition = Vector3.zero;

        [Header("Out-of-Bounds Fallback Safety")]
        [SerializeField] private float maxDistanceLimit = 150f;
        [SerializeField] private float killY = -15f;

        private XROrigin m_Origin;
        private CharacterController m_CharacterController;

        private void Awake()
        {
            m_Origin = GetComponent<XROrigin>();
            m_CharacterController = GetComponent<CharacterController>();

            ResetAllSimulatorOffsets();
            SpawnAtOrigin();
        }

        private void Start()
        {
            ResetAllSimulatorOffsets();
            SpawnAtOrigin();
        }

        private void Update()
        {
            // Fallback safety only if player falls through map or gets flung miles away
            if (transform.position.y < killY || Vector3.Distance(transform.position, spawnPosition) > maxDistanceLimit)
            {
                SpawnAtOrigin();
            }
        }

        public void SpawnAtOrigin()
        {
            if (m_CharacterController != null && m_CharacterController.enabled)
            {
                m_CharacterController.enabled = false;
                transform.position = spawnPosition;
                transform.rotation = Quaternion.identity;
                m_CharacterController.enabled = true;
            }
            else
            {
                transform.position = spawnPosition;
                transform.rotation = Quaternion.identity;
            }

            if (m_Origin != null)
            {
                if (m_Origin.CameraFloorOffsetObject != null)
                {
                    bool isRealVR = PlayerBodyCollision.IsRealVRDeviceActive();
                    if (isRealVR)
                    {
                        m_Origin.CameraFloorOffsetObject.transform.localPosition = Vector3.zero;
                    }
                    else
                    {
                        float eyeHeight = m_Origin.CameraYOffset > 0f ? m_Origin.CameraYOffset : 1.67f;
                        m_Origin.CameraFloorOffsetObject.transform.localPosition = new Vector3(0f, eyeHeight, 0f);
                    }
                    m_Origin.CameraFloorOffsetObject.transform.localRotation = Quaternion.identity;
                }
                if (m_Origin.Camera != null && !PlayerBodyCollision.IsRealVRDeviceActive())
                {
                    m_Origin.Camera.transform.localPosition = Vector3.zero;
                    m_Origin.Camera.transform.localRotation = Quaternion.identity;
                }
                if (!PlayerBodyCollision.IsRealVRDeviceActive())
                {
                    Transform camOffset = m_Origin.CameraFloorOffsetObject != null ? m_Origin.CameraFloorOffsetObject.transform : transform.Find("Camera Offset");
                    if (camOffset != null)
                    {
                        Transform leftCtrl = camOffset.Find("Left Controller");
                        Transform rightCtrl = camOffset.Find("Right Controller");
                        if (leftCtrl != null)
                            leftCtrl.localPosition = new Vector3(-0.22f, -0.57f, 0.45f);
                        if (rightCtrl != null)
                            rightCtrl.localPosition = new Vector3(0.22f, -0.57f, 0.45f);
                    }
                }
            }
        }

        public static void ResetAllSimulatorOffsets()
        {
            if (PlayerBodyCollision.IsRealVRDeviceActive()) return;

            VRSimulatorSceneBinder.ResetSimulatedInputDevices();
            var sim = FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
            if (sim != null)
            {
                VRSimulatorSceneBinder.ResetSimulatorTrackingStates(sim);
            }
        }
    }
}
