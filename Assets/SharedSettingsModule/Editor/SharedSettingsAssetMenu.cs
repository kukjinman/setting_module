#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SharedSettingsModule.Editor
{
    public static class SharedSettingsAssetMenu
    {
        private const string UiElegantPath = "Assets/resourceBox/UI Bundle/UiElegant.png";
        private const string PreferredPanelSpriteName = "UiElegant_146";
        private static readonly string[] DemoObjectNames =
        {
            "Shared Settings Demo Canvas",
            "Shared Settings Demo Launcher",
            "Shared Settings Modal",
            "Shared Settings Black Background"
        };

        [MenuItem("Tools/Shared Settings/Setup Ideal Mobile Demo")]
        public static void SetupIdealMobileDemo()
        {
            ClearDemoObjects();

            var canvasObject = new GameObject("Shared Settings Demo Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Shared Settings Demo Canvas");

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            canvas.pixelPerfect = true;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = SharedSettingsModal.AssetReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            var background = new GameObject("Black Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(canvasObject.transform, false);
            Stretch((RectTransform)background.transform);
            background.transform.SetAsFirstSibling();
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = Color.black;
            backgroundImage.raycastTarget = false;

            var launcherObject = new GameObject("Shared Settings Demo Launcher", typeof(RectTransform));
            launcherObject.transform.SetParent(canvasObject.transform, false);
            var launcher = launcherObject.AddComponent<SharedSettingsLauncher>();
            AssignLauncher(launcher, canvas, LoadPanelSprite());

            Selection.activeGameObject = canvasObject;
            EditorSceneManager.MarkSceneDirty(canvasObject.scene);
            Debug.Log("Created an ideal mobile landscape Shared Settings demo. Press Play to see the setting window slide in from the bottom.");
        }

        [MenuItem("Tools/Shared Settings/Setup Demo Setting Window")]
        public static void SetupDemoSettingWindow()
        {
            SetupIdealMobileDemo();
        }

        [MenuItem("Tools/Shared Settings/Clear Demo Objects")]
        public static void ClearDemoObjects()
        {
            var objects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var gameObject in objects)
            {
                if (!IsDemoObject(gameObject.name))
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(gameObject);
            }
        }

        private static Sprite LoadPanelSprite()
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(UiElegantPath);
            Sprite fallback = null;

            foreach (var asset in sprites)
            {
                var sprite = asset as Sprite;
                if (sprite == null)
                {
                    continue;
                }

                if (sprite.name == PreferredPanelSpriteName)
                {
                    return sprite;
                }

                if (fallback == null || sprite.rect.width * sprite.rect.height > fallback.rect.width * fallback.rect.height)
                {
                    fallback = sprite;
                }
            }

            if (fallback == null)
            {
                Debug.LogWarning("Could not find " + PreferredPanelSpriteName + " in " + UiElegantPath + ". The demo will use a plain fallback panel.");
            }

            return fallback;
        }

        private static void AssignLauncher(SharedSettingsLauncher launcher, Canvas targetCanvas, Sprite panelSprite)
        {
            var serialized = new SerializedObject(launcher);
            serialized.FindProperty("panelSprite").objectReferenceValue = panelSprite;
            serialized.FindProperty("openOnStart").boolValue = true;
            serialized.FindProperty("createBlackBackground").boolValue = true;
            serialized.FindProperty("targetCanvas").objectReferenceValue = targetCanvas;
            serialized.FindProperty("panelSize").vector2Value = Vector2.zero;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static bool IsDemoObject(string objectName)
        {
            foreach (var demoObjectName in DemoObjectNames)
            {
                if (objectName == demoObjectName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif
