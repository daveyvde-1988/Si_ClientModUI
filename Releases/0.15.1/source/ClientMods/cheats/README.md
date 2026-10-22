# Compatibility rebuild 0.3.1

This build references Si_ClientModUI 0.15.0. Source API and cheat behavior are unchanged. Install its companion OptionalProviders/cheats.dll when replacing the old Si_SquadHUD client assembly. Current Install-Cheats.ps1 delegates to ../Si_ClientModUI/Install-ModUI.ps1; original release archives remain historical.

# cheats 0.3.0

Separate Windows IL2CPP Silica client mod. Requires the included Silica Mod Menu 0.14.0 (`Si_SquadHUD.dll`). No server companion is needed. The native server still checks administrator privileges for remote commands.

With the client closed, copy **both DLLs** from the release's `Mods` folder to the client's `Mods` folder. Do not put these client DLLs on the dedicated server. The bundled installer verifies hashes, refuses to install while the client is running, backs up replaced files, and writes a rollback receipt.

Open F6 twice -> **cheats** -> **Controls**. Loaded mods appear automatically. The checkbox sends the native `cheats` toggle and displays `Game.CheatsEnabled`; standard native admin level or higher is required in multiplayer. Singleplayer is allowed. Non-admins see `user is not an admin. access denied`. Execution and sidebar delegates check access again on every use. Disabling cheats disables this mod's command execution and shortcut buttons.

The catalog contains the game's 30 cheat-gated commands plus Camera. Categories cover economy/construction, spawning, combat/AI, player/camera, time/environment, vision/debugging, match/scenario, and access. General commands show native syntax and argument input. Enable/Disable buttons and numeric +/- buttons execute immediately. Every used command remains in an editable console-entry bar, with a session history and Create sidebar button. Only known baseline values are labeled: normal time scale is 1. Local debug state and singleplayer damage/AI/construction state are read directly; multiplayer server state uses labeled last native feedback, never optimistic client copies.

Resources: select team, type a positive amount, then Add amount or Subtract amount. The -100/+100 and -1000/+1000 buttons immediately apply those signed amounts to the selected team. The result uses the exact native signed syntax, e.g. `resources centauri 5000` or `resources centauri -5000`.

Spawn: select faction/type category -> prefab -> team -> position. Prefabs and teams come from the loaded game database. Categories distinguish units, structures and other network prefabs per default team. Only single-token prefab names accepted by the native space-delimited parser are offered; duplicates use the first matching native name. Camera coordinates can be captured directly without opening the console. Alternatively run Camera and paste its clipboard output. Pose order is `x,y,z,w,qx,qy,qz`; yaw-only is the native default, with full rotation optional.

**Dynamic current view** is the default spawn position mode. Select a prefab and team, then Create sidebar button to choose its symbol, label and color. Close the menu and click that sidebar button wherever you want to spawn. Every execution runs native Camera, reads its fresh clipboard snapshot, and calculates position + camera forward * 100 metres. Looking up/down affects height; rotation is captured afresh too. Camera updates the clipboard as the native command normally does. No position is cached between clicks.

The saved command is a template such as `spawn Soldier centauri {view+100}`. The marker is resolved only when the mod runs the shortcut. It is not a native console token. History shows the template and the actual coordinates used by each execution. A fixed-coordinate mode remains available by unchecking Dynamic current view. Existing shortcuts with fixed coordinates remain fixed until edited. To make one dynamic, edit its position token to `{view+100}` or recreate it through Spawn.

There is no established universal free-placement validator for all network prefabs. Dynamic mode implements exactly the 100-metre viewing-direction offset; it does not ground-snap or avoid terrain/objects. The native Spawn handler does not validate those properties either.

The editable USED CONSOLE ENTRY bar stays at the bottom of Controls. Run console entry executes it; Create sidebar button saves it and opens the existing symbol/color/label editor. History lets you select a prior entry without executing it. Enable the persistent sidebar in sidebar settings. Clicking its button dispatches through the native console. Edit the saved command in cheats and its icon/layout in sidebar settings. Deleting a saved command leaves any old sidebar binding unavailable until removed.

0.3.0 fixes the reported native Spawn FormatException. Camera exports dot decimals, while the native spawn handler parses floats using the game's current culture. Dispatch now encodes fractional values with integer mantissas and exponents (for example 1.25 becomes 125E-2), avoiding decimal/group separator ambiguity without losing coordinate or quaternion precision. Dynamic spawning still resolves 100 metres forward. If a native command fails, the UI shows a short message and writes full details to the client log.

SunScale, TimeScale, DayScale and MoonScale now use -1/+1 buttons; NetTickMult retains -0.1/+0.1. These actions still apply immediately. Sidebar buttons are team-specific in Mod Menu 0.14.0. Saved cheats definitions remain available as a catalog; add a binding separately on each desired team.

Enable/Disable is one immediate action button. Locally authoritative and replicated values are read directly; other server effects use confirmed native feedback. Before a server-only boolean is known, Enable sends an explicit enable command; native toggle-only handlers say Toggle until their state is reported. Numeric steps change the displayed parameter and submit the new value on each click. Typed parameter values apply with Apply.

The **Commands** tab beside Controls is a searchable, read-only `command : description` text list from the native `DebugConsole.s_Commands` registry, the same registry enumerated by Help. It includes every registered console command, including non-cheat commands. Browsing does not send Help or other commands to the server.


Definitions save per Steam user in `UserData/cheats/shortcuts-<SteamID>.json`, with a previous-file `.bak`. Symbol/layout preferences remain in Mod Menu's existing user layout. Commands do not execute on load/import. No built-in resource button is added automatically.

## Validation and evidence

- Release builds: zero warnings/errors. 189 command and UI-policy checks pass across en-US, nl-BE and fr-BE, including permission policy, overflow/invalid input, signed resources, quaternion order, camera-forward offsets including pitch/yaw, fresh snapshots on repeated dynamic execution, malformed templates, exact command lookup, newline rejection, locale-sensitive native-parser round trips, overlay visibility, admin sections, and team sidebar persistence.
- Client reference: Steam build 25179420, IL2CPP metadata declarations in `GameReference/client/build-25179420-20261019T124741058Z/runs/smoke-20261019T124800914Z`.
- Managed server logic: Steam build 25179400, successful focused runs `smoke-20261020T132102724Z` (DebugConsole), `smoke-20261020T132446164Z` (GameDatabase), `smoke-20261020T132528247Z` (Game), and `smoke-20261020T141159491Z` (CameraOperations). These are reconstructed managed implementations; matching client interop signatures compile, but client implementations have not been decompiled.
- `Cheats` explicit boolean arguments are inverted in the inspected handler. The checkbox uses the parameterless toggle, so its displayed state always comes from the game. The server's `CheatsEnabledLocal` getter returns true in this build; the UI deliberately gates on the replicated `CheatsEnabled` flag.
- Dispatch uses `DebugConsole.TryExecuteCommand(line, false, null)`, preserving native client forwarding and server per-command admin checks. Nothing sets admin status, cheats fields, resource fields or spawn objects directly.

No game/server process was launched or stopped. Live UI layout, input isolation, permission changes, multiplayer effects, client/server prefab parity, remote feedback, and persistence across restarts still need in-game verification. Submitted commands are not labeled as successful without native feedback. Client-only installation cannot change how the server authorizes vanilla console commands.

Build: `dotnet build ClientMods/cheats/cheats.csproj -c Release --ignore-failed-sources`

Checks: `dotnet run --project ClientMods/cheats/tests/CommandChecks.csproj -c Release`

## In-game check

1. Join as a non-admin: open cheats; confirm denial and no executable cheat controls.
2. Join as a standard/root admin: toggle cheats and wait for the reported checkbox state; test resources +100 and -100 on a test team.
3. Create a Dynamic current view spawn button, close the menu, click, move/rotate, and click again. Verify each spawn is 100m along that click's view. Check pitch and fixed-coordinate mode separately.
4. Save `resources centauri 5000`, bind `$`, enable persistent sidebar, close the menu, and click once. Verify the server changes the pool once.
5. Edit the amount; verify the existing binding invokes the edited command. Restart and verify definitions/symbols persist. Disable cheats or lose admin and confirm shortcuts cannot execute.
6. Check Enable/Disable and immediate +/- actions. Compare Commands with native Help; check entry history, text input, F6/Escape and existing sidebar bindings.

