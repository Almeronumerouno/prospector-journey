using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ProspectorJourney.Tutorial.Editor
{
    [InitializeOnLoad]
    public static class TutorialPlayModeRunner
    {
        private const string TUTORIAL_SCENE_PATH = "Assets/Prospector-journey/Tutorial/Tutorial scene.unity";
        public const string AUDIT_PREF_KEY = "ProspectorJourney_RunTutorialAudit";
        public const string AUDIT_PASSED_KEY = "ProspectorJourney_AuditPassed";

        static TutorialPlayModeRunner()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
                {
                    Debug.Log("[TutorialPlayModeRunner] Play Mode started with Audit Flag active.");
                    EnsureRunnerComponentInScene();
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (EditorPrefs.GetBool(AUDIT_PREF_KEY, false))
                {
                    EditorPrefs.DeleteKey(AUDIT_PREF_KEY);
                    bool passed = EditorPrefs.GetBool(AUDIT_PASSED_KEY, false);

                    // Ensure scene is saved cleanly in Edit Mode
                    Scene activeScene = SceneManager.GetActiveScene();
                    if (activeScene.isDirty)
                    {
                        EditorSceneManager.SaveScene(activeScene);
                    }

                    if (passed)
                    {
                        Debug.Log("[TutorialPlayModeRunner] >>> TUTORIAL PLAY MODE AUDIT COMPLETED: 100% PASS (5/5 CHECKS) <<<\nScene saved cleanly in Edit Mode.");
                    }
                    else
                    {
                        Debug.LogWarning("[TutorialPlayModeRunner] >>> TUTORIAL PLAY MODE AUDIT DID NOT PASS ALL CHECKS! <<<");
                    }
                }
            }
        }

        [MenuItem("ProspectorJourney/Run Tutorial Play Mode Audit")]
        public static void RunPlayModeAudit()
        {
            Debug.Log("[TutorialPlayModeRunner] Initiating Tutorial Play Mode Audit...");

            // 1. Ensure Tutorial scene is active
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TUTORIAL_SCENE_PATH)
            {
                Debug.Log($"[TutorialPlayModeRunner] Opening {TUTORIAL_SCENE_PATH}...");
                activeScene = EditorSceneManager.OpenScene(TUTORIAL_SCENE_PATH, OpenSceneMode.Single);
            }

            if (!activeScene.IsValid() || !activeScene.isLoaded)
            {
                Debug.LogError($"[TutorialPlayModeRunner] Failed to load {TUTORIAL_SCENE_PATH}");
                return;
            }

            // Save any pending Edit Mode changes first
            if (activeScene.isDirty)
            {
                EditorSceneManager.SaveScene(activeScene);
            }

            EditorPrefs.SetBool(AUDIT_PREF_KEY, true);
            EditorPrefs.SetBool(AUDIT_PASSED_KEY, false);

            if (!EditorApplication.isPlaying)
            {
                Debug.Log("[TutorialPlayModeRunner] Entering Play Mode to execute TutorialPlayModeAuditRunner...");
                EditorApplication.isPlaying = true;
            }
            else
            {
                Debug.Log("[TutorialPlayModeRunner] Already in Play Mode. Executing audit runner directly...");
                EnsureRunnerComponentInScene();
            }
        }

        private static void EnsureRunnerComponentInScene()
        {
            var existing = Object.FindFirstObjectByType<TutorialPlayModeAuditRunner>();
            if (existing == null)
            {
                GameObject runnerGo = new GameObject("[TutorialPlayModeAuditRunner]");
                existing = runnerGo.AddComponent<TutorialPlayModeAuditRunner>();
                Debug.Log("[TutorialPlayModeRunner] Instantiated [TutorialPlayModeAuditRunner] GameObject in active scene.");
            }
            else
            {
                existing.StopAllCoroutines();
                existing.StartCoroutine(existing.RunAuditRoutine());
            }
        }
    }
}
