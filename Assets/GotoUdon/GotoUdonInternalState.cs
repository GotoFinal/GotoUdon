using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GotoUdon
{
    public class GotoUdonInternalState : ScriptableObject
    {
        // singleton, this state must be persistent no matter what, and unity does not like this 
        private static GotoUdonInternalState _instance;
        public static GotoUdonInternalState Instance => _instance == null ? _instance = LoadSetting() : _instance;

        public bool enableAutomaticPublish;
        public List<ClientProcess> processes;
        public string instanceId;
        public string accessType;

        public void Init()
        {
            if (processes == null) processes = new List<ClientProcess>();
        }

        private static GotoUdonInternalState LoadSetting()
        {
#if UNITY_EDITOR
            GotoUdonInternalState settings =
                AssetDatabase.LoadAssetAtPath<GotoUdonInternalState>("Assets/GotoUdon/Settings/GotoUdonInternalState.asset");
            if (settings == null)
            {
                settings = CreateInstance<GotoUdonInternalState>();
                settings.Init();
                AssetDatabase.CreateAsset(settings, "Assets/GotoUdon/Settings/GotoUdonInternalState.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else settings.Init();

            return settings;
#else
            return null;
#endif
        }

    }
}