using System.Linq;
using Game.UI;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    // Lets the user step through every state of every screen in Play mode.
    // Lives in an Editor folder, so it never ships.
    public sealed class UiStateSwitcherWindow : EditorWindow
    {
        private Vector2 scroll;

        [MenuItem("Tools/UI States")]
        private static void Open() => GetWindow<UiStateSwitcherWindow>("UI States");

        private void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play mode to switch screen states.", MessageType.Info);
                return;
            }

            UiMotion.Instant = EditorGUILayout.Toggle("Instant motion", UiMotion.Instant);
            EditorGUILayout.Space();

            var screens = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .OfType<IScreenStates>()
                .OrderBy(s => s.ScreenName);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var screen in screens)
            {
                EditorGUILayout.LabelField(screen.ScreenName, EditorStyles.boldLabel);
                foreach (var state in screen.StateNames)
                {
                    if (GUILayout.Button(state)) screen.Apply(state);
                }
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndScrollView();
        }

        private void OnInspectorUpdate() => Repaint();
    }
}
