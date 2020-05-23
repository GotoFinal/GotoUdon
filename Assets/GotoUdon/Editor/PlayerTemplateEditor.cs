using GotoUdon.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace GotoUdon.Editor
{
    public class PlayerTemplateEditor
    {
        public static bool DrawPlayerTemplateWithRemoveButton(PlayerTemplate playerTemplate)
        {
            return DrawPlayerTemplateWithRemoveButton(playerTemplate, true);
        }

        public static void DrawPlayerTemplate(PlayerTemplate playerTemplate)
        {
            DrawPlayerTemplateWithRemoveButton(playerTemplate, false);
        }

        private static bool DrawPlayerTemplateWithRemoveButton(PlayerTemplate playerTemplate, bool withRemoveButton)
        {
            EditorGUI.indentLevel++;
            playerTemplate.playerName = EditorGUILayout.TextField("Name", playerTemplate.playerName);

            SimpleGUI.DrawFoldout(playerTemplate, "More settings", () =>
            {
                playerTemplate.avatarPrefab = SimpleGUI.ObjectField("Custom avatar", playerTemplate.avatarPrefab, false);
                playerTemplate.spawnPoint = SimpleGUI.ObjectField("Custom spawn point", playerTemplate.spawnPoint, true);
                playerTemplate.customId = EditorGUILayout.IntField("Custom id", playerTemplate.customId);
            });

            GUILayout.BeginHorizontal();

            GUILayout.Label("Has Vr");
            playerTemplate.hasVr = EditorGUILayout.Toggle(playerTemplate.hasVr);

            GUILayout.Label("Join on start");
            playerTemplate.joinByDefault = EditorGUILayout.Toggle(playerTemplate.joinByDefault);

            bool remove = false;
            if (withRemoveButton)
                remove = GUILayout.Button("Remove");

            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
            return remove;
        }
    }
}