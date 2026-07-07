using UnityEngine;

namespace SharedCoreModule
{
    [DefaultExecutionOrder(-1000)]
    public sealed class SharedCoreRoot : MonoBehaviour
    {
        [SerializeField] private bool dontDestroyOnLoad = true;
        [SerializeField] private bool destroyDuplicateRoots = true;
        [SerializeField] private SharedSaveManager saveManager;
        [SerializeField] private SharedAudioManager audioManager;
        [SerializeField] private SharedAuthManager authManager;

        public static SharedCoreRoot Instance { get; private set; }

        public SharedSaveManager Save => saveManager;
        public SharedAudioManager Audio => audioManager;
        public SharedAuthManager Auth => authManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (destroyDuplicateRoots)
                {
                    Destroy(gameObject);
                }

                return;
            }

            Instance = this;

            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            EnsureManagers();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void EnsureManagers()
        {
            if (saveManager == null)
            {
                saveManager = GetComponentInChildren<SharedSaveManager>(true);
            }

            if (audioManager == null)
            {
                audioManager = GetComponentInChildren<SharedAudioManager>(true);
            }

            if (authManager == null)
            {
                authManager = GetComponentInChildren<SharedAuthManager>(true);
            }

            if (saveManager == null)
            {
                saveManager = gameObject.AddComponent<SharedSaveManager>();
            }

            if (audioManager == null)
            {
                audioManager = gameObject.AddComponent<SharedAudioManager>();
            }

            if (authManager == null)
            {
                authManager = gameObject.AddComponent<SharedAuthManager>();
            }
        }
    }
}
