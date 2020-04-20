using System.Collections.Generic;
using System.Diagnostics;
using GotoUdon.Utils;
using GotoUdon.Utils.Editor;
using UnityEditor;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Editor;
using Object = System.Object;

namespace GotoUdon.Editor.ClientManager
{
    public class ClientManagerEditor : EditorWindow
    {
        public static ClientManagerEditor Instance => GetWindow<ClientManagerEditor>(false, "GotoUdon Clients");

        [MenuItem("Window/GotoUdon/Client manager")]
        public static ClientManagerEditor ShowWindow()
        {
            return Instance;
        }

        private ClientManagerSettings _settings;

        private void OnGUI()
        {
            VRCSdkControlPanel.InitAccount();
            _settings.Init();
            SimpleGUI.InfoBox(true,
                "Here you can prepare profiles (vrchat --profile=x option) and launch them at once and connect to given world.\n" +
                "Each profile can be logged in to other vrchat account, allowing you for simple testing.\n" +
                "You can also disable some profiles, this will just simply ignore them when using button to start all clients.");

            EditorGUI.BeginChangeCheck();
            SimpleGUI.InfoBox(string.IsNullOrWhiteSpace(_settings.WorldId),
                "Make sure your world have a vrc descriptor and you are logged in to SDK, then world id field will be filled up automatically.");
            _settings.worldId = EditorGUILayout.TextField("World ID", _settings.worldId);
            SimpleGUI.InfoBox(string.IsNullOrWhiteSpace(_settings.UserId), "Login to SDK and User ID field will fill up itself");
            _settings.userId = EditorGUILayout.TextField("User ID", _settings.userId);
            SimpleGUI.ActionButton("Find Current WorldID", () =>
            {
                string oldId = _settings.worldId;
                _settings.worldId = VRCUtils.FindWorldID();
                if (!Object.Equals(oldId, _settings.worldId))
                {
                    _instanceId = null;
                }
            });
            _settings.accessType = (ApiWorldInstance.AccessType) EditorGUILayout.EnumPopup("Access Type", _settings.accessType);

            SimpleGUI.SectionSpacing();
            DrawClientSection();
            SimpleGUI.SectionSpacing();

            if (_settings.compactMode && GUILayout.Button("Disable Compact Mode"))
            {
                _settings.compactMode = false;
            }
            else if (!_settings.compactMode && GUILayout.Button("Enable Compact Mode"))
            {
                _settings.compactMode = true;
            }
        }

        private string _instanceId;
        private bool _keepInstance = true;

        private void DrawClientSection()
        {
            DrawClientSettingsList(_settings.clients);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_settings);

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
            _instanceId = EditorGUILayout.TextField(_instanceId);

            SimpleGUI.ActionButton("Start", StartClients);
            if (SimpleGUI.ErrorBox(APIUser.CurrentUser == null, "Can't find user for auto publish, please log in SDK."))
            {
                return;
            }

            SimpleGUI.ActionButton("Build & Auto Publish & Test", PublishAndTest);
        }

        private void PublishAndTest()
        {
            StartPublishing();
        }

        private void StartPublishing()
        {
            GotoUdonSettings.Instance.enableAutomaticPublish = true;
            EditorUtility.SetDirty(GotoUdonSettings.Instance);
            EnvConfig.ConfigurePlayerSettings();
            VRC_SdkBuilder.PreBuildBehaviourPackaging();
            VRC_SdkBuilder.ExportAndUploadSceneBlueprint();
        }

        public void StartClients()
        {
            string vrcInstallPath = SDKClientUtilities.GetSavedVRCInstallPath();
            _instanceId = (_keepInstance && !string.IsNullOrWhiteSpace(_instanceId)) ? _instanceId : CreateNewInstanceId();
            string sharedArgs = "--enable-debug-gui --enable-sdk-log-levels --enable-udon-debug-logging";
            foreach (ClientSettings clientSettings in _settings.clients)
            {
                if (!clientSettings.enabled) continue;
                string args = $"{sharedArgs} --profile={clientSettings.profile} \"--url=launch?id={_instanceId}\"";
                if (!clientSettings.vr) args += " --no-vr";
                GotoLog.Log($"Starting VRC with arguments: {args}");
                Process.Start(new ProcessStartInfo(vrcInstallPath, args));
            }
        }

        private string CreateNewInstanceId()
        {
            int instanceIndex = Random.Range(1, 99999);
            string accessTags = ApiWorldInstance.BuildAccessTags(_settings.accessType, _settings.userId);
            return _settings.worldId + ":" + instanceIndex + accessTags;
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
            if (_settings.compactMode)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Name", GUILayout.MaxWidth(40));
                settings.name = EditorGUILayout.TextField(settings.name);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(80));
                GUILayout.Label("Profile");
                settings.profile = EditorGUILayout.IntField(settings.profile, GUILayout.MaxWidth(40));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(80));
                GUILayout.Label("Enabled");
                settings.enabled = EditorGUILayout.Toggle(settings.enabled, GUILayout.MaxWidth(40));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                settings.name = EditorGUILayout.TextField("Name", settings.name);
                EditorGUILayout.BeginHorizontal();
                settings.profile = EditorGUILayout.IntField("Profile", settings.profile);
                settings.enabled = EditorGUILayout.Toggle("Enabled", settings.enabled);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Description");
                settings.description = EditorGUILayout.TextArea(settings.description);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();
            if (settings.vr && GUILayout.Button("Use Desktop"))
            {
                settings.vr = false;
            }
            else if (!settings.vr && GUILayout.Button("Use VR"))
            {
                settings.vr = true;
            }

            bool actionButton = GUILayout.Button(buttonAction);
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
        }
    }
}