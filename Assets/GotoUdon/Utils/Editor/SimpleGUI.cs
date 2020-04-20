using System;
using UnityEditor;
using UnityEngine;

namespace GotoUdon.Utils.Editor
{
    public static class SimpleGUI
    {
        private const float OPTION_SPACING = 7;
        private const float SECTION_SPACING = 15;

        public static void OptionSpacing()
        {
            GUILayout.Space(OPTION_SPACING);
        }

        public static void SectionSpacing()
        {
            GUILayout.Space(SECTION_SPACING);
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
    }
}