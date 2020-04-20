using System.IO;
using UnityEditor;
using UnityEngine;

namespace GotoUdonHelperThatCanBeRemovedLater.Editor
{
    [InitializeOnLoad]
    public class ForceUpdate
    {
        static ForceUpdate()
        {
            FileInfo fileToRemove = new FileInfo(Path.Combine(Application.dataPath, "GotoUdon", "GotoUdonAssembly.asmdef"));
            if (fileToRemove.Exists)
            {
                Debug.LogWarning("Removing GotoUdonAssembly.asmdef due to need to access VRChat SDK");
                fileToRemove.Delete();
            }
        }
    }
}