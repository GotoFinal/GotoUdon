using System;
using System.Collections.Generic;
using System.Diagnostics;
using GotoUdon.Utils;
using UnityEditor;
using UnityEngine;

namespace GotoUdon
{
    public class GotoUdonInternalState : ScriptableObject
    {
        // singleton, this state must be persistent no matter what, and unity does not like this 
        private static GotoUdonInternalState _instance;
        public static GotoUdonInternalState Instance => _instance == null ? _instance = LoadSetting() : _instance;

        public bool enableAutomaticPublish;
        public List<ClientProcess> processes;
        public string instanceId;
        public string accessType;

        public Dictionary<int, ClientProcess> GetProcessesByProfile()
        {
            Dictionary<int, ClientProcess> profileToProcessMapping = new Dictionary<int, ClientProcess>();
            foreach (var clientProcess in processes)
            {
                profileToProcessMapping[clientProcess.profile] = clientProcess;
            }

            return profileToProcessMapping;
        }

        public ClientProcess GetProcessByProfile(int profile)
        {
            foreach (var clientProcess in processes)
            {
                if (clientProcess.profile == profile) return clientProcess;
            }

            return null;
        }

        public void Init()
        {
            if (processes == null) processes = new List<ClientProcess>();
        }

        private static GotoUdonInternalState LoadSetting()
        {
#if UNITY_EDITOR
            GotoUdonInternalState settings =
                AssetDatabase.LoadAssetAtPath<GotoUdonInternalState>("Assets/GotoUdon/GotoUdonInternalState.asset");
            if (settings == null)
            {
                settings = CreateInstance<GotoUdonInternalState>();
                settings.Init();
                AssetDatabase.CreateAsset(settings, "Assets/GotoUdon/GotoUdonInternalState.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else settings.Init();

            return settings;
#else
            return null;
#endif
        }

        [Serializable]
        public class ClientProcess
        {
            public int pid;

            public int profile;

            // public string userId;
            public string logFilePrefix;

            public long lastReadPosition;

            public Process Process
            {
                get
                {
                    if (pid == 0) return null;
                    try
                    {
                        Process process = Process.GetProcessById(pid);
                        return !process.HasExited ? process : null;
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            public void StopProcess()
            {
                try
                {
                    Process process = Process;
                    if (process != null)
                    {
                        if (!process.CloseMainWindow())
                        {
                            GotoLog.Warn($"Failed to exit profile {profile} process {pid}, application refused to close window.");
                            process.Kill();
                        }

                        if (!process.WaitForExit(10000))
                        {
                            GotoLog.Warn($"Failed to exit profile {profile} process {pid}, waited 10 seconds but its still alive.");
                            process.Kill();
                        }
                    }
                }
                catch
                {
                    GotoLog.Warn($"Failed to exit profile {profile} process {pid}");
                }
            }
        }
    }
}