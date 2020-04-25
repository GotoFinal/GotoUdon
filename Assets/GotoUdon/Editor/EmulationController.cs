using System.Collections.Generic;
using GotoUdon.VRC;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace GotoUdon.Editor
{
    [InitializeOnLoad]
    public class EmulationController
    {
        // register an event handler when the class is initialized
        static EmulationController()
        {
            EditorApplication.playModeStateChanged += OnModeChange;
        }

        public readonly static EmulationController Instance = new EmulationController();

        private static void OnModeChange(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                Instance.OnPlayEnd();
                return;
            }

            Instance.OnPlay();
            foreach (UdonBehaviour udonBehaviour in Resources.FindObjectsOfTypeAll<UdonBehaviour>())
            {
                GameObject gameObject = udonBehaviour.gameObject;
                if (gameObject == null) return;
                if (gameObject.GetComponent<UdonDebugger>() == null)
                    gameObject.AddComponent<UdonDebugger>();
            }
        }

        internal VRCEmulator Emulator => VRCEmulator.Instance;

        internal GotoUdonSettings Settings =>
            GotoUdonSettings.Instance == null ? GotoUdonSettings.Instance = LoadSetting() : GotoUdonSettings.Instance;

        internal List<SimulatedVRCPlayer> RuntimePlayers => VRCEmulator.Instance.AllPlayers;

        private void OnPlay()
        {
            VRCEmulator.InitEmulator(GotoUdonInternalState.Instance, Settings);
            Emulator.OnPlayStart();
        }

        private void OnPlayEnd()
        {
            VRCEmulator.Destroy();
        }

        private readonly UpdaterEditor _updaterEditor = new UpdaterEditor();

        internal static GotoUdonSettings LoadSetting()
        {
            if (GotoUdonSettings.Instance != null) return GotoUdonSettings.Instance;
            GotoUdonSettings.Instance = AssetDatabase.LoadAssetAtPath<GotoUdonSettings>("Assets/GotoUdon/GotoUdonSettings.asset");
            if (GotoUdonSettings.Instance == null)
            {
                GotoUdonSettings.Instance = ScriptableObject.CreateInstance<GotoUdonSettings>();
                GotoUdonSettings.Instance.Init();
                AssetDatabase.CreateAsset(GotoUdonSettings.Instance, "Assets/GotoUdon/GotoUdonSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else GotoUdonSettings.Instance.Init();

            return GotoUdonSettings.Instance;
        }
    }
}