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

