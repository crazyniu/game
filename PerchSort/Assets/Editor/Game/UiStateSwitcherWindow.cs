using System;
using System.Collections.Generic;
using System.Reflection;
using PerchSort.Game;
using UnityEditor;
using UnityEngine;

namespace PerchSort.Editor
{
    public sealed class UiStateSwitcherWindow : EditorWindow
    {
        private sealed class Screen
        {
            public IScreenStates States;
            public bool Expanded = true;
        }
        private readonly List<Screen> screens = new List<Screen>();
        private Vector2 scroll;
        private string filter = "";
        private GameRoot root;
        private IScreenStates activeScreen;
        private string activeState;
        [MenuItem("Tools/UI States")]
        private static void Open() => GetWindow<UiStateSwitcherWindow>("UI States");
        private void OnEnable() => EditorApplication.playModeStateChanged += ModeChanged;
        private void OnDisable() => EditorApplication.playModeStateChanged -= ModeChanged;
        private void ModeChanged(PlayModeStateChange state)
        {
            root = null; screens.Clear(); activeScreen = null; activeState = null; Repaint();
        }
        private void Discover(GameRoot owner)
        {
            root = owner; screens.Clear(); activeScreen = null; activeState = null;
            // GameRoot routes board input and completion to this exact instance.
            if (root.BoardStates != null) screens.Add(new Screen { States = root.BoardStates });
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IScreenStates>())
            {
                if (type == typeof(BoardFakeData) || !type.IsPublic || !type.IsSealed || type.IsAbstract) continue;
                var constructor = type.GetConstructor(new[] { typeof(GameRoot) });
                if (constructor == null) continue;
                try { screens.Add(new Screen { States = (IScreenStates)constructor.Invoke(new object[] { root }) }); }
                catch (Exception error) { Debug.LogException(error is TargetInvocationException invocation ? invocation.InnerException ?? error : error); }
            }
            screens.Sort((left, right) => StringComparer.Ordinal.Compare(left.States.ScreenName, right.States.ScreenName));
        }
        private void Apply(IScreenStates screen, string state)
        {
            try
            {
                if (activeScreen != null && activeScreen != screen) root.EndBoardPreview();
                screen.Apply(state); activeScreen = screen; activeState = state;
            }
            catch (Exception error) { Debug.LogException(error); }
        }
        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Open Assets/Scenes/Game.unity and press Play to review UI states.", MessageType.Info); return;
            }
            if (root == null)
            {
                var owner = FindAnyObjectByType<GameRoot>();
                if (owner != null && owner.IsReady) Discover(owner);
            }
            if (root == null || !root.IsReady)
            {
                EditorGUILayout.HelpBox("Waiting for GameRoot initialization.", MessageType.Info); return;
            }
            bool instant = EditorGUILayout.Toggle("Instant motion", GameMotion.Instant);
            if (instant != GameMotion.Instant)
            {
                GameMotion.Instant = instant;
                if (root.BoardStates.Active && (activeScreen == null || activeScreen == root.BoardStates))
                {
                    root.BoardStates.Reapply(); activeState = root.BoardStates.CurrentState;
                }
                else if (activeScreen != null) Apply(activeScreen, activeState);
            }
            EditorGUILayout.HelpBox("Synthetic data only. Return to real game restores the untouched attempt and profile. Select a state again to replay its motion.", MessageType.Info);
            if (GUILayout.Button("Return to real game"))
            {
                root.EndBoardPreview(); activeScreen = null; activeState = null;
            }
            filter = EditorGUILayout.TextField("Filter", filter);
            EditorGUILayout.LabelField(activeScreen != null ? activeScreen.ScreenName + " / " + activeState :
                root.BoardStates.CurrentState != null && root.BoardStates.Active ? "Board / " + root.BoardStates.CurrentState : "Real game", EditorStyles.boldLabel);
            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var screen in screens)
            {
                var states = screen.States;
                bool screenMatches = filter.Length == 0 || states.ScreenName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                if (!screenMatches)
                {
                    bool found = false;
                    foreach (string state in states.StateNames)
                        if (state.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0) { found = true; break; }
                    if (!found) continue;
                }
                screen.Expanded = EditorGUILayout.Foldout(screen.Expanded, states.ScreenName, true);
                if (!screen.Expanded) continue;
                EditorGUI.indentLevel++;
                foreach (string state in states.StateNames)
                {
                    if (!screenMatches && state.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (GUILayout.Button(state)) Apply(states, state);
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndScrollView();
        }
        private void OnInspectorUpdate() => Repaint();
    }
}
