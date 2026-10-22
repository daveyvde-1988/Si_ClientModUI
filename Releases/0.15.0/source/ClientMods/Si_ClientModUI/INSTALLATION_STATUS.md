# Si_ClientModUI 0.15.0 / Si_ServerModUI 1.1.0 - prepared, not installed

Current source is in ClientMods/Si_ClientModUI and ClientMods/Si_ServerModUI. The old client source/scripts were backed up before integration. The existing game installations and settings have not been changed. The client package includes the compatible optional cheats 0.3.1 provider rebuild. Use Install-ModUI.ps1 -PlanOnly to inspect deployment and select -Side client, server or both. See TESTING.md and INTEGRATION_ANALYSIS.md.

Historical installation records below refer to prior DLLs and paths; they are not the deployment state of this release.

# Mod Menu 0.14.0 installed - 20 October 2026

Installed with cheats 0.3.0. SHA-256:
07abaa2b122ccaa8be9660bdeca9757d51c6ee40465f2cc22cb6a99a18746e9b

All menu surfaces are gated by loading, intro/video and cinematic state. Interrupted panels close and release input; status/sidebar presentation returns during gameplay. Sidebar bindings save per team, with separate spectator scope. Old unassigned buttons are assigned once to the first real team joined after upgrading, after a timestamped layout backup. The master sidebar visibility setting remains global.

ADMIN appears only in multiplayer when accessible admin commands or controls exist; singleplayer entries remain available in normal views. Debug -> Controls includes a saved Enable client UI debug checkbox, live counters, a bounded own-event/error list, filtering, clear and copy. This does not re-enable Diagnostics.

Both DLLs and their rollback backups, current source manifests and package checksum were verified. Build passed with zero warnings/errors; 189 combined checks passed. Live UI verification is pending. No game/server process was launched or stopped. User JSON was preserved at installation; team migration and debug preference changes occur through the running menu.

Receipt: ClientMods/cheats/Releases/0.3.0/installation/20261020T145601218Z/receipt.json
See ../cheats/INSTALLATION_STATUS.md for rollback and detailed validation.

Previous installation record follows.

# Mod Menu 0.13.0 installed - 20 October 2026

Installed with cheats 0.2.0. Adds the generic Definition.DrawCommands provider panel alongside Controls, with full content area and input isolation. The cheats provider displays the current native Help registry there. Existing user settings are preserved.

Installed SHA-256: 898d61b7e4a758fc3d3637218e2cf585391af1dc6a95548eed8433ea176f4557
Receipt: ClientMods/cheats/Releases/0.2.0/installation/20261020T141824066Z/receipt.json

Release build: zero warnings/errors. Source, package, installed DLL and rollback backups verified. Live UI verification remains pending. See ../cheats/INSTALLATION_STATUS.md for usage and rollback.

Previous installation record follows.

# Mod Menu 0.12.0 installed - 20 October 2026

Installed with the separate cheats 0.1.0 client mod. Current menu SHA-256:
10c08617258b702e6f5fd1cc96d9d0f32d94201e8232319fd55f2be7050d8bd1

Adds generic provider panels, a provider/sidebar-editor bridge and dollar/punctuation symbols. Provider panels receive the full content area and physical Enter does not submit the hidden chat draft. Existing user settings and server mods were preserved.

The release build passed with zero warnings/errors. Current source, release, installed DLL and previous menu backup hashes were verified. Live UI verification remains pending. No game/server was started/stopped.

Installation and rollback receipt:
ClientMods/cheats/Releases/0.1.0/installation/20261020T132923056Z/receipt.json

Use ClientMods/cheats/Install-Cheats.ps1 -RollbackReceipt with that receipt to restore the previous menu and remove cheats. See ../cheats/INSTALLATION_STATUS.md for full instructions.

Previous installation record follows.

# Mod menu 0.11.0 installed - 20 October 2026

Installed client SHA-256:
5d5659d907be3c44522fca9ec0569a38136ef10532fd8782e3907ef13d294ae8

Installation/rollback receipt:
Backups/SquadHUD-standalone-install-20261020T131513136Z/installation.json

Source/settings checkpoint, including pre-change client JSON:
Backups/ModMenu-sidebar-20261020T125910920Z

Release:
ClientMods/Si_SquadHUD/Releases/0.11.0

The Release build passed with zero warnings/errors. Installed DLL, previous-DLL backup, friend package and current source manifest hashes matched. All 645 recorded non-HUD/settings checks passed, and all 11 prior source checkpoint hashes matched. Existing user-created Help entries, layout and notification choices were not modified by installation. The separate server companion remains disabled/preserved.

Implemented:
- Persistent sidebar enabled by checkbox, individual enabled/edit/remove bindings, custom exact chat commands and mod-provided actions/symbols. Icons submit immediately when clicked, subject to native handler authorization or provider availability.
- Built-in symbol catalog: letters/digits, filled/open shapes, padlock and simple owned pictograms; 12 swatches and RGB hex selection. No seeded actions or predefined gameplay commands.
- Settings: 0..100% surface transparency and 75..150% text/control size, saved locally. Team accents/backgrounds use Player.Team.TeamColor on the client.
- Bottom bar sends arbitrary single-line chat/commands with SEND/Enter, without a Help entry; typed chat is not recorded in command JSON.
- Input isolation covers all native actions in both current input assets, action/map re-enabling, native button polls and ProcessInput. Mod menu text/mouse/Enter/Escape/F6 remain available. Original enabled states restore after held keys release.
- Commanders can access Player and Commander Help; admins can access all categories.
- User-created Help/custom-sidebar definitions write to UserData/Si_ModMenu/created-commands.json. This share/import file has no Steam/server bindings and never auto-executes. No empty/prepopulated command file is produced. Existing 0.10.0 user-created Help data can export on first profile load. Imported references work without a companion; actual command support/permissions remain native.
- Client extension API adds SidebarAction and SidebarSymbol to Definition. No mod-specific integration is assumed.

No tests were added/run and no game/server was started/stopped. Current API wrappers were inspected; runtime verification remains user-owned, especially native keyboard isolation, slider/scaled hit targets and sidebar persistence.

Rollback with the client closed:
powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_SquadHUD/Rollback-SquadHUD.ps1 -InstallRecord Backups/SquadHUD-standalone-install-20261020T131513136Z/installation.json

This restores 0.10.0. New user JSON/preferences remain separate from the DLL rollback; original pre-change JSON copies are in the checkpoint.


