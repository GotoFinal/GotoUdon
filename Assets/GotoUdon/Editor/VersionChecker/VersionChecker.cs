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
            string url = "https://www.vrchat.com/download/sdk3";
            UnityWebRequest www = UnityWebRequest.Get(url);
            try
            {
                www.redirectLimit = 0;
                UnityWebRequestAsyncOperation action = www.SendWebRequest();
                action.completed += operation =>
                {
                    UnityWebRequest request = action.webRequest;
                    var responseCode = request.responseCode;
                    if (responseCode != 302)
                    {
                        callback(new ReleaseResponse
                        {
                            Error = $"Failed to check for new SDK version: {request.error}"
                        });
                        www.Dispose();
                        return;
                    }

                    try
                    {
                        string packageUrl = request.GetResponseHeader("Location");
                        string fileName = packageUrl.Substring(packageUrl.LastIndexOf('/') + 1);
                        string version = fileName.Substring(fileName.LastIndexOf('-') + 1).Replace(".unitypackage", "");
                        string description = version == GotoUdonEditor.ImplementedSDKVersion
                            ? "This is recommended version of VRChat SDK for GotoUdon!"
                            : "This version might not be fully supported by GotoUdon. If there is new version of GotoUdon available first update SDK then GotoUdon for best compatibility.";
                        callback(new ReleaseResponse
                        {
                            ReleaseInfo = new ReleaseInfo
                            {
                                Name = fileName.Replace(".unitypackage", ""),
                                Version = version,
                                Description = description,
                                Assets = new List<ReleaseAsset>
                                {
                                    new ReleaseAsset
                                    {
                                        Name = fileName,
                                        DownloadUrl = packageUrl
                                    }
                                }
                            }
                        });
                    }
                    catch (Exception exception)
                    {
                        ReleaseResponse response = new ReleaseResponse
                        {
                            Error = "Failed to read response from VRChat API: " + exception.Message
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