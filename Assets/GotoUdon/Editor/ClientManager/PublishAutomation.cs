using UnityEditor;
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
                if (state != PlayModeStateChange.ExitingPlayMode || !GotoUdonInternalState.Instance.enableAutomaticPublish) return;
                GotoUdonInternalState.Instance.enableAutomaticPublish = false;
                EditorUtility.SetDirty(GotoUdonInternalState.Instance);
                AssetDatabase.SaveAssets();
                ClientManagerEditor.Instance.StartClients();
                return;
            }

            if (!GotoUdonInternalState.Instance.enableAutomaticPublish) return;
            GameObject gameObject = new GameObject("GotoUdonAutomation");
            gameObject.tag = "EditorOnly";
            gameObject.AddComponent<VRC.SDK.PublishAutomation>();
        }
    }
}