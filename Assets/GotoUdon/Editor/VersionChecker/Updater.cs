using System;
using GotoUdon.Utils;
using UnityEditor;
using UnityEngine.Networking;

namespace GotoUdon.Editor.VersionChecker
{
    public class Updater
    {
        public static void Update(ReleaseAsset asset, Action<UpdateResult> callback)
        {
            GotoLog.Log($"Downloading update ({asset.Name}) from {asset.DownloadUrl}");
            UnityWebRequest www = UnityWebRequest.Get(asset.DownloadUrl);
            try
            {
                string downloadLocation = FileUtil.GetUniqueTempPathInProject();
                www.downloadHandler = new DownloadHandlerFile(downloadLocation);
                UnityWebRequestAsyncOperation action = www.SendWebRequest();
                action.completed += operation =>
                {
                    UnityWebRequest request = action.webRequest;
                    var responseCode = request.responseCode;
                    if (responseCode != 200)
                    {
                        GotoLog.Error("Failed to download update: " + request.error);
                        callback(new UpdateResult {Error = "Failed to download" + request.error});
                        www.Dispose();
                        return;
                    }

                    callback(new UpdateResult {DownloadPath = downloadLocation});
                    www.Dispose();
                };
            }
            catch (Exception exception)
            {
                GotoLog.Exception("Failed to request for update: " + exception.Message, exception);
                www.Dispose();
            }
        }

        public class UpdateResult
        {
            public string Error { get; set; }
            public string DownloadPath { get; set; }
            public bool IsError => Error != null;
        }
    }
}