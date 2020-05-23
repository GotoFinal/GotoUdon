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
            if (restart)
            {
                KillAllClients();
            }

            string instanceId = GetOrGenerateInstanceId(keepInstance, _settings);
            foreach (ClientSettings clientSettings in _settings.clients)
            {
                if (!clientSettings.enabled) continue;
                StartClient(restart, keepInstance, clientSettings, true, instanceId);
            }

            EditorUtility.SetDirty(GotoUdonInternalState.Instance);
        }

        private void KillAllClients()
        {
            Dictionary<int, GotoUdonInternalState.ClientProcess>
                processesByProfile = GotoUdonInternalState.Instance.GetProcessesByProfile();
            foreach (ClientSettings clientSettings in _settings.clients)
            {
                if (!clientSettings.enabled || !processesByProfile.ContainsKey(clientSettings.profile)) continue;
                processesByProfile[clientSettings.profile].StopProcess();
            }
        }

        public void StartClient(bool restart, bool keepInstance, ClientSettings clientSettings, bool save = true, string instance = null)
        {
            GotoUdonInternalState internalState = GotoUdonInternalState.Instance;
            string vrcInstallPath = _settings.gamePath;
            if (instance == null)
                instance = GetOrGenerateInstanceId(keepInstance, _settings);
            string sharedArgs = _settings.launchOptions;
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

            SpawnClient(clientSettings, instance, sharedArgs, vrcInstallPath, keepInstance ? 10000 : 0, processesByProfile,
                (map, spawnedProcess) =>
                {
                    GotoUdonInternalState.Instance.processes.RemoveAll(client => client.profile == profile);
                    GotoUdonInternalState.Instance.processes.Add(new GotoUdonInternalState.ClientProcess
                    {
                        pid = spawnedProcess.Id,
                        profile = profile
                    });

                    if (save)
                    {
                        EditorUtility.SetDirty(GotoUdonInternalState.Instance);
                    }
                });
        }

        private void SpawnClient(ClientSettings settings, string instance, string args, string vrcInstallPath, int delayMs,
            Dictionary<int, GotoUdonInternalState.ClientProcess> processMap,
            Action<Dictionary<int, GotoUdonInternalState.ClientProcess>, Process> callback)
        {
            args = args.Replace("{profile}", settings.profile.ToString());
            args = args.Replace("{instance}", instance);
            args = args.Replace("{vr}", settings.vr ? "" : "--no-vr");
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
                synchronizationContext.Post(_ => callback(processMap, process), process);
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
        }

        private string GetOrGenerateInstanceId(bool keepInstance, ClientManagerSettings settings)
        {
            GotoUdonInternalState internalState = GotoUdonInternalState.Instance;
            if (string.IsNullOrWhiteSpace(InstanceId) || !keepInstance || settings.accessType.ToString() != internalState.accessType)
            {
                internalState.accessType = settings.accessType.ToString();
                return InstanceId = CreateNewInstanceId(settings);
            }

            if (InstanceId.Split(':')[0] != settings.WorldId)
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
            return settings.WorldId + ":" + instanceIndex + accessTags;
        }
    }
}