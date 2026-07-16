# Fira

A configurable macro-runner for FFXIV via Dalamud. `/fira` fires a user-configured,
ordered list of commands **once, on demand** — no timers, no auto-repeat.

- `/fira run` — fire the configured command list, once
- `/fira list` — print the configured command list to chat
- `/fira config` — open the config window

## Automation boundary

This plugin is deliberately fire-on-demand only. There is no timer, no `IFramework`
tick subscription, and no delay between commands — `/fira run` executes the whole
list synchronously in a single call. A timed/looped action queue would read as
botting to a GM and is out of scope, permanently, unless explicitly reconsidered
with that risk in mind.

## v1 scope

`/fira run` dispatches each configured line through `ICommandManager.ProcessCommand`.
That reaches **plugin-registered commands and `/echo`** — it does *not* reach native
game slash commands like `/emote` or `/glamour`, which require a chatbox sender
(different mechanism, v2).

## Setup checklist

1. **XIVLauncher + Dalamud**: install XIVLauncher, launch the game through it at
   least once. This creates `%AppData%\XIVLauncher\addon\Hooks\dev` (the Dalamud
   dev assemblies the SDK builds against) and `%AppData%\XIVLauncher\devPlugins`.
2. **.NET SDK**: this project uses `Dalamud.NET.Sdk/15.0.0`, which targets
   `net10.0-windows7.0` — install the **.NET 10 SDK**, not .NET 8.
3. **Editor**: VS Code with the C# Dev Kit (or plain C# extension + OmniSharp)
   works fine; Visual Studio works too if you have it.
4. Build once (`dotnet build` from the repo root) to confirm the SDK resolves
   Dalamud correctly before touching any plugin code.

## Dev loop

1. Edit code.
2. `dotnet build` (or build from your editor). Output lands in
   `Fira/bin/x64/Debug/net10.0-windows7.0/Fira.dll` alongside `Fira.json`.
3. In-game, open `/xlsettings` → **Experimental** → **Dev Plugin Locations**, and
   add the full path to that `Fira.dll` (one-time setup — after this, Dalamud
   loads straight from your build output, no copying).
4. Open the Plugin Installer (`/xlplugins`) → **Dev Tools** tab, find Fira, and
   click reload. No game restart needed for most changes; a full restart is only
   needed if you change `[PluginService]` fields.
5. Test: `/fira config` to edit the list, `/fira list` to confirm it saved,
   `/fira run` to fire it.

## Project layout

- `Fira/Plugin.cs` — entry point, command registration, command dispatch
- `Fira/Configuration.cs` — persisted config (`IPluginConfiguration`)
- `Fira/Windows/ConfigWindow.cs` — in-game config UI (ImGui)
- `Fira/Fira.json` — plugin manifest (name/author/description shown in the installer)
