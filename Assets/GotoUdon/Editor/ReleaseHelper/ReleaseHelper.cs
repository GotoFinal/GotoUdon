using System.Collections.Generic;
using System.IO;
using System.Linq;
using GotoUdon.Utils.Editor;
using UnityEditor;
using UnityEngine;

namespace GotoUdon.Editor.ReleaseHelper
{
    public class ReleaseHelper
    {
        public static void DrawReleaseHelper()
        {
#if GOTOUDON_DEV
            SimpleGUI.ActionButton($"Package GotoUdon-{Version}", PrepareRelease);
        }

        private static string Version => GotoUdonEditor.VERSION.Substring(1);

        private static void PrepareRelease()
        {
            string[] listOfFiles = BuildListOfFiles();
            string output = Path.Combine(Application.dataPath, "GotoUdon", "Releases", $"GotoUdon-{Version}.unitypackage");
            AssetDatabase.ExportPackage(listOfFiles, output);
        }

        private static string[] BuildListOfFiles()
        {
            List<string> filesToExport = new List<string>();
            string assetFolder = Path.Combine(Application.dataPath, "GotoUdon");
            DirectoryInfo directory = new DirectoryInfo(assetFolder);
            FindFilesToExport(directory, filesToExport);
            // assetFolder = Path.Combine(Application.dataPath, "GotoUdonHelper");
            // directory = new DirectoryInfo(assetFolder);
            // FindFilesToExport(directory, filesToExport);

            filesToExport = filesToExport.ConvertAll(str => "Assets/" + str.Substring(Application.dataPath.Length + 1));
            return filesToExport.ToArray();
        }

        private static readonly HashSet<string> blacklistedEndings = new HashSet<string>
            {".unitypackage", ".asset", ".meta"};
        private static readonly HashSet<string> blacklistedFolders = new HashSet<string>
            {"\\Releases"};

        private static void FindFilesToExport(DirectoryInfo directory, List<string> filesToExport)
        {
            foreach (FileInfo fileInfo in directory.GetFiles())
            {
                string filePath = fileInfo.FullName;
                if (blacklistedEndings.Any(blacklisted => filePath.EndsWith(blacklisted))) continue;
                filesToExport.Add(filePath);
            }

            foreach (DirectoryInfo directoryInfo in directory.GetDirectories())
            {
                if (blacklistedFolders.Any(blacklisted => directoryInfo.FullName.EndsWith(blacklisted))) continue;
                FindFilesToExport(directoryInfo, filesToExport);
            }
#endif
        }
    }
}