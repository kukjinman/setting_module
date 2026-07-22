using SharedCoreModule;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    /// <summary>
    /// Runtime customization boundary for the generated home screen.
    /// Each game can replace the artwork and profile copy without rebuilding the shared layout.
    /// </summary>
    public sealed class SharedHomeView : MonoBehaviour
    {
        [SerializeField] private Image artworkImage;
        [SerializeField] private Text profileSlotText;
        [SerializeField] private Text profileNameText;
        [SerializeField] private bool useSavedProfile = true;

        private LocalizedString playingAsString;
        private bool isProfileLocalizationBound;
        private string currentDisplayName = "Guest";

        public Sprite Artwork => artworkImage != null ? artworkImage.sprite : null;

        public void Configure(Image artwork, Text profileSlot, Text profileName)
        {
            artworkImage = artwork;
            profileSlotText = profileSlot;
            profileNameText = profileName;
        }

        private void OnEnable()
        {
            BindProfileLocalization();

            if (useSavedProfile)
            {
                RefreshSavedProfile();
            }
        }

        private void OnDisable()
        {
            if (playingAsString != null && isProfileLocalizationBound)
            {
                playingAsString.StringChanged -= ApplyLocalizedProfileName;
            }

            isProfileLocalizationBound = false;
        }

        public void SetArtwork(Sprite artwork)
        {
            if (artworkImage == null)
            {
                return;
            }

            artworkImage.sprite = artwork;
            artworkImage.color = artwork == null ? Color.black : Color.white;
            artworkImage.preserveAspect = false;
        }

        public void ClearArtwork()
        {
            SetArtwork(null);
        }

        public void SetProfile(string slotLabel, string displayName)
        {
            useSavedProfile = false;
            ApplyProfile(slotLabel, displayName);
        }

        public void UseSavedProfile()
        {
            useSavedProfile = true;
            RefreshSavedProfile();
        }

        public void RefreshSavedProfile()
        {
            string displayName = "Guest";
            SharedCoreRoot core = SharedCoreRoot.Instance;

            if (core != null && core.Auth != null && !string.IsNullOrWhiteSpace(core.Auth.DisplayName))
            {
                displayName = core.Auth.DisplayName;
            }

            ApplyProfile("P1", displayName);
        }

        private void ApplyProfile(string slotLabel, string displayName)
        {
            string resolvedDisplayName = string.IsNullOrWhiteSpace(displayName) ? "Guest" : displayName;
            currentDisplayName = resolvedDisplayName;

            if (profileSlotText != null)
            {
                profileSlotText.text = string.IsNullOrWhiteSpace(slotLabel) ? "P1" : slotLabel;
            }

            if (profileNameText != null)
            {
                profileNameText.text = "Playing as " + resolvedDisplayName;
            }

            BindProfileLocalization();
        }

        private void BindProfileLocalization()
        {
            if (playingAsString == null)
            {
                playingAsString = new LocalizedString("Shared UI", "playing_as");
            }

            playingAsString.Arguments = new object[] { currentDisplayName };

            if (isActiveAndEnabled && !isProfileLocalizationBound)
            {
                playingAsString.StringChanged += ApplyLocalizedProfileName;
                isProfileLocalizationBound = true;
            }
        }

        private void ApplyLocalizedProfileName(string value)
        {
            if (profileNameText != null && !string.IsNullOrEmpty(value))
            {
                profileNameText.text = value;
            }
        }
    }
}
