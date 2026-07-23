using System;
using System.Collections.Generic;
using System.IO;
using SharedAppFlowModule;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UI;

namespace SharedAppFlowModule.Editor
{
    public static class SharedAppFlowAssetMenu
    {
        // Generated assets must live under Assets. Git/registry packages may be read-only.
        private const string ModuleRoot = "Assets/SharedModules/Generated";
        private const string PrefabsFolder = ModuleRoot + "/Prefabs";
        private const string LocalizationFolder = ModuleRoot + "/Localization";
        private const string LocalesFolder = LocalizationFolder + "/Locales";
        private const string TablesFolder = LocalizationFolder + "/Tables";
        private const string LocalizationSettingsPath = LocalizationFolder + "/Shared Localization Settings.asset";
        private const string SharedUiTableName = "Shared UI";
        private const string RootPrefabPath = PrefabsFolder + "/Shared App Flow Root.prefab";
        private const string RootName = "Shared App Flow Root";
        private const string CanvasName = "Shared App Flow Canvas";
        private const string PixelPanelPath = "Assets/2D Pixel Quest Vol.3 - The UI-GUI/Sprites PNG/Panels/Panels/F_UI_Panel_A.png";
        private const string IntroLogoGuid = "b140f2c6ca9534e6db981042d930d5ba";

        private static readonly Vector2 ReferenceResolution = new Vector2(960f, 540f);

        [MenuItem("Tools/Shared Modules/App Flow/Setup Intro Login Home Gameplay Demo")]
        public static void SetupIntroLoginHomeDemo()
        {
            EnsureFolders();
            EnsureLocalizationAssets();

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

            SharedAppScreenPanel homePanel = CreateHomePanel(canvasObject.transform, controller, font);
            SharedAppScreenPanel gameplayPanel = CreateGameplayPanel(canvasObject.transform, controller, panelSprite, font);
            SharedAppScreenPanel collectionPanel = CreateCollectionPanel(canvasObject.transform, controller, panelSprite, font);
            SharedSettingsModal settingsModal = CreateSettingsModal(canvasObject.transform, controller, panelSprite, font);

            controller.Configure(
                new[] { introPanel, loginPanel, homePanel, gameplayPanel, collectionPanel },
                settingsModal,
                SharedAppScreenId.Intro);

            introPanel.Show(true);
            loginPanel.Hide(true);
            homePanel.Hide(true);
            gameplayPanel.Hide(true);
            collectionPanel.Hide(true);
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
                font,
                SharedButtonVariant.Primary,
                "login",
                "login_subtitle",
                "guest_login");

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
                font,
                variant: SharedButtonVariant.Secondary);

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
            Font font)
        {
            GameObject panelObject = CreateUiObject("Home Panel", parent);
            Stretch(panelObject.GetComponent<RectTransform>());

            CanvasGroup canvasGroup = panelObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            GameObject contentObject = CreateUiObject("Home Content", panelObject.transform);
            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            Stretch(contentRect);

            Image artwork = CreateFullImage("Game Artwork", contentObject.transform, Color.black);
            artwork.raycastTarget = false;
            artwork.preserveAspect = false;

            GameObject profileObject = CreateUiObject("Profile", contentObject.transform);
            RectTransform profileRect = profileObject.GetComponent<RectTransform>();
            AnchorBottomLeft(profileRect, new Vector2(128f, 82f), new Vector2(24f, 58f));
            Image profileBackground = profileObject.AddComponent<Image>();
            profileBackground.color = new Color(0.09f, 0.14f, 0.15f, 0.96f);
            profileBackground.raycastTarget = false;
            Outline profileOutline = profileObject.AddComponent<Outline>();
            profileOutline.effectColor = new Color(0.48f, 0.6f, 0.56f, 0.92f);
            profileOutline.effectDistance = new Vector2(2f, -2f);

            Text profileTitle = CreateText("Title", profileObject.transform, new Vector2(0f, 27f),
                new Vector2(116f, 20f), "Profile", 13, font, FontStyle.Bold);
            ConfigureLocalizedText(profileTitle, "profile");
            GameObject profileSlotObject = CreateUiObject("Profile Slot", profileObject.transform);
            RectTransform profileSlotRect = profileSlotObject.GetComponent<RectTransform>();
            Center(profileSlotRect, new Vector2(96f, 25f), new Vector2(0f, 2f));
            Image profileSlotBackground = profileSlotObject.AddComponent<Image>();
            profileSlotBackground.color = new Color(0.25f, 0.33f, 0.34f, 1f);
            profileSlotBackground.raycastTarget = false;
            Text profileSlot = CreateText("Slot", profileSlotObject.transform, Vector2.zero,
                new Vector2(96f, 25f), "P1", 15, font, FontStyle.Bold);
            profileSlot.color = Color.white;
            Text profileName = CreateText("Player Name", profileObject.transform, new Vector2(0f, -27f),
                new Vector2(118f, 22f), "Playing as Guest", 10, font, FontStyle.Normal);
            profileName.color = new Color(0.86f, 0.9f, 0.87f, 1f);

            GameObject dockObject = CreateUiObject("Bottom Navigation", contentObject.transform);
            RectTransform dockRect = dockObject.GetComponent<RectTransform>();
            AnchorBottomLeft(dockRect, new Vector2(540f, 82f), new Vector2(180f, 58f));
            Image dockBackground = dockObject.AddComponent<Image>();
            dockBackground.color = new Color(0.05f, 0.1f, 0.11f, 0.96f);
            dockBackground.raycastTarget = false;
            Outline dockOutline = dockObject.AddComponent<Outline>();
            dockOutline.effectColor = new Color(0.42f, 0.53f, 0.5f, 0.9f);
            dockOutline.effectDistance = new Vector2(2f, -2f);

            Button playButton = CreateButton(
                "Play Button", dockObject.transform, new Vector2(-171f, 0f), new Vector2(150f, 48f),
                "PLAY", controller, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Gameplay, font,
                variant: SharedButtonVariant.Primary,
                localizationKey: "play");
            playButton.GetComponent<SharedButtonVisual>().Configure(
                SharedButtonVariant.Primary,
                new Color(0.05f, 0.5f, 0.78f, 1f),
                new Color(0.25f, 0.82f, 1f, 1f),
                Color.white);

            Button optionsButton = CreateButton(
                "Options Button", dockObject.transform, new Vector2(20f, 0f), new Vector2(130f, 48f),
                "OPTIONS", controller, SharedAppFlowButtonAction.OpenSettings, SharedAppScreenId.Home, font,
                localizationKey: "options");
            optionsButton.GetComponent<SharedButtonVisual>().Configure(
                SharedButtonVariant.Secondary,
                new Color(0.83f, 0.48f, 0.08f, 1f),
                new Color(1f, 0.72f, 0.22f, 1f),
                Color.white);

            Button quitButton = CreateButton(
                "Quit Button", dockObject.transform, new Vector2(191f, 0f), new Vector2(110f, 48f),
                "QUIT", controller, SharedAppFlowButtonAction.Quit, SharedAppScreenId.Home, font,
                variant: SharedButtonVariant.Destructive,
                localizationKey: "quit");
            quitButton.GetComponent<SharedButtonVisual>().Configure(
                SharedButtonVariant.Destructive,
                new Color(0.68f, 0.16f, 0.19f, 1f),
                new Color(1f, 0.38f, 0.38f, 1f),
                Color.white);

            Button collectionButton = CreateButton(
                "Collection Button", contentObject.transform, Vector2.zero, new Vector2(52f, 52f),
                "\u25a6", controller, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Collection, font);
            AnchorBottomRight(
                collectionButton.GetComponent<RectTransform>(),
                new Vector2(52f, 52f),
                new Vector2(46f, 120f));
            collectionButton.GetComponent<SharedButtonVisual>().Configure(
                SharedButtonVariant.Secondary,
                new Color(0.12f, 0.51f, 0.32f, 1f),
                new Color(0.28f, 0.82f, 0.52f, 1f),
                Color.white);
            Text collectionLabel = collectionButton.transform.Find("Label").GetComponent<Text>();
            collectionLabel.fontSize = 28;

            Button languageButton = CreateButton(
                "Language Button", contentObject.transform, Vector2.zero, new Vector2(96f, 46f),
                "A/文  Language", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                SharedButtonVariant.Navigation);
            AnchorBottomRight(
                languageButton.GetComponent<RectTransform>(),
                new Vector2(96f, 46f),
                new Vector2(24f, 58f));
            languageButton.GetComponent<SharedButtonVisual>().Configure(
                SharedButtonVariant.Navigation,
                new Color(0.15f, 0.2f, 0.21f, 1f),
                new Color(0.52f, 0.66f, 0.62f, 1f),
                Color.white);
            Text currentLanguageLabel = languageButton.transform.Find("Label").GetComponent<Text>();
            currentLanguageLabel.fontSize = 11;

            GameObject languagePopup = CreateUiObject("Language Popup", contentObject.transform);
            RectTransform popupRect = languagePopup.GetComponent<RectTransform>();
            AnchorBottomRight(popupRect, new Vector2(160f, 10f), new Vector2(24f, 188f));
            Image popupBackground = languagePopup.AddComponent<Image>();
            popupBackground.color = new Color(0.05f, 0.1f, 0.11f, 0.98f);
            Outline popupOutline = languagePopup.AddComponent<Outline>();
            popupOutline.effectColor = new Color(0.52f, 0.66f, 0.62f, 0.95f);
            popupOutline.effectDistance = new Vector2(2f, -2f);

            VerticalLayoutGroup languageLayout = languagePopup.AddComponent<VerticalLayoutGroup>();
            languageLayout.padding = new RectOffset(8, 8, 8, 8);
            languageLayout.spacing = 6f;
            languageLayout.childAlignment = TextAnchor.LowerCenter;
            languageLayout.childControlWidth = true;
            languageLayout.childControlHeight = true;
            languageLayout.childForceExpandWidth = true;
            languageLayout.childForceExpandHeight = false;

            ContentSizeFitter popupFitter = languagePopup.AddComponent<ContentSizeFitter>();
            popupFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            popupFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            Button languageOptionTemplate = CreateButton(
                "Language Option Template", languagePopup.transform, Vector2.zero, new Vector2(144f, 36f),
                "Language", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                SharedButtonVariant.Secondary);
            LayoutElement languageOptionLayout = languageOptionTemplate.gameObject.AddComponent<LayoutElement>();
            languageOptionLayout.preferredHeight = 36f;
            languageOptionTemplate.gameObject.SetActive(false);
            languagePopup.SetActive(false);

            SharedLanguageSelector languageSelector = panelObject.AddComponent<SharedLanguageSelector>();
            languageSelector.Configure(
                languageButton,
                currentLanguageLabel,
                languagePopup,
                languagePopup.transform,
                languageOptionTemplate);

            SharedHomeView homeView = panelObject.AddComponent<SharedHomeView>();
            homeView.Configure(artwork, profileSlot, profileName);

            SharedAppScreenPanel homePanel = panelObject.AddComponent<SharedAppScreenPanel>();
            homePanel.Configure(SharedAppScreenId.Home, contentRect);
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
                font,
                SharedButtonVariant.Back,
                "gameplay",
                "gameplay_subtitle",
                "back_to_home");
        }

        private static SharedAppScreenPanel CreateCollectionPanel(
            Transform parent,
            SharedAppFlowController controller,
            Sprite panelSprite,
            Font font)
        {
            return CreateScreenPanel(
                parent,
                SharedAppScreenId.Collection,
                "Collection Panel",
                "COLLECTION",
                "Game-specific collection content goes here",
                "BACK TO HOME",
                SharedAppFlowButtonAction.ShowScreen,
                SharedAppScreenId.Home,
                controller,
                panelSprite,
                font,
                SharedButtonVariant.Back,
                "collection",
                "collection_subtitle",
                "back_to_home");
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
            Font font,
            SharedButtonVariant primaryVariant = SharedButtonVariant.Primary,
            string titleLocalizationKey = null,
            string subtitleLocalizationKey = null,
            string primaryLocalizationKey = null)
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

            Text titleText = CreateText(
                "Title", contentObject.transform, new Vector2(0f, 88f), new Vector2(440f, 48f),
                title, 28, font, FontStyle.Bold);
            Text subtitleText = CreateText(
                "Subtitle", contentObject.transform, new Vector2(0f, 34f), new Vector2(420f, 44f),
                subtitle, 16, font, FontStyle.Normal);

            if (!string.IsNullOrEmpty(titleLocalizationKey))
            {
                ConfigureLocalizedText(titleText, titleLocalizationKey);
            }

            if (!string.IsNullOrEmpty(subtitleLocalizationKey))
            {
                ConfigureLocalizedText(subtitleText, subtitleLocalizationKey);
            }

            CreateButton(
                "Primary Button",
                contentObject.transform,
                new Vector2(0f, -110f),
                new Vector2(180f, 42f),
                primaryButtonText,
                controller,
                primaryAction,
                primaryTarget,
                font,
                variant: primaryVariant,
                localizationKey: primaryLocalizationKey);

            return panel;
        }

        private static SharedSettingsModal CreateSettingsModal(
            Transform parent,
            SharedAppFlowController controller,
            Sprite panelSprite,
            Font font)
        {
            GameObject modalObject = CreateUiObject("Shared Options Modal", parent);
            Stretch(modalObject.GetComponent<RectTransform>());

            CanvasGroup canvasGroup = modalObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            Image dim = CreateFullImage("Dim Overlay", modalObject.transform, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            dim.raycastTarget = true;

            GameObject panelObject = CreateUiObject("Options Panel", modalObject.transform);
            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            Center(panelRect, new Vector2(360f, 330f), Vector2.zero);

            Image panelImage = panelObject.AddComponent<Image>();
            ApplyPanelSprite(panelImage, panelSprite, Color.white);

            GameObject optionsPage = CreateModalPage("Options Menu", panelObject.transform);
            Text optionsTitle = CreateText("Title", optionsPage.transform, new Vector2(0f, 125f),
                new Vector2(300f, 36f), "OPTIONS", 24, font, FontStyle.Bold);
            ConfigureLocalizedText(optionsTitle, "options");

            Button settingsButton = CreateButton(
                "Settings Button", optionsPage.transform, new Vector2(0f, 75f), new Vector2(250f, 36f),
                "SETTINGS", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                localizationKey: "settings");
            Button statsButton = CreateButton(
                "Stats Button", optionsPage.transform, new Vector2(0f, 33f), new Vector2(250f, 36f),
                "STATS", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                localizationKey: "stats");
            Button creditsButton = CreateButton(
                "Credits Button", optionsPage.transform, new Vector2(0f, -9f), new Vector2(250f, 36f),
                "CREDITS", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                localizationKey: "credits");
            CreateButton(
                "Logout Button", optionsPage.transform, new Vector2(0f, -51f), new Vector2(250f, 36f),
                "LOGOUT", controller, SharedAppFlowButtonAction.Logout, SharedAppScreenId.Login, font,
                variant: SharedButtonVariant.Destructive,
                localizationKey: "logout");
            Button closeButton = CreateButton(
                "Back Button", optionsPage.transform, new Vector2(0f, -103f), new Vector2(270f, 36f),
                "BACK", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                SharedButtonVariant.Back,
                "back");

            GameObject settingsPage = CreateModalPage("Settings Page", panelObject.transform);
            Text settingsTitle = CreateText("Title", settingsPage.transform, new Vector2(0f, 125f),
                new Vector2(300f, 36f), "SETTINGS", 24, font, FontStyle.Bold);
            ConfigureLocalizedText(settingsTitle, "settings");
            Slider masterSlider = CreateVolumeSlider(
                "Master Volume", "master_volume", settingsPage.transform, new Vector2(55f, 65f), font);
            Slider bgmSlider = CreateVolumeSlider(
                "Music Volume", "music_volume", settingsPage.transform, new Vector2(55f, 15f), font);
            Slider sfxSlider = CreateVolumeSlider(
                "Sound Effects", "sound_effects", settingsPage.transform, new Vector2(55f, -35f), font);
            Toggle vibrationToggle = CreateSettingsToggle(
                "Vibration", "vibration", settingsPage.transform, new Vector2(55f, -75f), font);
            Button settingsBackButton = CreateButton(
                "Back Button", settingsPage.transform, new Vector2(0f, -130f), new Vector2(270f, 34f),
                "BACK", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                SharedButtonVariant.Back,
                "back");

            GameObject statsPage = CreateModalPage("Stats Page", panelObject.transform);
            Text statsTitle = CreateText("Title", statsPage.transform, new Vector2(0f, 125f),
                new Vector2(300f, 36f), "STATS", 24, font, FontStyle.Bold);
            ConfigureLocalizedText(statsTitle, "stats");
            CreateText("Body", statsPage.transform, new Vector2(0f, 25f), new Vector2(290f, 120f),
                "Games Played    0\nBest Score      0\nWins            0", 16, font, FontStyle.Normal);
            Button statsBackButton = CreateButton(
                "Back Button", statsPage.transform, new Vector2(0f, -100f), new Vector2(270f, 38f),
                "BACK", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                SharedButtonVariant.Back,
                "back");

            GameObject creditsPage = CreateModalPage("Credits Page", panelObject.transform);
            Text creditsTitle = CreateText("Title", creditsPage.transform, new Vector2(0f, 125f),
                new Vector2(300f, 36f), "CREDITS", 24, font, FontStyle.Bold);
            ConfigureLocalizedText(creditsTitle, "credits");
            CreateText("Body", creditsPage.transform, new Vector2(0f, 25f), new Vector2(290f, 120f),
                "GAME BY YOUR STUDIO\n\nPowered by Shared Modules", 16, font, FontStyle.Normal);
            Button creditsBackButton = CreateButton(
                "Back Button", creditsPage.transform, new Vector2(0f, -100f), new Vector2(270f, 38f),
                "BACK", null, SharedAppFlowButtonAction.ShowScreen, SharedAppScreenId.Home, font, false,
                SharedButtonVariant.Back,
                "back");

            optionsPage.SetActive(true);
            settingsPage.SetActive(false);
            statsPage.SetActive(false);
            creditsPage.SetActive(false);

            SharedSettingsModal modal = modalObject.AddComponent<SharedSettingsModal>();
            modal.Configure(
                panelRect, optionsPage, settingsPage, statsPage, creditsPage,
                settingsButton, statsButton, creditsButton, closeButton,
                settingsBackButton, statsBackButton, creditsBackButton,
                masterSlider, bgmSlider, sfxSlider, vibrationToggle);
            return modal;
        }

        private static GameObject CreateModalPage(string name, Transform parent)
        {
            GameObject page = CreateUiObject(name, parent);
            Stretch(page.GetComponent<RectTransform>());
            return page;
        }

        private static Slider CreateVolumeSlider(
            string name,
            string localizationKey,
            Transform parent,
            Vector2 position,
            Font font)
        {
            Text label = CreateText(name + " Label", parent, new Vector2(-105f, position.y),
                new Vector2(115f, 30f), name.ToUpperInvariant(), 13, font, FontStyle.Bold);
            ConfigureLocalizedText(label, localizationKey);

            GameObject sliderObject = CreateUiObject(name + " Slider", parent);
            RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
            Center(sliderRect, new Vector2(180f, 22f), position);

            Image background = sliderObject.AddComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.1f, 1f);

            GameObject fillObject = CreateUiObject("Fill", sliderObject.transform);
            RectTransform fillRect = fillObject.GetComponent<RectTransform>();
            Stretch(fillRect);
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);
            Image fillImage = fillObject.AddComponent<Image>();
            fillImage.color = new Color(0.95f, 0.35f, 0.22f, 1f);

            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            slider.fillRect = fillRect;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static Toggle CreateSettingsToggle(
            string name,
            string localizationKey,
            Transform parent,
            Vector2 position,
            Font font)
        {
            Text label = CreateText(name + " Label", parent, new Vector2(-105f, position.y),
                new Vector2(115f, 30f), name.ToUpperInvariant(), 13, font, FontStyle.Bold);
            ConfigureLocalizedText(label, localizationKey);

            GameObject toggleObject = CreateUiObject(name + " Toggle", parent);
            RectTransform toggleRect = toggleObject.GetComponent<RectTransform>();
            Center(toggleRect, new Vector2(180f, 28f), position);

            GameObject backgroundObject = CreateUiObject("Background", toggleObject.transform);
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            Center(backgroundRect, new Vector2(62f, 24f), Vector2.zero);
            Image background = backgroundObject.AddComponent<Image>();
            background.color = new Color(0.06f, 0.08f, 0.1f, 1f);

            GameObject checkmarkObject = CreateUiObject("On", backgroundObject.transform);
            RectTransform checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
            Center(checkmarkRect, new Vector2(52f, 14f), Vector2.zero);
            Image checkmark = checkmarkObject.AddComponent<Image>();
            checkmark.color = new Color(0.95f, 0.35f, 0.22f, 1f);

            Toggle toggle = toggleObject.AddComponent<Toggle>();
            toggle.targetGraphic = background;
            toggle.graphic = checkmark;
            toggle.isOn = true;
            return toggle;
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
            Font font,
            bool addFlowButton = true,
            SharedButtonVariant variant = SharedButtonVariant.Secondary,
            string localizationKey = null)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            Center(rect, size, position);

            Image image = buttonObject.AddComponent<Image>();
            image.color = Color.white;

            Button button = buttonObject.AddComponent<Button>();
            SharedButtonVisual visual = buttonObject.AddComponent<SharedButtonVisual>();

            if (addFlowButton)
            {
                SharedAppFlowButton flowButton = buttonObject.AddComponent<SharedAppFlowButton>();
                flowButton.Configure(controller, action, target);
            }

            Text buttonLabel = CreateText(
                "Label", buttonObject.transform, Vector2.zero, size, label, 15, font, FontStyle.Bold);
            if (!string.IsNullOrEmpty(localizationKey))
            {
                ConfigureLocalizedText(buttonLabel, localizationKey);
            }

            visual.Configure(variant);
            return button;
        }

        private static void ConfigureLocalizedText(Text text, string entryKey)
        {
            if (text == null || string.IsNullOrWhiteSpace(entryKey))
            {
                return;
            }

            SharedLocalizedText localizedText = text.gameObject.AddComponent<SharedLocalizedText>();
            localizedText.Configure(SharedUiTableName, entryKey);
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

        private static void AnchorBottomLeft(RectTransform rect, Vector2 size, Vector2 position)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one;
        }

        private static void AnchorBottomRight(RectTransform rect, Vector2 size, Vector2 inset)
        {
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(-inset.x, inset.y);
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
            string introLogoPath = AssetDatabase.GUIDToAssetPath(IntroLogoGuid);
            if (string.IsNullOrEmpty(introLogoPath))
            {
                return null;
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(introLogoPath);

            if (sprite != null)
            {
                return sprite;
            }

            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(introLogoPath);
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
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void EnsureFolders()
        {
            EnsureFolder(ModuleRoot);
            EnsureFolder(PrefabsFolder);
            EnsureFolder(LocalizationFolder);
            EnsureFolder(LocalesFolder);
            EnsureFolder(TablesFolder);
            AssetDatabase.Refresh();
        }

        private static void EnsureLocalizationAssets()
        {
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                settings.name = "Shared Localization Settings";
                AssetDatabase.CreateAsset(settings, LocalizationSettingsPath);
                LocalizationEditorSettings.ActiveLocalizationSettings = settings;
            }

            Locale english = EnsureLocale("en", SystemLanguage.English, "English");
            Locale korean = EnsureLocale("ko", SystemLanguage.Korean, "한국어");
            if (LocalizationSettings.ProjectLocale == null)
            {
                LocalizationSettings.ProjectLocale = english;
            }

            StringTableCollection collection =
                LocalizationEditorSettings.GetStringTableCollection(SharedUiTableName);
            if (collection == null)
            {
                collection = LocalizationEditorSettings.CreateStringTableCollection(
                    SharedUiTableName,
                    TablesFolder,
                    new List<Locale> { english, korean });
            }

            StringTable englishTable = EnsureStringTable(collection, english);
            StringTable koreanTable = EnsureStringTable(collection, korean);

            EnsureString(englishTable, "profile", "Profile");
            EnsureSmartString(englishTable, "playing_as", "Playing as {0}");
            EnsureString(englishTable, "login", "LOGIN");
            EnsureString(englishTable, "login_subtitle", "Choose how you want to continue");
            EnsureString(englishTable, "guest_login", "GUEST LOGIN");
            EnsureString(englishTable, "play", "PLAY");
            EnsureString(englishTable, "options", "OPTIONS");
            EnsureString(englishTable, "quit", "QUIT");
            EnsureString(englishTable, "collection", "COLLECTION");
            EnsureString(englishTable, "gameplay", "GAMEPLAY");
            EnsureString(englishTable, "back_to_home", "BACK TO HOME");
            EnsureString(englishTable, "gameplay_subtitle", "Gameplay scene entry placeholder");
            EnsureString(englishTable, "collection_subtitle", "Game-specific collection content goes here");
            EnsureString(englishTable, "settings", "SETTINGS");
            EnsureString(englishTable, "stats", "STATS");
            EnsureString(englishTable, "credits", "CREDITS");
            EnsureString(englishTable, "logout", "LOGOUT");
            EnsureString(englishTable, "back", "BACK");
            EnsureString(englishTable, "master_volume", "MASTER VOLUME");
            EnsureString(englishTable, "music_volume", "MUSIC VOLUME");
            EnsureString(englishTable, "sound_effects", "SOUND EFFECTS");
            EnsureString(englishTable, "vibration", "VIBRATION");

            EnsureString(koreanTable, "profile", "프로필");
            EnsureSmartString(koreanTable, "playing_as", "{0} 플레이 중");
            EnsureString(koreanTable, "login", "로그인");
            EnsureString(koreanTable, "login_subtitle", "계속할 로그인 방식을 선택하세요");
            EnsureString(koreanTable, "guest_login", "게스트 로그인");
            EnsureString(koreanTable, "play", "플레이");
            EnsureString(koreanTable, "options", "설정");
            EnsureString(koreanTable, "quit", "종료");
            EnsureString(koreanTable, "collection", "컬렉션");
            EnsureString(koreanTable, "gameplay", "게임 플레이");
            EnsureString(koreanTable, "back_to_home", "홈으로");
            EnsureString(koreanTable, "gameplay_subtitle", "게임 플레이 화면이 연결되는 자리입니다");
            EnsureString(koreanTable, "collection_subtitle", "게임별 컬렉션 화면이 연결되는 자리입니다");
            EnsureString(koreanTable, "settings", "환경 설정");
            EnsureString(koreanTable, "stats", "통계");
            EnsureString(koreanTable, "credits", "제작진");
            EnsureString(koreanTable, "logout", "로그아웃");
            EnsureString(koreanTable, "back", "뒤로");
            EnsureString(koreanTable, "master_volume", "전체 음량");
            EnsureString(koreanTable, "music_volume", "음악 음량");
            EnsureString(koreanTable, "sound_effects", "효과음");
            EnsureString(koreanTable, "vibration", "진동");

            EditorUtility.SetDirty(settings);
            EditorUtility.SetDirty(collection);
            AssetDatabase.SaveAssets();
        }

        private static Locale EnsureLocale(string code, SystemLanguage language, string displayName)
        {
            Locale locale = LocalizationEditorSettings.GetLocale(code);
            if (locale == null)
            {
                locale = Locale.CreateLocale(language);
                locale.name = displayName + " (" + code + ")";
                locale.LocaleName = displayName;
                AssetDatabase.CreateAsset(locale, LocalesFolder + "/" + locale.name + ".asset");
                LocalizationEditorSettings.AddLocale(locale);
            }

            return locale;
        }

        private static StringTable EnsureStringTable(StringTableCollection collection, Locale locale)
        {
            StringTable table = collection.GetTable(locale.Identifier) as StringTable;
            if (table == null)
            {
                table = collection.AddNewTable(locale.Identifier, TablesFolder) as StringTable;
            }

            return table;
        }

        private static void EnsureString(StringTable table, string key, string value)
        {
            if (table == null)
            {
                return;
            }

            StringTableEntry entry = table.GetEntry(key);
            if (entry == null)
            {
                table.AddEntry(key, value);
                EditorUtility.SetDirty(table.SharedData);
            }

            EditorUtility.SetDirty(table);
        }

        private static void EnsureSmartString(StringTable table, string key, string value)
        {
            EnsureString(table, key, value);
            StringTableEntry entry = table != null ? table.GetEntry(key) : null;
            if (entry != null && !entry.IsSmart)
            {
                entry.IsSmart = true;
                EditorUtility.SetDirty(table);
            }
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
