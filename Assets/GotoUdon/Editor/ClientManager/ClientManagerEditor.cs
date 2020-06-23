using System.Collections.Generic;
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
            SimpleGUI.DrawFoldout(this, "Help", () =>
            {
                SimpleGUI.InfoBox(true,
                    "Here you can prepare profiles (vrchat --profile=x option) and launch them at once and connect to given world.\n" +
                    "Each profile can be logged in to other vrchat account, allowing you for simple testing.\n" +
                    "You can also disable some profiles, this will just simply ignore them when using button to start all clients.\n" +
                    "Keeping instance might cause issues on restart with multiple clients, vrchat servers might still think you are trying to join twice.");
            });

            EditorGUI.BeginChangeCheck();

            SimpleGUI.DrawFoldout(this, "Advanced settings", () =>
            {
                SimpleGUI.WarningBox(string.IsNullOrWhiteSpace(_settings.WorldId),
                    "Missing world. Make sure your world have a vrc world descriptor and you are logged in to SDK.");
                SimpleGUI.InfoBox(string.IsNullOrWhiteSpace(_settings.UserId), "Login to SDK and User ID field will fill up itself");
                _settings.userId = EditorGUILayout.TextField("User ID", _settings.userId);
                SimpleGUI.InfoBox(string.IsNullOrWhiteSpace(_settings.gamePath),
                    "This should be automatically filled from sdk, but if its not, point it to your vrchat.exe");
                _settings.gamePath = EditorGUILayout.TextField("Game path", _settings.gamePath);
                _settings.launchOptions = EditorGUILayout.TextField("Launch options", _settings.launchOptions);
                _settings.localLaunchOptions = EditorGUILayout.TextField("Local launch options", _settings.localLaunchOptions);
                // _settings.sendInvitesOnUpdate = EditorGUILayout.Toggle("Send invites on world update", _settings.sendInvitesOnUpdate);
                _settings.accessType = (ApiWorldInstance.AccessType) EditorGUILayout.EnumPopup("Access Type", _settings.accessType);
                _keepInstanceForce = EditorGUILayout.Toggle("Force keep instance ID", _keepInstanceForce);
                if (_keepInstanceForce) _keepInstance = true;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Same room restart wait time (s)", GUILayout.Width(200));
                _settings.sameInstanceRestartDelay = EditorGUILayout.IntField(_settings.sameInstanceRestartDelay, GUILayout.Width(30));
                GUILayout.EndHorizontal();
            });

            SimpleGUI.SectionSpacing();
            DrawClientSection();
            SimpleGUI.SectionSpacing();

            if (SimpleGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_settings);
                EditorUtility.SetDirty(GotoUdonInternalState.Instance);
            }

            SimpleGUI.DrawFooterInformation();
            GUILayout.EndScrollView();
        }

        private bool _keepInstance = false;
        private bool _keepInstanceForce = false;
        private bool _localTesting = true;

        private void DrawClientSection()
        {
            DrawClientSettingsList(_settings.clients);

            SimpleGUI.SectionSpacing();

            if (SimpleGUI.ErrorBox(_settings.WorldId == null, "Can't start clients, missing WorldID"))
            {
                return;
            }

            if (SimpleGUI.ErrorBox(_settings.userId == null, "Can't find user id, please log in SDK."))
            {
                return;
            }

            if (SimpleGUI.ErrorBox(!_settings.IsGamePathValid(), "Can't find game executable"))
            {
                return;
            }

            if (!_localTesting)
            {
                _keepInstance = EditorGUILayout.Toggle("Keep current instance ID", _keepInstance);
            }

            _localTesting = EditorGUILayout.Toggle("Use local testing", _localTesting);
            _clientsManager.InstanceId = EditorGUILayout.TextField(_clientsManager.InstanceId);

            GUILayout.BeginHorizontal();
            string startButtonText = _localTesting ? "Start last version (no build)" : "Start";
            SimpleGUI.ActionButton(startButtonText,
                () => _clientsManager.StartClients(false, _keepInstance, _localTesting, _keepInstanceForce));
            if (_clientsManager.IsAnyClientRunning())
                SimpleGUI.ActionButton("Restart",
                    () => _clientsManager.StartClients(true, _keepInstance, _localTesting, _keepInstanceForce));
            GUILayout.EndHorizontal();

            if (!Application.isPlaying)
            {
                if (_localTesting)
                {
                    SimpleGUI.ActionButton("Build & Start", BuildAndTest);
                }
                else
                {
                    if (SimpleGUI.ErrorBox(APIUser.CurrentUser == null, "Can't find user for auto publish, please log in SDK."))
                    {
                        return;
                    }

                    SimpleGUI.ActionButton("Build & Auto Publish & Start", PublishAndTest);
                }
            }
        }

        private void PublishAndTest()
        {
            StartPublishing();
        }

        private void BuildAndTest()
        {
            EnvConfig.ConfigurePlayerSettings();
            VRC_SdkBuilder.shouldBuildUnityPackage = false;
            AssetExporter.CleanupUnityPackageExport(); // force unity package rebuild on next publish
            VRC_SdkBuilder.numClientsToLaunch = 0;
            VRC_SdkBuilder.forceNoVR = true;
            VRC_SdkBuilder.PreBuildBehaviourPackaging();
            VRC_SdkBuilder.ExportSceneResourceAndRun();
            StartClients();
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
            _clientsManager.StartClients(false, _keepInstance, _localTesting, _keepInstanceForce);
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
                    duplicates = 1,
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
            if (_localTesting)
            {
                GUILayout.Label("Num of", GUILayout.Width(45));
                settings.duplicates = EditorGUILayout.IntField(settings.duplicates, GUILayout.Width(20));
            }

            GUILayout.Label("Enabled", GUILayout.Width(55));
            settings.enabled = EditorGUILayout.Toggle(settings.enabled, GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            GUILayout.Label("VR", GUILayout.Width(20));
            settings.vr = EditorGUILayout.Toggle(settings.vr, GUILayout.Width(15));

            List<GotoUdonInternalState.ClientProcess>
                clientProcess = GotoUdonInternalState.Instance.GetProcessesByProfile(settings.profile);
            if (clientProcess.Count > 0)
            {
                string all = clientProcess.Count > 1 ? " All " + clientProcess.Count : "";
                SimpleGUI.ActionButton("Stop" + all, () => clientProcess.ForEach(p => p.StopProcess()), GUILayout.Width(65));
                SimpleGUI.ActionButton("Restart" + all,
                    () => _clientsManager.StartClients(true, _keepInstance, _keepInstanceForce, _localTesting, settings),
                    GUILayout.Width(90));
                if (!_localTesting)
                    SimpleGUI.ActionButton("Keep room" + all,
                        () => _clientsManager.StartClients(true, true, _keepInstanceForce, _localTesting, settings),
                        GUILayout.Width(100));
            }

            if (_localTesting || clientProcess.Count == 0)
            {
                SimpleGUI.ActionButton("Start One",
                    () => _clientsManager.StartClients(false, _keepInstance, _keepInstanceForce, _localTesting, settings.withDuplicates(1)),
                    GUILayout.Width(80));
                if (!_localTesting)
                    SimpleGUI.ActionButton("Start One [keep room]",
                        () => _clientsManager.StartClients(false, true, _keepInstanceForce, _localTesting, settings.withDuplicates(1)),
                        GUILayout.Width(140));
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
        }
    }
}