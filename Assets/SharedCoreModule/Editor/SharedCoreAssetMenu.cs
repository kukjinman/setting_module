using System.IO;
using SharedCoreModule;
using UnityEditor;
using UnityEngine;

namespace SharedCoreModule.Editor
{
    public static class SharedCoreAssetMenu
    {
        // Generated assets must live under Assets. Git/registry packages may be read-only.
        private const string ModuleRoot = "Assets/SharedModules/Generated";
        private const string PrefabsFolder = ModuleRoot + "/Prefabs";
        private const string RootName = "Shared Core Root";
        private const string RootPrefabPath = PrefabsFolder + "/Shared Core Root.prefab";

        [MenuItem("Tools/Shared Modules/Core/Setup Core Root")]
        public static void SetupCoreRoot()
        {
            EnsureFolders();

            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace Core Root?",
                    "A Shared Core Root already exists in this scene. Replace it?",
                    "Replace",
                    "Cancel");

                if (!replace)
                {
                    Selection.activeGameObject = existingRoot;
                    return;
                }

                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameObject root = CreateCoreRoot();
            SaveRootPrefab(root);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        [MenuItem("Tools/Shared Modules/Core/Create Or Refresh Prefab From Scene Root")]
        public static void CreateOrRefreshPrefab()
        {
            EnsureFolders();

            GameObject root = GameObject.Find(RootName);
            if (root == null)
            {
                EditorUtility.DisplayDialog(
                    "Core Root Missing",
                    "Run Tools > Shared Modules > Core > Setup Core Root first.",
                    "OK");
                return;
            }

            SaveRootPrefab(root);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(RootPrefabPath));
        }

        [MenuItem("Tools/Shared Modules/Core/Ping Prefab Folder")]
        public static void PingPrefabFolder()
        {
            EnsureFolders();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(PrefabsFolder));
        }

        private static GameObject CreateCoreRoot()
        {
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Shared Core Root");

            SharedCoreRoot coreRoot = root.AddComponent<SharedCoreRoot>();
            root.AddComponent<SharedSaveManager>();
            root.AddComponent<SharedAuthManager>();
            SharedAudioManager audioManager = root.AddComponent<SharedAudioManager>();
            audioManager.ConfigureSources(
                CreateAudioSource(root.transform, "BGM Source", true),
                CreateAudioSource(root.transform, "SFX Source", false));
            coreRoot.EnsureManagers();

            return root;
        }

        private static AudioSource CreateAudioSource(Transform parent, string name, bool loop)
        {
            GameObject sourceObject = new GameObject(name);
            sourceObject.transform.SetParent(parent, false);

            AudioSource source = sourceObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            return source;
        }

        private static void SaveRootPrefab(GameObject root)
        {
            PrefabUtility.SaveAsPrefabAsset(root, RootPrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ModuleRoot);
            EnsureFolder(PrefabsFolder);
            AssetDatabase.Refresh();
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace("\\", "/");
            string name = Path.GetFileName(folder);

            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
