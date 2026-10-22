# Mod menu 0.10.0 installed - 20 October 2026

The active client is a clean, generic Mod menu shell. It has no bundled mod catalog, mod-specific state parser, score/credits/rank/squad header, predefined status values, imported HUD geometry or old preferences. UserData/Si_ModMenu is a fresh namespace; old HUD files remain only as inactive rollback data.

Installed client SHA-256:
24f1a658ac472a79672e5eca4edba2c02ce94ec88245bbcfb7dbe4f6382cc128

Receipt:
Backups/SquadHUD-standalone-install-20261020T115326193Z/installation.json

Source/settings checkpoint:
Backups/ModMenu-shell-20261020T114347932Z

Release/friend package:
ClientMods/Si_SquadHUD/Releases/0.10.0

The build passed with zero warnings/errors. Installed DLL, previous-DLL backup, friend package and source manifest matched their hashes. All 643 non-HUD/settings preservation checks passed. The new settings namespace has no pre-existing files at installation. The separate companion remains disabled and preserved; no server component was changed.

Current behavior:
- Status, sidebar, notifications and debug buttons; sidebar intentionally has no action.
- Actual loaded client mods get tabs; mods can supply their own title/groups/controls/commands/status/notifications. No generic master switch is invented.
- Admin-only controls live under Admin; Help retains Player/Commander/Admin separation. Native admin/role state is checked when rendering and before invoking actions. Unknown remote permission metadata is not guessed.
- Command clicks only prepare drafts. SEND or a physical Enter press submits once and leaves the menu open; native chat actions are suppressed while open and restored on close. This still needs live verification.
- Help ADD ENTRY saves locally per Steam user and server. No previous catalog/entries are imported.
- Debug shows supplied/discovered options; server debug is admin-only. Arbitrary remote catalogs/permissions cannot be enumerated without metadata; structured server integration is future work.
- There are no automatic server queries/polling/retries. No pre-known gameplay mod integration remains.

No tests were added/run and no game/server was started/stopped. Runtime verification is pending and user-owned.

Rollback with the client closed:
powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_SquadHUD/Rollback-SquadHUD.ps1 -InstallRecord Backups/SquadHUD-standalone-install-20261020T115326193Z/installation.json

This restores the previous 0.9.0 client; its companion remains disabled, as it was before this update. New Mod menu files are retained separately from old HUD settings.

