# SquadHUD 0.8.0

Independent persistent commander sidebar; lowercase DLL-name admin headers and expandable reviewed Logistics/Crab Cannon subfeatures; selectable status providers; opt-in recipient-scoped mod notification window. All HUD windows are draggable/resizable with per-Steam-user local saved geometry. F6 supplies a cursor for arranging overlays beside the full menu. Existing menu/sidebar positions migrate without changing legacy files.

Status streams use changed-value deltas, bounded snapshots and periodic recovery/liveness. Module states are shared per server status tick. Notice history holds 200 entries, measures text only when content/width/font changes and draws visible rows. No layout/preferences traffic or new ports. Public Unit Balance layout, native admin editors, authoritative lock and Centauri GUI.Box repair retained.

Reviewed notification paths: AdminMod per-recipient chat and mod-origin native server chat. Normal player chat stays in chat; native team/private scope is retained. Unknown custom transports, errors/expired leases, >1100 UTF-8-byte notices and floods retain chat as fallback. X disables routing. History is memory-only. Stats show known available values; unsupported states stay Loaded/Unavailable. Crab Cannon detail stats retain Generic-admin access. Future adapters are documented in EXTENDING.md.

Client/server Release builds: zero warnings/errors. No tests, settings/save/reload/switch operations, or game/server start/stop actions. User live verification pending. Only two HUD destinations changed; all 618 non-HUD preservation hashes matched. Gameplay DLLs/source and existing configuration retained. Formation 2.15.1, Ranks 1.4.3, Unit Balance 7.7.1, Starter Units 1.0.0, Diagnostics 0.1.0 unchanged. AntiGrief ownership and allowed direct harvester/Shrimp control retained. Major 1.0 extraction deferred.

Source checkpoint: Backups/Persistent-HUD-20261019T234525169Z.
Final verified rollback receipt: Backups/SquadHUD-install-final-20261019T235513780Z/installation.json (original 0.7.0 backups; final 0.8.0 hashes; all three install steps retained).