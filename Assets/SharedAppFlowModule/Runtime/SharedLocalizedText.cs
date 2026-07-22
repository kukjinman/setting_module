using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public sealed class SharedLocalizedText : MonoBehaviour
    {
        [SerializeField] private string tableName = "Shared UI";
        [SerializeField] private string entryKey;

        private Text target;
        private LocalizedString localizedString;
        private bool isBound;

        public void Configure(string table, string key)
        {
            tableName = table;
            entryKey = key;
            RebuildReference();

            if (isActiveAndEnabled)
            {
                Bind();
            }
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(entryKey))
            {
                return;
            }

            if (target == null)
            {
                target = GetComponent<Text>();
            }

            if (localizedString == null)
            {
                RebuildReference();
            }

            if (localizedString == null || isBound)
            {
                return;
            }

            localizedString.StringChanged += ApplyText;
            isBound = true;
        }

        private void Unbind()
        {
            if (localizedString != null && isBound)
            {
                localizedString.StringChanged -= ApplyText;
            }

            isBound = false;
        }

        private void RebuildReference()
        {
            Unbind();

            if (!string.IsNullOrWhiteSpace(tableName) && !string.IsNullOrWhiteSpace(entryKey))
            {
                localizedString = new LocalizedString(tableName, entryKey);
            }
        }

        private void ApplyText(string value)
        {
            if (target != null && !string.IsNullOrEmpty(value))
            {
                target.text = value;
            }
        }
    }
}
