using UnityEngine;

namespace SharedCoreModule
{
    public sealed class SharedSaveManager : MonoBehaviour
    {
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetInt(string key, int value, bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(key, value);
            SaveIfNeeded(saveImmediately);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetFloat(string key, float value, bool saveImmediately = true)
        {
            PlayerPrefs.SetFloat(key, value);
            SaveIfNeeded(saveImmediately);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetString(string key, string value, bool saveImmediately = true)
        {
            PlayerPrefs.SetString(key, value);
            SaveIfNeeded(saveImmediately);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public void SetBool(string key, bool value, bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            SaveIfNeeded(saveImmediately);
        }

        public void DeleteKey(string key, bool saveImmediately = true)
        {
            PlayerPrefs.DeleteKey(key);
            SaveIfNeeded(saveImmediately);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }

        private static void SaveIfNeeded(bool saveImmediately)
        {
            if (saveImmediately)
            {
                PlayerPrefs.Save();
            }
        }
    }
}
