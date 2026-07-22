using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    public sealed class SharedLanguageSelector : MonoBehaviour
    {
        private const string SavedLocaleKey = "shared_app_flow_locale";

        [SerializeField] private Button toggleButton;
        [SerializeField] private Text currentLanguageLabel;
        [SerializeField] private GameObject popup;
        [SerializeField] private Transform optionsRoot;
        [SerializeField] private Button optionTemplate;

        private readonly List<GameObject> generatedOptions = new List<GameObject>();
        private bool initialized;

        public bool IsOpen => popup != null && popup.activeSelf;
        public event Action<string> LanguageChanged;

        public void Configure(
            Button languageButton,
            Text languageLabel,
            GameObject popupObject,
            Transform languageOptionsRoot,
            Button languageOptionTemplate)
        {
            toggleButton = languageButton;
            currentLanguageLabel = languageLabel;
            popup = popupObject;
            optionsRoot = languageOptionsRoot;
            optionTemplate = languageOptionTemplate;
        }

        private void Awake()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(Toggle);
            }

            if (popup != null)
            {
                popup.SetActive(false);
            }
        }

        private void OnEnable()
        {
            LocalizationSettings.SelectedLocaleChanged += HandleLocaleChanged;
            StartCoroutine(Initialize());
        }

        private void OnDisable()
        {
            LocalizationSettings.SelectedLocaleChanged -= HandleLocaleChanged;

            if (popup != null)
            {
                popup.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (toggleButton != null)
            {
                toggleButton.onClick.RemoveListener(Toggle);
            }
        }

        public void Toggle()
        {
            SetOpen(!IsOpen);
        }

        public void SetOpen(bool open)
        {
            if (popup == null)
            {
                return;
            }

            if (open)
            {
                RebuildOptions();
                popup.transform.SetAsLastSibling();
            }

            popup.SetActive(open);
        }

        public bool SelectLanguage(string localeCode)
        {
            if (!initialized || string.IsNullOrWhiteSpace(localeCode))
            {
                return false;
            }

            Locale locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
            if (locale == null)
            {
                return false;
            }

            LocalizationSettings.SelectedLocale = locale;
            return true;
        }

        private IEnumerator Initialize()
        {
            yield return LocalizationSettings.InitializationOperation;
            initialized = true;

            string savedCode = PlayerPrefs.GetString(SavedLocaleKey, string.Empty);
            if (!string.IsNullOrEmpty(savedCode))
            {
                Locale savedLocale = LocalizationSettings.AvailableLocales.GetLocale(savedCode);
                if (savedLocale != null)
                {
                    LocalizationSettings.SelectedLocale = savedLocale;
                }
            }

            if (LocalizationSettings.SelectedLocale == null)
            {
                Locale systemLocale = LocalizationSettings.AvailableLocales.GetLocale(Application.systemLanguage);
                IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
                LocalizationSettings.SelectedLocale = systemLocale != null
                    ? systemLocale
                    : locales.Count > 0 ? locales[0] : null;
            }

            RefreshCurrentLanguage();
        }

        private void RebuildOptions()
        {
            ClearGeneratedOptions();

            if (!initialized || optionsRoot == null || optionTemplate == null)
            {
                return;
            }

            IList<Locale> locales = LocalizationSettings.AvailableLocales.Locales;
            foreach (Locale locale in locales)
            {
                if (locale == null)
                {
                    continue;
                }

                Locale capturedLocale = locale;
                Button option = Instantiate(optionTemplate, optionsRoot);
                option.gameObject.name = "Language - " + locale.Identifier.Code;
                option.gameObject.SetActive(true);

                Text label = option.GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.text = GetLocaleDisplayName(locale);
                }

                option.interactable = LocalizationSettings.SelectedLocale != locale;
                option.onClick.AddListener(() => SelectLocale(capturedLocale));
                generatedOptions.Add(option.gameObject);
            }
        }

        private void SelectLocale(Locale locale)
        {
            LocalizationSettings.SelectedLocale = locale;
            SetOpen(false);
        }

        private void HandleLocaleChanged(Locale locale)
        {
            if (locale == null)
            {
                return;
            }

            PlayerPrefs.SetString(SavedLocaleKey, locale.Identifier.Code);
            PlayerPrefs.Save();
            RefreshCurrentLanguage();
            LanguageChanged?.Invoke(locale.Identifier.Code);
        }

        private void RefreshCurrentLanguage()
        {
            if (currentLanguageLabel == null)
            {
                return;
            }

            Locale selected = LocalizationSettings.SelectedLocale;
            currentLanguageLabel.text = selected == null
                ? "A/文  Language"
                : "A/文  " + GetLocaleDisplayName(selected);
        }

        private void ClearGeneratedOptions()
        {
            foreach (GameObject option in generatedOptions)
            {
                if (option != null)
                {
                    Destroy(option);
                }
            }

            generatedOptions.Clear();
        }

        private static string GetLocaleDisplayName(Locale locale)
        {
            return string.IsNullOrWhiteSpace(locale.LocaleName)
                ? locale.Identifier.Code
                : locale.LocaleName;
        }
    }
}
