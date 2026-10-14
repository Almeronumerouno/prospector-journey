using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProspectorJourney.UI.Editor
{
    [InitializeOnLoad]
    public static class MainMenuReturnAuditRunner
    {
        private const string TutorialScenePath = "Assets/Prospector-journey/Tutorial/Tutorial scene.unity";
        public const string AUDIT_PREF_KEY = "ProspectorJourney_ReturnAuditPending";

        static MainMenuReturnAuditRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("ProspectorJourney/Run Return to Main Menu Play Mode Audit")]
        public static void TriggerAudit()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != TutorialScenePath)
            {
                EditorSceneManager.OpenScene(TutorialScenePath);
            }

            EditorPrefs.SetBool(AUDIT_PREF_KEY, true);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
                {
                    EditorPrefs.DeleteKey(AUDIT_PREF_KEY);
                    var runnerGo = new GameObject("[ReturnAuditRunner]");
                    runnerGo.AddComponent<MainMenuReturnPlayModeAudit>();
                    Debug.Log("[MainMenuReturnAuditRunner] Started Return to Main Menu Play Mode Audit runner.");
                }
            }
        }
    }
}
