#if GOTOUDON_SIMULATION_TEMP_DISABLED
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
        internal GotoUdonSettings Settings => GotoUdonSettings.Instance;
        
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
                if (state == PlayModeStateChange.EnteredEditMode)
                {
                    // TODO: remove in future, was needed to remove stuff added cause bugs
                    foreach (UdonDebugger udonDebugger in Resources.FindObjectsOfTypeAll<UdonDebugger>())
                    {
                        Object.DestroyImmediate(udonDebugger, true);
                    }
                }
                Instance.OnPlayEnd();
                return;
            }

            Instance.OnPlay();
            foreach (UdonBehaviour udonBehaviour in Resources.FindObjectsOfTypeAll<UdonBehaviour>())
            {
                GameObject gameObject = udonBehaviour.gameObject;
                if (gameObject == null || gameObject.scene.name == null) return;
                if (gameObject.GetComponent<UdonDebugger>() == null)
                    gameObject.AddComponent<UdonDebugger>();
            }
        }

        internal VRCEmulator Emulator => VRCEmulator.Instance;

        internal List<SimulatedVRCPlayer> RuntimePlayers => VRCEmulator.Instance?.AllPlayers ?? new List<SimulatedVRCPlayer>();

        private void OnPlay()
        {
            if (VRCEmulator.InitEmulator(GotoUdonInternalState.Instance, GotoUdonSettings.Instance) == null)
            {
                return;
            }

            Emulator.OnPlayStart();
        }

        private void OnPlayEnd()
        {
            VRCEmulator.Destroy();
        }
    }
}
#endif