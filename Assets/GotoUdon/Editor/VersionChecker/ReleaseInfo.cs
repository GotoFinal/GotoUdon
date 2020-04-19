using System.Collections.Generic;
using GotoUdon.Utils;

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
            return VersionUtils.IsRightNewerThanLeft(version, Version);
        }
    }
}