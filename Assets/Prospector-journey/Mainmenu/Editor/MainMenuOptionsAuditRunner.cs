using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProspectorJourney.UI.Editor
{
    [InitializeOnLoad]
    public static class MainMenuOptionsAuditRunner
    {
        private const string ScenePath = "Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity";
        public const string AUDIT_KEY = "ProspectorJourney_RunMainMenuAudit";

        static MainMenuOptionsAuditRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("ProspectorJourney/Run Game Options Play Mode Audit")]
        public static void RunAudit()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            EditorPrefs.SetBool(AUDIT_KEY, true);
            EditorApplication.isPlaying = true;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(AUDIT_KEY, false))
                {
                    EditorPrefs.DeleteKey(AUDIT_KEY);
                    var runnerGo = new GameObject("[MainMenuOptionsAuditRunner]");
                    runnerGo.AddComponent<MainMenuOptionsPlayModeAudit>();
                    Debug.Log("[MainMenuOptionsAuditRunner] Instantiated in Play Mode.");
                }
            }
        }
    }
}
