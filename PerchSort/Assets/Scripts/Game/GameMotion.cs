using System;
using System.Collections.Generic;
using LitMotion;
using UnityEngine;

namespace PerchSort.Game
{
    public sealed class GameMotion : MonoBehaviour
    {
        public static bool Instant;
        private readonly List<MotionHandle> handles = new List<MotionHandle>();
        public void Play(float duration, Action<float> update, Action complete = null, float delay = 0, Ease ease = Ease.Linear)
        {
            if (Instant) { update(1); complete?.Invoke(); return; }
            Prune();
            var motion = LMotion.Create(0f, 1f, duration).WithDelay(delay).WithEase(ease)
                .WithOnComplete(() => complete?.Invoke()).Bind(update).AddTo(gameObject);
            handles.Add(motion);
        }
        public void Loop(float duration, Action<float> update)
        {
            if (Instant) return;
            Prune();
            handles.Add(LMotion.Create(0f, 1f, duration).WithLoops(-1)
                .Bind(update).AddTo(gameObject));
        }
        private void Prune()
        {
            for (int i = handles.Count - 1; i >= 0; i--)
                if (!handles[i].IsActive()) handles.RemoveAt(i);
        }
        public void CancelAll()
        {
            foreach (var handle in handles) if (handle.IsActive()) handle.Cancel();
            handles.Clear();
        }
        private void OnDisable() => CancelAll();
        private void OnDestroy() => CancelAll();
    }
}
