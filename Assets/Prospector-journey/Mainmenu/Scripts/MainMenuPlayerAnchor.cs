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
                    m_Origin.CameraFloorOffsetObject.transform.localPosition = new Vector3(0f, 1.36144f, 0f);
                    m_Origin.CameraFloorOffsetObject.transform.localRotation = Quaternion.identity;
                }
                if (m_Origin.Camera != null)
                {
                    m_Origin.Camera.transform.localPosition = Vector3.zero;
                    m_Origin.Camera.transform.localRotation = Quaternion.identity;
                }
            }
        }

        public static void ResetAllSimulatorOffsets()
        {
            VRSimulatorSceneBinder.ResetSimulatedInputDevices();
            var sim = FindAnyObjectByType<UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator>();
            if (sim != null)
            {
                VRSimulatorSceneBinder.ResetSimulatorTrackingStates(sim);
            }
        }
    }
}
