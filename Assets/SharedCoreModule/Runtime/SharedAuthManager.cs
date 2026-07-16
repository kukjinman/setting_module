using UnityEngine;

namespace SharedCoreModule
{
    public enum SharedLoginType
    {
        None,
        Guest,
        GooglePlayGames,
        AppleGameCenter
    }

    public sealed class SharedAuthManager : MonoBehaviour
    {
        private const string LoggedInKey = "shared_auth_logged_in";
        private const string LoginTypeKey = "shared_auth_login_type";
        private const string PlayerIdKey = "shared_auth_player_id";
        private const string DisplayNameKey = "shared_auth_display_name";
        private const string AutoLoginSuppressedKey = "shared_auth_auto_login_suppressed";

        public bool IsLoggedIn => HasSavedLoginState();
        public string LoginType => PlayerPrefs.GetString(LoginTypeKey, string.Empty);
        public SharedLoginType SavedLoginType => GetSavedLoginType();
        public string PlayerId => PlayerPrefs.GetString(PlayerIdKey, string.Empty);
        public string DisplayName => PlayerPrefs.GetString(DisplayNameKey, string.Empty);

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

        public static SharedLoginType GetSavedLoginType()
        {
            return ParseLoginType(PlayerPrefs.GetString(LoginTypeKey, string.Empty));
        }

        public static bool CanAutoLoginWithPlatform()
        {
            return PlayerPrefs.GetInt(AutoLoginSuppressedKey, 0) == 0;
        }

        public static void SaveGuestLogin(bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(LoggedInKey, 1);
            PlayerPrefs.SetString(LoginTypeKey, "guest");
            PlayerPrefs.DeleteKey(PlayerIdKey);
            PlayerPrefs.SetString(DisplayNameKey, "Guest");
            PlayerPrefs.DeleteKey(AutoLoginSuppressedKey);
            SaveIfNeeded(saveImmediately);
        }

        public void SaveGooglePlayGamesLogin(string playerId, string displayName, bool saveImmediately = true)
        {
            SaveGooglePlayGamesLoginState(playerId, displayName, saveImmediately);
        }

        public static void SaveGooglePlayGamesLoginState(
            string playerId,
            string displayName,
            bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(LoggedInKey, 1);
            PlayerPrefs.SetString(LoginTypeKey, "google_play_games");
            PlayerPrefs.SetString(PlayerIdKey, playerId ?? string.Empty);
            PlayerPrefs.SetString(DisplayNameKey, displayName ?? string.Empty);
            PlayerPrefs.DeleteKey(AutoLoginSuppressedKey);
            SaveIfNeeded(saveImmediately);
        }

        public void SaveAppleGameCenterLogin(string playerId, string displayName, bool saveImmediately = true)
        {
            SaveAppleGameCenterLoginState(playerId, displayName, saveImmediately);
        }

        public static void SaveAppleGameCenterLoginState(
            string playerId,
            string displayName,
            bool saveImmediately = true)
        {
            PlayerPrefs.SetInt(LoggedInKey, 1);
            PlayerPrefs.SetString(LoginTypeKey, "apple_game_center");
            PlayerPrefs.SetString(PlayerIdKey, playerId ?? string.Empty);
            PlayerPrefs.SetString(DisplayNameKey, displayName ?? string.Empty);
            PlayerPrefs.DeleteKey(AutoLoginSuppressedKey);
            SaveIfNeeded(saveImmediately);
        }

        public static void ClearSavedLogin(bool saveImmediately = true)
        {
            PlayerPrefs.DeleteKey(LoggedInKey);
            PlayerPrefs.DeleteKey(LoginTypeKey);
            PlayerPrefs.DeleteKey(PlayerIdKey);
            PlayerPrefs.DeleteKey(DisplayNameKey);
            PlayerPrefs.SetInt(AutoLoginSuppressedKey, 1);

            SaveIfNeeded(saveImmediately);
        }

        private static void SaveIfNeeded(bool saveImmediately)
        {
            if (saveImmediately)
            {
                PlayerPrefs.Save();
            }
        }

        private static SharedLoginType ParseLoginType(string value)
        {
            switch (value)
            {
                case "guest":
                    return SharedLoginType.Guest;
                case "google_play_games":
                    return SharedLoginType.GooglePlayGames;
                case "apple_game_center":
                    return SharedLoginType.AppleGameCenter;
                default:
                    return SharedLoginType.None;
            }
        }
    }
}
