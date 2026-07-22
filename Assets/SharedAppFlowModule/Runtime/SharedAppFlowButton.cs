using UnityEngine;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    public enum SharedAppFlowButtonAction
    {
        ShowScreen,
        OpenSettings,
        CloseSettings,
        ToggleSettings,
        LoginAsGuest,
        Logout,
        LoginWithGooglePlayGames,
        LoginWithPlatform,
        Quit
    }

    [RequireComponent(typeof(Button))]
    public sealed class SharedAppFlowButton : MonoBehaviour
    {
        [SerializeField] private SharedAppFlowController controller;
        [SerializeField] private SharedAppFlowButtonAction action = SharedAppFlowButtonAction.ShowScreen;
        [SerializeField] private SharedAppScreenId targetScreen = SharedAppScreenId.Home;

        private Button button;

        public void Configure(SharedAppFlowController flowController, SharedAppFlowButtonAction buttonAction, SharedAppScreenId screen)
        {
            controller = flowController;
            action = buttonAction;
            targetScreen = screen;
        }

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<SharedAppFlowController>();
            }

            if (controller == null)
            {
                return;
            }

            switch (action)
            {
                case SharedAppFlowButtonAction.ShowScreen:
                    controller.ShowScreen(targetScreen);
                    break;
                case SharedAppFlowButtonAction.OpenSettings:
                    controller.OpenSettings();
                    break;
                case SharedAppFlowButtonAction.CloseSettings:
                    controller.CloseSettings();
                    break;
                case SharedAppFlowButtonAction.ToggleSettings:
                    controller.ToggleSettings();
                    break;
                case SharedAppFlowButtonAction.LoginAsGuest:
                    controller.LoginAsGuest();
                    break;
                case SharedAppFlowButtonAction.LoginWithGooglePlayGames:
                    controller.LoginWithGooglePlayGames();
                    break;
                case SharedAppFlowButtonAction.LoginWithPlatform:
                    controller.LoginWithPlatform();
                    break;
                case SharedAppFlowButtonAction.Logout:
                    controller.Logout();
                    break;
                case SharedAppFlowButtonAction.Quit:
                    controller.QuitApplication();
                    break;
            }
        }
    }
}
