using System.Collections.Generic;
using GotoUdon;
using GotoUdon.Editor;
#if GOTOUDON_DEV
using GotoUdon.Editor.ReleaseHelper;
#endif
using GotoUdon.Utils.Editor;
using GotoUdon.VRC;
using UnityEditor;
using UnityEngine;

public class GotoUdonEditor : EditorWindow
{
    public const string VERSION = "v1.0.9";
    public const string ImplementedSDKVersion = "2020.04.17.11.34";
    public static string CurrentSDKVersion => VRC.Core.SDKClientUtilities.GetSDKVersionDate();

    [MenuItem("Window/GotoUdon/Debugger Tools")]
    public static GotoUdonEditor ShowWindow()
    {
        return Instance;
    }

    public static GotoUdonEditor Instance => GetWindow<GotoUdonEditor>(false, "GotoUdon Tools", false);
    private EmulationController _controller = EmulationController.Instance;

    private PlayerTemplate _currentlyEdited = PlayerTemplate.CreateNewPlayer(true);
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
        PlayerTemplateEditor.DrawPlayerTemplate(_currentlyEdited);
        SimpleGUI.ActionButton("Add player", () =>
        {
            _controller.Emulator.SpawnPlayer(_controller.Settings, _currentlyEdited);
            _currentlyEdited = PlayerTemplate.CreateNewPlayer(true);
        });
    }

    private void DrawTemplatesEditor()
    {
        EditorGUI.BeginChangeCheck();
        DrawGlobalOptions(_controller.Settings);
        SimpleGUI.SectionSpacing();

        List<PlayerTemplate> templates = _controller.Settings.playerTemplates;

        GUILayout.Label("Players to create at startup:");
        PlayerTemplate toRemove = null;
        foreach (var template in templates)
        {
            if (PlayerTemplateEditor.DrawPlayerTemplateWithRemoveButton(template)) toRemove = template;
            SimpleGUI.OptionSpacing();
        }

        templates.Remove(toRemove);

        SimpleGUI.ActionButton("Add another player",
            () => templates.Add(PlayerTemplate.CreateNewPlayer(templates.Count == 0)));
        SimpleGUI.SectionSpacing();

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(_controller.Settings);

        SimpleGUI.InfoBox(true, "In play mode you will be able to control all created players here, or add more");
    }

    private void DrawGlobalOptions(GotoUdonSettings settings)
    {
        settings.Init();
        SimpleGUI.ErrorBox(settings.avatarPrefab == null,
            "You need to select some avatar prefab to use this resource. Recommended one: https://assetstore.unity.com/packages/3d/characters/robots/space-robot-kyle-4696 (remember to import as humanoid avatar)");
        SimpleGUI.ErrorBox(settings.spawnPoint == null,
            "You need to select some spawn point to use this resource!");

        GUILayout.Label("Global settings");
        SimpleGUI.Indent(() =>
        {
            settings.enableSimulation = EditorGUILayout.Toggle("Enable simulation", settings.enableSimulation);
            settings.avatarPrefab = SimpleGUI.ObjectField("Avatar prefab", settings.avatarPrefab, false);
            settings.spawnPoint = SimpleGUI.ObjectField("Spawn point", settings.spawnPoint, true);
        });
    }

    protected void OnEnable()
    {
        GotoUdonSettings.Instance.Init();
    }
}