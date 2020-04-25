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

        internal GotoUdonSettings Settings => GotoUdonSettings.Instance;

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
    }
}