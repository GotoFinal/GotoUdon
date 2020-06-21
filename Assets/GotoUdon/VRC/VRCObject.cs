#if GOTOUDON_SIMULATION_TEMP_DISABLED
using UnityEngine;
using VRC.SDKBase;

namespace GotoUdon.VRC
{
    public class VRCObject : VRC_ObjectApi
    {
        public bool isReady = true;
        public SimulatedVRCPlayer owner;
        public VRCPlayer VRCPlayer => owner == null ? VRCEmulator.Instance?.Master : owner.VRCPlayer;

        public static VRCObject AsVrcObject(GameObject gameObject)
        {
            VRCObject vrcObject = gameObject.GetComponent<VRCObject>();
            if (vrcObject == null)
            {
                vrcObject = gameObject.AddComponent<VRCObject>();
            }

            return vrcObject;
        }
    }
}
#endif