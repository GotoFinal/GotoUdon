#if GOTOUDON_SIMULATION_TEMP_DISABLED
using System.Collections.Generic;
using GotoUdon.Utils.Editor;
using GotoUdon.VRC;
using UnityEditor;
using UnityEngine;

namespace GotoUdon.Editor
{
    public class SimulatedPlayerEditor
    {
        public static void DrawAvailableRuntimePlayer(VRCEmulator emulator, SimulatedVRCPlayer player)
        {
            DrawRuntimePlayer(player);
            SimpleGUI.ActionButton("Connect", () => emulator.AddPlayer(player));
        }

        public static void DrawActiveRuntimePlayer(VRCEmulator emulator, SimulatedVRCPlayer player)
        {
            DrawRuntimePlayer(player);
            GUILayout.BeginHorizontal();
            if (emulator.GetAmountOfPlayers() > 1)
            {
                SimpleGUI.ActionButton("Disconnect", () => emulator.RemovePlayer(player));
            }

            if (!player.IsUsingVR())
            {
                SimpleGUI.ActionButton("Make VR", player.PromoteToVRUser);
            }
            else SimpleGUI.ActionButton("Make Desktop", player.DemoteToDesktopUser);

            SimpleGUI.ActionButton("Make Master", () => emulator.MakeMaster(player));
            SimpleGUI.ActionButton("Make Local", () => emulator.MakeLocal(player));
            GUILayout.EndHorizontal();

            SimpleGUI.DrawFoldout(player, "More settings", () =>
            {
                AvatarChangeDialog(player);
                TagsDialog(player);
            });
        }

        private static void TagsDialog(SimulatedVRCPlayer player)
        {
            Dictionary<string, string> tags = player.GetRawTags();
            if (tags.Count > 0)
            {
                SimpleGUI.IndentWithHeader(
                    () => GUILayout.Label("Tags: "),
                    () =>
                    {
                        foreach (string key in new List<string>(tags.Keys))
                        {
                            GUILayout.BeginHorizontal();
                            tags[key] = EditorGUILayout.TextField(key, tags[key]);
                            SimpleGUI.ActionButton("X", () => tags.Remove(key), GUILayout.MaxWidth(30));
                            GUILayout.EndHorizontal();
                        }
                    }
                );
            }

            GUILayout.BeginHorizontal();
            string tagName = player.SetMetadata<string>("tagName", oldName => EditorGUILayout.TextField(oldName));
            string tagValue = player.SetMetadata<string>("tagValue", oldName => EditorGUILayout.TextField(oldName));
            SimpleGUI.ActionButton("Add tag", () => tags[tagName] = tagValue);
            GUILayout.EndHorizontal();
        }

        private static void AvatarChangeDialog(SimulatedVRCPlayer player)
        {
            GameObject avatar = player.SetMetadata<GameObject>("avatar",
                oldValue => SimpleGUI.ObjectField("Avatar", oldValue, false));
            if (avatar != null)
                SimpleGUI.ActionButton("Change Avatar", () => player.ChangeAvatar(avatar));
        }

        private static void DrawRuntimePlayer(SimulatedVRCPlayer player)
        {
            string playerString = GetPlayerDisplayFormat(player);

            EditorGUILayout.HelpBox(
                $"{playerString}, (id: {player.Id}) In VR: {player.IsUsingVR()}\n" +
                $"Grounded: {player.IsGrounded()}\n" +
                $"Walk speed: {player.walkSpeed}, run speed: {player.runSpeed}\n" +
                $"Gravity: {player.gravityStrength}, jump: {player.jumpImpulse}" +
                (player.silencedLevel != 0 ? $"\nSilenced level: {player.silencedLevel}\n" : "\n") +
                GetPlayerSpecialMarks(player),
                MessageType.None
            );
        }

        private static string GetPlayerSpecialMarks(SimulatedVRCPlayer player)
        {
            string otherMarks = "";
            if (player.legacyLocomotion)
                otherMarks += "Using legacy locomotion | ";
            if (player.immobile)
                otherMarks += "Immobile | ";
            if (!player.visible)
                otherMarks += "Invisible | ";
            if (!player.pickupsEnabled)
                otherMarks += "Pickups Disabled";
            return otherMarks;
        }

        private static string GetPlayerDisplayFormat(SimulatedVRCPlayer player)
        {
            string tag = "";
            if (player.VRCPlayer.isMaster)
            {
                tag = "MASTER ";
            }

            if (player.VRCPlayer.isLocal)
            {
                tag += "LOCAL ";
            }

            return tag + player.Name;
        }
    }
}
#endif