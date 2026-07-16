using System;
using System.IO;
using SharedAppFlowModule;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SharedAppFlowModule.Editor
{
    public static class SharedAppFlowAssetMenu
    {
        private const string ModuleRoot = "Assets/SharedAppFlowModule";
        private const string PrefabsFolder = ModuleRoot + "/Prefabs";
        private const string RootPrefabPath = PrefabsFolder + "/Shared App Flow Root.prefab";
        private const string RootName = "Shared App Flow Root";
        private const string CanvasName = "Shared App Flow Canvas";
        private const string PixelPanelPath = "Assets/2D Pixel Quest Vol.3 - The UI-GUI/Sprites PNG/Panels/Panels/F_UI_Panel_A.png";
        private const string PixelFontPath = "Assets/2D Pixel Quest Vol.3 - The UI-GUI/Font/Fantasypixelfont.ttf";
        private const string IntroLogoPath = "Assets/SharedAppFlowModule/Art/Intro/intro-logo.png";

        private static readonly Vector2 ReferenceResolution = new Vector2(960f, 540f);

        [MenuItem("Tools/Shared Modules/App Flow/Setup Intro Login Home Gameplay Demo")]
        public static void SetupIntroLoginHomeDemo()
        {
            EnsureFolders();

            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                bool replace = EditorUtility.DisplayDialog(
                    "Replace App Flow Demo?",
                    "A Shared App Flow Root already exists in this scene. Replace it?",
                    "Replace",
                    "Cancel");

                if (!replace)
                {
                    Selection.activeGameObject = existingRoot;
                    return;
                }

                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameObject root = CreateRoot();
            SaveRootPrefab(root);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        [MenuItem("Tools/Shared Modules/App Flow/Create Or Refresh Prefab From Scene Root")]
        public static void CreateOrRefreshPrefab()
        {
            EnsureFolders();

            GameObject root = GameObject.Find(RootName);
            if (root == null)
            {
                EditorUtility.DisplayDialog(
                    "App Flow Root Missing",
                    "Run Tools > Shared Modules > App Flow > Setup Intro Login Home Gameplay Demo first.",
                    "OK");
                return;
            }

            SaveRootPrefab(root);
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(RootPrefabPath));
        }

        [MenuItem("Tools/Shared Modules/App Flow/Ping Prefab Folder")]
        public static void PingPrefabFolder()
        {
            EnsureFolders();
            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(PrefabsFolder));
        }

        private static GameObject CreateRoot()
        {
            Sprite panelSprite = LoadPanelSprite();
            Sprite introLogoSprite = LoadIntroLogoSprite();
            Font font = LoadFont();

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Shared App Flow Root");
            SharedAppFlowController controller = root.AddComponent<SharedAppFlowController>();
            SharedAppleGameCenterLoginProvider appleGameCenterLogin = root.AddComponent<SharedAppleGameCenterLoginProvider>();
            SharedGooglePlayGamesLoginProvider googlePlayGamesLogin = root.AddComponent<SharedGooglePlayGamesLoginProvider>();
            controller.ConfigurePlatformLogins(appleGameCenterLogin, googlePlayGamesLogin);

            GameObject canvasObject = CreateUiObject(CanvasName, root.transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            canvasObject.AddComponent<GraphicRaycaster>();

            Image background = CreateFullImage("Black Background", canvasObject.transform, new Color(0f, 0f, 0f, 1f));
            background.raycastTarget = false;

            SharedAppScreenPanel introPanel = CreateIntroPanel(
                canvasObject.transform,
                controller,
                introLogoSprite,
                font);

            SharedAppScreenPanel loginPanel = CreateLoginPanel(canvasObject.transform, controller, panelSprite, font);

            SharedAppScreenPanel homePanel = CreateHomePanel(canvasObject.transform, controller, panelSprite, font);
            SharedAppScreenPanel gameplayPanel = CreateGameplayPanel(canvasObject.transform, controller, panelSprite, font);
            SharedSettingsModal settingsModal = CreateSettingsModal(canvasObject.transform, panelSprite, font);

            controller.Configure(
                new[] { introPanel, loginPanel, homePanel, gameplayPanel },
                settingsModal,
                SharedAppScreenId.Intro);

            introPanel.Show(true);
            loginPanel.Hide(true);
            homePanel.Hide(true);
            gameplayPanel.Hide(true);
            settingsModal.SetOpen(false, true);

            EnsureEventSystem();
            return root;
        }

        private static SharedAppScreenPanel CreateLoginPanel(
            Transform parent,
            SharedAppFlowController controller,
            Sprite panelSprite,
            Font font)
        {
            SharedAppScreenPanel loginPanel = CreateScreenPanel(
                parent,
                SharedAppScreenId.Login,
                "Login Panel",
                "LOGIN",
                "Choose how you want to continue",
                "GUEST LOGIN",
                SharedAppFlowButtonAction.LoginAsGuest,
                SharedAppScreenId.Home,
                controller,
                panelSprite,
                font);

            Transform content = loginPanel.transform.Find("Content");
            RectTransform guestButton = content.Find("Primary Button").GetComponent<RectTransform>();
            guestButton.anchoredPosition = new Vector2(0f, -50f);

            CreateButton(
                "Platform Login Button",
                content,
                new Vector2(0f, -105f),
                new Vector2(240f, 42f),
                GetPlatformLoginLabel(),
                controller,
                SharedAppFlowButtonAction.LoginWithPlatform,
                SharedAppScreenId.Home,
                font);

            Text status = CreateText(
                "Login Status",
                content,
                new Vector2(0f, -145f),
                new Vector2(420f, 28f),
                string.Empty,
                12,
                font,
                FontStyle.Normal);
            SharedLoginStatusText statusText = status.gameObject.AddComponent<SharedLoginStatusText>();
            statusText.Configure(controller);
            return loginPanel;
        }

        private static string GetPlatformLoginLabel()
        {
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.iOS:
                    return "APPLE GAME CENTER";
                case BuildTarget.Android:
                    return "GOOGLE PLAY GAMES";
                default:
                    return "PLATFORM LOGIN";
            }
        }

        private static SharedAppScreenPanel CreateHomePanel(
            Transform parent,
            SharedAppFlowController controller,
            Sprite panelSprite,
            Font font)
        {
            SharedAppScreenPanel homePanel = CreateScreenPanel(
                parent,
                SharedAppScreenId.Home,
                "Home Panel",
                "HOME",
                "Main hub placeholder",
                "PLAY",
                SharedAppFlowButtonAction.ShowScreen,
                SharedAppScreenId.Gameplay,
                controller,
                panelSprite,
                font);

            Transform content = homePanel.transform.Find("Content");
            content.Find("Primary Button").GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -48f);

            CreateButton(
                "Settings Button",
                content,
                new Vector2(-90f, -110f),
                new Vector2(160f, 38f),
                "SETTINGS",
                controller,
                SharedAppFlowButtonAction.OpenSettings,
                SharedAppScreenId.Home,
                font);

            CreateButton(
                "Logout Button",
                content,
                new Vector2(110f, -110f),
                new Vector2(160f, 38f),
                "LOGOUT",
                controller,
                SharedAppFlowButtonAction.Logout,
                SharedAppScreenId.Login,
                font);

            return homePanel;
        }

        private static SharedAppScreenPanel CreateGameplayPanel(
            Transform parent,
            SharedAppFlowController controller,
            Sprite panelSprite,
            Font font)
        {
            return CreateScreenPanel(
                parent,
                SharedAppScreenId.Gameplay,
                "Gameplay Panel",
                "GAMEPLAY",
                "Gameplay scene entry placeholder",
                "BACK TO HOME",
                SharedAppFlowButtonAction.ShowScreen,
                SharedAppScreenId.Home,
                controller,
                panelSprite,
                font);
        }

        private static SharedAppScreenPanel CreateIntroPanel(
            Transform parent,
            SharedAppFlowController controller,
            Sprite logoSprite,
            Font font)
        {
            GameObject panelObject = CreateUiObject("Intro Panel", parent);
            Stretch(panelObject.GetComponent<RectTransform>());

            CanvasGroup canvasGroup = panelObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject contentObject = CreateUiObject("Intro Content", panelObject.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            Center(contentRect, new Vector2(240f, 240f), Vector2.zero);

            Image logoImage = null;

            if (logoSprite != null)
            {
                logoImage = CreateImage("Intro Logo", contentObject.transform, Vector2.zero, new Vector2(190f, 190f), logoSprite);
                logoImage.preserveAspect = true;
                logoImage.raycastTarget = false;
            }

            SharedAppScreenPanel panel = panelObject.AddComponent<SharedAppScreenPanel>();
            panel.Configure(SharedAppScreenId.Intro, contentRect);
            panel.ConfigureTransition(1.2f, new Vector2(0f, -180f), Vector2.zero);

            SharedIntroLogoBreakEffect breakEffect = panelObject.AddComponent<SharedIntroLogoBreakEffect>();
            breakEffect.Configure(logoImage);

            SharedIntroFlow introFlow = panelObject.AddComponent<SharedIntroFlow>();
            introFlow.Configure(controller, null);
            introFlow.ConfigurePanel(panel);
            introFlow.ConfigureTiming(1.2f, 0.4f);
            introFlow.ConfigureLogoBreak(breakEffect);

            return panel;
        }

        private static SharedAppScreenPanel CreateScreenPanel(
            Transform parent,
            SharedAppScreenId screenId,
            string name,
            string title,
            string subtitle,
            string primaryButtonText,
            SharedAppFlowButtonAction primaryAction,
            SharedAppScreenId primaryTarget,
            SharedAppFlowController controller,
            Sprite panelSprite,
            Font font)
        {
            GameObject panelObject = CreateUiObject(name, parent);
            Stretch(panelObject.GetComponent<RectTransform>());

            CanvasGroup canvasGroup = panelObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            SharedAppScreenPanel panel = panelObject.AddComponent<SharedAppScreenPanel>();

            GameObject contentObject = CreateUiObject("Content", panelObject.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            Center(contentRect, new Vector2(520f, 320f), Vector2.zero);

            Image contentImage = contentObject.AddComponent<Image>();
            ApplyPanelSprite(contentImage, panelSprite, new Color(1f, 1f, 1f, 1f));

            panel.Configure(screenId, contentRect);

            CreateText("Title", contentObject.transform, new Vector2(0f, 88f), new Vector2(440f, 48f), title, 28, font, FontStyle.Bold);
            CreateText("Subtitle", contentObject.transform, new Vector2(0f, 34f), new Vector2(420f, 44f), subtitle, 16, font, FontStyle.Normal);

            CreateButton(
                "Primary Button",
                contentObject.transform,
                new Vector2(0f, -110f),
                new Vector2(180f, 42f),
                primaryButtonText,
                controller,
                primaryAction,
                primaryTarget,
                font);

            return panel;
        }

        private static SharedSettingsModal CreateSettingsModal(Transform parent, Sprite panelSprite, Font font)
        {
            GameObject modalObject = CreateUiObject("Shared Settings Modal", parent);
            Stretch(modalObject.GetComponent<RectTransform>());

            CanvasGroup canvasGroup = modalObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Image dim = CreateFullImage("Dim Overlay", modalObject.transform, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            dim.raycastTarget = true;

            GameObject panelObject = CreateUiObject("Settings Panel", modalObject.transform);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            Center(panelRect, new Vector2(460f, 300f), Vector2.zero);

            Image panelImage = panelObject.AddComponent<Image>();
            ApplyPanelSprite(panelImage, panelSprite, Color.white);

            CreateText("Title", panelObject.transform, new Vector2(0f, 100f), new Vector2(360f, 44f), "SETTINGS", 24, font, FontStyle.Bold);
            CreateText("Body", panelObject.transform, new Vector2(0f, 20f), new Vector2(360f, 90f), "Settings module placeholder", 15, font, FontStyle.Normal);

            Button closeButton = CreateButton(
                "Close Button",
                panelObject.transform,
                new Vector2(190f, 110f),
                new Vector2(38f, 32f),
                "X",
                null,
                SharedAppFlowButtonAction.CloseSettings,
                SharedAppScreenId.Home,
                font);

            SharedSettingsModal modal = modalObject.AddComponent<SharedSettingsModal>();
            modal.Configure(panelRect, closeButton);
            return modal;
        }

        private static Button CreateButton(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            string label,
            SharedAppFlowController controller,
            SharedAppFlowButtonAction action,
            SharedAppScreenId target,
            Font font)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            Center(rect, size, position);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.12f, 0.16f, 0.2f, 1f);

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.12f, 0.16f, 0.2f, 1f);
            colors.highlightedColor = new Color(0.18f, 0.24f, 0.3f, 1f);
            colors.pressedColor = new Color(0.08f, 0.11f, 0.14f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            SharedAppFlowButton flowButton = buttonObject.AddComponent<SharedAppFlowButton>();
            flowButton.Configure(controller, action, target);

            CreateText("Label", buttonObject.transform, Vector2.zero, size, label, 15, font, FontStyle.Bold);
            return button;
        }

        private static Text CreateText(
            string name,
            Transform parent,
            Vector2 position,
            Vector2 size,
            string text,
            int fontSize,
            Font font,
            FontStyle style)
        {
            GameObject textObject = CreateUiObject(name, parent);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            Center(rect, size, position);

            Text label = textObject.AddComponent<Text>();
            label.text = text;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(0.95f, 0.92f, 0.78f, 1f);
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.raycastTarget = false;

            if (font != null)
            {
                label.font = font;
            }

            return label;
        }

        private static Image CreateFullImage(string name, Transform parent, Color color)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            Stretch(imageObject.GetComponent<RectTransform>());

            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 position, Vector2 size, Sprite sprite)
        {
            GameObject imageObject = CreateUiObject(name, parent);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            Center(rect, size, position);

            Image image = imageObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            return image;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void Center(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one;
        }

        private static void ApplyPanelSprite(Image image, Sprite sprite, Color fallbackColor)
        {
            image.color = fallbackColor;

            if (sprite == null)
            {
                image.color = new Color(0.12f, 0.08f, 0.13f, 1f);
                return;
            }

            image.sprite = sprite;
            image.type = sprite.border == Vector4.zero ? Image.Type.Simple : Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = 1f;
        }

        private static Sprite LoadPanelSprite()
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(PixelPanelPath);
        }

        private static Sprite LoadIntroLogoSprite()
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(IntroLogoPath);

            if (sprite != null)
            {
                return sprite;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(IntroLogoPath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is Sprite loadedSprite)
                {
                    return loadedSprite;
                }
            }

            return null;
        }

        private static Font LoadFont()
        {
            Font font = AssetDatabase.LoadAssetAtPath<Font>(PixelFontPath);
            return font != null ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

            Type inputSystemModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModuleType != null)
            {
                eventSystemObject.AddComponent(inputSystemModuleType);
            }
            else
            {
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }
        }

        private static void SaveRootPrefab(GameObject root)
        {
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(RootPrefabPath);

            if (File.Exists(RootPrefabPath))
            {
                prefabPath = RootPrefabPath;
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
