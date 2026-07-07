using System.Collections.Generic;
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
        [SerializeField] private KeyCode settingsToggleKey = KeyCode.Escape;

        private readonly Dictionary<SharedAppScreenId, SharedAppScreenPanel> panelMap = new Dictionary<SharedAppScreenId, SharedAppScreenPanel>();
        private SharedAppScreenId currentScreen;

        public SharedAppScreenId CurrentScreen => currentScreen;

        public SharedAuthManager Auth => ResolveAuthManager();

        public void Configure(SharedAppScreenPanel[] screenPanels, SharedSettingsModal modal, SharedAppScreenId initialScreen)
        {
            panels = screenPanels;
            settingsModal = modal;
            startScreen = initialScreen;
            BuildPanelMap();
        }

        private void Awake()
        {
            BuildPanelMap();
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

        public void ShowScreen(SharedAppScreenId screenId)
        {
            ShowScreen(screenId, false);
        }

        public void ShowScreen(SharedAppScreenId screenId, bool instant)
        {
            BuildPanelMap();
            currentScreen = screenId;

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

        public void LoginAsGuest()
        {
            SharedAuthManager auth = ResolveAuthManager();

            if (auth != null)
            {
                auth.LoginAsGuest();
            }
            else
            {
                PlayerPrefs.SetInt("shared_auth_logged_in", 1);
                PlayerPrefs.SetString("shared_auth_login_type", "guest");
                PlayerPrefs.Save();
            }

            ShowHome();
        }

        public void Logout()
        {
            SharedAuthManager auth = ResolveAuthManager();

            if (auth != null)
            {
                auth.Logout();
            }
            else
            {
                PlayerPrefs.DeleteKey("shared_auth_logged_in");
                PlayerPrefs.DeleteKey("shared_auth_login_type");
                PlayerPrefs.Save();
            }

            ShowLogin();
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

        private void BuildPanelMap()
        {
            panelMap.Clear();

            if (panels == null)
            {
                return;
            }

            foreach (SharedAppScreenPanel panel in panels)
            {
                if (panel != null)
                {
                    panelMap[panel.ScreenId] = panel;
                }
            }
        }
    }
}
