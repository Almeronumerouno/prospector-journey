using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ProspectorJourney.Tutorial.Editor
{
    [InitializeOnLoad]
    public static class TutorialPondPlayModeRunner
    {
        private const string TUTORIAL_SCENE_PATH = "Assets/Prospector-journey/Tutorial/Tutorial scene.unity";
        public const string AUDIT_PREF_KEY = "ProspectorJourney_RunPondAudit";
        public const string AUDIT_PASSED_KEY = "ProspectorJourney_PondAuditPassed";

        static TutorialPondPlayModeRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
                {
                    Debug.Log("[TutorialPondPlayModeRunner] Play Mode started with Pond Audit Flag active.");
                    EnsureRunnerComponentInScene();
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
                {
                    EditorPrefs.DeleteKey(AUDIT_PREF_KEY);
                    bool passed = EditorPrefs.GetBool(AUDIT_PASSED_KEY, false);

                    Scene activeScene = SceneManager.GetActiveScene();
                    if (activeScene.isDirty)
                    {
                        EditorSceneManager.SaveScene(activeScene);
                    }

                    if (passed)
                    {
                        Debug.Log("[TutorialPondPlayModeRunner] >>> POND PLAY MODE AUDIT COMPLETED: 100% PASS (5/5 CHECKS) <<<\nScene saved cleanly in Edit Mode.");
                    }
                    else
                    {
                        Debug.LogWarning("[TutorialPondPlayModeRunner] >>> POND PLAY MODE AUDIT DID NOT PASS ALL CHECKS! <<<");
                    }
                }
            }
        }

        [MenuItem("ProspectorJourney/Run Pond Play Mode Audit")]
        public static void RunPlayModeAudit()
        {
            Debug.Log("[TutorialPondPlayModeRunner] Initiating Pond Play Mode Audit...");

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TUTORIAL_SCENE_PATH)
            {
                Debug.Log($"[TutorialPondPlayModeRunner] Opening {TUTORIAL_SCENE_PATH}...");
                activeScene = EditorSceneManager.OpenScene(TUTORIAL_SCENE_PATH, OpenSceneMode.Single);
            }

            if (activeScene.isDirty)
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            EditorPrefs.SetBool(AUDIT_PREF_KEY, true);
            EditorPrefs.SetBool(AUDIT_PASSED_KEY, false);

            if (!EditorApplication.isPlaying)
            {
                Debug.Log("[TutorialPondPlayModeRunner] Entering Play Mode to execute TutorialPondPlayModeAuditRunner...");
                EditorApplication.isPlaying = true;
            }
            else
            {
                EnsureRunnerComponentInScene();
            }
        }

        private static void EnsureRunnerComponentInScene()
        {
            var existing = Object.FindFirstObjectByType<TutorialPondPlayModeAuditRunner>();
            if (existing == null)
            {
                GameObject runnerGo = new GameObject("[TutorialPondPlayModeAuditRunner]");
                existing = runnerGo.AddComponent<TutorialPondPlayModeAuditRunner>();
                Debug.Log("[TutorialPondPlayModeRunner] Instantiated [TutorialPondPlayModeAuditRunner] GameObject in active scene.");
            }
            else
            {
                existing.StopAllCoroutines();
                existing.StartCoroutine(existing.RunAuditRoutine());
            }
        }
    }
}
