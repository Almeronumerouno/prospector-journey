using UnityEngine;

namespace ProspectorJourney.UI
{
    /// <summary>
    /// Applies player preferences for Turn Type (Snap vs Continuous)
    /// and Locomotion Type (Teleport vs Continuous Move) to the XR Origin rig.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class VRSettingsApplier : MonoBehaviour
    {
        private void Awake()
        {
            ApplySettingsToRig(gameObject);
        }

        private void Start()
        {
            ApplySettingsToRig(gameObject);
        }

        public static void ApplySettingsToRig(GameObject rigObject)
        {
            if (rigObject == null) return;

            int turnType = PlayerPrefs.GetInt("VR_TurnType", PlayerPrefs.GetInt("VR_SnapTurn", 1) == 1 ? 0 : 1);
            int locoType = PlayerPrefs.GetInt("VR_LocomotionType", 1);

            // Locate children (direct, nested under Locomotion, or anywhere in hierarchy)
            Transform snapTurn = FindLocomotionChild(rigObject.transform, "SnapTurn") ?? FindLocomotionChild(rigObject.transform, "Turn");
            Transform continuousTurn = FindLocomotionChild(rigObject.transform, "ContinuousTurn");
            Transform continuousMove = FindLocomotionChild(rigObject.transform, "ContinuousMove") ?? FindLocomotionChild(rigObject.transform, "Move");
            Transform teleportation = FindLocomotionChild(rigObject.transform, "Teleportation");

            // Apply Turn Type: 0 = Snap, 1 = Continuous
            if (snapTurn != null) snapTurn.gameObject.SetActive(turnType == 0);
            if (continuousTurn != null) continuousTurn.gameObject.SetActive(turnType == 1);

            // Apply Locomotion Type: 0 = Teleport, 1 = Continuous
            if (teleportation != null) teleportation.gameObject.SetActive(locoType == 0);
            if (continuousMove != null) continuousMove.gameObject.SetActive(locoType == 1);

            Debug.Log($"[VRSettingsApplier] Successfully applied to '{rigObject.name}': Turn={(turnType == 0 ? "SNAP" : "CONTINUOUS")} (snapTurn={(snapTurn != null ? snapTurn.gameObject.activeSelf.ToString() : "null")}, contTurn={(continuousTurn != null ? continuousTurn.gameObject.activeSelf.ToString() : "null")}), Locomotion={(locoType == 0 ? "TELEPORT" : "CONTINUOUS")} (teleport={(teleportation != null ? teleportation.gameObject.activeSelf.ToString() : "null")}, contMove={(continuousMove != null ? continuousMove.gameObject.activeSelf.ToString() : "null")})");
        }

        private static Transform FindLocomotionChild(Transform parent, string targetName)
        {
            if (parent == null) return null;
            // 1. Direct child
            var direct = parent.Find(targetName);
            if (direct != null) return direct;

            // 2. Under a "Locomotion" container if present
            var loc = parent.Find("Locomotion");
            if (loc != null)
            {
                var underLoc = loc.Find(targetName);
                if (underLoc != null) return underLoc;
            }

            // 3. Recursive search
            foreach (Transform child in parent)
            {
                if (child.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                    return child;
                var nested = FindLocomotionChild(child, targetName);
                if (nested != null) return nested;
            }
            return null;
        }
    }
}
