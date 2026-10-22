# Standalone Mod Controller 0.9.0 installed - 20 October 2026

Client DLL: C:/Program Files (x86)/Steam/steamapps/common/Silica/Mods/Si_SquadHUD.dll
SHA-256: 84fa95880a01b65d0a779729b1e10f55eb5994eed7896f89642fcf7991afc2b3

The previous server companion 0.8.1 is disabled, preserved outside Mods under ClientMods/Si_SquadHUD_Companion/Disabled. Its original server/shared source, paired 0.8.1 client source, binary and historical docs are separate in that project. All 26 legacy source manifest checks matched. No gameplay mod source/DLL was changed.

Installation/rollback receipt:
Backups/SquadHUD-standalone-install-20261020T105815659Z/installation.json

Source/settings checkpoint (including client layout copies):
Backups/HUD-standalone-20261020T104606517Z

Release:
ClientMods/Si_SquadHUD/Releases/0.9.0

The client build passed with zero warnings/errors. Installed client, replacement backup, disabled companion and companion backup hashes matched. All 643 recorded non-HUD preservation checks passed. Existing client geometry/preferences, Formation 2.15.1, Ranks 1.4.3, Unit Balance 7.7.1, Starter Units 1.0.0 and Diagnostics 0.1.0 are retained. AntiGrief ownership and direct harvester/Shrimp control remain intact.

Standalone behavior:
- No companion packets, server polling, automatic chat queries, heartbeats, retries or routing leases.
- 88 commands across 23 module tabs and clickable Help, prepared from installed mod DLLs. Catalog presence does not prove remote mod availability.
- A command click/SEND uses normal native chat. EDIT supports arguments. Native menu responses become scoped clickable choices for reviewed numbered menus, including Unit Balance multi-column numbered rows/value prompts.
- Local status is read from existing game state; selected recognized chat-derived values include observation age. Unavailable server-private data remains unknown. Continuous authoritative selection/lock highlights and full companion JSON/rank catalogs are no longer available.
- Commander sidebar sends Formation commands; SP MODS lists loaded client mods in singleplayer and supports exposed local preferences or opt-in controls. Diagnostics exposes no such controls.
- Existing local geometry/status choices are retained. Native server-chat notifications use the client receiver; player chat stays native.

No tests were added/run. No game/server was launched/stopped. Runtime verification is pending; the user handles it. See TESTING.md for focused manual checks.

Rollback both sides with affected programs closed:
powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_SquadHUD/Rollback-SquadHUD.ps1 -InstallRecord Backups/SquadHUD-standalone-install-20261020T105815659Z/installation.json

The original paired 0.8.1 installation receipt remains historical at Backups/SquadHUD-install-20261020T010049517Z/installation.json; it does not describe the current standalone deployment.

