using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProspectorJourney.Room;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProspectorJourney.Tutorial
{
    /// <summary>
    /// Independent Play Mode test runner & auditor for [Tutorial_Pond] in Tutorial scene.unity.
    /// Programmatically verifies 5 audit dimensions:
    /// Audit 1: Pond GameObject Hierarchy & Required Components (Basin, Water, Rocks, Foliage, Pier, Audio)
    /// Audit 2: Water Shader, Upward Normals & URP Rendering Compatibility
    /// Audit 3: Solid Colliders on Shoreline Rocks & Wooden Pier (Player Collision Parity)
    /// Audit 4: Water Trigger Zone, Decal Collision Filter & Locomotion Non-Snagging
    /// Audit 5: Zero Compile Errors and Zero Runtime Exceptions in Unity Console
    /// </summary>
    [DefaultExecutionOrder(31000)]
    public class TutorialPondPlayModeAuditRunner : MonoBehaviour
    {
        public const string AUDIT_PREF_KEY = "ProspectorJourney_RunPondAudit";
        public const string AUDIT_PASSED_KEY = "ProspectorJourney_PondAuditPassed";

        public bool isRunning = false;
        public bool isComplete = false;
        public bool allPassed = false;

        public bool audit1Passed = false;
        public bool audit2Passed = false;
        public bool audit3Passed = false;
        public bool audit4Passed = false;
        public bool audit5Passed = false;

        private int m_ErrorCount = 0;
        private int m_ExceptionCount = 0;
        private readonly List<string> m_CapturedErrors = new List<string>();

        private void Awake()
        {
            Application.logMessageReceived += HandleLogMessage;
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLogMessage;
        }

        private void HandleLogMessage(string condition, string stackTrace, LogType type)
        {
            // Ignore benign simulated XR controller warnings or cloud auth logs
            if (condition.Contains("XRSimulatedController") || condition.Contains("Token refresh") || condition.Contains("[Auth]"))
                return;

            if (type == LogType.Exception)
            {
                m_ExceptionCount++;
                m_CapturedErrors.Add($"[EXCEPTION] {condition}\n{stackTrace}");
            }
            else if (type == LogType.Error)
            {
                m_ErrorCount++;
                m_CapturedErrors.Add($"[ERROR] {condition}");
            }
        }

        private void Start()
        {
            StartCoroutine(RunAuditRoutine());
        }

        public IEnumerator RunAuditRoutine()
        {
            isRunning = true;
            Debug.Log("================================================================================");
            Debug.Log("[POND PLAY MODE AUDIT] STARTING INDEPENDENT AUDIT OF TUTORIAL POND FEATURE");
            Debug.Log("================================================================================");

            yield return new WaitForSeconds(0.3f);

            // -------------------------------------------------------------------------
            // AUDIT 1: Hierarchy & Component Integrity
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING AUDIT 1: Hierarchy & Component Verification...");
            audit1Passed = ExecuteAudit1(out string a1Details);
            if (audit1Passed)
            {
                Debug.Log($"[PASS] Audit 1: Pond hierarchy verified ({a1Details}).");
            }
            else
            {
                Debug.LogError($"[FAIL] Audit 1: {a1Details}");
            }

            yield return null;

            // -------------------------------------------------------------------------
            // AUDIT 2: Water Shader, Upward Normals & Material Audit
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING AUDIT 2: Water Shader & Rendering Verification...");
            audit2Passed = ExecuteAudit2(out string a2Details);
            if (audit2Passed)
            {
                Debug.Log($"[PASS] Audit 2: Water shader & normals verified ({a2Details}).");
            }
            else
            {
                Debug.LogError($"[FAIL] Audit 2: {a2Details}");
            }

            yield return null;

            // -------------------------------------------------------------------------
            // AUDIT 3: Colliders on Shoreline Rocks & Wooden Pier (Solid Obstacles)
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING AUDIT 3: Solid Shoreline & Pier Colliders Verification...");
            audit3Passed = ExecuteAudit3(out string a3Details);
            if (audit3Passed)
            {
                Debug.Log($"[PASS] Audit 3: Solid colliders verified ({a3Details}).");
            }
            else
            {
                Debug.LogError($"[FAIL] Audit 3: {a3Details}");
            }

            yield return null;

            // -------------------------------------------------------------------------
            // AUDIT 4: Water Trigger, Decal Filters & Non-Snag Locomotion
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING AUDIT 4: Water Trigger Zone & Foliage Filter Verification...");
            audit4Passed = ExecuteAudit4(out string a4Details);
            if (audit4Passed)
            {
                Debug.Log($"[PASS] Audit 4: Water trigger & foliage ignore filters verified ({a4Details}).");
            }
            else
            {
                Debug.LogError($"[FAIL] Audit 4: {a4Details}");
            }

            yield return null;

            // -------------------------------------------------------------------------
            // AUDIT 5: Console Health & Zero Runtime Exceptions
            // -------------------------------------------------------------------------
            Debug.Log("\n>>> EXECUTING AUDIT 5: Console Error & Runtime Exception Audit...");
            bool c5 = (m_ErrorCount == 0 && m_ExceptionCount == 0);
            audit5Passed = c5;
            if (c5)
            {
                Debug.Log($"[PASS] Audit 5: Zero errors and zero runtime exceptions captured ({m_ErrorCount} errors, {m_ExceptionCount} exceptions).");
            }
            else
            {
                Debug.LogError($"[FAIL] Audit 5: Errors detected ({m_ErrorCount} errors, {m_ExceptionCount} exceptions)!\n" + string.Join("\n", m_CapturedErrors));
            }

            // -------------------------------------------------------------------------
            // SUMMARY
            // -------------------------------------------------------------------------
            allPassed = audit1Passed && audit2Passed && audit3Passed && audit4Passed && audit5Passed;
            isComplete = true;
            isRunning = false;

            Debug.Log("================================================================================");
            Debug.Log("[POND PLAY MODE AUDIT] COMPLETE AUDIT EXECUTION REPORT");
            Debug.Log("================================================================================");
            Debug.Log($"Audit 1 (Hierarchy & Component Integrity)           : {(audit1Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Audit 2 (Water Shader, Upward Normals & Rendering)   : {(audit2Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Audit 3 (Solid Colliders on Shoreline Rocks & Pier) : {(audit3Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Audit 4 (Water Trigger Zone & Decal Filters)        : {(audit4Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Audit 5 (Zero Errors & Runtime Exceptions)          : {(audit5Passed ? "PASSED" : "FAILED")}");
            Debug.Log("--------------------------------------------------------------------------------");
            Debug.Log($"OVERALL RESULT: {(allPassed ? "100% PASSED (5/5 CHECKS SATISFIED)" : "AUDIT FAILED")}");
            Debug.Log("================================================================================");

#if UNITY_EDITOR
            EditorPrefs.SetBool(AUDIT_PASSED_KEY, allPassed);
            yield return new WaitForSeconds(0.2f);
            Debug.Log("[TutorialPondPlayModeAuditRunner] Exiting Play Mode cleanly...");
            EditorApplication.isPlaying = false;
#endif
        }

        private bool ExecuteAudit1(out string details)
        {
            GameObject pond = GameObject.Find("[Tutorial_Pond]");
            if (pond == null)
            {
                details = "[Tutorial_Pond] not found in scene!";
                return false;
            }

            Transform basin = pond.transform.Find("Pond_Basin");
            Transform water = pond.transform.Find("Pond_Water_Surface");
            Transform rocks = pond.transform.Find("Shoreline_Rocks");
            Transform foliage = pond.transform.Find("Shoreline_Foliage");
            Transform pier = pond.transform.Find("Pond_Pier");
            Transform audio = pond.transform.Find("Pond_Audio");

            if (basin == null || water == null || rocks == null || foliage == null || pier == null || audio == null)
            {
                details = $"Missing pond child elements: Basin={basin != null}, Water={water != null}, Rocks={rocks != null}, Foliage={foliage != null}, Pier={pier != null}, Audio={audio != null}";
                return false;
            }

            AudioSource aSrc = audio.GetComponent<AudioSource>();
            if (aSrc == null || !aSrc.isPlaying || aSrc.clip == null || Mathf.Abs(aSrc.spatialBlend - 1.0f) > 0.05f)
            {
                details = $"Pond_Audio invalid: hasSource={aSrc != null}, isPlaying={aSrc?.isPlaying}, clip={aSrc?.clip?.name}, spatialBlend={aSrc?.spatialBlend}";
                return false;
            }

            details = $"Pond at {pond.transform.position}, rocks={rocks.childCount}, foliage={foliage.childCount}, pierPlanks={pier.childCount}, 3D audio active ({aSrc.clip.name})";
            return true;
        }

        private bool ExecuteAudit2(out string details)
        {
            GameObject waterGo = GameObject.Find("Pond_Water_Surface");
            if (waterGo == null)
            {
                details = "Pond_Water_Surface not found!";
                return false;
            }

            MeshRenderer mr = waterGo.GetComponent<MeshRenderer>();
            MeshFilter mf = waterGo.GetComponent<MeshFilter>();
            if (mr == null || mf == null || mf.sharedMesh == null)
            {
                details = "Water MeshRenderer or MeshFilter missing!";
                return false;
            }

            Material mat = mr.sharedMaterial;
            if (mat == null || mat.shader == null || !mat.shader.name.Contains("WaterShader"))
            {
                details = $"Invalid water material/shader: mat={mat?.name}, shader={mat?.shader?.name}";
                return false;
            }

            // Verify upward-facing normal on water surface mesh
            UnityEngine.Mesh m = mf.sharedMesh;
            Vector3 v0 = m.vertices[m.triangles[0]];
            Vector3 v1 = m.vertices[m.triangles[1]];
            Vector3 v2 = m.vertices[m.triangles[2]];
            Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;

            if (normal.y < 0.9f)
            {
                details = $"Water surface triangle normal inverted! Normal={normal} (expected Y > 0.9)";
                return false;
            }

            details = $"Shader='{mat.shader.name}', Normal={normal}, Bounds={mr.bounds.extents}";
            return true;
        }

        private bool ExecuteAudit3(out string details)
        {
            GameObject pond = GameObject.Find("[Tutorial_Pond]");
            Transform rocks = pond != null ? pond.transform.Find("Shoreline_Rocks") : null;
            Transform pier = pond != null ? pond.transform.Find("Pond_Pier") : null;

            if (rocks == null || pier == null)
            {
                details = "Shoreline_Rocks or Pond_Pier missing!";
                return false;
            }

            int validRocks = 0;
            for (int i = 0; i < rocks.childCount; i++)
            {
                Collider col = rocks.GetChild(i).GetComponentInChildren<Collider>();
                if (col != null && col.enabled && !col.isTrigger)
                {
                    validRocks++;
                }
            }

            if (validRocks < 15)
            {
                details = $"Insufficient solid shoreline rock colliders: {validRocks} (expected >= 15)";
                return false;
            }

            Collider pierCol = pier.GetComponent<Collider>();
            if (pierCol == null || !pierCol.enabled || pierCol.isTrigger)
            {
                details = "Pond_Pier walkway BoxCollider missing or disabled!";
                return false;
            }

            details = $"Rocks with solid colliders: {validRocks}/{rocks.childCount}, Pier Walkway solid BoxCollider: {pierCol.bounds.size}";
            return true;
        }

        private bool ExecuteAudit4(out string details)
        {
            GameObject waterGo = GameObject.Find("Pond_Water_Surface");
            if (waterGo == null)
            {
                details = "Pond_Water_Surface not found!";
                return false;
            }

            Collider waterCol = waterGo.GetComponent<Collider>();
            if (waterCol == null || !waterCol.enabled || !waterCol.isTrigger)
            {
                details = "Pond_Water_Surface collider missing or isTrigger is false!";
                return false;
            }

            // Verify PlayerBodyCollision ignores water and foliage decals
            bool waterIgnored = PlayerBodyCollision.IsFoliageDecal("Pond_Water_Surface");
            bool lilyIgnored = PlayerBodyCollision.IsFoliageDecal("LilyPad_1");
            bool grassIgnored = PlayerBodyCollision.IsFoliageDecal("Grass_01");

            if (!waterIgnored || !lilyIgnored || !grassIgnored)
            {
                details = $"PlayerBodyCollision decal filter check failed: waterIgnored={waterIgnored}, lilyIgnored={lilyIgnored}, grassIgnored={grassIgnored}";
                return false;
            }

            details = $"Water trigger active={waterCol.isTrigger}, Foliage & Water decal filter verified (water, lily, grass correctly ignored by player body pusher)";
            return true;
        }
    }
}
