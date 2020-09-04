using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using GotoUdon.Utils;
using UnityEditor;
using VRC.Core;
using Random = UnityEngine.Random;
using Tools = VRC.Tools;

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

        public void StartClients(bool restart, bool keepInstance, bool localTesting, bool forceDontValidate)
        {
            if (restart) ClientProcessesManager.KillAll();
            string instanceId = GetOrGenerateInstanceId(forceDontValidate, keepInstance, localTesting, _settings);
            foreach (ClientSettings clientSettings in _settings.clients)
            {
                if (!clientSettings.enabled) continue;
                StartClients(keepInstance, forceDontValidate, localTesting, clientSettings, instanceId);
            }

            EditorUtility.SetDirty(GotoUdonInternalState.Instance);
        }

        public void StartClients(
            bool keepInstance,
            bool keepInstanceForce,
            bool localTesting,
            ClientSettings clientSettings,
            string instance = null)
        {
            string vrcInstallPath = _settings.gamePath;
            if (instance == null)
                instance = GetOrGenerateInstanceId(keepInstanceForce, keepInstance, localTesting, _settings);
            string sharedArgs = localTesting ? _settings.localLaunchOptions : _settings.launchOptions;
            SpawnClient(clientSettings, localTesting, instance, sharedArgs, vrcInstallPath,
                keepInstance ? _settings.sameInstanceRestartDelay * 100 : 0,
                (spawnedProcess) => { ClientProcessesManager.RegisterProcess(spawnedProcess.Id, clientSettings.profile); });
        }

        private void SpawnClient(ClientSettings settings, bool local, string instance, string args, string vrcInstallPath, int delayMs,
            Action<Process> callback)
        {
            List<Action> startVrcActions = new List<Action>();
            args = args.Replace("{profile}", settings.profile.ToString());
            args = args.Replace("{instance}", instance);
            args = args.Replace("{file}", EditorPrefs.GetString("currentBuildingAssetBundlePath"));
            args = args.Replace("{vr}", settings.vr ? "" : "--no-vr");
            int num = local ? settings.instances : 1;
            GotoLog.Log($"Starting {num} instances of VRC with arguments: {args}");
            for (var i = 0; i < num; i++)
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(vrcInstallPath, args);
                process.StartInfo.UseShellExecute = false;
                // process.StartInfo.RedirectStandardOutput = true;
                SynchronizationContext synchronizationContext = SynchronizationContext.Current;
                startVrcActions.Add(() =>
                {
                    process.Start();
                    // process.BeginOutputReadLine();
                    synchronizationContext.Post(_ => callback(process), process);
                });
            }

            if (delayMs != 0)
            {
                new Thread(() =>
                {
                    // VrChat need some time to register that we are no longer in this same instance
                    Thread.Sleep(delayMs);
                    startVrcActions.ForEach(a => a.Invoke());
                }).Start();
            }
            else startVrcActions.ForEach(a => a.Invoke());
        }

        private string GetOrGenerateInstanceId(bool forceDontValidate, bool keepInstance, bool localTesting, ClientManagerSettings settings)
        {
            String localTestingAsset = localTesting ? EditorPrefs.GetString("currentBuildingAssetBundlePath") : null;
            if (forceDontValidate)
            {
                return InstanceId ?? (InstanceId = CreateNewInstanceId(settings, localTestingAsset));
            }

            GotoUdonInternalState internalState = GotoUdonInternalState.Instance;
            if (string.IsNullOrWhiteSpace(InstanceId) || !keepInstance || settings.accessType.ToString() != internalState.accessType)
            {
                internalState.accessType = settings.accessType.ToString();
                return InstanceId = CreateNewInstanceId(settings, localTestingAsset);
            }

            if (!localTesting && InstanceId.Split(':')[0] != settings.WorldId)
            {
                internalState.accessType = settings.accessType.ToString();
                return InstanceId = CreateNewInstanceId(settings, null);
            }

            if (localTesting && !InstanceId.Contains(localTestingAsset))
            {
                return InstanceId = CreateNewInstanceId(settings, localTestingAsset);
            }

            if (!localTesting && InstanceId.Contains(EditorPrefs.GetString("currentBuildingAssetBundlePath")))
            {
                return InstanceId = CreateNewInstanceId(settings, null);
            }

            return InstanceId;
        }

        private string CreateNewInstanceId(ClientManagerSettings settings, String localTestingAsset)
        {
            if (localTestingAsset != null)
            {
                return Tools.GetRandomDigits(10) + "&hidden=true&name=BuildAndRun_GotoUdon&url=file:///" + localTestingAsset;
            }

            int instanceIndex = Random.Range(1, 99999);
            string accessTags = ApiWorldInstance.BuildAccessTags(settings.accessType, settings.userId);
            string id = settings.WorldId + ":" + instanceIndex + accessTags;

            return id;
        }
    }
}