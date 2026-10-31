#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;

namespace PerchSort.UI
{
    // Editor screenshot sequences need a runtime coroutine host. Excluded from player builds.
    public sealed class CaptureRoutine : MonoBehaviour
    {
        private Action<int> finish;
        public void Run(IEnumerator sequence, Action<int> onFinished)
        {
            DontDestroyOnLoad(gameObject);
            finish = onFinished;
            Application.logMessageReceived += OnLog;
            StartCoroutine(sequence);
        }

        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
                UnityEditor.EditorApplication.delayCall += () => finish(1);
        }

        private void OnDestroy() => Application.logMessageReceived -= OnLog;
    }
}
#endif
