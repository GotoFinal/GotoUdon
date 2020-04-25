using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GotoUdon.Utils;
using UnityEditor;
using VRC.Core;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GotoUdon.Editor.ClientManager
{
    public class ClientsManager
    {
        private readonly ClientManagerSettings _settings;

        public ClientsManager(ClientManagerSettings settings)
        {
            _settings = settings;
        }

        public string InstanceId
        {
            get => GotoUdonInternalState.Instance.instanceId;
            set => GotoUdonInternalState.Instance.instanceId = value;
        }

        public void Init()
        {
            GotoUdonInternalState internalState = GotoUdonInternalState.Instance;
            Dictionary<int, GotoUdonInternalState.ClientProcess> processesByProfile = internalState.GetProcessesByProfile();
            foreach (ClientSettings clientSettings in _settings.clients)
            {
                if (!processesByProfile.ContainsKey(clientSettings.profile)) continue;
            }
        }

        public bool IsAnyClientRunning()
        {
            foreach (GotoUdonInternalState.ClientProcess clientProcess in GotoUdonInternalState.Instance.processes)
            {
                if (clientProcess.Process != null && !clientProcess.Process.HasExited) return true;
            }

            return false;
        }

        public void StartClients(bool restart, bool keepInstance)
        {
            string instanceId = GetOrGenerateInstanceId(keepInstance, _settings);
            foreach (ClientSettings clientSettings in _settings.clients)
            {
                if (!clientSettings.enabled) continue;
                StartClient(restart, keepInstance, clientSettings, false, instanceId);
            }

            EditorUtility.SetDirty(GotoUdonInternalState.Instance);
        }

        public void StartClient(bool restart, bool keepInstance, ClientSettings clientSettings, bool save = true, string instance = null)
        {
            GotoUdonInternalState internalState = GotoUdonInternalState.Instance;
            string vrcInstallPath = SDKClientUtilities.GetSavedVRCInstallPath();
            if (instance == null)
                instance = GetOrGenerateInstanceId(keepInstance, _settings);
            string sharedArgs = "--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging";
            Dictionary<int, GotoUdonInternalState.ClientProcess>
                processesByProfile = internalState.GetProcessesByProfile();

            int profile = clientSettings.profile;
            if (processesByProfile.ContainsKey(profile))
            {
                Process process = processesByProfile[profile].Process;
                if (restart)
                {
                    if (process != null)
                    {
                        processesByProfile[profile].StopProcess();
                    }
                }
                else if (process != null) return;
            }

            Process newClientProcess = SpawnClient(clientSettings, instance, sharedArgs, vrcInstallPath, keepInstance ? 10000 : 0);
            processesByProfile[profile] = new GotoUdonInternalState.ClientProcess
            {
                pid = newClientProcess.Id,
                profile = profile
            };

            internalState.processes = new List<GotoUdonInternalState.ClientProcess>(processesByProfile.Values);

            if (save)
            {
                EditorUtility.SetDirty(internalState);
            }
        }

        private Process SpawnClient(ClientSettings settings, string instance, string sharedArgs, string vrcInstallPath, int delayMs)
        {
            string args = $"{sharedArgs} --profile={settings.profile} \"--url=launch?id={instance}\"";
            if (!settings.vr) args += " --no-vr";
            GotoLog.Log($"Starting VRC with arguments: {args}");
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo(vrcInstallPath, args);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            SynchronizationContext synchronizationContext = SynchronizationContext.Current;
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                SynchronizationContext.SetSynchronizationContext(synchronizationContext);
                if (string.IsNullOrWhiteSpace(eventArgs?.Data)) return;
                if (!eventArgs.Data.StartsWith("Using log prefix ")) return;
                GotoUdonInternalState.ClientProcess clientProcess =
                    GotoUdonInternalState.Instance.GetProcessesByProfile()[settings.profile];
                clientProcess.logFilePrefix = eventArgs.Data.Replace("Using log prefix ", "");
                clientProcess.lastReadPosition = 0;
                SynchronizationContext.Current.Post(state => EditorUtility.SetDirty((Object) state), GotoUdonInternalState.Instance);
            };
            Action startVrcAction = () =>
            {
                process.Start();
                process.BeginOutputReadLine();
            };
            if (delayMs != 0)
            {
                new Thread(() =>
                {
                    // VrChat need some time to register that we are no longer in this same instance
                    Thread.Sleep(delayMs);
                    startVrcAction();
                }).Start();
            }
            else startVrcAction();

            return process;
        }

        private string GetOrGenerateInstanceId(bool keepInstance, ClientManagerSettings settings)
        {
            GotoUdonInternalState internalState = GotoUdonInternalState.Instance;
            if (string.IsNullOrWhiteSpace(InstanceId) || !keepInstance || settings.accessType.ToString() != internalState.accessType)
            {
                internalState.accessType = settings.accessType.ToString();
                return InstanceId = CreateNewInstanceId(settings);
            }

            if (InstanceId.Split(':')[0] != settings.worldId)
            {
                internalState.accessType = settings.accessType.ToString();
                return InstanceId = CreateNewInstanceId(settings);
            }

            return InstanceId;
        }

        private string CreateNewInstanceId(ClientManagerSettings settings)
        {
            int instanceIndex = Random.Range(1, 99999);
            string accessTags = ApiWorldInstance.BuildAccessTags(settings.accessType, settings.userId);
            return settings.worldId + ":" + instanceIndex + accessTags;
        }
    }
}