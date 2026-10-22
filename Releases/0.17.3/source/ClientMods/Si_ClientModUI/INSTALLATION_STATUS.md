# Installed: Si_ClientModUI 0.17.3 / cheats 0.4.1 - 22 October 2026

Installed and SHA-256 verified, including both previous-DLL rollback backups. All 753 unrelated DLL/configuration preservation hashes match. Requested custom Ranks /rank, Ranks /promote and Yo mama /woops entries were removed from the per-user file and shared export, with deletion markers and verified JSON backups. Other commands, sidebar bindings, geometry, status selections and cheats shortcuts were preserved.

Both affected Release builds passed with zero warnings/errors. No tests added/run, game/server start/stop or delegation. Live verification remains user-owned.

[Changes/live checks](FOLLOW_UP_0.17.3.md) · [DLL receipt](installation/20261022T164708668Z/installation.json) · [Deployment verification](Releases/0.17.3/deployment.json).

Source/settings checkpoint and cleanup receipt: Backups/ModUI-menu-followups-20261022T164103646Z/command-cleanup.json. Old JSON copies are in cleanup-rollback/ there. Standard DLL rollback preserves cleaned settings: powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_ClientModUI/Rollback-ModUI.ps1 -InstallRecord ClientMods/Si_ClientModUI/installation/20261022T164708668Z/installation.json.

Historical installation records follow.

# Installed: Si_ClientModUI 0.17.1 — 22 October 2026

Installed client 0.17.1 and compatible cheats provider 0.3.3. Both installed DLLs and their rollback backups match their expected hashes. 101 other DLL/configuration files were verified unchanged. No game/server was started or stopped.

[Changes and live checks](FOLLOW_UP_0.17.1.md) · [Installation receipt](installation/20261022T142005952Z/installation.json) · [Deployment verification](Releases/0.17.1/deployment.json).

Source/settings backup: Backups/ModUI-0.17.1-20261022T161705. Rollback with the game closed: powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_ClientModUI/Rollback-ModUI.ps1 -InstallRecord ClientMods/Si_ClientModUI/installation/20261022T142005952Z/installation.json.

Historical records follow.

# Installed: 0.17.0 client / 2.0.0 server — 21 October 2026

Installed Si_ClientModUI 0.17.0, Si_ServerModUI 2.0.0, Logistics 0.9.1, Formation 2.15.2 and compatible cheats 0.3.2. The temporary friend's Si_ModUI / Si_ModUI_Server DLLs were disabled. Five installed hashes and 602 unchanged configuration files were verified; unrelated server/client DLLs, including Ranks, were preserved. No game/server process was launched or stopped. Live verification is pending.

[Installation receipt](installation/20261021T145722965Z/installation.json) · [Deployment verification](Releases/0.17.0/deployment.json) · [Build details and live checklist](STRUCTURED_INTEGRATION_0.17.0.md).

Rollback with the affected client/server closed: `Rollback-ModUI.ps1 -InstallRecord installation/20261021T145722965Z/installation.json`. Configuration snapshot: `Backups/ModUI-0.17.0-install-20261021T165701086` in the server workspace.

Earlier entries below record historical states, including the previous installation hold.

# New build staged; installation pending
Si_ClientModUI 0.17.0, Si_ServerModUI 2.0.0, Logistics 0.9.1, Formation 2.15.2 and the compatible cheats provider are built for review. **No installed DLL was replaced during this implementation.** The friend's temporary test pair remains active. Wait for the user to close the server/game and confirm readiness before installation. See [implementation and validation](STRUCTURED_INTEGRATION_0.17.0.md).

# Temporary friend-version test active — 21 October 2026

The friend's matching client/server UI and eight server mods are now installed for testing. Our UI binaries and dependent client cheats provider are disabled; Logistics, Formation and Ranks were preserved. See [test installation, compatibility notes and rollback](FRIEND_TEST_INSTALL_2026-10-21.md). Historical installation records below describe our previous baseline.

# Si_ClientModUI 0.16.0 and cheats 0.3.2 installed - 20 October 2026

Installed and verified in the Windows Silica client. Client/provider release builds have zero warnings/errors; 456 combined offline checks pass. Installed DLLs and their rollback backups match the release manifest. Four user settings JSON files were backed up and verified unchanged during deployment. No game/server was started or stopped. Existing server addon 1.1.0 is compatible.

Receipt: installation/20261020T175713587Z/installation.json
Source/settings backup: Backups/ModUI-followups-20261020T173532836Z
Rollback: powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_ClientModUI/Rollback-ModUI.ps1 -InstallRecord ClientMods/Si_ClientModUI/installation/20261020T175713587Z/installation.json

See FOLLOWUP_0.16.0.md for features, known metadata limits and the focused live checklist. Cursor/input behavior, visual divider/grid layout and actual runtime performance still require in-game verification.

Historical installation/preparation records follow.
# Client 0.15.1 generic-menu patch prepared, not installed

Supports explicit numeric commands, sender/rich-text prefixes, inline options, readable labels and server ordering across chat-menu mods. Navigation uses actual reply advertisements. Server 1.1.0 requires no update. See MENU_COMPATIBILITY.md for scope and live checks. Build passes with zero warnings/errors; 201 network/catalog/menu/lifecycle checks and 17 installer checks pass. Package files/checksums verified. The installed client remains 0.15.0.

To install explicitly: powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_ClientModUI/Install-ModUI.ps1 -Side client -ReleaseVersion 0.15.1

Previous installation records follow.
# Si_ClientModUI 0.15.0 / Si_ServerModUI 1.1.0 installed - 20 October 2026

Installed on the Windows Silica client and this dedicated server, with the compatible cheats 0.3.1 provider rebuild. Installed DLLs, replaced-file backups and retired legacy DLL hashes were verified. The old Si_SquadHUD.dll is preserved under a .disabled timestamped name; user settings were not changed. No game/server was started or stopped. Live gameplay verification remains pending.

Receipt: installation/20261020T154820752Z/installation.json
Rollback: powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_ClientModUI/Rollback-ModUI.ps1 -InstallRecord ClientMods/Si_ClientModUI/installation/20261020T154820752Z/installation.json

Previous preparation/installation records follow.
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





