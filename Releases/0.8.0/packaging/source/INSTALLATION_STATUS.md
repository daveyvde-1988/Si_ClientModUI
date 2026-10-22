# Installed - persistent windows and mod overlays 0.8.0

HUD client/server **0.8.0** are installed and SHA-256 verified. Exactly two HUD DLLs replaced; unchanged Formation was skipped. Both Release projects built with zero warnings/errors. No tests, live settings changes, or game/server start/stop actions were performed. User runtime verification is pending.

- Commander sidebar is an independent overlay with its own authenticated scoped catalog. Closing the full menu, chat, pause and alt-tab no longer disable its preference. It is shown only for commanders in a loaded match with Formation enabled, and returns when that context is valid again.
- Every HUD window uses local drag/resize geometry: full menu (including all its tabs/editors), sidebar, collapsed header, expanded status, status selector and notifications. Drag the title/grip and resize from the bottom-right // grip. Layout and choices save per Steam user in client UserData/Si_SquadHUD/layout-<SteamID64>.json. Legacy menu/sidebar coordinates migrate without overwriting their files. No layout data goes to the server.
- Admin rows use lowercase DLL filenames without .dll. Expand + on si_logistics for harvesterfix/aitransport, and si_crabcannon for its reviewed native feature toggles. All actions retain server permission/revision checks and original handler save/cleanup semantics; enabling a child does not enable its parent.
- Top toolbar: status (left of sidebar), sidebar (commander), notification bar. Status selects the parameters shown in expanded status. Available providers cover existing rank/squad/recruitment/credits, client role/score/squad/FPS, mod states, Logistics settings and Heavy Teleporter settings/team charges/recharge. Crab Cannon details retain its native Generic-admin restriction. Loaded means loaded, not an invented enabled state; missing values are unavailable.
- Notification bar keeps up to 200 notices in memory, with CLEAR and X. Enabling routes mod chat notices for that recipient from reviewed AdminMod delivery and mod-origin native server broadcasts. Player chat stays in chat. Private/team delivery scope is preserved. X disables routing and restores ordinary mod chat. Unknown custom transports, failures, expired leases, messages above 1100 UTF-8 bytes or floods retain native chat as fallback. History clears on connection/team/round context changes and is not written to disk.
- Optional status data sends a bounded initial snapshot, then changed values, with small heartbeats and periodic recovery snapshots. It is requested only while expanded status or its picker is visible. Existing conditional catalogs, public balance cache and traffic counters remain. Notice text measurement is cached and rows outside its viewport are skipped; mod states are read once per server status tick rather than once per client.

Gameplay DLLs, Unit Balance/Starter Units JSON, preferences, Ranks config and other recorded settings matched all **618** preservation hashes after installation. No Unit Balance, Starter Units, Logistics, Formation or other gameplay source/DLL was modified. AntiGrief ownership and allowed direct harvester/Shrimp control remain intact. Existing Centauri rendering repair and authoritative commander lock remain. Major 1.0 extraction remains deferred.

Source checkpoint: Backups/Persistent-HUD-20261019T234525169Z.
Installation/rollback receipt: Backups/SquadHUD-install-final-20261019T235513780Z/installation.json.
Release: Releases/0.8.0/manifest.json. Extending adapters: EXTENDING.md.

With affected programs closed, rollback uses:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ClientMods\Si_SquadHUD\Rollback-SquadHUD.ps1 -InstallRecord '.\Backups\SquadHUD-install-final-20261019T235513780Z\installation.json'
```