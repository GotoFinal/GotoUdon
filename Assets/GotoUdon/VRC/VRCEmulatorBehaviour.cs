#if GOTOUDON_SIMULATION_TEMP_DISABLED
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