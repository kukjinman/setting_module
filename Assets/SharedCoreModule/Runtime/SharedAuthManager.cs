using UnityEngine;

namespace SharedCoreModule
{
    public sealed class SharedAuthManager : MonoBehaviour
    {
        private const string LoggedInKey = "shared_auth_logged_in";
        private const string LoginTypeKey = "shared_auth_login_type";

        public bool IsLoggedIn => PlayerPrefs.GetInt(LoggedInKey, 0) == 1;
        public string LoginType => PlayerPrefs.GetString(LoginTypeKey, string.Empty);

        public bool HasSavedLogin()
        {
            return IsLoggedIn;
        }

        public void LoginAsGuest(bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(LoggedInKey, 1);
            PlayerPrefs.SetString(LoginTypeKey, "guest");

            if (saveImmediately)
            {
                PlayerPrefs.Save();
            }
        }

        public void Logout(bool saveImmediately = true)
        {
            PlayerPrefs.DeleteKey(LoggedInKey);
            PlayerPrefs.DeleteKey(LoginTypeKey);

            if (saveImmediately)
            {
                PlayerPrefs.Save();
            }
        }
    }
}
