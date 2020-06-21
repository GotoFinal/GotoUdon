using System;
using GotoUdon.Editor.VersionChecker;
using GotoUdon.Utils;
using GotoUdon.Utils.Editor;
using UnityEditor;
using UnityEngine;
using static GotoUdon.Editor.VersionChecker.VersionChecker;

namespace GotoUdon.Editor
{
    public class UpdaterEditor
    {
        internal static readonly UpdaterEditor Instance = new UpdaterEditor();

        private DateTime _lastUpdateCheck = DateTime.UtcNow.Subtract(TimeSpan.FromDays(1));
        private ReleaseResponse _updateCheckerLibraryResponse = null;
        private bool _downloadingUpdate = false;
        private ReleaseResponse _updateCheckerSdkResponse = null;
        private bool _downloadingSdk = false;

        internal void DrawVersionInformation()
        {
            DrawSdkUpdateComponent();
            string version = GotoUdonEditor.VERSION;
            string sdkVersion = GotoUdonEditor.CurrentSDKVersion;
            string recommendedSdkVersion = GotoUdonEditor.ImplementedSDKVersion;
            string versionString = sdkVersion == recommendedSdkVersion
                ? $"GotoUdon {version} for SDK {recommendedSdkVersion}"
                : $"GotoUdon {version} for SDK {recommendedSdkVersion}, running on SDK {sdkVersion}";
            string githubUrl = "https://github.com/GotoFinal/GotoUdon/releases";
            if (_updateCheckerLibraryResponse != null)
            {
                if (SimpleGUI.WarningBox(_updateCheckerLibraryResponse.IsError, _updateCheckerLibraryResponse.Error))
                {
                    if (GUILayout.Button($"{versionString}. Click to check for new version at: {githubUrl}",
                        EditorStyles.helpBox))
                    {
                        Application.OpenURL(githubUrl);
                    }

                    return;
                }

                ReleaseInfo releaseInfo = _updateCheckerLibraryResponse.ReleaseInfo;
                if (releaseInfo.UnityPackage != null && SimpleGUI.InfoBox(releaseInfo.IsNewerThan(version),
                    $"There is new version available: {releaseInfo.Version}! Click to update!\n{releaseInfo.Name}\n{releaseInfo.Description}")
                )
                {
                    GUILayout.BeginHorizontal();
                    if (!_downloadingUpdate)
                        SimpleGUI.ActionButton($"Update to {releaseInfo.Version}!", UpdateLibrary);
                    SimpleGUI.ActionButton("Download manually.", () => Application.OpenURL(releaseInfo.UnityPackage.DownloadUrl));
                    GUILayout.EndHorizontal();
                    return;
                }
            }

            if (GUILayout.Button($"{versionString}. Click to retry check for new version.", EditorStyles.helpBox))
            {
                ForceCheckForUpdate();
            }
        }

        private void DrawSdkUpdateComponent()
        {
            if (_updateCheckerSdkResponse != null)
            {
                if (_updateCheckerSdkResponse.IsError)
                {
#if GOTOUDON_DEV
                    SimpleGUI.ErrorBox(_updateCheckerSdkResponse.IsError, "Failed to check for VRChat SDK update.");
#endif
                    return;
                }

                ReleaseInfo releaseInfo = _updateCheckerSdkResponse.ReleaseInfo;
                if (releaseInfo.Version == null) return;
                string newestSdkVersion = NormalizeVrChatSDKVersion(releaseInfo.Version);
                string currentSdkVersion = NormalizeVrChatSDKVersion(GotoUdonEditor.CurrentSDKVersion);

                // I give up, TODO: save version in own repository instead of using vrchat
                if (currentSdkVersion.EndsWith("05.06") && newestSdkVersion.EndsWith("05.12")) return;
                if (releaseInfo.Version.StartsWith(GotoUdonEditor.ImplementedSDKVersion)) return;

                if (releaseInfo.UnityPackage != null &&
                    SimpleGUI.InfoBox(
                        VersionUtils.IsRightNewerThanLeft(currentSdkVersion, newestSdkVersion),
                        $"There is new VRChat UDON SDK version available: {releaseInfo.Version}! Click to update!\n{releaseInfo.Name}\n{releaseInfo.Description}")
                )
                {
                    GUILayout.BeginHorizontal();
                    if (!_downloadingSdk)
                        SimpleGUI.ActionButton($"Update to {releaseInfo.Version}!", UpdateSdk);
                    SimpleGUI.ActionButton("Download manually.", () => Application.OpenURL(releaseInfo.UnityPackage.DownloadUrl));
                    GUILayout.EndHorizontal();
                }
            }
        }

        private string NormalizeVrChatSDKVersion(string version)
        {
            int lastIndex = version.LastIndexOf('.');
            if (lastIndex == -1) return version;
            lastIndex = version.LastIndexOf('.', lastIndex - 1);
            return lastIndex == -1 ? version : version.Substring(0, lastIndex);
        }

        internal void TryCheckUpdate()
        {
            if (DateTime.UtcNow.Subtract(_lastUpdateCheck).TotalHours < 1)
            {
                return;
            }

            _lastUpdateCheck = DateTime.UtcNow;
            ForceCheckForUpdate();
        }

        private void ForceCheckForUpdate()
        {
            GetNewestVersion("GotoFinal", "GotoUdon", response => { _updateCheckerLibraryResponse = response; });
            GetNewestSdkVersion(response => { _updateCheckerSdkResponse = response; });
        }

        private void UpdateLibrary()
        {
            if (_updateCheckerLibraryResponse == null || _updateCheckerLibraryResponse.IsError) return;

            ReleaseAsset unityPackage = _updateCheckerLibraryResponse.ReleaseInfo.UnityPackage;
            if (unityPackage == null) return;

            _downloadingUpdate = true;
            Updater.Update(_updateCheckerLibraryResponse.ReleaseInfo.UnityPackage, result =>
            {
                _downloadingUpdate = false;
                if (!result.IsError)
                {
                    AssetDatabase.ImportPackage(result.DownloadPath, true);
                }
            });
        }

        private void UpdateSdk()
        {
            if (_updateCheckerSdkResponse == null || _updateCheckerSdkResponse.IsError) return;

            ReleaseAsset unityPackage = _updateCheckerSdkResponse.ReleaseInfo.UnityPackage;
            if (unityPackage == null) return;

            _downloadingSdk = true;
            Updater.Update(_updateCheckerSdkResponse.ReleaseInfo.UnityPackage, result =>
            {
                _downloadingSdk = false;
                if (!result.IsError)
                {
                    AssetDatabase.ImportPackage(result.DownloadPath, true);
                }
            });
        }
    }
}