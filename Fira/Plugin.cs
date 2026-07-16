using System;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Windowing;
using Fira.Windows;

namespace Fira;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/fira";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Fira");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Run a configured command list. Subcommands: run, list, config"
        });

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    public void ToggleConfigUi() => ConfigWindow.Toggle();

    private void OnCommand(string command, string args)
    {
        var subcommand = args.Trim();

        switch (subcommand.ToLowerInvariant())
        {
            case "run":
                RunConfiguredCommands();
                break;
            case "list":
                PrintConfiguredCommands();
                break;
            case "config":
                ToggleConfigUi();
                break;
            default:
                ChatGui.Print("[Fira] Usage: /fira run | /fira list | /fira config");
                break;
        }
    }

    // Fires every configured command exactly once, synchronously, in order.
    // No delays and no re-entry into this loop from a timer or framework tick —
    // that boundary is intentional, see README "Automation boundary".
    private void RunConfiguredCommands()
    {
        if (Configuration.Commands.Count == 0)
        {
            ChatGui.Print("[Fira] No commands configured. Use /fira config to add some.");
            return;
        }

        foreach (var cmd in Configuration.Commands)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                continue;

            var handled = CommandManager.ProcessCommand(cmd);
            if (!handled)
            {
                Log.Warning($"[Fira] Command not recognized by ProcessCommand: {cmd}");
                ChatGui.PrintError($"[Fira] Not recognized (only /echo and plugin commands work in v1): {cmd}");
            }
        }
    }

    private void PrintConfiguredCommands()
    {
        if (Configuration.Commands.Count == 0)
        {
            ChatGui.Print("[Fira] No commands configured.");
            return;
        }

        ChatGui.Print("[Fira] Configured commands:");
        for (var i = 0; i < Configuration.Commands.Count; i++)
        {
            ChatGui.Print($"  {i + 1}. {Configuration.Commands[i]}");
        }
    }
}
