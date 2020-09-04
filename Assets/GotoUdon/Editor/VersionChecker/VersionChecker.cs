using System;
using System.Collections.Generic;
using GotoUdon.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace GotoUdon.Editor.VersionChecker
{
    public class VersionChecker
    {
        public static void GetNewestSdkVersion(Action<ReleaseResponse> callback)
        {
            string url = "https://raw.githubusercontent.com/GotoFinal/GotoUdon/master/Assets/GotoUdon/Settings/VRCSDKVersion";

            UnityWebRequest www = UnityWebRequest.Get(url);
            try
            {
                UnityWebRequestAsyncOperation action = www.SendWebRequest();
                DownloadHandler downloadHandler = www.downloadHandler;
                action.completed += operation =>
                {
                    try
                    {
                        UnityWebRequest request = action.webRequest;
                        var responseCode = request.responseCode;
                        if (responseCode != 200)
                        {
                            callback(new ReleaseResponse
                            {
                                Error = $"Failed to check for new VrChat SDK version: {request.error}"
                            });
                            www.Dispose();
                            return;
                        }

                        string version = downloadHandler.text;
                        string description = version == GotoUdonEditor.ImplementedSDKVersion
                            ? "This is recommended version of VRChat SDK for GotoUdon!"
                            : "This version might not be fully supported by GotoUdon. If there is new version of GotoUdon available first update SDK then GotoUdon for best compatibility.";
                        callback(new ReleaseResponse
                        {
                            ReleaseInfo = new ReleaseInfo
                            {
                                Name = $"VRCSDK3-WORLD-{version}_Public",
                                Version = version,
                                Description = description,
                                Assets = new List<ReleaseAsset>
                                {
                                    new ReleaseAsset
                                    {
                                        Name = $"VRCSDK3-WORLD-{version}_Public.unitypackage",
                                        DownloadUrl = "https://vrchat.com/download/sdk3-worlds"
                                    }
                                }
                            }
                        });
                    }
                    catch (Exception exception)
                    {
                        ReleaseResponse response = new ReleaseResponse
                        {
                            Error = "Failed to read response from GotoUdon GitHub sdk file: " + exception.Message
                        };
                        GotoLog.Exception(response.Error, exception);
                        callback(response);
                    }
                    finally
                    {
                        www.Dispose();
                    }
                };
            }
            catch (Exception exception)
            {
                www.Dispose();
                ReleaseResponse response = new ReleaseResponse
                {
                    Error = "Failed to send request to check VRChat SDK update: " + exception.Message
                };
                GotoLog.Exception(response.Error, exception);
                callback(response);
            }
        }

        public static void GetNewestVersion(string author, string repository, Action<ReleaseResponse> callback)
        {
            string url = $"https://api.github.com/repos/{author}/{repository}/releases/latest";
            UnityWebRequest www = UnityWebRequest.Get(url);
            try
            {
                UnityWebRequestAsyncOperation action = www.SendWebRequest();
                DownloadHandler downloadHandler = www.downloadHandler;
                action.completed += operation =>
                {
                    UnityWebRequest request = action.webRequest;
                    var responseCode = request.responseCode;
                    if (responseCode != 200)
                    {
                        callback(new ReleaseResponse
                        {
                            Error = $"Failed to check for new version: {request.error}"
                        });
                        www.Dispose();
                        return;
                    }

                    GithubRespone githubRespone = JsonUtility.FromJson<GithubRespone>(downloadHandler.text);
                    try
                    {
                        callback(new ReleaseResponse
                        {
                            ReleaseInfo = new ReleaseInfo
                            {
                                Name = githubRespone.name,
                                Version = githubRespone.tag_name,
                                Description = githubRespone.body,
                                Assets = ReadAssets(githubRespone.assets)
                            }
                        });
                    }
                    catch (Exception exception)
                    {
                        ReleaseResponse response = new ReleaseResponse
                        {
                            Error = "Failed to read response from Gtihub API: " + exception.Message
                        };
                        GotoLog.Exception(response.Error, exception);
                        callback(response);
                    }
                    finally
                    {
                        www.Dispose();
                    }
                };
            }
            catch (Exception exception)
            {
                www.Dispose();
                ReleaseResponse response = new ReleaseResponse
                {
                    Error = "Failed to send request to check update: " + exception.Message
                };
                GotoLog.Exception(response.Error, exception);
                callback(response);
            }
        }

        private static List<ReleaseAsset> ReadAssets(List<GithubAssert> assets)
        {
            List<ReleaseAsset> result = new List<ReleaseAsset>();
            foreach (var asset in assets)
            {
                result.Add(
                    new ReleaseAsset
                    {
                        Name = asset.name,
                        DownloadUrl = asset.browser_download_url
                    });
            }

            return result;
        }

#pragma warning disable 0649
        [Serializable]
        private class GithubRespone
        {
            public string tag_name;
            public string name;
            public string body;
            public List<GithubAssert> assets;
        }

        [Serializable]
        private class GithubAssert
        {
            public string name;
            public string browser_download_url;
        }
    }
#pragma warning restore 0649
}