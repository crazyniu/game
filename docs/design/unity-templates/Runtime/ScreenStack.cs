using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    // The only way screens and popups open and close. Back and close always pop,
    // and listeners such as the music player follow TopChanged.
    public sealed class ScreenStack : MonoBehaviour
    {
        private readonly List<UiScreen> stack = new List<UiScreen>();

        public UiScreen Top => stack.Count > 0 ? stack[stack.Count - 1] : null;

        // Raised after every change with the new top screen (null when empty).
        public event Action<UiScreen> TopChanged;

        public void Push(UiScreen screen)
        {
            if (screen == null || stack.Contains(screen)) return;
            Top?.OnCovered(screen);
            stack.Add(screen);
            screen.CloseRequested += Pop;
            screen.Show();
            TopChanged?.Invoke(screen);
        }

        public void Pop()
        {
            if (stack.Count == 0) return;
            var top = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            top.CloseRequested -= Pop;
            top.Hide();
            Top?.OnRevealed();
            TopChanged?.Invoke(Top);
        }

        public void Replace(UiScreen screen)
        {
            if (stack.Count > 0)
            {
                var top = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                top.CloseRequested -= Pop;
                top.Hide();
            }
            Push(screen);
        }

        public void ClearTo(UiScreen screen)
        {
            while (stack.Count > 0)
            {
                var top = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
                top.CloseRequested -= Pop;
                top.Hide();
            }
            Push(screen);
        }

        // Call from the project's input handling for the Android back key or Escape.
        public void OnBack()
        {
            if (stack.Count > 1) Pop();
        }
    }
}
