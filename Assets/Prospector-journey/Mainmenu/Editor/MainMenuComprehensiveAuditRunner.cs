using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProspectorJourney.UI.Editor
{
    [InitializeOnLoad]
    public static class MainMenuComprehensiveAuditRunner
    {
        private const string ScenePath = "Assets/Prospector-journey/Mainmenu/Mainmenu scene.unity";
        public const string AUDIT_PREF_KEY = "ProspectorJourney_ComprehensiveAuditPending";

        static MainMenuComprehensiveAuditRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        [MenuItem("ProspectorJourney/Run Comprehensive Play Mode Audit")]
        public static void TriggerAudit()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            // Save dirty scene before entering Play Mode
            if (activeScene.isDirty)
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            EditorPrefs.SetBool(AUDIT_PREF_KEY, true);
            EditorApplication.isPlaying = true;
        }

        /// <summary>
        /// Command line entry point for Unity batchmode / CI execution.
        /// e.g. Unity.exe -batchmode -projectPath ... -executeMethod ProspectorJourney.UI.Editor.MainMenuComprehensiveAuditRunner.RunFromCommandLine
        /// </summary>
        public static void RunFromCommandLine()
        {
            TriggerAudit();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
                {
                    EditorPrefs.DeleteKey(AUDIT_PREF_KEY);
                    var runnerGo = new GameObject("[ComprehensiveAuditRunner]");
                    runnerGo.AddComponent<MainMenuPlayModeComprehensiveAudit>();
                    Debug.Log("[MainMenuComprehensiveAuditRunner] Started Play Mode Comprehensive Audit runner.");
                }
            }
        }
    }
}
