﻿using UnityEditor;
using UnityEngine;

namespace GotoUdon.Editor.ClientManager
{
    [InitializeOnLoad]
    public class PublishAutomation
    {
        // register an event handler when the class is initialized
        static PublishAutomation()
        {
            EditorApplication.playModeStateChanged += OnModeChange;
        }

        private static void OnModeChange(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                if (state != PlayModeStateChange.ExitingPlayMode || !GotoUdonSettings.Instance.EnableAutomaticPublish) return;
                GotoUdonSettings.Instance.EnableAutomaticPublish = false;
                EditorUtility.SetDirty(GotoUdonSettings.Instance);
                AssetDatabase.SaveAssets();
                ClientManagerEditor.Instance.StartClients();
                return;
            }

            if (!GotoUdonSettings.Instance.EnableAutomaticPublish) return;
            GameObject gameObject = new GameObject("GotoUdonAutomation");
            gameObject.tag = "EditorOnly";
            gameObject.AddComponent<VRC.SDK.PublishAutomation>();
        }
    }
}