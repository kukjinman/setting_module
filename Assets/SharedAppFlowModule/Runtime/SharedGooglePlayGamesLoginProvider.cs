using System;

namespace SharedAppFlowModule
{
    /// <summary>
    /// Safe default used until a Google Play Games SDK adapter is installed.
    /// Replace this component with an SDK-backed SharedLoginProvider implementation.
    /// </summary>
    public sealed class SharedGooglePlayGamesLoginProvider : SharedLoginProvider
    {
        public override void Login(Action<SharedLoginResult> completed)
        {
            completed?.Invoke(SharedLoginResult.Failed(
                "Google Play Games SDK is not installed yet"));
        }
    }
}
