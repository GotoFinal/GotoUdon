using System.Collections.Generic;
using GotoUdon.Utils;
using GotoUdon.Utils.Editor;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Editor;
using static GotoUdon.Editor.ReleaseHelper.ReleaseHelper;

namespace GotoUdon.Editor.ClientManager
{
    public class ClientManagerEditor : EditorWindow
    {
        public static ClientManagerEditor Instance => GetWindow<ClientManagerEditor>(false, "GotoUdon Clients", false);

        [MenuItem("Window/GotoUdon/Client manager")]
        public static ClientManagerEditor ShowWindow()
        {
            return Instance;
        }

        private ClientManagerSettings _settings;
        private ClientsManager _clientsManager;
        private Vector2 _scroll = Vector2.up;


        private void OnFocus()
        {
            UpdaterEditor.Instance.TryCheckUpdate();
        }

        private void OnGUI()
        {
#if GOTOUDON_DEV
            DrawReleaseHelper();
#endif
            UpdaterEditor.Instance.DrawVersionInformation();

            _scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none);
            if (!Application.isPlaying)
            {
                VRCSdkControlPanel.InitAccount();
            }

            _settings.Init();
            SimpleGUI.InfoBox(true,
                "Here you can prepare profiles (vrchat --profile=x option) and launch them at once and connect to given world.\n" +
                "Each profile can be logged in to other vrchat account, allowing you for simple testing.\n" +
                "You can also disable some profiles, this will just simply ignore them when using button to start all clients.\n" +
                "Keeping instance might cause issues on restart with multiple clients, vrchat servers might still think you are trying to join twice.");

            EditorGUI.BeginChangeCheck();
            SimpleGUI.InfoBox(string.IsNullOrWhiteSpace(_settings.WorldId),
                "Make sure your world have a vrc descriptor and you are logged in to SDK, then world id field will be filled up automatically.");
            _settings.worldId = EditorGUILayout.TextField("World ID", _settings.worldId);
            SimpleGUI.InfoBox(string.IsNullOrWhiteSpace(_settings.UserId), "Login to SDK and User ID field will fill up itself");
            _settings.userId = EditorGUILayout.TextField("User ID", _settings.userId);
            SimpleGUI.ActionButton("Find Current WorldID", () => { _settings.worldId = VRCUtils.FindWorldID(); });
            // _settings.sendInvitesOnUpdate = EditorGUILayout.Toggle("Send invites on world update", _settings.sendInvitesOnUpdate);
            _settings.accessType = (ApiWorldInstance.AccessType) EditorGUILayout.EnumPopup("Access Type", _settings.accessType);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Same room restart wait time (s)", GUILayout.Width(200));
            _settings.sameInstanceRestartDelay = EditorGUILayout.IntField(_settings.sameInstanceRestartDelay, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            SimpleGUI.SectionSpacing();
            DrawClientSection();
            SimpleGUI.SectionSpacing();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_settings);
                EditorUtility.SetDirty(GotoUdonInternalState.Instance);
            }

            GUILayout.EndScrollView();
        }

        private bool _keepInstance = false;

        private void DrawClientSection()
        {
            DrawClientSettingsList(_settings.clients);

            SimpleGUI.SectionSpacing();

            if (SimpleGUI.ErrorBox(_settings.worldId == null, "Can't start clients, missing WorldID"))
            {
                return;
            }

            if (SimpleGUI.ErrorBox(_settings.userId == null, "Can't find user id, please log in SDK."))
            {
                return;
            }

            _keepInstance = EditorGUILayout.Toggle("Keep current instance ID", _keepInstance);
            _clientsManager.InstanceId = EditorGUILayout.TextField(_clientsManager.InstanceId);

            GUILayout.BeginHorizontal();
            SimpleGUI.ActionButton("Start", () => _clientsManager.StartClients(false, _keepInstance));
            if (_clientsManager.IsAnyClientRunning())
                SimpleGUI.ActionButton("Restart", () => _clientsManager.StartClients(true, _keepInstance));
            GUILayout.EndHorizontal();

            if (!Application.isPlaying)
            {
                if (SimpleGUI.ErrorBox(APIUser.CurrentUser == null, "Can't find user for auto publish, please log in SDK."))
                {
                    return;
                }

                SimpleGUI.ActionButton("Build & Auto Publish & Start", PublishAndTest);
            }
        }

        private void PublishAndTest()
        {
            StartPublishing();
        }

        private void StartPublishing()
        {
            GotoUdonInternalState.Instance.enableAutomaticPublish = true;
            EditorUtility.SetDirty(GotoUdonInternalState.Instance);
            EnvConfig.ConfigurePlayerSettings();
            VRC_SdkBuilder.PreBuildBehaviourPackaging();
            VRC_SdkBuilder.ExportAndUploadSceneBlueprint();
        }

        public void StartClients()
        {
            _clientsManager.StartClients(false, _keepInstance);
        }

        private void DrawClientSettingsList(List<ClientSettings> allClients)
        {
            ClientSettings removed = null;
            int maxProfile = 10;
            foreach (ClientSettings clientSettings in allClients)
            {
                if (clientSettings.profile > maxProfile) maxProfile = clientSettings.profile;
                bool beforeVr = clientSettings.vr;
                if (DrawClientSettings(clientSettings, "Remove"))
                {
                    removed = clientSettings;
                }

                if (!beforeVr && clientSettings.vr)
                {
                    allClients.ForEach(client =>
                    {
                        if (client != clientSettings)
                            client.vr = false;
                    });
                }

                SimpleGUI.OptionSpacing();
            }

            if (removed != null) allClients.Remove(removed);

            SimpleGUI.ActionButton("Add client", () =>
            {
                allClients.Add(new ClientSettings()
                {
                    name = "",
                    profile = maxProfile + 1,
                    enabled = true
                });
            });
        }

        private bool DrawClientSettings(ClientSettings settings, string buttonAction)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name", GUILayout.Width(35));
            settings.name = EditorGUILayout.TextField(settings.name, GUILayout.Width(100), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Profile", GUILayout.Width(45));
            settings.profile = EditorGUILayout.IntField(settings.profile, GUILayout.Width(20));
            GUILayout.Label("Enabled", GUILayout.Width(55));
            settings.enabled = EditorGUILayout.Toggle(settings.enabled, GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("VR", GUILayout.Width(20));
            settings.vr = EditorGUILayout.Toggle(settings.vr, GUILayout.Width(15));

            GotoUdonInternalState.ClientProcess clientProcess = GotoUdonInternalState.Instance.GetProcessByProfile(settings.profile);
            if (clientProcess?.Process != null)
            {
                SimpleGUI.ActionButton("Stop", () => clientProcess.StopProcess(), GUILayout.Width(45));
                SimpleGUI.ActionButton("Restart", () => _clientsManager.StartClient(true, _keepInstance, settings), GUILayout.Width(70));
                SimpleGUI.ActionButton("Keep room", () => _clientsManager.StartClient(true, true, settings),
                    GUILayout.Width(80));
            }
            else
            {
                SimpleGUI.ActionButton("Start", () => _clientsManager.StartClient(false, _keepInstance, settings), GUILayout.Width(60));
                SimpleGUI.ActionButton("Start [keep room]", () => _clientsManager.StartClient(false, true, settings), GUILayout.Width(120));
            }

            bool actionButton = GUILayout.Button(buttonAction, GUILayout.Width(70));

            EditorGUILayout.EndHorizontal();

            return actionButton;
        }

        protected void OnEnable()
        {
            _settings = AssetDatabase.LoadAssetAtPath<ClientManagerSettings>("Assets/GotoUdon/ClientManagerSettings.asset");
            if (_settings == null)
            {
                _settings = CreateInstance<ClientManagerSettings>();
                _settings.Init();
                AssetDatabase.CreateAsset(_settings, "Assets/GotoUdon/ClientManagerSettings.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            _settings.Init();
            _clientsManager = new ClientsManager(_settings);
            _clientsManager.Init();
        }
    }
}