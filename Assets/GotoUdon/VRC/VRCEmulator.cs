using System;
using System.Collections.Generic;
using GotoUdon.Editor;
using GotoUdon.Utils;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
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

        public static VRCEmulator Instance => _instance ?? (_instance = InitEmulator(GotoUdonSettings.Instance));
        public static bool IsReady => _instance != null;

        public void Init(GotoUdonSettings settings)
        {
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

            // if its first play on startup we delay firing on OnJoin events as this seems to fire before all components are ready in code and might cause weird issues.
            if (_hasStarted)
            {
                ForAllUdon(behaviour => behaviour.OnPlayerJoined(player.VRCPlayer));
            }
            else _delayedPlayers.Add(player);
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
            player.gameObject.SetActive(false);

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

        public static VRCEmulator InitEmulator(GotoUdonSettings settings)
        {
            if (_instance != null) return _instance;
            return _instance = CreateEmulator(settings);
        }

        public static void Destroy()
        {
            _instance = null;
        }

        private static VRCEmulator CreateEmulator(GotoUdonSettings settings)
        {
            if (!Application.isPlaying)
            {
                throw new ApplicationException(
                    "Something is trying to launch GotoUdon not in play mode, this would cause objects to be permanently added to the scene.");
            }

            VRCEmulator emulator = new VRCEmulator();
            emulator.Init(settings);
            return emulator;
        }

        public void SpawnPlayer(GotoUdonSettings settings, PlayerTemplate template)
        {
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
            playerGameObject.transform.position = spawnPoint.position;
            playerGameObject.transform.rotation = spawnPoint.rotation;

            SimulatedVRCPlayer simulatedVrcPlayer = playerGameObject.AddComponent<SimulatedVRCPlayer>();
            simulatedVrcPlayer.Initialize(new VRCPlayer(template.playerName), avatar);

            if (template.hasVr) simulatedVrcPlayer.PromoteToVRUser();
            playerGameObject.SetActive(false);
            _runtimePlayers.Add(simulatedVrcPlayer);

            if (template.joinByDefault) AddPlayer(simulatedVrcPlayer);
        }

#if UNITY_EDITOR
        static VRCEmulator()
        {
            Networking._GetUniqueName = obj => GetGameObjectPath(obj.transform);
            Networking._RPC = (destination, o, arg3, arg4) => ActionNotImplemented("_RPC");
            Networking._RPCtoPlayer = (destination, o, arg3, arg4) => ActionNotImplemented("_RPCtoPlayer");
            Networking._Message = (type, o, arg3) => ActionNotImplemented("_Message");
            Networking._IsNetworkSettled = () => Instance.isNetworkSettled;
            Networking._IsMaster = () => Instance.localPlayer == Instance.master;
            Networking._LocalPlayer = () => Instance.localPlayer;
            Networking._IsOwner = (player, obj) => player.IsOwner(obj);
            Networking._SetOwner = (player, obj) => player.TakeOwnership(obj);
            Networking._IsObjectReady = obj => VRCObject.AsVrcObject(obj).isReady;
            Networking._GetOwner = obj => VRCObject.AsVrcObject(obj).VRCPlayer;
            Networking._Destroy = Object.Destroy;
            Networking._SceneEventHandler = Object.FindObjectOfType<VRC_EventHandler>;
            Networking._GetNetworkDateTime = () => DateTime.Now;
            Networking._GetServerTimeInSeconds = () => Time.fixedTime;
            Networking._GetServerTimeInMilliseconds = () => DateTime.Now.Millisecond;
            Networking._Instantiate = (type, s, arg3, arg4) => throw ActionNotImplemented("_Instantiate");
            Networking._ParameterEncoder = arg => throw ActionNotImplemented("_ParameterEncoder");
            Networking._ParameterDecoder = arg => throw ActionNotImplemented("_ParameterDecoder");
            Networking._GoToRoom = arg => throw ActionNotImplemented("_GoToRoom");
            Networking._CalculateServerDeltaTime = (d, d1) => throw ActionNotImplemented("_CalculateServerDeltaTime");
            Networking._SafeStartCoroutine = enumerator => throw ActionNotImplemented("_SafeStartCoroutine");
            Networking._GetEventDispatcher = () => throw ActionNotImplemented("_GetEventDispatcher");
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