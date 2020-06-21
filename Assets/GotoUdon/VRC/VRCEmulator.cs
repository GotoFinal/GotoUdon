#if GOTOUDON_SIMULATION_TEMP_DISABLED
using System;
using System.Collections.Generic;
using GotoUdon.Editor;
using GotoUdon.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRCSDK2;
using Object = UnityEngine.Object;

namespace GotoUdon.VRC
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class VRCEmulator
    {
        private static VRCEmulator _instance;
        private List<SimulatedVRCPlayer> _runtimePlayers = new List<SimulatedVRCPlayer>();

        private VRCPlayer master;
        private VRCPlayer localPlayer;
        private bool isNetworkSettled = true;
        public List<SimulatedVRCPlayer> AllPlayers => _runtimePlayers;

        public VRCPlayer Master => master;
        public VRCPlayer LocalPlayer => localPlayer;

        public bool IsNetworkSettled
        {
            get => isNetworkSettled;
            set => isNetworkSettled = value;
        }

        public static VRCEmulator Instance =>
            _instance ?? (_instance = InitEmulator(GotoUdonInternalState.Instance, GotoUdonSettings.Instance));

        public static bool IsReady => _instance != null;

        public void Init(GotoUdonInternalState state, GotoUdonSettings settings)
        {
            if (state.enableAutomaticPublish) return;
            if (!settings.enableSimulation) return;
#if UNITY_EDITOR
            RuntimeWorldCreation worldCreation = Object.FindObjectOfType<RuntimeWorldCreation>();
            if (worldCreation != null && worldCreation.pipelineManager != null &&
                worldCreation.pipelineManager.launchedFromSDKPipeline) return;
#endif

            GameObject emulatorObject = new GameObject("GotoUdonEmulator");
            emulatorObject.AddComponent<VRCEmulatorBehaviour>();
            emulatorObject.tag = "EditorOnly";
            foreach (PlayerTemplate template in settings.playerTemplates)
            {
                SpawnPlayer(settings, template);
            }

            if (GetAmountOfPlayers() == 0 && _runtimePlayers.Count > 0)
            {
                AddPlayer(_runtimePlayers[0]);
            }
        }

        public int GetAmountOfPlayers()
        {
            return VRCPlayerApi.sPlayers.Count;
        }

        public void MakeMaster(SimulatedVRCPlayer player)
        {
            master = player.VRCPlayer;
        }

        public void MakeLocal(SimulatedVRCPlayer player)
        {
            if (localPlayer != null)
            {
                localPlayer.isLocal = false;
                localPlayer.SimulatedVrcPlayer.OnBecameRemote();
            }

            localPlayer = player.VRCPlayer;
            localPlayer.isLocal = true;
            player.OnBecameLocal();
        }

        private bool _hasStarted = false;
        private List<SimulatedVRCPlayer> _delayedPlayers = new List<SimulatedVRCPlayer>();

        public void Update()
        {
            if (_hasStarted && _delayedPlayers.Count != 0)
            {
                foreach (SimulatedVRCPlayer player in _delayedPlayers)
                {
                    ForAllUdon(behaviour => behaviour.RunEvent(player.VRCPlayer));
                }

                _delayedPlayers.Clear();
            }
        }

        public void AddPlayer(SimulatedVRCPlayer player)
        {
            player.gameObject.SetActive(true);
            VRCPlayerApi.sPlayers.Add(player.VRCPlayer);
            if (master == null)
            {
                MakeMaster(player);
            }

            if (localPlayer == null)
            {
                MakeLocal(player);
            }
            else player.OnBecameRemote();

            _delayedPlayers.Add(player);
        }

        public void OnPlayStart()
        {
            _hasStarted = true;
            foreach (var delayed in _delayedPlayers)
            {
                ForAllUdon(behaviour => behaviour.OnPlayerJoined(delayed.VRCPlayer));
            }

            _delayedPlayers.Clear();
        }

        public void RemovePlayer(SimulatedVRCPlayer player)
        {
            ForAllUdon(behaviour => behaviour.OnPlayerLeft(player.VRCPlayer));

            VRCPlayerApi.sPlayers.Remove(player.VRCPlayer);

            // might cause weird issues? but doing nothing could do this too.
            if (master == player.VRCPlayer)
            {
                MakeMaster(GetAnyPlayer());
            }

            if (localPlayer == player.VRCPlayer)
            {
                MakeLocal(GetAnyPlayer());
            }

            player.gameObject.SetActive(false);
        }

        private SimulatedVRCPlayer GetAnyPlayer()
        {
            return VRCPlayerApi.sPlayers.Count > 0 ? ((VRCPlayer) VRCPlayerApi.sPlayers[0]).SimulatedVrcPlayer : null;
        }

        private void ForAllUdon(Action<UdonBehaviour> action)
        {
            foreach (UdonBehaviour behaviour in Object.FindObjectsOfType<UdonBehaviour>())
            {
                action(behaviour);
            }
        }

        public static VRCEmulator InitEmulator(GotoUdonInternalState state, GotoUdonSettings settings)
        {
            if (_instance != null) return _instance;
            return _instance = CreateEmulator(state, settings);
        }

        public static void Destroy()
        {
            _instance = null;
        }

        private static VRCEmulator CreateEmulator(GotoUdonInternalState state, GotoUdonSettings settings)
        {
            if (!Application.isPlaying)
            {
                GotoLog.Warn(
                    "Something is trying to launch GotoUdon not in play mode, this would cause objects to be permanently added to the scene. Skipping emulator...");
                return null;
            }

            VRCEmulator emulator = new VRCEmulator();
            emulator.Init(state, settings);
            return emulator;
        }

        public void SpawnPlayer(GotoUdonSettings settings, PlayerTemplate template)
        {
            if (!settings.enableSimulation || GotoUdonInternalState.Instance.enableAutomaticPublish) return;
            Transform spawnPoint = template.spawnPoint == null ? settings.spawnPoint : template.spawnPoint;
            if (spawnPoint == null)
            {
                Debug.LogError(
                    "Can't spawn simulated player, missing spawn point. Please open GotoUdon window and assign a spawn point.");
                return;
            }

            GameObject avatar = template.avatarPrefab == null ? settings.avatarPrefab : template.avatarPrefab;
            if (avatar == null)
            {
                Debug.LogError(
                    "Can't spawn simulated player, missing avatar prefab. Please open GotoUdon window and assign an avatar prefab.");
                return;
            }

            GameObject playerGameObject = new GameObject("Player " + template.playerName);
            playerGameObject.tag = "EditorOnly";
            playerGameObject.transform.position = spawnPoint.position;
            playerGameObject.transform.rotation = spawnPoint.rotation;

            SimulatedVRCPlayer simulatedVrcPlayer = playerGameObject.AddComponent<SimulatedVRCPlayer>();
            simulatedVrcPlayer.tag = "EditorOnly";
            if (template.customId != -1)
            {
                simulatedVrcPlayer.Initialize(new VRCPlayer(template.playerName, template.customId), avatar);
            }
            else
            {
                simulatedVrcPlayer.Initialize(new VRCPlayer(template.playerName), avatar);
            }

            if (template.hasVr) simulatedVrcPlayer.PromoteToVRUser();
            playerGameObject.SetActive(false);
            _runtimePlayers.Add(simulatedVrcPlayer);

            if (template.joinByDefault) AddPlayer(simulatedVrcPlayer);
        }

#if UNITY_EDITOR
        static VRCEmulator()
        {
            UdonBehaviour.RunProgramAsRPCHook = (behaviour, target, evt) => behaviour.SendCustomEvent(evt);
            Networking._GetUniqueName = obj => GetGameObjectPath(obj.transform);
            Networking._RPC = (destination, obj, name, arg) =>
            {
                GotoLog.Log($"[Networking] RPC, dest: {destination}, obj: {obj}, name: {name}, args: {string.Join(", ", arg)}");
            };
            Networking._RPCtoPlayer = (destination, obj, name, arg) =>
            {
                GotoLog.Log($"[Networking] RPCtoPlayer, dest: {destination}, obj: {obj}, name: {name}, args: {string.Join(", ", arg)}");
            };
            Networking._Message = (type, o, arg3) => { };
            Networking._IsNetworkSettled = () => Instance?.isNetworkSettled ?? true;
            Networking._IsMaster = () => Instance?.localPlayer == Instance?.master;
            Networking._LocalPlayer = () => Instance?.localPlayer;
            Networking._IsOwner = (player, obj) => !GotoUdonSettings.Instance.enableSimulation || player.IsOwner(obj);
            Networking._SetOwner = (player, obj) =>
            {
                if (!GotoUdonSettings.Instance.enableSimulation) return;
                player.TakeOwnership(obj);
            };
            Networking._IsObjectReady = obj => !GotoUdonSettings.Instance.enableSimulation || VRCObject.AsVrcObject(obj).isReady;
            Networking._GetOwner = obj => GotoUdonSettings.Instance.enableSimulation ? VRCObject.AsVrcObject(obj).VRCPlayer : null;
            Networking._Destroy = obj =>
            {
                if (!GotoUdonSettings.Instance.enableSimulation) return;
                Object.Destroy(obj);
            };
            Networking._SceneEventHandler = Object.FindObjectOfType<VRC_EventHandler>;
            Networking._GetNetworkDateTime = () => DateTime.UtcNow;
            Networking._GetServerTimeInSeconds = () => (double) Time.time;
            Networking._GetServerTimeInMilliseconds = () => (int) ((double) Time.time * 1000.0);
            Networking._Instantiate = (type, s, arg3, arg4) => null;
            Networking._ParameterEncoder = arg => null;
            Networking._ParameterDecoder = arg => null;
            Networking._GoToRoom = arg => false;
            Networking._CalculateServerDeltaTime = (d, d1) => d - d1;
            Networking._SafeStartCoroutine = enumerator => null;
            Networking._GetEventDispatcher = () => null;
        }
#endif

        public static NotImplementedException ActionNotImplemented(string name)
        {
            throw new NotImplementedException($"Sorry, this action `{name}` is not yet implemented.\n" +
                                              "Please contact the developer at: " +
                                              "https://github.com/GotoFinal/GotoUdon " +
                                              "or GotoFinal#5189 / https://discord.gg/B8hbbax" +
                                              "or twitter/ThatGotoFinal " +
                                              "or email: your@lolis.exposed\n" +
                                              "Please include example unity scene that uses this behaviour so I can test how it works in vrchat.");
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }

            return path;
        }
    }
}
#endif