﻿#if GOTOUDON_SIMULATION
using UnityEngine;

namespace GotoUdon.VRC
{
    public class VRCEmulatorBehaviour : MonoBehaviour
    {
        private void Update()
        {
            VRCEmulator.Instance?.Update();
        }
    }
}
#endif