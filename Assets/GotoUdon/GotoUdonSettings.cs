using System.Collections.Generic;
using GotoUdon.Editor;
using GotoUdon.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Components;

namespace GotoUdon
{
    public class GotoUdonSettings : ScriptableObject
    {
        // singleton because can't delay udon and we need to be ready to create emulator at any point
        private static GotoUdonSettings _instance;
        public static GotoUdonSettings Instance => _instance == null ? _instance = LoadSetting() : _instance;

        public bool enableSimulation = true;
        public GameObject avatarPrefab;
        public Transform spawnPoint;
        public List<PlayerTemplate> playerTemplates = new List<PlayerTemplate>();

        private const string SIMULATOR_DEFINE = "GOTOUDON_SIMULATION";

#if UNITY_EDITOR
        public bool IsSimulatorInstalled
        {
            get => UnityCompilerUtils.IsDefineEnabled(SIMULATOR_DEFINE);
            set => UnityCompilerUtils.SetDefineEnabled(SIMULATOR_DEFINE, value);
        }
#endif

        public void Init()
        {
            // lazy handle / ignore incompatible changes
            if (playerTemplates == null)
            {
                playerTemplates = new List<PlayerTemplate>();
            }

            playerTemplates.RemoveAll(obj => obj == null);

            // try to use vrc component and get first spawn location
#if UNITY_EDITOR
            if (spawnPoint == null)
            {
                VRCSceneDescriptor descriptor = FindObjectOfType<VRCSceneDescriptor>();
                if (descriptor != null)
                {
                    if (descriptor.spawns != null && descriptor.spawns.Length > 0)
                        spawnPoint = descriptor.spawns[0];
                    else spawnPoint = descriptor.transform;
                }

                // if still null, just try to use anything, maybe camera?
                if (spawnPoint == null)
                {
                    spawnPoint = FindObjectOfType<Camera>()?.transform;
                }
            }

            if (avatarPrefab == null)
            {
                avatarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GotoUdon/Assets/ybot-mini.prefab");
            }
#endif
        }

        private static GotoUdonSettings LoadSetting()
        {
#if UNITY_EDITOR
            GotoUdonSettings settings =
                AssetDatabase.LoadAssetAtPath<GotoUdonSettings>("Assets/GotoUdon/GotoUdonSettings.asset");
            if (settings == null)
            {
                settings = CreateInstance<GotoUdonSettings>();
                settings.Init();
                AssetDatabase.CreateAsset(settings, "Assets/GotoUdon/GotoUdonSettings.asset");
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