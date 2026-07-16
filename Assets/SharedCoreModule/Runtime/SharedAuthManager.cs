using UnityEngine;

namespace SharedCoreModule
{
    public sealed class SharedAuthManager : MonoBehaviour
    {
        private const string LoggedInKey = "shared_auth_logged_in";
        private const string LoginTypeKey = "shared_auth_login_type";

        public bool IsLoggedIn => HasSavedLoginState();
        public string LoginType => PlayerPrefs.GetString(LoginTypeKey, string.Empty);

        public bool HasSavedLogin()
        {
            return IsLoggedIn;
        }

        public void LoginAsGuest(bool saveImmediately = true)
        {
            SaveGuestLogin(saveImmediately);
        }

        public void Logout(bool saveImmediately = true)
        {
            ClearSavedLogin(saveImmediately);
        }

        public static bool HasSavedLoginState()
        {
            return PlayerPrefs.GetInt(LoggedInKey, 0) == 1;
        }

        public static void SaveGuestLogin(bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(LoggedInKey, 1);
            PlayerPrefs.SetString(LoginTypeKey, "guest");
            SaveIfNeeded(saveImmediately);
        }

        public static void ClearSavedLogin(bool saveImmediately = true)
        {
            PlayerPrefs.DeleteKey(LoggedInKey);
            PlayerPrefs.DeleteKey(LoginTypeKey);

            SaveIfNeeded(saveImmediately);
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
