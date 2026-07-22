using System;
using SharedCoreModule;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SharedAppFlowModule
{
    public sealed class SharedAppFlowController : MonoBehaviour
    {
        [SerializeField] private SharedAppScreenId startScreen = SharedAppScreenId.Intro;
        [SerializeField] private SharedAppScreenPanel[] panels;
        [SerializeField] private SharedSettingsModal settingsModal;
        [SerializeField] private SharedAuthManager authManager;
        [SerializeField] private SharedLoginProvider appleGameCenterLoginProvider;
        [SerializeField] private SharedLoginProvider googlePlayGamesLoginProvider;
        [SerializeField] private KeyCode settingsToggleKey = KeyCode.Escape;

        private SharedAppScreenId currentScreen;

        public SharedAppScreenId CurrentScreen => currentScreen;

        public SharedAuthManager Auth => ResolveAuthManager();
        public bool IsLoginInProgress { get; private set; }
        public event Action<string> LoginStatusChanged;

        public void Configure(SharedAppScreenPanel[] screenPanels, SharedSettingsModal modal, SharedAppScreenId initialScreen)
        {
            panels = screenPanels;
            settingsModal = modal;
            startScreen = initialScreen;
        }

        public void ConfigureGooglePlayGamesLogin(SharedLoginProvider loginProvider)
        {
            googlePlayGamesLoginProvider = loginProvider;
        }

        public void ConfigurePlatformLogins(
            SharedLoginProvider appleLoginProvider,
            SharedLoginProvider googleLoginProvider)
        {
            appleGameCenterLoginProvider = appleLoginProvider;
            googlePlayGamesLoginProvider = googleLoginProvider;
        }

        private void Start()
        {
            bool animateStartScreen = startScreen == SharedAppScreenId.Intro;
            ShowScreen(startScreen, !animateStartScreen);

            if (settingsModal != null)
            {
                settingsModal.SetOpen(false, true);
            }
        }

        private void Update()
        {
            if (currentScreen == SharedAppScreenId.Intro)
            {
                return;
            }

            if (settingsToggleKey != KeyCode.None && IsKeyPressed(settingsToggleKey))
            {
                ToggleSettings();
            }
        }

        public void ShowIntro()
        {
            ShowScreen(SharedAppScreenId.Intro);
        }

        public void ShowLogin()
        {
            ShowScreen(SharedAppScreenId.Login);
        }

        public void ShowHome()
        {
            ShowScreen(SharedAppScreenId.Home);
        }

        public void ShowGameplay()
        {
            ShowScreen(SharedAppScreenId.Gameplay);
        }

        public void ShowCollection()
        {
            ShowScreen(SharedAppScreenId.Collection);
        }

        public void ShowScreen(SharedAppScreenId screenId)
        {
            ShowScreen(screenId, false);
        }

        public void ShowScreen(SharedAppScreenId screenId, bool instant)
        {
            currentScreen = screenId;

            if (panels == null)
            {
                return;
            }

            foreach (SharedAppScreenPanel panel in panels)
            {
                if (panel == null)
                {
                    continue;
                }

                if (panel.ScreenId == screenId)
                {
                    panel.Show(instant);
                }
                else
                {
                    panel.Hide(instant);
                }
            }
        }

        public void OpenSettings()
        {
            if (settingsModal != null)
            {
                settingsModal.Open();
            }
        }

        public void CloseSettings()
        {
            if (settingsModal != null)
            {
                settingsModal.Close();
            }
        }

        public void ToggleSettings()
        {
            if (settingsModal != null)
            {
                settingsModal.Toggle();
            }
        }

        public void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoginAsGuest()
        {
            if (IsLoginInProgress)
            {
                return;
            }

            SharedAuthManager auth = ResolveAuthManager();

            if (auth != null)
            {
                auth.LoginAsGuest();
            }
            else
            {
                SharedAuthManager.SaveGuestLogin();
            }

            LoginStatusChanged?.Invoke("Guest login complete");
            ShowHome();
        }

        public void LoginWithGooglePlayGames()
        {
            if (IsLoginInProgress)
            {
                return;
            }

            if (googlePlayGamesLoginProvider == null)
            {
                LoginStatusChanged?.Invoke("Google Play Games login is not configured");
                return;
            }

            IsLoginInProgress = true;
            LoginStatusChanged?.Invoke("Signing in to Google Play Games...");
            googlePlayGamesLoginProvider.Login(HandleGooglePlayGamesLogin);
        }

        public void LoginWithPlatform()
        {
#if UNITY_IOS
            LoginWithAppleGameCenter();
#elif UNITY_ANDROID
            LoginWithGooglePlayGames();
#else
            LoginStatusChanged?.Invoke("Platform login is available only on an iOS or Android build");
#endif
        }

        public void TryRestoreLogin(Action<bool> completed)
        {
            SharedAuthManager auth = ResolveAuthManager();
            SharedLoginType savedType = auth != null
                ? auth.SavedLoginType
                : SharedAuthManager.GetSavedLoginType();

            if (savedType == SharedLoginType.Guest)
            {
                completed?.Invoke(true);
                return;
            }

            if (!SharedAuthManager.CanAutoLoginWithPlatform())
            {
                completed?.Invoke(false);
                return;
            }

            SharedLoginProvider provider = ResolveCurrentPlatformProvider();
            if (provider == null)
            {
                completed?.Invoke(false);
                return;
            }

            IsLoginInProgress = true;
            LoginStatusChanged?.Invoke("Restoring platform login...");
            provider.TryRestore(result => HandleRestoredPlatformLogin(result, completed));
        }

        public void Logout()
        {
            CloseSettings();
            SharedAuthManager auth = ResolveAuthManager();

            if (auth != null)
            {
                auth.Logout();
            }
            else
            {
                SharedAuthManager.ClearSavedLogin();
            }

            IsLoginInProgress = false;
            LoginStatusChanged?.Invoke(string.Empty);
            ShowLogin();
        }

        private void HandleGooglePlayGamesLogin(SharedLoginResult result)
        {
            IsLoginInProgress = false;

            if (!result.Success)
            {
                LoginStatusChanged?.Invoke(string.IsNullOrEmpty(result.ErrorMessage)
                    ? "Google Play Games login failed"
                    : result.ErrorMessage);
                return;
            }

            SharedAuthManager auth = ResolveAuthManager();
            if (auth != null)
            {
                auth.SaveGooglePlayGamesLogin(result.PlayerId, result.DisplayName);
            }
            else
            {
                SharedAuthManager.SaveGooglePlayGamesLoginState(result.PlayerId, result.DisplayName);
            }

            LoginStatusChanged?.Invoke("Google Play Games login complete");
            ShowHome();
        }

        private void LoginWithAppleGameCenter()
        {
            if (IsLoginInProgress)
            {
                return;
            }

            if (appleGameCenterLoginProvider == null)
            {
                LoginStatusChanged?.Invoke("Apple Game Center login is not configured");
                return;
            }

            IsLoginInProgress = true;
            LoginStatusChanged?.Invoke("Signing in to Apple Game Center...");
            appleGameCenterLoginProvider.Login(result =>
            {
                IsLoginInProgress = false;
                if (!result.Success)
                {
                    LoginStatusChanged?.Invoke(result.ErrorMessage);
                    return;
                }

                SavePlatformLogin(result, SharedLoginType.AppleGameCenter);
                LoginStatusChanged?.Invoke("Apple Game Center login complete");
                ShowHome();
            });
        }

        private void HandleRestoredPlatformLogin(SharedLoginResult result, Action<bool> completed)
        {
            IsLoginInProgress = false;

            if (!result.Success)
            {
                LoginStatusChanged?.Invoke(result.ErrorMessage);
                completed?.Invoke(false);
                return;
            }

#if UNITY_IOS
            SavePlatformLogin(result, SharedLoginType.AppleGameCenter);
#elif UNITY_ANDROID
            SavePlatformLogin(result, SharedLoginType.GooglePlayGames);
#endif
            LoginStatusChanged?.Invoke(string.Empty);
            completed?.Invoke(true);
        }

        private void SavePlatformLogin(SharedLoginResult result, SharedLoginType loginType)
        {
            SharedAuthManager auth = ResolveAuthManager();
            if (loginType == SharedLoginType.AppleGameCenter)
            {
                if (auth != null)
                {
                    auth.SaveAppleGameCenterLogin(result.PlayerId, result.DisplayName);
                }
                else
                {
                    SharedAuthManager.SaveAppleGameCenterLoginState(result.PlayerId, result.DisplayName);
                }
                return;
            }

            if (auth != null)
            {
                auth.SaveGooglePlayGamesLogin(result.PlayerId, result.DisplayName);
            }
            else
            {
                SharedAuthManager.SaveGooglePlayGamesLoginState(result.PlayerId, result.DisplayName);
            }
        }

        private SharedLoginProvider ResolveCurrentPlatformProvider()
        {
#if UNITY_IOS
            return appleGameCenterLoginProvider;
#elif UNITY_ANDROID
            return googlePlayGamesLoginProvider;
#else
            return null;
#endif
        }

        private SharedAuthManager ResolveAuthManager()
        {
            if (authManager != null)
            {
                return authManager;
            }

            if (SharedCoreRoot.Instance != null)
            {
                authManager = SharedCoreRoot.Instance.Auth;
            }

            if (authManager == null)
            {
                authManager = UnityEngine.Object.FindFirstObjectByType<SharedAuthManager>();
            }

            return authManager;
        }

        private static bool IsKeyPressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (key == KeyCode.Escape && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }
    }
}
