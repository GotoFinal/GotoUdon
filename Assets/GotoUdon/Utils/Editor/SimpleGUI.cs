using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;

namespace GotoUdon.Utils.Editor
{
    public static class SimpleGUI
    {
        private static readonly Color SECTION_COLOR = new Color(0.311F, 0.349F, 0.340F);
        private const float OPTION_SPACING = 7;
        private const float SECTION_SPACING = 15;

        // sometimes I get semi random stack error, might be related to focus, or some unity bug
        public static bool EndChangeCheck()
        {
            try
            {
                return EditorGUI.EndChangeCheck();
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static void OptionSpacing()
        {
            GUILayout.Space(OPTION_SPACING);
        }

        public static void SectionSpacing()
        {
            GUILayout.Space(SECTION_SPACING / 2);
            DrawUILine(SECTION_COLOR);
            GUILayout.Space(SECTION_SPACING / 2);
        }

        public static void ActionButton(string name, Action action, params GUILayoutOption[] options)
        {
            if (GUILayout.Button(name, options))
            {
                action();
            }
        }

        public static void Indent(Action action)
        {
            EditorGUI.indentLevel++;
            action();
            EditorGUI.indentLevel--;
        }

        public static void IndentWithHeader(Action header, Action action)
        {
            header();
            EditorGUI.indentLevel++;
            action();
            EditorGUI.indentLevel--;
        }

        public static bool InfoBox(bool when, string info)
        {
            if (when)
                EditorGUILayout.HelpBox(info, MessageType.Info);
            return when;
        }

        public static bool WarningBox(bool when, string info)
        {
            if (when)
                EditorGUILayout.HelpBox(info, MessageType.Warning);
            return when;
        }

        public static bool ErrorBox(bool when, string info)
        {
            if (when)
                EditorGUILayout.HelpBox(info, MessageType.Error);
            return when;
        }

        public static T ObjectField<T>(
            string label,
            T obj,
            bool allowSceneObjects,
            params GUILayoutOption[] options
        ) where T : UnityEngine.Object
        {
            return EditorGUILayout.ObjectField(label, obj, typeof(T), allowSceneObjects, options) as T;
        }

        private static Dictionary<object, bool> foldouts = new Dictionary<object, bool>();

        public static bool DrawFoldout(object key, string title, bool defaultState = false)
        {
            key = new Tuple<object, string>(key, title);
            if (!foldouts.ContainsKey(key))
                foldouts[key] = defaultState;

            return foldouts[key] = EditorGUILayout.Foldout(foldouts[key], title, true);
        }

        public static bool DrawFoldout(object key, string title, Action foldout, bool defaultState = false)
        {
            if (DrawFoldout(key, title, defaultState))
            {
                Indent(foldout);
                return true;
            }

            return false;
        }

        public static void DrawSetOptionHorizontal<T>(string label, ICollection<T> enabledStates, T state, int width)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
            GUILayout.Label(label);
            bool stateEnabled = enabledStates.Contains(state);
            if (stateEnabled != EditorGUILayout.Toggle(stateEnabled, GUILayout.MaxWidth(20)))
            {
                if (stateEnabled) enabledStates.Remove(state);
                else enabledStates.Add(state);
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }

        public static void DrawFooterInformation()
        {
            if (!DrawFoldout("Footer", "Contact", true))
                return;
            string discordUrl = "https://discord.gg/B8hbbax";
            if (GUILayout.Button($"Click to join (or just add me GotoFinal#5189) on discord for help: {discordUrl}", EditorStyles.helpBox))
            {
                Application.OpenURL(discordUrl);
            }

            if (GUILayout.Button(
                "For best experience also try UdonSharp by Merlin and write Udon in C#! https://github.com/Merlin-san/UdonSharp/",
                EditorStyles.helpBox))
            {
                Application.OpenURL("https://github.com/Merlin-san/UdonSharp/");
            }
        }
    }
}