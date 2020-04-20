using UnityEngine;
using VRC.Core;
using VRC.SDK3.Components;

namespace GotoUdon.Utils
{
    public static class VRCUtils
    {
        public static string FindWorldID()
        {
            VRCSceneDescriptor sceneDescriptor = Object.FindObjectOfType<VRCSceneDescriptor>();
            PipelineManager pipelineManager =
                sceneDescriptor != null ? sceneDescriptor.gameObject.GetComponent<PipelineManager>() : null;
            return pipelineManager != null ? pipelineManager.blueprintId : null;
        }
    }
}