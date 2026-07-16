using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace SharedAppFlowModule
{
    public sealed class SharedAppleGameCenterLoginProvider : SharedLoginProvider
    {
        [SerializeField, Min(1f)] private float timeoutSeconds = 20f;

        private Action<SharedLoginResult> pendingCompletion;
        private Coroutine timeoutRoutine;

        public override void Login(Action<SharedLoginResult> completed)
        {
            BeginAuthentication(completed, true);
        }

        public override void TryRestore(Action<SharedLoginResult> completed)
        {
            BeginAuthentication(completed, false);
        }

        private void BeginAuthentication(Action<SharedLoginResult> completed, bool allowLoginUi)
        {
            if (pendingCompletion != null)
            {
                completed?.Invoke(SharedLoginResult.Failed("Game Center login is already in progress"));
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            pendingCompletion = completed;
            timeoutRoutine = StartCoroutine(WaitForTimeout());
            SharedGameCenter_Authenticate(gameObject.name, allowLoginUi ? 1 : 0);
#else
            completed?.Invoke(SharedLoginResult.Failed("Game Center is only available on an iOS build"));
#endif
        }

        // Called by Assets/Plugins/iOS/SharedGameCenterBridge.mm via UnitySendMessage.
        [Preserve]
        private void OnGameCenterAuthenticationSucceeded(string json)
        {
            GameCenterPayload payload = JsonUtility.FromJson<GameCenterPayload>(json);
            if (payload == null)
            {
                Complete(SharedLoginResult.Failed("Game Center returned invalid player data"));
                return;
            }

            Complete(SharedLoginResult.Succeeded(payload.playerId, payload.displayName));
        }

        // Called by Assets/Plugins/iOS/SharedGameCenterBridge.mm via UnitySendMessage.
        [Preserve]
        private void OnGameCenterAuthenticationFailed(string message)
        {
            Complete(SharedLoginResult.Failed(string.IsNullOrEmpty(message)
                ? "Game Center login failed"
                : message));
        }

        private IEnumerator WaitForTimeout()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(1f, timeoutSeconds));
            Complete(SharedLoginResult.Failed("Game Center login timed out"));
        }

        private void Complete(SharedLoginResult result)
        {
            if (pendingCompletion == null)
            {
                return;
            }

            if (timeoutRoutine != null)
            {
                StopCoroutine(timeoutRoutine);
                timeoutRoutine = null;
            }

            Action<SharedLoginResult> completion = pendingCompletion;
            pendingCompletion = null;
            completion(result);
        }

        [Serializable]
        private sealed class GameCenterPayload
        {
            public string playerId;
            public string displayName;
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SharedGameCenter_Authenticate(string gameObjectName, int allowLoginUi);
#endif
    }
}
