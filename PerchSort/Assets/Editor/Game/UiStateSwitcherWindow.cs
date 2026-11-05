using System;
using PerchSort.Game;
using UnityEditor;
using UnityEngine;

namespace PerchSort.Editor
{
    public sealed class UiStateSwitcherWindow : EditorWindow
    {
        private Vector2 scroll;
        private string filter = "";
        private GameRoot root;
        [MenuItem("Tools/UI States")]
        private static void Open() => GetWindow<UiStateSwitcherWindow>("UI States");
        private void OnEnable() => EditorApplication.playModeStateChanged += ModeChanged;
        private void OnDisable() => EditorApplication.playModeStateChanged -= ModeChanged;
        private void ModeChanged(PlayModeStateChange state) { root = null; Repaint(); }
        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Open Assets/Scenes/Game.unity and press Play to review board states.", MessageType.Info); return;
            }
            if (root == null) root = FindAnyObjectByType<GameRoot>();
            if (root == null || !root.IsReady || root.BoardStates == null)
            {
                EditorGUILayout.HelpBox("Waiting for GameRoot initialization.", MessageType.Info); return;
            }
            bool instant = EditorGUILayout.Toggle("Instant motion", GameMotion.Instant);
            if (instant != GameMotion.Instant)
            {
                GameMotion.Instant = instant;
                if (root.BoardStates.Active) root.BoardStates.Reapply();
            }
            EditorGUILayout.HelpBox("Synthetic data only. Return to real game restores the untouched attempt and profile. Select a state again to replay its motion.", MessageType.Info);
            if (GUILayout.Button("Return to real game")) root.EndBoardPreview();
            filter = EditorGUILayout.TextField("Filter", filter);
            EditorGUILayout.LabelField(root.BoardStates.CurrentState ?? "Real game", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (string state in root.BoardStates.StateNames)
            {
                if (filter.Length > 0 && state.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                if (GUILayout.Button(state)) root.BoardStates.Apply(state);
            }
            EditorGUILayout.EndScrollView();
        }
        private void OnInspectorUpdate() => Repaint();
    }
}
