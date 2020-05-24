using System.Collections.Generic;
using UnityEditor;

namespace GotoUdon.Utils
{
    public class UnityCompilerUtils
    {
#if UNITY_EDITOR
        public static bool IsDefineEnabled(string define)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = new List<string>(definesString.Split(';'));
            return allDefines.Contains(define);
        }

        public static void SetDefineEnabled(string define, bool value)
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> allDefines = new List<string>(definesString.Split(';'));
            if (value)
            {
                if (allDefines.Contains(define)) return;
                allDefines.Add(define);
            }
            else
            {
                if (!allDefines.Contains(define)) return;
                allDefines.RemoveAll(str => str == define);
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }
#endif
    }
}