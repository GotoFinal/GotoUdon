using System.Collections.Generic;
using GotoUdon;
using GotoUdon.Editor;
using GotoUdon.Utils;
#if GOTOUDON_DEV
using GotoUdon.Editor.ReleaseHelper;
#endif
using GotoUdon.Utils.Editor;
using GotoUdon.VRC;
using UnityEditor;
using UnityEngine;

public class GotoUdonEditor : EditorWindow
{
    public const string VERSION = "v1.3.1";
    public const string ImplementedSDKVersion = "2020.06.16.20.53";
    public static string CurrentSDKVersion => VRC.Core.SDKClientUtilities.GetSDKVersionDate();

    [MenuItem("Window/GotoUdon/Debugger Tools")]
    public static GotoUdonEditor ShowWindow()
    {
        return Instance;
    }

    public static GotoUdonEditor Instance => GetWindow<GotoUdonEditor>(false, "GotoUdon Tools", false);
    private Vector2 _scroll = Vector2.up;

    private void OnFocus()
    {
        UpdaterEditor.Instance.TryCheckUpdate();
    }

    private void OnGUI()
    {
#if GOTOUDON_DEV
        ReleaseHelper.DrawReleaseHelper();
#endif
        UpdaterEditor.Instance.DrawVersionInformation();


#if GOTOUDON_SIMULATION_TEMP_DISABLED
        ImplementationValidator.DrawValidationErrors(ImplementationValidator.ValidateEmulator());
#endif

        SimpleGUI.WarningBox(true,
            "NETWORK AND VRCHAT PHYSICS ARE NOT SIMULATED, NETWORK RELATED SETTINGS ONLY AFFECT RETURNED VALUES IN SCRIPTS, DEFAULT UNITY PHYSICS APPLIES (might be improved later)");

        _scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none);

        if (EditorApplication.isPlaying) DrawPlayersEditor();
        else DrawTemplatesEditor();

        SimpleGUI.DrawFooterInformation();
        GUILayout.EndScrollView();
    }

#if GOTOUDON_SIMULATION_TEMP_DISABLED
    private EmulationController _controller = EmulationController.Instance;

    private PlayerTemplate _currentlyEdited = null;

    private void DrawPlayersEditor()
    {
        GotoUdonSettings settings = _controller.Settings;
        if (SimpleGUI.WarningBox(!settings.enableSimulation, "Simulation is disabled"))
        {
            return;
        }

        VRCEmulator emulator = _controller.Emulator;

        if (SimpleGUI.InfoBox(!VRCEmulator.IsReady, "Waiting for emulation to begin...")) return;
        SimpleGUI.ErrorBox(emulator.GetAmountOfPlayers() == 0,
            "Emulator should not be started without at least one player!");

        SimpleGUI.OptionSpacing();
        GUILayout.Label("Global settings");
        emulator.IsNetworkSettled = GUILayout.Toggle(emulator.IsNetworkSettled, "Is network settled");

        GUILayout.Label("Spawned players: ");
        SimpleGUI.OptionSpacing();
        foreach (SimulatedVRCPlayer runtimePlayer in _controller.RuntimePlayers)
        {
            if (!runtimePlayer.gameObject.activeSelf) continue;
            SimulatedPlayerEditor.DrawActiveRuntimePlayer(emulator, runtimePlayer);
            SimpleGUI.OptionSpacing();
        }

        SimpleGUI.SectionSpacing();

        GUILayout.Label("Available players: ");
        SimpleGUI.OptionSpacing();
        foreach (SimulatedVRCPlayer runtimePlayer in _controller.RuntimePlayers)
        {
            if (runtimePlayer.gameObject.activeSelf) continue;
            SimulatedPlayerEditor.DrawAvailableRuntimePlayer(emulator, runtimePlayer);
            SimpleGUI.OptionSpacing();
        }

        DrawAddPlayerBox();
    }

    private void DrawAddPlayerBox()
    {
        if (_currentlyEdited == null || (_currentlyEdited.playerName == "" && _currentlyEdited.customId == 0))
            _currentlyEdited = PlayerTemplate.CreateNewPlayer(true);
        PlayerTemplateEditor.DrawPlayerTemplate(_currentlyEdited);
        SimpleGUI.ActionButton("Add player", () =>
        {
            _controller.Emulator.SpawnPlayer(GotoUdonSettings.Instance, _currentlyEdited);
            _currentlyEdited = PlayerTemplate.CreateNewPlayer(true);
        });
    }
#else
    private void DrawPlayersEditor()
    {
    }
#endif

    private void DrawTemplatesEditor()
    {
        EditorGUI.BeginChangeCheck();
        DrawGlobalOptions(GotoUdonSettings.Instance);
        SimpleGUI.SectionSpacing();

#if GOTOUDON_SIMULATION_TEMP_DISABLED
        List<PlayerTemplate> templates = GotoUdonSettings.Instance.playerTemplates;
        if (templates.Count == 0)
            templates.Add(PlayerTemplate.CreateNewPlayer(true));

        GUILayout.Label("Players to create at startup:");
        PlayerTemplate toRemove = null;
        foreach (var template in templates)
        {
            if (PlayerTemplateEditor.DrawPlayerTemplateWithRemoveButton(template)) toRemove = template;
            SimpleGUI.OptionSpacing();
        }

        templates.Remove(toRemove);
        if (templates.Count == 0)
            templates.Add(PlayerTemplate.CreateNewPlayer(true));

        SimpleGUI.ActionButton("Add another player",
            () => templates.Add(PlayerTemplate.CreateNewPlayer(templates.Count == 0)));
        SimpleGUI.SectionSpacing();

        if (SimpleGUI.EndChangeCheck())
            EditorUtility.SetDirty(GotoUdonSettings.Instance);

        SimpleGUI.InfoBox(true, "In play mode you will be able to control all created players here, or add more");
#endif
    }

    private void DrawGlobalOptions(GotoUdonSettings settings)
    {
        settings.Init();

        SimpleGUI.WarningBox(true,
            "Sorry, currently simulator is not available. Please use client manager with new local testing functionality.\n" +
            "Emulation will be restored in 1.4.0");
        // if (!settings.IsSimulatorInstalled)
        // {
        //     SimpleGUI.ActionButton("Install simulator", () => settings.IsSimulatorInstalled = true);
        // }
        // else
        // {
        //     SimpleGUI.ActionButton("Remove simulator", () => settings.IsSimulatorInstalled = false);
        // }

#if GOTOUDON_SIMULATION_TEMP_DISABLED
        SimpleGUI.ErrorBox(settings.avatarPrefab == null,
            "You need to select some avatar prefab to use this resource. You can find ybot-mini in Assets folder with this resource.");
        SimpleGUI.ErrorBox(settings.spawnPoint == null,
            "You need to select some spawn point to use this resource!");

        GUILayout.Label("Global settings");
        SimpleGUI.Indent(() =>
        {
            settings.enableSimulation = EditorGUILayout.Toggle("Enable simulation", settings.enableSimulation);
            settings.avatarPrefab = SimpleGUI.ObjectField("Avatar prefab", settings.avatarPrefab, false);
            settings.spawnPoint = SimpleGUI.ObjectField("Spawn point", settings.spawnPoint, true);
        });

        // nah, not really working
        // SimpleGUI.DrawFoldout(this, "Advanced settings", () =>
        // {
        //     SimpleGUI.WarningBox(true,
        //         "Enabling vrchat client mode might cause some issues, but also allow to test your scripts with secure heap enabled\n" +
        //         "This will add or remove VRC_CLIENT define for compiler, meaning that all internal sdk code will think its running on client and not in editor.\n" +
        //         "Use at own risk.");
        //     string VRC_CLIENT = "VRC_CLIENT";
        //     bool vrchatClientMode = UnityCompilerUtils.IsDefineEnabled(VRC_CLIENT);
        //     string buttonName = vrchatClientMode ? "Use vrchat editor mode" : "Use vrchat client mode";
        //     SimpleGUI.ActionButton(buttonName, () => UnityCompilerUtils.SetDefineEnabled(VRC_CLIENT, !vrchatClientMode));
        // });
#endif
    }

    protected void OnEnable()
    {
        GotoUdonSettings.Instance.Init();
    }
}