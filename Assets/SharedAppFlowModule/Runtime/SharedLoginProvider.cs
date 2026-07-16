using System;
using UnityEngine;

namespace SharedAppFlowModule
{
    public readonly struct SharedLoginResult
    {
        public bool Success { get; }
        public string PlayerId { get; }
        public string DisplayName { get; }
        public string ErrorMessage { get; }

        private SharedLoginResult(bool success, string playerId, string displayName, string errorMessage)
        {
            Success = success;
            PlayerId = playerId;
            DisplayName = displayName;
            ErrorMessage = errorMessage;
        }

        public static SharedLoginResult Succeeded(string playerId, string displayName)
        {
            return new SharedLoginResult(true, playerId, displayName, string.Empty);
        }

        public static SharedLoginResult Failed(string errorMessage)
        {
            return new SharedLoginResult(false, string.Empty, string.Empty, errorMessage);
        }
    }

    public abstract class SharedLoginProvider : MonoBehaviour
    {
        public abstract void Login(Action<SharedLoginResult> completed);

        public virtual void TryRestore(Action<SharedLoginResult> completed)
        {
            Login(completed);
        }
    }
}
