using UnityEngine;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    [RequireComponent(typeof(Text))]
    public sealed class SharedLoginStatusText : MonoBehaviour
    {
        [SerializeField] private SharedAppFlowController controller;
        private Text label;

        public void Configure(SharedAppFlowController flowController)
        {
            controller = flowController;
        }

        private void Awake()
        {
            label = GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<SharedAppFlowController>();
            }

            if (controller != null)
            {
                controller.LoginStatusChanged += HandleStatusChanged;
            }
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.LoginStatusChanged -= HandleStatusChanged;
            }
        }

        private void HandleStatusChanged(string message)
        {
            if (label != null)
            {
                label.text = message;
            }
        }
    }
}
