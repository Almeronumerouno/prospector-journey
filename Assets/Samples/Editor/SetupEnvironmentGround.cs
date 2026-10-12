using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

namespace Jarvis.EditorTools
{
    [InitializeOnLoad]
    public static class SetupEnvironmentGround
    {
        static SetupEnvironmentGround()
        {
            EditorApplication.delayCall += () =>
            {
                SetupGround(false);
            };
        }

        [MenuItem("Tools/JARVIS/Make Meshy Ground Walkable")]
        public static void MakeWalkableMenu()
        {
            SetupGround(true);
        }

        public static void SetupGround(bool showDialog)
        {
            var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            GameObject meshyObj = null;

            foreach (var go in allObjects)
            {
                if (go.name.IndexOf("Meshy", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    go.name.IndexOf("Hnatu", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    meshyObj = go;
                    break;
                }
            }

            if (meshyObj == null)
            {
                if (showDialog)
                {
                    string glbPath = "Assets/Assets-meet3/Meshy_AI_Hnatu_0920065005_texture.glb";
                    var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(glbPath);
                    if (modelPrefab != null)
                    {
                        meshyObj = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab);
                        meshyObj.name = "Meshy_AI_Hnatu_Environment";
                        meshyObj.transform.position = Vector3.zero;
                        meshyObj.transform.localScale = new Vector3(15f, 15f, 15f);
                        Undo.RegisterCreatedObjectUndo(meshyObj, "Spawn Meshy Ground");
                        Debug.Log("[JARVIS] Meshy model di-spawn ke scene pada posisi (0,0,0) dengan scale (15,15,15).");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("JARVIS", "Tidak menemukan GameObject Meshy di scene ataupun file GLB di Assets.", "OK");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }

            Undo.RegisterFullObjectHierarchyUndo(meshyObj, "Setup Walkable Ground");

            // Pastikan layer adalah Default (0)
            meshyObj.layer = 0;

            // Cari semua MeshFilter di objek dan seluruh children
            var meshFilters = meshyObj.GetComponentsInChildren<MeshFilter>(true);
            if (meshFilters.Length == 0)
            {
                Debug.LogWarning("[JARVIS] Tidak ada MeshFilter ditemukan pada " + meshyObj.name);
                return;
            }

            int collidersConfigured = 0;
            foreach (var mf in meshFilters)
            {
                var targetGo = mf.gameObject;
                targetGo.layer = 0;

                var col = targetGo.GetComponent<MeshCollider>();
                if (col == null)
                {
                    col = targetGo.AddComponent<MeshCollider>();
                }

                col.sharedMesh = mf.sharedMesh;
                col.convex = false; // Non-convex agar kontur ground akurat bisa dipijak
                col.isTrigger = false;
                col.enabled = true;
                collidersConfigured++;
            }

            // Tambahkan TeleportationArea agar XR Teleportation ray bisa berfungsi
            var teleportArea = meshyObj.GetComponent<TeleportationArea>();
            if (teleportArea == null)
            {
                teleportArea = meshyObj.AddComponent<TeleportationArea>();
            }

            int teleportLayer = InteractionLayerMask.GetMask("Teleport");
            if (teleportLayer != 0)
            {
                teleportArea.interactionLayers = teleportLayer;
            }

            // Jika scale masih 1, besarkan agar cukup luas untuk dipijak (misal 15x)
            if (meshyObj.transform.localScale == Vector3.one)
            {
                meshyObj.transform.localScale = new Vector3(15f, 15f, 15f);
                Debug.Log("[JARVIS] Scale Meshy disesuaikan ke (15, 15, 15) agar luas sebagai environment.");
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            string message = $"Setup Walkable Ground BERHASIL!\n\n" +
                             $"- Target: {meshyObj.name}\n" +
                             $"- MeshCollider: {collidersConfigured} collider aktif (Convex: OFF, Layer: Default)\n" +
                             $"- TeleportationArea: Aktif (Support XR Teleport)\n" +
                             $"- Scale: {meshyObj.transform.localScale}\n\n" +
                             $"Sekarang environment Meshy sudah solid dan bisa dipijak!";

            Debug.Log("[JARVIS] " + message.Replace("\n", " "));

            if (showDialog)
            {
                EditorUtility.DisplayDialog("JARVIS - Setup Walkable Ground", message, "Selesai");
            }
        }
    }
}
