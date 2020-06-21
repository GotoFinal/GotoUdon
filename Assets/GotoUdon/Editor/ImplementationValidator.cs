#if GOTOUDON_SIMULATION_TEMP_DISABLED
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GotoUdon.Utils;
using GotoUdon.Utils.Editor;
using VRC.SDKBase;

namespace GotoUdon.Editor
{
    public static class ImplementationValidator
    {
        private static List<MissingImplementationData> _cachedErrors;

        public static List<MissingImplementationData> ValidateEmulator()
        {
            if (_cachedErrors != null)
            {
                return _cachedErrors;
            }

            List<MissingImplementationData> missingImpls = new List<MissingImplementationData>();
            List<Type> emulatedTypes = new List<Type> {typeof(VRCPlayerApi), typeof(Networking)};
            foreach (Type emulatedType in emulatedTypes)
            {
                foreach (FieldInfo fieldInfo in emulatedType.GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    if (fieldInfo.GetValue(null) == null)
                    {
                        missingImpls.Add(new MissingImplementationData(fieldInfo));
                    }
                }
            }

            return _cachedErrors = missingImpls;
        }

        public static void DrawValidationErrors(List<MissingImplementationData> missingImpls)
        {
            VRCVersionStatus versionStatus = IsVRChatSdkOutdated();
            if (missingImpls.Count == 0)
            {
                SimpleGUI.ErrorBox(versionStatus == VRCVersionStatus.Error,
                    "SDK version check failed, please report this to author. You can try to keep using current version, but some features might not work correctly");
                SimpleGUI.ErrorBox(versionStatus == VRCVersionStatus.Outdated,
                    $"Please update your VRChat SDK, this GotoUdon was made for: {GotoUdonEditor.ImplementedSDKVersion}");
                SimpleGUI.ErrorBox(versionStatus == VRCVersionStatus.TooNew,
                    "Please update GotoUdon if new version is available, if not you can try to keep using current version, but some features might not work correctly.\n" +
                    "You can also check our discord to see if we are already working on update.");
                return;
            }

            StringBuilder errorStr =
                new StringBuilder(
                    $"GotoUdon {GotoUdonEditor.VERSION} is not compatible with VRChat sdk {GotoUdonEditor.CurrentSDKVersion}.\n");
            if (versionStatus == VRCVersionStatus.Same)
            {
                errorStr.AppendLine("Please report this author, this seems to be a bug. This version of sdk should be fully supported.");
            }

            errorStr.AppendLine("Auto detected problematic methods: ");

            foreach (MissingImplementationData missingImpl in missingImpls)
            {
                errorStr.AppendLine($" - {missingImpl.FieldName}");
            }

            errorStr.AppendLine();
            errorStr.AppendLine("If your udon scripts don't use methods above they might work just fine in emulator.");
            SimpleGUI.ErrorBox(true, errorStr.ToString());
        }

        private static VRCVersionStatus IsVRChatSdkOutdated()
        {
            string currentSdkVersion = GotoUdonEditor.CurrentSDKVersion;
            string implementedSdkVersion = GotoUdonEditor.ImplementedSDKVersion;
            if (currentSdkVersion == implementedSdkVersion)
            {
                return VRCVersionStatus.Same;
            }

            try
            {
                return VersionUtils.IsRightNewerThanLeft(currentSdkVersion, implementedSdkVersion)
                    ? VRCVersionStatus.Outdated
                    : VRCVersionStatus.TooNew;
            }
            catch
            {
                return VRCVersionStatus.Error;
            }
        }

        private enum VRCVersionStatus
        {
            Same,
            Outdated,
            TooNew,
            Error
        }

        public class MissingImplementationData
        {
            internal FieldInfo Field { get; set; }
            internal String FieldName => Field?.DeclaringType?.Name + "." + Field?.Name;

            public MissingImplementationData(FieldInfo field)
            {
                Field = field;
            }
        }
    }
}
#endif