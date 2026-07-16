using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace Fira.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Plugin plugin;
    private string newCommandBuffer = string.Empty;

    public ConfigWindow(Plugin plugin) : base("Fira Config###FiraConfigWindow")
    {
        Size = new Vector2(420, 350);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var commands = plugin.Configuration.Commands;

        ImGui.TextWrapped("Commands run in order, top to bottom, when you use /fira run.");
        ImGui.Separator();

        var changed = false;
        var removeIndex = -1;
        var moveUpIndex = -1;
        var moveDownIndex = -1;

        for (var i = 0; i < commands.Count; i++)
        {
            ImGui.PushID(i);

            var buf = commands[i];
            ImGui.SetNextItemWidth(230);
            if (ImGui.InputText("##cmd", ref buf, 256))
            {
                commands[i] = buf;
                changed = true;
            }

            ImGui.SameLine();
            ImGui.BeginDisabled(i == 0);
            if (ImGui.ArrowButton("##up", ImGuiDir.Up))
                moveUpIndex = i;
            ImGui.EndDisabled();

            ImGui.SameLine();
            ImGui.BeginDisabled(i == commands.Count - 1);
            if (ImGui.ArrowButton("##down", ImGuiDir.Down))
                moveDownIndex = i;
            ImGui.EndDisabled();

            ImGui.SameLine();
            if (ImGui.Button("Remove"))
                removeIndex = i;

            ImGui.PopID();
        }

        // Mutations are applied after the loop, not during it — see the
        // "why apply-after-the-loop" note in the README dev-loop section.
        if (removeIndex >= 0)
        {
            commands.RemoveAt(removeIndex);
            changed = true;
        }
        else if (moveUpIndex > 0)
        {
            (commands[moveUpIndex - 1], commands[moveUpIndex]) = (commands[moveUpIndex], commands[moveUpIndex - 1]);
            changed = true;
        }
        else if (moveDownIndex >= 0 && moveDownIndex < commands.Count - 1)
        {
            (commands[moveDownIndex + 1], commands[moveDownIndex]) = (commands[moveDownIndex], commands[moveDownIndex + 1]);
            changed = true;
        }

        ImGui.Separator();

        ImGui.SetNextItemWidth(230);
        ImGui.InputText("##newcmd", ref newCommandBuffer, 256);
        ImGui.SameLine();
        if (ImGui.Button("Add") && !string.IsNullOrWhiteSpace(newCommandBuffer))
        {
            commands.Add(newCommandBuffer);
            newCommandBuffer = string.Empty;
            changed = true;
        }

        if (changed)
        {
            plugin.Configuration.Save();
        }
    }
}
