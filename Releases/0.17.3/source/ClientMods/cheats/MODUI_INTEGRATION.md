Current provider 0.4.1 / UI 0.17.3: PanelAdminOnly remains true; PlayerPanelAvailable returns the native enabled state. ADMIN retains the cheat toggle; PLAYER gets controls/reference only while enabled. Run rechecks current access and rejects non-admin cheat toggles. Status metadata refreshes on access changes; native dispatch and its per-command permissions remain. Definition.Description supplies the Commands summary.

# cheats 0.4.0 / Si_ClientModUI 0.17.2

Dynamic spawning now uses 50 metres along a fresh camera forward vector on every execution, including pitch. New shortcuts use `{view+50}`. Existing `{view+100}` shortcuts are accepted and resolved at 50 metres, normalized in memory on load and written in the new form on the next explicit save. IDs, custom labels and sidebar bindings are retained. Fixed-coordinate shortcuts retain their coordinates.

## Command reference placement

The cheats provider already registers `ClientModControls.Definition.DrawCommands = DrawCommands` and `DrawPanel = Draw`, with `PanelAdminOnly = true`. No console commands need to be registered as chat commands.

Si_ClientModUI 0.17.2 routes `DrawCommands` to **COMMANDS -> CLIENT MODS -> cheats -> ADMIN**. The native command registry remains a searchable, read-only reference, using the same source as native Help. It replaces the generic list/chat dock for that provider selection. The interactive cheats panel remains under **MODS -> CLIENT MODS -> cheats -> ADMIN**. EXTRA PANEL is removed. The generic ALL command list is unchanged.

For another MODUI implementation, consume that same `DrawCommands` callback in its Commands section. Pass the existing styled/scaled `PanelContext`, preserve admin checks, and suppress hidden chat-draft Enter handling while drawing the provider. No cheats-specific shell catalog, command dispatch change or server addon is required. The local implementation changes are in `Si_ClientModUI/Client/ControllerMenu.cs` and `LocalModControls.cs`.

## Status contract

The provider supplies 21 `Definition.Statuses` entries. MODUI reads the callbacks on its existing refresh interval. Each row has a stable ID, label and group; all detailed rows set `AdminOnly = true`, and callbacks recheck access. The old `access` ID is preserved. Full selection IDs are `local:cheats:<id>`.

| Provider status ID | Content |
| --- | --- |
| `access` | Native cheats enabled/disabled, or access denied |
| `active` | Confirmed activated effects, including unlimited construction, damage/AI/fog disabled, debug displays and lights hidden; unknown-state count |
| `altered-values` | Numeric values different from the native reset value 1; unknown-value count |
| `toggle:Build`, `toggle:Damage`, `toggle:AI`, `toggle:Fog`, `toggle:Units`, `toggle:DamageDebug`, `toggle:Lights`, `toggle:NetList`, `toggle:AllowVisionModes` | Individual native toggle states; Damage/AI/Fog/Lights refer to whether those game systems are enabled |
| `health` | Native local health display mode |
| `value:TimeScale`, `value:DayScale`, `value:SunScale`, `value:MoonScale`, `value:NetTickMult` | Individual numeric values |
| `teams:AICommander`, `teams:UnlinkedStructures` | Separate named values for all available teams |
| `teams:resources` | Stored resource pools reported by the game, by team |

Enable the status window in **SETTINGS -> STATUS**, then select the desired cheats rows. MODUI intentionally starts with no parameters selected. No user layout or status selections are overwritten.

Singleplayer values and local debug effects are read from game state. Multiplayer server effects and multipliers use recognized native feedback and are explicitly labeled **last native feedback**. Before a reply is received they show **unknown**. They are not inferred from command submission, mirrored client statics or a successful dispatch return. Replies received from native console use outside this panel also update statuses; the eight-second UI feedback window does not limit status collection. Feedback is cleared on scene/session changes. Native replies may not reveal subsequent server changes; these rows are not continuous server telemetry.

Native feedback parsing is limited to known command/reply shapes, accepts dot/comma decimal feedback, and rejects denial, error and non-finite numeric replies. Summary comparisons use the native reset value 1, not an assumed map-specific or server-configured baseline. Resource/spawn one-shot actions and arbitrary `Set` parameters are not represented as persistent activated toggles.

## Build and verification

- Build both through `dotnet build ClientMods/cheats/cheats.csproj -c Release --no-restore`.
- Run `dotnet run --project ClientMods/cheats/tests/CommandChecks.csproj -c Release --no-restore` (258 checks in en-US, nl-BE and fr-BE).
- Run `dotnet run --project ClientMods/Si_ClientModUI/tests/NetworkChecks.csproj -c Release --no-restore` (270 existing UI/network/catalog/lifecycle checks).
- Native reply shapes and reset behavior were checked against the existing successful managed server DebugConsole extraction for Steam build 25179400: `GameReference/server/build-25179400-20261019T124743376Z/runs/smoke-20261020T132102724Z/result.json` and its output. Client interop compilation does not establish native implementation behavior.

Live checks still required: select the new status rows; change and reset toggles/multipliers; compare native console feedback with the status window; test native console use after eight seconds; check per-team commander/decay values; lose admin access; change scene/reconnect; check the Commands reference search and Enter isolation; click old/new spawn shortcuts after moving and pitching the camera and verify a 50 m offset.

Prepared client DLLs are packaged together. Close the Silica client, back up its existing `Mods/cheats.dll` and `Mods/Si_ClientModUI.dll`, and copy both supplied DLLs to that client's `Mods` folder. Keep `UserData` intact. The release is prepared only; no live installation or in-game verification was performed.
