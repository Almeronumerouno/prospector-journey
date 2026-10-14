using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ProspectorJourney.Tutorial.Editor
{
    public static class TutorialPondBuilder
    {
        private const string TUTORIAL_SCENE_PATH = "Assets/Prospector-journey/Tutorial/Tutorial scene.unity";
        public const string POND_ROOT_NAME = "[Tutorial_Pond]";
        private const string MESH_DIR = "Assets/Prospector-journey/Tutorial/Meshes";

        private const string WATER_MAT_PATH = "Assets/OccaSoftware/Low Poly Fantasy Village/Materials/Village_River_Water.mat";
        private const string NATURE_MAT_PATH = "Assets/SimpleNaturePack/Materials/SimpleNaturePack_Texture_01.mat";
        private const string WATER_AUDIO_PATH = "Assets/Assets-meet3/_Course Library/Audio/FX/SFX_Loop_Liquid_Pour.ogg";
        private const string GROUND_FBX_PATH = "Assets/SimpleNaturePack/Models/Ground_03.fbx";

        // Exact swatch UV coordinates extracted from FBX assets:
        // Ground_03 meadow grass: (0.059, 0.848)
        private static readonly Vector2 UV_GRASS = new Vector2(0.059f, 0.848f);
        // Rich dark earthen soil/mud bank: (0.085, 0.942)
        private static readonly Vector2 UV_DIRT = new Vector2(0.085f, 0.942f);
        // Neutral smooth riverbed stone (from Rock_01): (0.189, 0.911)
        private static readonly Vector2 UV_STONE_BED = new Vector2(0.189f, 0.911f);
        // Natural weathered wood: (0.109, 0.969)
        private static readonly Vector2 UV_WOOD_PLANK = new Vector2(0.109f, 0.969f);
        // Vibrant leaf green: (0.219, 0.844)
        private static readonly Vector2 UV_LILY_GREEN = new Vector2(0.219f, 0.844f);

        // Center position and radii for the LARGE gold panning pond in the tutorial clearing
        public static readonly Vector3 POND_CENTER = new Vector3(4.2f, 0.0f, -1.0f);
        public const float RADIUS_X = 6.4f;
        public const float RADIUS_Z = 5.0f;

        // Heights for the sunken crater pit (kubangan yang nyata)
        public const float GROUND_LEVEL_Y = 0.45f;
        public const float RIM_CREST_Y = 0.52f;      // Gentle earthen berm crest (7 cm above meadow)
        public const float WATER_LEVEL_Y = 0.36f;    // Water is sunken 16 cm below rim, 9 cm below meadow
        public const float BED_DEPTH_Y = 0.08f;      // Deep concave basin bed (28 cm water depth)

        /// <summary>
        /// Natural organic non-oval kidney pond contour.
        /// Asymmetric geometry: wide North-East pier bay, gentle waist, and natural curved coves.
        /// Smooth C-infinity harmonics ensure zero self-intersection or sharp creases.
        /// </summary>
        public static float GetOrganicRadiusFactor(float angle)
        {
            float f = 1.0f 
                + 0.18f * Mathf.Cos(angle - 0.70f)  // Wide pier bay at North-East (~40 deg)
                + 0.12f * Mathf.Sin(2f * angle)      // Kidney-bean contour & asymmetric lobes
                + 0.05f * Mathf.Cos(3f * angle);     // Subtle natural shoreline meander

            return Mathf.Clamp(f, 0.68f, 1.35f);
        }

        [MenuItem("ProspectorJourney/Build Tutorial Pond")]
        public static void BuildPond()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != TUTORIAL_SCENE_PATH)
            {
                Debug.Log($"Opening {TUTORIAL_SCENE_PATH}...");
                activeScene = EditorSceneManager.OpenScene(TUTORIAL_SCENE_PATH, OpenSceneMode.Single);
            }

            // Remove existing pond root if present
            var existingPond = GameObject.Find(POND_ROOT_NAME);
            if (existingPond != null)
            {
                Undo.DestroyObjectImmediate(existingPond);
            }

            // Ensure meshes folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Prospector-journey/Tutorial/Meshes"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prospector-journey/Tutorial"))
                {
                    AssetDatabase.CreateFolder("Assets/Prospector-journey", "Tutorial");
                }
                AssetDatabase.CreateFolder("Assets/Prospector-journey/Tutorial", "Meshes");
            }

            // 1. Excavate terrain mesh under pond (depress Ground_03 vertices organically)
            ExcavateTerrainMesh();

            // 2. Reposition conflicting Environment2 clearing props to shore
            RepositionClearingProps();

            // 3. Create Root Pond GameObject
            GameObject pondRoot = new GameObject(POND_ROOT_NAME);
            pondRoot.transform.position = POND_CENTER;
            pondRoot.transform.rotation = Quaternion.identity;
            pondRoot.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(pondRoot, "Build Tutorial Pond");

            // 4. Create Pond Basin (authentic earthen berm & sunken crater bowl)
            CreatePondBasin(pondRoot.transform);

            // 5. Create Recessed Water Surface (organic subdivided planar mesh recessed inside crater)
            CreateWaterSurface(pondRoot.transform);

            // 6. Create Shoreline Rocks Ring along the sunken rim
            CreateShorelineRocks(pondRoot.transform);

            // 7. Create Shoreline Foliage & Floating Lily Pads
            CreateShorelineFoliage(pondRoot.transform);

            // 8. Create Wooden Pier / Gold Panning Station
            CreateWoodenPier(pondRoot.transform);

            // 9. Create 3D Spatial Water Ambience Audio
            CreatePondAudio(pondRoot.transform);

            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log($"[TutorialPondBuilder] Organic Sunken Gold Panning Pond (Kubangan) built at {POND_CENTER} (WaterY: {WATER_LEVEL_Y}, RimY: {RIM_CREST_Y}, BedY: {BED_DEPTH_Y}) and saved successfully!");
        }

        private static void ExcavateTerrainMesh()
        {
            GameObject groundGo = GameObject.Find("Ground_03");
            if (groundGo == null) return;

            MeshFilter mf = groundGo.GetComponent<MeshFilter>();
            MeshCollider mc = groundGo.GetComponent<MeshCollider>();
            if (mf == null) return;

            Mesh sourceMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GROUND_FBX_PATH);
            if (sourceMesh == null) sourceMesh = mf.sharedMesh;

            Mesh excavatedMesh = UnityEngine.Object.Instantiate(sourceMesh);
            excavatedMesh.name = "Ground_03_Excavated";

            Vector3[] verts = excavatedMesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 worldV = groundGo.transform.TransformPoint(verts[i]);
                float dx = (worldV.x - POND_CENTER.x) / RADIUS_X;
                float dz = (worldV.z - POND_CENTER.z) / RADIUS_Z;
                float angle = Mathf.Atan2(dz, dx);
                float organicFactor = GetOrganicRadiusFactor(angle);
                float r = Mathf.Sqrt(dx * dx + dz * dz) / organicFactor;

                // Depress vertices inside r < 1.05 safely below basin bed
                if (r < 1.05f)
                {
                    float targetY = Mathf.Lerp(-0.15f, worldV.y, Mathf.SmoothStep(0f, 1f, r));
                    worldV.y = Mathf.Min(worldV.y, targetY);
                    verts[i] = groundGo.transform.InverseTransformPoint(worldV);
                }
            }

            excavatedMesh.vertices = verts;
            excavatedMesh.RecalculateNormals();
            excavatedMesh.RecalculateBounds();

            string assetPath = $"{MESH_DIR}/Ground_03_Excavated.asset";
            AssetDatabase.CreateAsset(excavatedMesh, assetPath);

            Undo.RecordObject(mf, "Excavate Ground_03 MeshFilter");
            mf.sharedMesh = excavatedMesh;

            if (mc != null)
            {
                Undo.RecordObject(mc, "Excavate Ground_03 MeshCollider");
                mc.sharedMesh = excavatedMesh;
            }
        }

        private static void RepositionClearingProps()
        {
            GameObject env = GameObject.Find("Environment2");
            if (env == null) return;

            // Move props that were inside the clearing to the outer shoreline banks
            Transform tree03 = env.transform.Find("Tree_03");
            if (tree03 != null)
            {
                Undo.RecordObject(tree03, "Reposition Tree_03");
                tree03.position = new Vector3(-3.5f, 0.40f, 3.5f);
            }

            Transform treeForeground = env.transform.Find("Tree_04 (2)");
            if (treeForeground != null)
            {
                Undo.RecordObject(treeForeground, "Reposition Tree_04 (2)");
                treeForeground.position = new Vector3(3.0f, 0.40f, 5.0f);
                treeForeground.rotation = Quaternion.Euler(0f, 65f, 0f);
            }

            Transform t1 = env.transform.Find("Tree_05 (1)");
            if (t1 != null)
            {
                Undo.RecordObject(t1, "Reposition Tree_05 (1)");
                t1.position = new Vector3(-3.2f, 0.42f, 0.0f);
                t1.rotation = Quaternion.Euler(0f, 45f, 0f);
            }

            Transform t2 = env.transform.Find("Tree_05 (2)");
            if (t2 != null)
            {
                Undo.RecordObject(t2, "Reposition Tree_05 (2)");
                t2.position = new Vector3(6.8f, 0.52f, 9.8f);
                t2.rotation = Quaternion.Euler(0f, 190f, 0f);
            }

            Transform tEast = env.transform.Find("Tree_02 (9)");
            if (tEast != null)
            {
                Undo.RecordObject(tEast, "Reposition Tree_02 (9)");
                tEast.position = new Vector3(12.5f, -0.05f, -4.5f);
            }

            Transform tSouth1 = env.transform.Find("Tree_04");
            if (tSouth1 != null)
            {
                Undo.RecordObject(tSouth1, "Reposition Tree_04");
                tSouth1.position = new Vector3(9.5f, -0.08f, -9.0f);
            }

            Transform tSouth2 = env.transform.Find("Tree_04 (6)");
            if (tSouth2 != null)
            {
                Undo.RecordObject(tSouth2, "Reposition Tree_04 (6)");
                tSouth2.position = new Vector3(1.5f, -0.08f, -9.5f);
            }

            Transform stump = env.transform.Find("Stump_01 (1)");
            if (stump != null)
            {
                Undo.RecordObject(stump, "Reposition Stump_01 (1)");
                stump.position = new Vector3(11.5f, 0.32f, 2.5f);
            }

            Transform bush2 = env.transform.Find("Bush_03 (2)");
            if (bush2 != null)
            {
                Undo.RecordObject(bush2, "Reposition Bush_03 (2)");
                bush2.position = new Vector3(11.2f, 0.32f, 2.0f);
            }

            Transform flowers2 = env.transform.Find("Flowers_02");
            if (flowers2 != null)
            {
                Undo.RecordObject(flowers2, "Reposition Flowers_02");
                flowers2.position = new Vector3(11.0f, 0.32f, 3.0f);
            }

            Transform grass1 = env.transform.Find("Grass_01 (1)");
            if (grass1 != null)
            {
                Undo.RecordObject(grass1, "Reposition Grass_01 (1)");
                grass1.position = new Vector3(11.3f, 0.32f, 2.2f);
            }

            Transform shroom1 = env.transform.Find("Mushroom_01");
            if (shroom1 != null)
            {
                Undo.RecordObject(shroom1, "Reposition Mushroom_01");
                shroom1.position = new Vector3(11.1f, 0.32f, 1.8f);
            }

            Transform r4_3 = env.transform.Find("Rock_04 (3)");
            if (r4_3 != null)
            {
                Undo.RecordObject(r4_3, "Reposition Rock_04 (3)");
                r4_3.position = new Vector3(-2.8f, 0.40f, -2.5f);
            }

            Transform r1_1 = env.transform.Find("Rock_01 (1)");
            if (r1_1 != null)
            {
                Undo.RecordObject(r1_1, "Reposition Rock_01 (1)");
                r1_1.position = new Vector3(-3.0f, 0.42f, -1.2f);
            }

            Transform r1 = env.transform.Find("Rock_05 (1)");
            if (r1 != null)
            {
                Undo.RecordObject(r1, "Reposition Rock_05 (1)");
                r1.position = new Vector3(-2.6f, 0.42f, 1.8f);
            }

            Transform r5 = env.transform.Find("Rock_05 (5)");
            if (r5 != null)
            {
                Undo.RecordObject(r5, "Reposition Rock_05 (5)");
                r5.position = new Vector3(1.2f, 0.45f, 4.8f);
            }

            Transform r4 = env.transform.Find("Rock_04");
            if (r4 != null)
            {
                Undo.RecordObject(r4, "Reposition Rock_04");
                r4.position = new Vector3(11.8f, 0.43f, 4.8f);
            }

            Transform r3 = env.transform.Find("Rock_03");
            if (r3 != null)
            {
                Undo.RecordObject(r3, "Reposition Rock_03");
                r3.position = new Vector3(11.6f, 0.47f, 7.8f);
            }

            Transform b1 = env.transform.Find("Bush_03 (1)");
            if (b1 != null)
            {
                Undo.RecordObject(b1, "Reposition Bush_03 (1)");
                b1.position = new Vector3(-3.2f, 0.42f, 2.2f);
            }
        }

        private static void CreatePondBasin(Transform parent)
        {
            GameObject basinGo = new GameObject("Pond_Basin");
            basinGo.transform.SetParent(parent, false);
            basinGo.transform.localPosition = Vector3.zero;

            int radialSegments = 54;
            int ringSegments = 8;
            UnityEngine.Mesh mesh = new UnityEngine.Mesh();
            mesh.name = "Pond_Basin_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            // Center vertex at deep basin bottom (index 0)
            verts.Add(new Vector3(0f, BED_DEPTH_Y, 0f));
            uvs.Add(UV_STONE_BED);

            // 8 concentric rings forming:
            // Rings 1..3: Deep stone basin bed (r = 0.0 to 0.55, pebbles/stone bed: 0.08m to 0.22m)
            // Rings 4..5: Inner earthen bank slope (r = 0.55 to 0.90, dark earth/dirt: 0.22m to 0.38m)
            // Ring 6: Raised earthen berm crest lip (r = 0.90 to 1.05, meadow grass at RIM_CREST_Y = 0.52m)
            // Ring 7: Outer berm skirt sloping down flush to Ground_03 (r = 1.05 to 1.20, grass: 0.52m to 0.45m)
            // Ring 8: Seamless flat grass apron extending over Ground_03 (r = 1.20 to 1.35, grass: 0.45m)
            for (int r = 1; r <= ringSegments; r++)
            {
                float radiusMul;
                float height;
                Vector2 uv;

                if (r <= 3)
                {
                    // Zone 1: River stone bed
                    float t = (float)r / 3f;
                    radiusMul = t * 0.55f;
                    height = Mathf.Lerp(BED_DEPTH_Y, 0.22f, Mathf.Pow(t, 1.3f));
                    uv = UV_STONE_BED;
                }
                else if (r <= 5)
                {
                    // Zone 2: Submerged & waterline earthen bank slope
                    float t = (float)(r - 3) / 2f;
                    radiusMul = Mathf.Lerp(0.55f, 0.90f, t);
                    height = Mathf.Lerp(0.22f, WATER_LEVEL_Y + 0.02f, t);
                    uv = UV_DIRT;
                }
                else if (r == 6)
                {
                    // Zone 3: Raised berm crest lip (meadow grass)
                    radiusMul = 1.05f;
                    height = RIM_CREST_Y;
                    uv = UV_GRASS;
                }
                else if (r == 7)
                {
                    // Zone 4: Outer berm slope meeting Ground_03
                    radiusMul = 1.20f;
                    height = GROUND_LEVEL_Y;
                    uv = UV_GRASS;
                }
                else
                {
                    // Zone 5: Wide seamless apron over Ground_03
                    radiusMul = 1.35f;
                    height = GROUND_LEVEL_Y;
                    uv = UV_GRASS;
                }

                for (int i = 0; i < radialSegments; i++)
                {
                    float angle = (float)i / radialSegments * Mathf.PI * 2f;
                    float organicFactor = GetOrganicRadiusFactor(angle);

                    float x = Mathf.Cos(angle) * RADIUS_X * radiusMul * organicFactor;
                    float z = Mathf.Sin(angle) * RADIUS_Z * radiusMul * organicFactor;

                    // Subtle crest height variation along the berm lip
                    float h = height;
                    if (r == 6)
                    {
                        h += Mathf.Sin(angle * 4f) * 0.015f;
                    }

                    verts.Add(new Vector3(x, h, z));
                    uvs.Add(uv);
                }
            }

            // Center fan triangles: Upward normals
            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                tris.Add(0);
                tris.Add(1 + next);
                tris.Add(1 + i);
            }

            // Concentric ring triangles: Upward normals
            for (int r = 1; r < ringSegments; r++)
            {
                int ringStart = 1 + (r - 1) * radialSegments;
                int nextRingStart = 1 + r * radialSegments;

                for (int i = 0; i < radialSegments; i++)
                {
                    int next = (i + 1) % radialSegments;

                    int c1 = ringStart + i;
                    int c2 = ringStart + next;
                    int n1 = nextRingStart + i;
                    int n2 = nextRingStart + next;

                    tris.Add(c1);
                    tris.Add(c2);
                    tris.Add(n1);

                    tris.Add(c2);
                    tris.Add(n2);
                    tris.Add(n1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            string meshAssetPath = $"{MESH_DIR}/Pond_Basin.asset";
            AssetDatabase.CreateAsset(mesh, meshAssetPath);

            var mf = basinGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = basinGo.AddComponent<MeshRenderer>();
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(NATURE_MAT_PATH);
            if (mat != null) mr.sharedMaterial = mat;

            var mc = basinGo.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }

        private static void CreateWaterSurface(Transform parent)
        {
            GameObject waterGo = new GameObject("Pond_Water_Surface");
            waterGo.transform.SetParent(parent, false);
            // Water sits safely recessed deep inside crater at WATER_LEVEL_Y (0.36m), 16 cm below rim
            waterGo.transform.localPosition = new Vector3(0f, WATER_LEVEL_Y, 0f);

            int radialSegments = 48;
            int ringSegments = 8;
            UnityEngine.Mesh mesh = new UnityEngine.Mesh();
            mesh.name = "Pond_Water_Mesh";

            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> tris = new List<int>();

            // Center vertex
            verts.Add(Vector3.zero);
            uvs.Add(new Vector2(0.5f, 0.5f));
            normals.Add(Vector3.up);

            // Water radius fits cleanly inside the inner bank walls (radiusMul = 0.86)
            float waterBaseRadX = RADIUS_X * 0.86f;
            float waterBaseRadZ = RADIUS_Z * 0.86f;

            for (int r = 1; r <= ringSegments; r++)
            {
                float t = (float)r / ringSegments;

                for (int i = 0; i < radialSegments; i++)
                {
                    float angle = (float)i / radialSegments * Mathf.PI * 2f;
                    float organicFactor = GetOrganicRadiusFactor(angle);

                    float x = Mathf.Cos(angle) * waterBaseRadX * t * organicFactor;
                    float z = Mathf.Sin(angle) * waterBaseRadZ * t * organicFactor;

                    verts.Add(new Vector3(x, 0f, z));
                    uvs.Add(new Vector2(x * 0.15f + 0.5f, z * 0.15f + 0.5f));
                    normals.Add(Vector3.up);
                }
            }

            // Center fan triangles: Upward normal (Y > 0.9)
            for (int i = 0; i < radialSegments; i++)
            {
                int next = (i + 1) % radialSegments;
                tris.Add(0);
                tris.Add(1 + next);
                tris.Add(1 + i);
            }

            // Concentric rings
            for (int r = 1; r < ringSegments; r++)
            {
                int ringStart = 1 + (r - 1) * radialSegments;
                int nextRingStart = 1 + r * radialSegments;

                for (int i = 0; i < radialSegments; i++)
                {
                    int next = (i + 1) % radialSegments;

                    int c1 = ringStart + i;
                    int c2 = ringStart + next;
                    int n1 = nextRingStart + i;
                    int n2 = nextRingStart + next;

                    tris.Add(c1);
                    tris.Add(c2);
                    tris.Add(n1);

                    tris.Add(c2);
                    tris.Add(n2);
                    tris.Add(n1);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetNormals(normals);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateBounds();

            string meshAssetPath = $"{MESH_DIR}/Pond_Water.asset";
            AssetDatabase.CreateAsset(mesh, meshAssetPath);

            var mf = waterGo.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            var mr = waterGo.AddComponent<MeshRenderer>();
            Material waterMat = AssetDatabase.LoadAssetAtPath<Material>(WATER_MAT_PATH);
            if (waterMat != null)
            {
                mr.sharedMaterial = waterMat;
            }
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = true;

            var boxCol = waterGo.AddComponent<BoxCollider>();
            boxCol.isTrigger = true;
            boxCol.center = new Vector3(0f, -0.2f, 0f);
            boxCol.size = new Vector3(waterBaseRadX * 2.2f, 0.5f, waterBaseRadZ * 2.2f);
        }

        private static void CreateShorelineRocks(Transform parent)
        {
            GameObject rocksContainer = new GameObject("Shoreline_Rocks");
            rocksContainer.transform.SetParent(parent, false);
            rocksContainer.transform.localPosition = Vector3.zero;

            string[] rockPrefabPaths = new string[]
            {
                "Assets/SimpleNaturePack/Prefabs/Rock_01.prefab",
                "Assets/SimpleNaturePack/Prefabs/Rock_02.prefab",
                "Assets/SimpleNaturePack/Prefabs/Rock_03.prefab",
                "Assets/SimpleNaturePack/Prefabs/Rock_04.prefab",
                "Assets/SimpleNaturePack/Prefabs/Rock_05.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Rock_1.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Rock_2.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Rock_3.prefab"
            };

            int count = 40;
            for (int i = 0; i < count; i++)
            {
                float angle = (float)i / count * Mathf.PI * 2f;
                float deg = angle * Mathf.Rad2Deg;

                // Leave pier and player shoreline path open around 34-62 degrees (North-East, towards cabin)
                if (deg >= 34f && deg <= 62f) continue;

                float organicFactor = GetOrganicRadiusFactor(angle);

                // Rocks sit firmly along the berm rim crest (r = 0.96 to 1.04)
                float rMul = 0.98f + Mathf.Sin(i * 3.1f) * 0.04f;
                float x = Mathf.Cos(angle) * RADIUS_X * rMul * organicFactor;
                float z = Mathf.Sin(angle) * RADIUS_Z * rMul * organicFactor;

                // Rocks sit embedded into the rim crest
                float y = RIM_CREST_Y - 0.05f;

                string prefabPath = rockPrefabPaths[i % rockPrefabPaths.Length];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) continue;

                GameObject rock = (GameObject)PrefabUtility.InstantiatePrefab(prefab, rocksContainer.transform);
                rock.name = $"Shore_Rock_{i:D2}";
                rock.transform.localPosition = new Vector3(x, y, z);

                float rotY = (i * 47f) % 360f;
                float rotX = Mathf.Sin(i * 1.5f) * 8f;
                float rotZ = Mathf.Cos(i * 2.3f) * 8f;
                rock.transform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);

                // Natural boulder scaling: 0.85m to 1.35m
                float scaleMul = 0.90f + Mathf.Abs(Mathf.Sin(i * 2.3f)) * 0.45f;
                rock.transform.localScale = Vector3.one * scaleMul;

                if (rock.GetComponent<Collider>() == null && rock.GetComponentInChildren<Collider>() == null)
                {
                    var mf = rock.GetComponentInChildren<MeshFilter>();
                    if (mf != null && mf.sharedMesh != null)
                    {
                        var mc = mf.gameObject.AddComponent<MeshCollider>();
                        mc.sharedMesh = mf.sharedMesh;
                    }
                }
            }
        }

        private static void CreateShorelineFoliage(Transform parent)
        {
            GameObject foliageContainer = new GameObject("Shoreline_Foliage");
            foliageContainer.transform.SetParent(parent, false);
            foliageContainer.transform.localPosition = Vector3.zero;

            string[] foliagePaths = new string[]
            {
                "Assets/SimpleNaturePack/Prefabs/Grass_01.prefab",
                "Assets/SimpleNaturePack/Prefabs/Grass_02.prefab",
                "Assets/SimpleNaturePack/Prefabs/Flowers_01.prefab",
                "Assets/SimpleNaturePack/Prefabs/Flowers_02.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_1.prefab",
                "Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_3.prefab"
            };

            int foliageCount = 32;
            for (int i = 0; i < foliageCount; i++)
            {
                float angle = (float)i / foliageCount * Mathf.PI * 2f + 0.1f;
                float deg = angle * Mathf.Rad2Deg;
                if (deg >= 24f && deg <= 38f) continue; // Keep pier path clear

                float organicFactor = GetOrganicRadiusFactor(angle);
                // Foliage populates the outer berm slope (r = 1.12 to 1.25)
                float rMul = 1.15f + Mathf.Sin(i * 2.7f) * 0.06f;

                float x = Mathf.Cos(angle) * RADIUS_X * rMul * organicFactor;
                float z = Mathf.Sin(angle) * RADIUS_Z * rMul * organicFactor;
                float y = Mathf.Lerp(RIM_CREST_Y, GROUND_LEVEL_Y, 0.65f);

                string path = foliagePaths[i % foliagePaths.Length];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                GameObject foliage = (GameObject)PrefabUtility.InstantiatePrefab(prefab, foliageContainer.transform);
                foliage.name = $"Shore_Foliage_{i:D2}";
                foliage.transform.localPosition = new Vector3(x, y, z);
                foliage.transform.localRotation = Quaternion.Euler(0f, (i * 61f) % 360f, 0f);
                foliage.transform.localScale = Vector3.one * (0.8f + Mathf.Abs(Mathf.Sin(i)) * 0.35f);
            }

            CreateLilyPads(foliageContainer.transform);
        }

        private static void CreateLilyPads(Transform parent)
        {
            GameObject lilyGroup = new GameObject("Pond_LilyPads");
            lilyGroup.transform.SetParent(parent, false);
            lilyGroup.transform.localPosition = Vector3.zero;

            Vector2[] clusterCenters = new Vector2[]
            {
                new Vector2(-2.6f, 1.4f),
                new Vector2(0.4f, 2.2f),
                new Vector2(-2.5f, -1.5f),
                new Vector2(0.3f, -2.2f),
                new Vector2(2.6f, -1.8f),
                new Vector2(-1.0f, 0.0f)
            };

            Material natureMat = AssetDatabase.LoadAssetAtPath<Material>(NATURE_MAT_PATH);
            GameObject flowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Flower_1.prefab");

            UnityEngine.Mesh lilyMesh = GenerateLilyPadMesh();
            string meshAssetPath = $"{MESH_DIR}/Pond_LilyPad.asset";
            AssetDatabase.CreateAsset(lilyMesh, meshAssetPath);

            for (int c = 0; c < clusterCenters.Length; c++)
            {
                GameObject cluster = new GameObject($"LilyCluster_{c + 1}");
                cluster.transform.SetParent(lilyGroup.transform, false);
                cluster.transform.localPosition = new Vector3(clusterCenters[c].x, WATER_LEVEL_Y + 0.003f, clusterCenters[c].y);

                int padCount = 2 + (c % 2);
                for (int p = 0; p < padCount; p++)
                {
                    float angle = (float)p / padCount * Mathf.PI * 2f;
                    float dist = 0.32f + p * 0.14f;
                    Vector3 offset = new Vector3(Mathf.Cos(angle) * dist, 0.002f * p, Mathf.Sin(angle) * dist);

                    GameObject pad = new GameObject($"LilyPad_{p + 1}");
                    pad.transform.SetParent(cluster.transform, false);
                    pad.transform.localPosition = offset;
                    pad.transform.localRotation = Quaternion.Euler(0f, (p * 85f) % 360f, 0f);
                    pad.transform.localScale = Vector3.one * (0.42f + (p * 0.12f));

                    var mf = pad.AddComponent<MeshFilter>();
                    mf.sharedMesh = lilyMesh;

                    var mr = pad.AddComponent<MeshRenderer>();
                    if (natureMat != null) mr.sharedMaterial = natureMat;
                    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    if (p == 0 && flowerPrefab != null)
                    {
                        GameObject flower = (GameObject)PrefabUtility.InstantiatePrefab(flowerPrefab, pad.transform);
                        flower.name = "Water_Flower";
                        flower.transform.localPosition = new Vector3(0f, 0.02f, 0f);
                        flower.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
                        flower.transform.localScale = Vector3.one * 0.45f;
                    }
                }
            }
        }

        private static UnityEngine.Mesh GenerateLilyPadMesh()
        {
            UnityEngine.Mesh mesh = new UnityEngine.Mesh();
            mesh.name = "LilyPad_Mesh";

            int segments = 12;
            List<Vector3> verts = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> tris = new List<int>();

            verts.Add(Vector3.zero);
            uvs.Add(UV_LILY_GREEN);

            float startAngle = 0.35f;
            float endAngle = Mathf.PI * 2f - 0.35f;

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = Mathf.Lerp(startAngle, endAngle, t);
                float x = Mathf.Cos(angle) * 0.5f;
                float z = Mathf.Sin(angle) * 0.5f;
                verts.Add(new Vector3(x, 0f, z));
                uvs.Add(UV_LILY_GREEN);
            }

            for (int i = 1; i <= segments; i++)
            {
                tris.Add(0);
                tris.Add(i + 1);
                tris.Add(i);
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void CreateWoodenPier(Transform parent)
        {
            GameObject pierGo = new GameObject("Pond_Pier");
            pierGo.transform.SetParent(parent, false);
            // Position on north-east bank facing south-west into the organic sunken pond
            pierGo.transform.localPosition = new Vector3(4.0f, 0f, 2.3f);
            pierGo.transform.localRotation = Quaternion.Euler(0f, 225f, 0f);

            Material woodMat = AssetDatabase.LoadAssetAtPath<Material>(NATURE_MAT_PATH);

            UnityEngine.Mesh plankMesh = GenerateBoxMeshWithUV(UV_WOOD_PLANK, "Pier_Plank_Mesh");
            string plankAssetPath = $"{MESH_DIR}/Pier_Plank.asset";
            AssetDatabase.CreateAsset(plankMesh, plankAssetPath);

            // 1. Four wooden support posts plunging deep into the water bed
            float[] postX = new float[] { -0.7f, 0.7f };
            float[] postZ = new float[] { 0.4f, 1.5f };
            foreach (float px in postX)
            {
                foreach (float pz in postZ)
                {
                    GameObject post = new GameObject($"Pier_Post_{px:F1}_{pz:F1}");
                    post.transform.SetParent(pierGo.transform, false);
                    post.transform.localPosition = new Vector3(px, 0.12f, pz);
                    post.transform.localScale = new Vector3(0.18f, 0.85f, 0.18f);
                    var mf = post.AddComponent<MeshFilter>();
                    mf.sharedMesh = plankMesh;
                    var mr = post.AddComponent<MeshRenderer>();
                    if (woodMat != null) mr.sharedMaterial = woodMat;
                }
            }

            // 2. Main wooden deck planks (8 planks extending out over the sunken water)
            int plankCount = 8;
            float plankWidth = 0.28f;
            for (int p = 0; p < plankCount; p++)
            {
                GameObject plank = new GameObject($"Pier_Plank_{p + 1}");
                plank.transform.SetParent(pierGo.transform, false);
                float plankZ = -0.1f + p * plankWidth;
                plank.transform.localPosition = new Vector3(0f, WATER_LEVEL_Y + 0.12f, plankZ);
                plank.transform.localScale = new Vector3(1.65f, 0.07f, plankWidth * 0.92f);
                var mf = plank.AddComponent<MeshFilter>();
                mf.sharedMesh = plankMesh;
                var mr = plank.AddComponent<MeshRenderer>();
                if (woodMat != null) mr.sharedMaterial = woodMat;
            }

            // 3. Pier Walkway Solid Collider for player walking and gold panning staging
            var col = pierGo.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, WATER_LEVEL_Y + 0.12f, 0.85f);
            col.size = new Vector3(1.7f, 0.14f, 2.4f);

            // 4. Moored decorative wooden crate at pier side
            GameObject cratePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Crate.prefab");
            if (cratePrefab != null)
            {
                GameObject crate = (GameObject)PrefabUtility.InstantiatePrefab(cratePrefab, pierGo.transform);
                crate.name = "Pier_Crate";
                crate.transform.localPosition = new Vector3(0.65f, WATER_LEVEL_Y + 0.16f, 0.2f);
                crate.transform.localRotation = Quaternion.Euler(0f, 18f, 0f);
                crate.transform.localScale = Vector3.one * 0.75f;
            }

            // 5. Decorative lantern on pier post
            GameObject lanternPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/OccaSoftware/Low Poly Fantasy Village/Prefabs/Lantern.prefab");
            if (lanternPrefab != null)
            {
                GameObject lantern = (GameObject)PrefabUtility.InstantiatePrefab(lanternPrefab, pierGo.transform);
                lantern.name = "Pier_Lantern";
                lantern.transform.localPosition = new Vector3(-0.7f, WATER_LEVEL_Y + 0.22f, 1.5f);
                lantern.transform.localScale = Vector3.one * 0.85f;
            }
        }

        private static UnityEngine.Mesh GenerateBoxMeshWithUV(Vector2 uvCoord, string meshName)
        {
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            UnityEngine.Mesh origMesh = tempCube.GetComponent<MeshFilter>().sharedMesh;

            UnityEngine.Mesh newMesh = new UnityEngine.Mesh();
            newMesh.name = meshName;
            newMesh.vertices = origMesh.vertices;
            newMesh.triangles = origMesh.triangles;
            newMesh.normals = origMesh.normals;

            Vector2[] uvs = new Vector2[origMesh.vertexCount];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = uvCoord;
            }
            newMesh.uv = uvs;
            newMesh.RecalculateBounds();

            UnityEngine.Object.DestroyImmediate(tempCube);
            return newMesh;
        }

        private static void CreatePondAudio(Transform parent)
        {
            GameObject audioGo = new GameObject("Pond_Audio");
            audioGo.transform.SetParent(parent, false);
            audioGo.transform.localPosition = new Vector3(0f, WATER_LEVEL_Y, 0f);

            AudioSource audio = audioGo.AddComponent<AudioSource>();
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(WATER_AUDIO_PATH);
            if (clip != null) audio.clip = clip;

            audio.playOnAwake = true;
            audio.loop = true;
            audio.volume = 0.16f;
            audio.spatialBlend = 1.0f;
            audio.rolloffMode = AudioRolloffMode.Logarithmic;
            audio.minDistance = 3.0f;
            audio.maxDistance = 24.0f;
            audio.dopplerLevel = 0f;
        }

        [MenuItem("ProspectorJourney/Capture Pond Eye Level View")]
        public static void CaptureEyeLevelView()
        {
            // 1. Player eye level view from path looking west across pier
            CaptureView(
                new Vector3(6.5f, 1.10f, 3.8f), 
                new Vector3(2.5f, 0.35f, -0.6f), 
                "C:/Users/almer/.gemini/antigravity/brain/f44d3472-0851-4593-9fa3-dd038045a5c4/tutorial_pond_eyelevel_verified.png"
            );

            // 2. South shoreline view matching user screenshot media_1791867328845_caa3be9f.png
            CaptureView(
                new Vector3(4.2f, 0.95f, -6.8f), 
                new Vector3(4.2f, 0.35f, -0.5f), 
                "C:/Users/almer/.gemini/antigravity/brain/f44d3472-0851-4593-9fa3-dd038045a5c4/tutorial_pond_south_verified.png"
            );

            // 3. Elevated perspective showing complete organic non-oval shape
            CaptureView(
                new Vector3(10.5f, 7.0f, 5.0f), 
                new Vector3(4.2f, 0.35f, -1.0f), 
                "C:/Users/almer/.gemini/antigravity/brain/f44d3472-0851-4593-9fa3-dd038045a5c4/tutorial_pond_aerial_verified.png"
            );

            Debug.Log("[TutorialPondBuilder] All 3 verification captures saved successfully!");
        }

        private static void CaptureView(Vector3 position, Vector3 lookAtTarget, string outputPath)
        {
            GameObject camGo = new GameObject("Temp_Capture_Cam");
            Camera cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 65f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            camGo.transform.position = position;
            camGo.transform.LookAt(lookAtTarget);

            RenderTexture rt = new RenderTexture(1280, 720, 24);
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            tex.Apply();

            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(outputPath, bytes);

            RenderTexture.active = null;
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(camGo);

            Debug.Log($"[TutorialPondBuilder] Capture saved to: {outputPath}");
        }

        [MenuItem("ProspectorJourney/Remove Tutorial Pond")]
        public static void RemovePond()
        {
            var pond = GameObject.Find(POND_ROOT_NAME);
            if (pond != null)
            {
                Undo.DestroyObjectImmediate(pond);
                Debug.Log("[TutorialPondBuilder] Removed Tutorial Pond.");
            }

            GameObject groundGo = GameObject.Find("Ground_03");
            if (groundGo != null)
            {
                Mesh originalMesh = AssetDatabase.LoadAssetAtPath<Mesh>(GROUND_FBX_PATH);
                if (originalMesh != null)
                {
                    var mf = groundGo.GetComponent<MeshFilter>();
                    if (mf != null) mf.sharedMesh = originalMesh;
                    var mc = groundGo.GetComponent<MeshCollider>();
                    if (mc != null) mc.sharedMesh = originalMesh;
                }
            }
        }
    }
}
