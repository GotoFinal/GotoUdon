using System;
using System.Collections.Generic;
using System.IO;
using GotoUdon.Utils;
using Microsoft.Win32;
using UnityEngine;
using VRC.Core;

namespace GotoUdon.Editor.ClientManager
{
    public class ClientManagerSettings : ScriptableObject
    {
        public string userId;

        public string gamePath;

        public string launchOptions;
        public string localLaunchOptions;

        // public bool sendInvitesOnUpdate = true;
        public ApiWorldInstance.AccessType accessType = ApiWorldInstance.AccessType.InviteOnly;
        public List<ClientSettings> clients;
        public int sameInstanceRestartDelay = 10;

        public string WorldId => VRCUtils.FindWorldID();

        public string UserId
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(userId) && APIUser.CurrentUser?.id != null && userId != APIUser.CurrentUser.id)
                {
                    userId = APIUser.CurrentUser.id;
                }

                return !string.IsNullOrWhiteSpace(userId) ? userId : userId = VRCUtils.FindWorldID();
            }
            set => userId = value;
        }

        public bool IsGamePathValid()
        {
            if (string.IsNullOrWhiteSpace(gamePath)) return false;
            try
            {
                return new FileInfo(gamePath).Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Init()
        {
            if (userId == null) userId = APIUser.CurrentUser?.id;
            if (gamePath == null) gamePath = SDKClientUtilities.GetSavedVRCInstallPath();
            else if (!IsGamePathValid() && new FileInfo(SDKClientUtilities.GetSavedVRCInstallPath()).Exists)
            {
                gamePath = SDKClientUtilities.GetSavedVRCInstallPath();
            }

#if UNITY_EDITOR_WIN
            if (!IsGamePathValid())
            {
                RegistryKey vrchatProtocolKey = Registry.ClassesRoot.OpenSubKey(@"VRChat\shell\open\command");
                if (vrchatProtocolKey?.GetValue("") != null)
                {
                    // assume its default one a try to extract the path from 
                    // "C:\Games\steamapps\common\VRChat\launch.bat" "C:\Games\steamapps\common\VRChat" "%1"
                    string startupScript = vrchatProtocolKey.GetValue("").ToString();
                    int indexOfStart = startupScript.IndexOf('"') + 1;
                    int indexOfEnd = indexOfStart < 1 ? -1 : startupScript.IndexOf('"', indexOfStart);
                    if (indexOfEnd > indexOfStart)
                    {
                        startupScript = startupScript.Substring(indexOfStart, indexOfEnd - indexOfStart);
                        startupScript = startupScript.Replace("launch.bat", "VRChat.exe");
                        try
                        {
                            if (new FileInfo(startupScript).Exists)
                                gamePath = startupScript;
                        }
                        catch (ArgumentException) // if it was not a standard one
                        {
                        }
                        catch (Exception exception)
                        {
                            GotoLog.Exception("Can't read vrchat executable path", exception);
                        }
                    }
                }
            }
#endif

            if (clients == null) clients = new List<ClientSettings>();
            if (clients.Count == 0)
            {
                clients.Add(new ClientSettings()
                {
                    name = "Default profile",
                    profile = 0,
                    duplicates = 1,
                    enabled = true,
                    vr = true
                });
            }

            if (string.IsNullOrWhiteSpace(launchOptions))
            {
                launchOptions =
                    "--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging --profile={profile} \"--url=launch?id={instance}\" {vr}";
            }

            if (string.IsNullOrWhiteSpace(localLaunchOptions))
            {
                localLaunchOptions =
                    "--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging --profile={profile} \"--url=create?roomId={instance}\" {vr}";
            }
        }
    }
}