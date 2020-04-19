using System;

namespace GotoUdon.Utils
{
    public class VersionUtils
    {
        // supports only simple versions like "v1.0.0"
        public static bool IsRightNewerThanLeft(string leftVersion, string rightVersion)
        {
            if (leftVersion.Equals(rightVersion))
            {
                return false;
            }

            string[] leftVersionNumbers = ExtractVersionComponents(leftVersion);
            string[] rightVersionNumbers = ExtractVersionComponents(rightVersion);

            for (int i = 0; i < Math.Min(leftVersionNumbers.Length, rightVersionNumbers.Length); i++)
            {
                if (int.Parse(rightVersionNumbers[i]) == int.Parse(leftVersionNumbers[i])) continue;
                return int.Parse(rightVersionNumbers[i]) > int.Parse(leftVersionNumbers[i]);
            }

            // eg: 1.0.1 vs 1.0
            return rightVersionNumbers.Length > leftVersionNumbers.Length;
        }

        private static string[] ExtractVersionComponents(string version)
        {
            if (version.StartsWith("v"))
            {
                version = version.Substring(1);
            }

            return version.Split('.');
        }
    }
}