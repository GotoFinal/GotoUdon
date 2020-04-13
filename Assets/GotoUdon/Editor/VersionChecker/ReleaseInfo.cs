using System;
using System.Collections.Generic;

namespace GotoUdon.Editor
{
    public class ReleaseInfo
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public List<ReleaseAsset> Assets { get; set; }

        public ReleaseAsset UnityPackage
        {
            get
            {
                foreach (ReleaseAsset asset in Assets)
                {
                    if (asset.IsUnityPackage)
                    {
                        return asset;
                    }
                }

                return null;
            }
        }

        // supports only simple versions like "v1.0.0"
        public bool IsNewerThan(string version)
        {
            if (version.Equals(Version))
            {
                return false;
            }

            string[] versionNumbers = ExtractVersionComponents(version);
            string[] thisVersionNumbers = ExtractVersionComponents(Version);

            for (int i = 0; i < Math.Min(versionNumbers.Length, thisVersionNumbers.Length); i++)
            {
                if (int.Parse(thisVersionNumbers[i]) > int.Parse(versionNumbers[i]))
                {
                    return true;
                }
            }

            // eg: 1.0.1 vs 1.0
            return thisVersionNumbers.Length > versionNumbers.Length;
        }

        private string[] ExtractVersionComponents(string version)
        {
            if (version.StartsWith("v"))
            {
                version = version.Substring(1);
            }

            return version.Split('.');
        }
    }
}