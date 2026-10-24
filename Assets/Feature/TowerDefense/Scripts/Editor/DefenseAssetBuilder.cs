using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.Editor
{
    public static class DefenseAssetBuilder
    {
        public const string ROOT = "Assets/Feature/TowerDefense";
        public const string SCENE_PATH = "Assets/Scenes/Main/TowerDefense.unity";

        /// <summary>Generate the initial assets once, without replacing existing assets or scenes.</summary>
        [InitializeOnLoadMethod]
        private static void ScheduleInitialCreation()
        {
            EditorApplication.delayCall += CreateInitialAssets;
        }

        /// <summary>Create the playable scene if this feature has not been initialized yet.</summary>
        [MenuItem("Tools/Tower Defense/Create Missing Game Assets")]
        public static void CreateInitialAssets()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || File.Exists(SCENE_PATH))
                return;

            Scene previousScene = SceneManager.GetActiveScene();
            bool replaceEmptyScene = string.IsNullOrEmpty(previousScene.path);
            if (replaceEmptyScene && previousScene.isDirty)
            {
                Debug.LogWarning("Save the untitled scene, then use Tools > Tower Defense > Create Missing Game Assets.");
                return;
            }

            EnsureFolder(ROOT + "/Materials");
            EnsureFolder(ROOT + "/Prefabs");
            EnsureFolder(ROOT + "/ScriptableObjects");
            EnsureFolder("Assets/Scenes/Main");

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                throw new InvalidOperationException("Tower Defense requires the project's URP Lit shader.");

            Material ground = CreateMaterial("Ground", new Color(0.055f, 0.12f, 0.18f), shader);
            Material path = CreateMaterial("Path", new Color(0.28f, 0.36f, 0.43f), shader);
            Material pad = CreateMaterial("BuildPad", new Color(0.08f, 0.65f, 0.74f), shader);
            Material pulse = CreateMaterial("PulseTower", new Color(0.18f, 0.9f, 0.62f), shader);
            Material rail = CreateMaterial("RailTower", new Color(0.55f, 0.38f, 0.95f), shader);
            Material enemyMaterial = CreateMaterial("Enemy", new Color(0.95f, 0.23f, 0.28f), shader);
            Material core = CreateMaterial("Core", new Color(1f, 0.7f, 0.12f), shader);
            Material shot = CreateMaterial("Shot", new Color(1f, 0.95f, 0.45f), shader);

            Scene generatedScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                replaceEmptyScene ? NewSceneMode.Single : NewSceneMode.Additive);
            SceneManager.SetActiveScene(generatedScene);
            try
            {
                GameObject tilePrefab = CreatePrimitivePrefab("Tile", PrimitiveType.Cube,
                    new Vector3(0.94f, 0.24f, 0.94f), ground);
                GameObject basePrefab = CreatePrimitivePrefab("Core", PrimitiveType.Cylinder,
                    new Vector3(0.8f, 0.6f, 0.8f), core);
                DefenseEnemy enemyPrefab = CreateEnemyPrefab(enemyMaterial);
                DefenseTower pulsePrefab = CreateTowerPrefab("PulseTower", pulse);
                DefenseTower railPrefab = CreateTowerPrefab("RailTower", rail);
                TowerDefinition pulseDefinition = CreateTowerDefinition("Pulse", 60, 3.0f, 0.55f, 14, pulsePrefab);
                TowerDefinition railDefinition = CreateTowerDefinition("Rail", 100, 4.2f, 1.4f, 50, railPrefab);
                DefenseSettings settings = CreateSettings(pulseDefinition, railDefinition,
                    tilePrefab, basePrefab, enemyPrefab, ground, path, pad, shot);

                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 6.8f;
                camera.rect = new Rect(0.26f, 0f, 0.74f, 1f);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.025f, 0.045f, 0.08f);
                camera.nearClipPlane = 0.1f;
                camera.farClipPlane = 100f;
                camera.transform.position = new Vector3(0f, 15f, -11f);
                camera.transform.LookAt(Vector3.zero);

                var lightObject = new GameObject("Sun", typeof(Light));
                Light light = lightObject.GetComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.5f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                RenderSettings.ambientLight = new Color(0.6f, 0.65f, 0.75f);

                DefenseGame game = new GameObject("Tower Defense", typeof(DefenseGame)).GetComponent<DefenseGame>();
                var serializedGame = new SerializedObject(game);
                serializedGame.FindProperty("_settings").objectReferenceValue = settings;
                serializedGame.FindProperty("_camera").objectReferenceValue = camera;
                serializedGame.ApplyModifiedPropertiesWithoutUndo();
                if (!EditorSceneManager.SaveScene(generatedScene, SCENE_PATH))
                    throw new IOException("Could not save the Tower Defense scene.");
            }
            finally
            {
                if (replaceEmptyScene)
                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                else
                {
                    SceneManager.SetActiveScene(previousScene);
                    EditorSceneManager.CloseScene(generatedScene, true);
                }
            }

            if (!EditorBuildSettings.scenes.Any(scene => scene.path == SCENE_PATH))
                EditorBuildSettings.scenes = EditorBuildSettings.scenes
                    .Concat(new[] { new EditorBuildSettingsScene(SCENE_PATH, true) }).ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("Tower Defense ready. Open " + SCENE_PATH + " and press Play.");
        }

        /// <summary>Create missing folders through Unity's asset database.</summary>
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = path.Substring(0, path.LastIndexOf('/'));
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, path.Substring(path.LastIndexOf('/') + 1));
        }

        /// <summary>Create a shared URP material without overwriting customizations.</summary>
        private static Material CreateMaterial(string name, Color color, Shader shader)
        {
            string path = ROOT + "/Materials/" + name + ".mat";
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
                return existing;
            var material = new Material(shader) { name = name, color = color };
            material.SetFloat("_Smoothness", 0.3f);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>Save a simple reusable primitive prefab.</summary>
        private static GameObject CreatePrimitivePrefab(string name, PrimitiveType primitive, Vector3 scale,
            Material material)
        {
            string path = ROOT + "/Prefabs/" + name + ".prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
                return existing;
            GameObject instance = GameObject.CreatePrimitive(primitive);
            instance.name = name;
            instance.transform.localScale = scale;
            instance.GetComponent<Renderer>().sharedMaterial = material;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab;
        }

        /// <summary>Create an enemy visual and movement component as a prefab.</summary>
        private static DefenseEnemy CreateEnemyPrefab(Material material)
        {
            string path = ROOT + "/Prefabs/Enemy.prefab";
            DefenseEnemy existing = AssetDatabase.LoadAssetAtPath<DefenseEnemy>(path);
            if (existing != null)
                return existing;
            GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            instance.name = "Enemy";
            instance.transform.localScale = Vector3.one * 0.6f;
            instance.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(instance.GetComponent<Collider>());
            instance.AddComponent<DefenseEnemy>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
            UnityEngine.Object.DestroyImmediate(instance);
            return prefab.GetComponent<DefenseEnemy>();
        }

        /// <summary>Create a tower with a rotatable turret and forward-facing barrel.</summary>
        private static DefenseTower CreateTowerPrefab(string name, Material material)
        {
            string path = ROOT + "/Prefabs/" + name + ".prefab";
            DefenseTower existing = AssetDatabase.LoadAssetAtPath<DefenseTower>(path);
            if (existing != null)
                return existing;
            var root = new GameObject(name);
            DefenseTower tower = root.AddComponent<DefenseTower>();
            CreatePart(root.transform, "Pedestal", PrimitiveType.Cylinder,
                new Vector3(0f, 0.28f, 0f), new Vector3(0.65f, 0.28f, 0.65f), material);
            var turret = new GameObject("Turret");
            turret.transform.SetParent(root.transform, false);
            turret.transform.localPosition = Vector3.up * 0.7f;
            CreatePart(turret.transform, "Head", PrimitiveType.Sphere,
                Vector3.zero, Vector3.one * 0.5f, material);
            CreatePart(turret.transform, "Barrel", PrimitiveType.Cube,
                Vector3.forward * 0.35f, new Vector3(0.16f, 0.16f, 0.7f), material);
            var serializedTower = new SerializedObject(tower);
            serializedTower.FindProperty("_turret").objectReferenceValue = turret.transform;
            serializedTower.ApplyModifiedPropertiesWithoutUndo();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab.GetComponent<DefenseTower>();
        }

        /// <summary>Add a non-colliding visual part to a tower prefab.</summary>
        private static void CreatePart(Transform parent, string name, PrimitiveType primitive,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
        }

        /// <summary>Serialize one tower's independently editable balance settings.</summary>
        private static TowerDefinition CreateTowerDefinition(string name, int cost, float range,
            float interval, int damage, DefenseTower prefab)
        {
            string path = ROOT + "/ScriptableObjects/" + name + ".asset";
            TowerDefinition existing = AssetDatabase.LoadAssetAtPath<TowerDefinition>(path);
            if (existing != null)
                return existing;
            TowerDefinition definition = ScriptableObject.CreateInstance<TowerDefinition>();
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_displayName").stringValue = name;
            serialized.FindProperty("_cost").intValue = cost;
            serialized.FindProperty("_range").floatValue = range;
            serialized.FindProperty("_attackInterval").floatValue = interval;
            serialized.FindProperty("_damage").intValue = damage;
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        /// <summary>Serialize the default five waves and all runtime asset references.</summary>
        private static DefenseSettings CreateSettings(TowerDefinition pulse, TowerDefinition rail,
            GameObject tile, GameObject core, DefenseEnemy enemy, Material ground, Material pathMaterial,
            Material pad, Material shot)
        {
            string path = ROOT + "/ScriptableObjects/GameSettings.asset";
            DefenseSettings existing = AssetDatabase.LoadAssetAtPath<DefenseSettings>(path);
            if (existing != null)
                return existing;
            DefenseSettings settings = ScriptableObject.CreateInstance<DefenseSettings>();
            var serialized = new SerializedObject(settings);
            SerializedProperty towers = serialized.FindProperty("_towers");
            towers.arraySize = 2;
            towers.GetArrayElementAtIndex(0).objectReferenceValue = pulse;
            towers.GetArrayElementAtIndex(1).objectReferenceValue = rail;
            serialized.FindProperty("_enemyPrefab").objectReferenceValue = enemy;
            serialized.FindProperty("_tilePrefab").objectReferenceValue = tile;
            serialized.FindProperty("_basePrefab").objectReferenceValue = core;
            serialized.FindProperty("_groundMaterial").objectReferenceValue = ground;
            serialized.FindProperty("_pathMaterial").objectReferenceValue = pathMaterial;
            serialized.FindProperty("_padMaterial").objectReferenceValue = pad;
            serialized.FindProperty("_shotMaterial").objectReferenceValue = shot;
            SerializedProperty waves = serialized.FindProperty("_waves");
            waves.arraySize = 5;
            for (int i = 0; i < waves.arraySize; i++)
            {
                SerializedProperty wave = waves.GetArrayElementAtIndex(i);
                wave.FindPropertyRelative("Count").intValue = 8 + i * 3;
                wave.FindPropertyRelative("Health").intValue = 35 + i * 25;
                wave.FindPropertyRelative("Speed").floatValue = 1.5f + i * 0.15f;
                wave.FindPropertyRelative("SpawnInterval").floatValue = 1f - i * 0.1f;
                wave.FindPropertyRelative("Reward").intValue = 12 + i * 2;
                wave.FindPropertyRelative("ClearBonus").intValue = 35 + i * 10;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(settings, path);
            return settings;
        }
    }
}
