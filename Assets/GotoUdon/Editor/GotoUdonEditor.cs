using System.Collections.Generic;
using GotoUdon.Editor;
#if GOTOUDON_DEV
using GotoUdon.Editor.ReleaseHelper;
#endif
using GotoUdon.Utils.Editor;
using GotoUdon.VRC;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

[InitializeOnLoad]
public class GotoUdonEditor : EditorWindow
{
    public const string VERSION = "v1.0.4";
    public const string ImplementedSDKVersion = "2020.04.17.11.34";
    public static string CurrentSDKVersion => VRC.Core.SDKClientUtilities.GetSDKVersionDate();

    // register an event handler when the class is initialized
    static GotoUdonEditor()
    {
        EditorApplication.playModeStateChanged += OnModeChange;
    }

    [MenuItem("Window/GotoUdon/Debugger Tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(GotoUdonEditor));
    }

    public static GotoUdonEditor Instance => GetWindow<GotoUdonEditor>();

    private static void OnModeChange(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode)
        {
            Instance.OnPlayEnd();
            return;
        }

        Instance.OnPlay();
        foreach (UdonBehaviour udonBehaviour in Resources.FindObjectsOfTypeAll<UdonBehaviour>())
        {
            GameObject gameObject = udonBehaviour.gameObject;
            if (gameObject == null) return;
            if (gameObject.GetComponent<UdonDebugger>() == null)
                gameObject.AddComponent<UdonDebugger>();
        }
    }

    private VRCEmulator Emulator => VRCEmulator.Instance;

    private GotoUdonSettings Settings
    {
        get => GotoUdonSettings.Instance;
        set => GotoUdonSettings.Instance = value;
    }

    private List<SimulatedVRCPlayer> RuntimePlayers => VRCEmulator.Instance.AllPlayers;
    private PlayerTemplate _currentlyEdited = PlayerTemplate.CreateNewPlayer(true);
    private Vector2 _scroll = Vector2.up;

    private void OnPlay()
    {
        VRCEmulator.InitEmulator(Settings);
        Emulator.OnPlayStart();
    }

    private void OnPlayEnd()
    {
        VRCEmulator.Destroy();
    }


    private const float OPTION_SPACING = 7;
    private const float SECTION_SPACING = 15;
    private readonly UpdaterEditor _updaterEditor = new UpdaterEditor();

    private void OnFocus()
    {
        _updaterEditor.TryCheckUpdate();
    }

    private void OnGUI()
    {
#if GOTOUDON_DEV
        ReleaseHelper.DrawReleaseHelper();
#endif
        _updaterEditor.DrawVersionInformation();

        ImplementationValidator.DrawValidationErrors(ImplementationValidator.ValidateEmulator());

        SimpleGUI.WarningBox(true,
            "NETWORK AND VRCHAT PHYSICS ARE NOT SIMULATED, NETWORK RELATED SETTINGS ONLY AFFECT RETURNED VALUES IN SCRIPTS, DEFAULT UNITY PHYSICS APPLIES (might be improved later)");

        _scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none);

        if (EditorApplication.isPlaying) DrawPlayersEditor();
        else DrawTemplatesEditor();

        DrawFooterInformation();
        GUILayout.EndScrollView();
    }

    private void DrawFooterInformation()
    {
        string discordUrl = "https://discord.gg/B8hbbax";
        if (GUILayout.Button($"Click to join (or just add me GotoFinal#5189) on discord for help: {discordUrl}", EditorStyles.helpBox))
        {
            Application.OpenURL(discordUrl);
        }

        if (GUILayout.Button(
            "For best experience also try UdonSharp by Merlin and write Udon in C#! https://github.com/Merlin-san/UdonSharp/",
            EditorStyles.helpBox))
        {
            Application.OpenURL("https://github.com/Merlin-san/UdonSharp/");
        }
    }

    private void DrawPlayersEditor()
    {
        if (SimpleGUI.InfoBox(!VRCEmulator.IsReady, "Waiting for emulation to begin...")) return;
        SimpleGUI.ErrorBox(Emulator.GetAmountOfPlayers() == 0,
            "Emulator should not be started without at least one player!");

        GUILayout.Space(OPTION_SPACING);
        GUILayout.Label("Global settings");
        Emulator.IsNetworkSettled = GUILayout.Toggle(Emulator.IsNetworkSettled, "Is network settled");

        GUILayout.Label("Spawned players: ");
        GUILayout.Space(OPTION_SPACING);
        foreach (SimulatedVRCPlayer runtimePlayer in RuntimePlayers)
        {
            if (!runtimePlayer.gameObject.activeSelf) continue;
            SimulatedPlayerEditor.DrawActiveRuntimePlayer(Emulator, runtimePlayer);
            GUILayout.Space(OPTION_SPACING);
        }

        GUILayout.Space(SECTION_SPACING);

        GUILayout.Label("Available players: ");
        GUILayout.Space(OPTION_SPACING);
        foreach (SimulatedVRCPlayer runtimePlayer in RuntimePlayers)
        {
            if (runtimePlayer.gameObject.activeSelf) continue;
            SimulatedPlayerEditor.DrawAvailableRuntimePlayer(Emulator, runtimePlayer);
            GUILayout.Space(OPTION_SPACING);
        }

        DrawAddPlayerBox();
    }

    private void DrawAddPlayerBox()
    {
        PlayerTemplateEditor.DrawPlayerTemplate(_currentlyEdited);
        SimpleGUI.ActionButton("Add player", () =>
        {
            Emulator.SpawnPlayer(Settings, _currentlyEdited);
            _currentlyEdited = PlayerTemplate.CreateNewPlayer(true);
        });
    }

    private void DrawTemplatesEditor()
    {
        EditorGUI.BeginChangeCheck();
        DrawGlobalOptions(Settings);
        GUILayout.Space(SECTION_SPACING);

        List<PlayerTemplate> templates = Settings.playerTemplates;

        GUILayout.Label("Players to create at startup:");
        PlayerTemplate toRemove = null;
        foreach (var template in templates)
        {
            if (PlayerTemplateEditor.DrawPlayerTemplateWithRemoveButton(template)) toRemove = template;
            GUILayout.Space(OPTION_SPACING);
        }

        templates.Remove(toRemove);

        SimpleGUI.ActionButton("Add another player",
            () => templates.Add(PlayerTemplate.CreateNewPlayer(templates.Count == 0)));
        GUILayout.Space(SECTION_SPACING);

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(Settings);

        SimpleGUI.InfoBox(true, "In play mode you will be able to control all created players here, or add more");
    }

    private void DrawGlobalOptions(GotoUdonSettings settings)
    {
        SimpleGUI.ErrorBox(settings.avatarPrefab == null,
            "You need to select some avatar prefab to use this resource. Recommended one: https://assetstore.unity.com/packages/3d/characters/robots/space-robot-kyle-4696 (remember to import as humanoid avatar)");
        SimpleGUI.ErrorBox(settings.spawnPoint == null,
            "You need to select some spawn point to use this resource!");

        GUILayout.Label("Global settings");
        SimpleGUI.Indent(() =>
        {
            settings.avatarPrefab = SimpleGUI.ObjectField("Avatar prefab", settings.avatarPrefab, false);
            settings.spawnPoint = SimpleGUI.ObjectField("Spawn point", settings.spawnPoint, true);
        });
    }

    protected void OnEnable()
    {
        Settings = AssetDatabase.LoadAssetAtPath<GotoUdonSettings>("Assets/GotoUdon/GotoUdonSettings.asset");
        if (Settings == null)
        {
            Settings = CreateInstance<GotoUdonSettings>();
            Settings.Init();
            AssetDatabase.CreateAsset(Settings, "Assets/GotoUdon/GotoUdonSettings.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Settings.Init();
    }
}