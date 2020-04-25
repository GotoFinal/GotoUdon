using System;
using UnityEngine;

namespace GotoUdon.Utils
{
    public static class GotoLog
    {
        public static void Log(string message)
        {
            Debug.Log("[GotoUdon] " + message);
        }

        public static void Warn(string message)
        {
            Debug.LogWarning("[GotoUdon] " + message);
        }

        public static void Error(string message)
        {
            Debug.LogError("[GotoUdon] " + message);
        }

        public static void Assert(string message)
        {
            Debug.LogAssertion("[GotoUdon] " + message);
        }

        public static void Exception(string message, Exception exception)
        {
            Error(message);
            Debug.LogException(exception);
        }
    }
}