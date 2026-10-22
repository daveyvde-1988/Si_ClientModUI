# HUD 0.8.1 follow-up fixes

Installed client/server 0.8.1, with verified backups and SHA-256 copies. Both Release builds completed with zero warnings/errors. No tests or game/server start/stop actions were performed. User live verification is pending.

- Permanently unsupported NO LIVE SWITCH buttons stay disabled/grey. Background-read appearance is now explicit per control, so it cannot accidentally enable the appearance of unsupported controls. Tabs retain their normal appearance while click guards still honor pending work. Read-only Unitbalance does not poll for fresh JSON continuously; its existing published cache/reload semantics are retained.
- Enabled notifications also intercept authenticated native ChatMessage deliveries marked as server messages on the client. This covers remaining Ranks/server announcements without guessing from player text. Ordinary player chat keeps its original path, and existing native recipient/team filtering is respected. Server AdminMod notices still use the reviewed recipient-scoped bridge. The broad server SendServerPacket hook was removed, avoiding interception of every gameplay packet. Malformed/unrecognized native data fails back to the game.
- MOD NOTIFICATIONS replaces MOD NOTICES. Separators in the notification window are removed; entries use a small gap. Decorative separator-only lines in captured notification text are removed. No native chat/player styling is changed.
- The header is Mod status (collapsed and expanded); its expanded navigation button remains MOD MENU [F6]. Selected values render Label: value, without a mod-name prefix.
- All window geometry now has independent -fps and -commander keys within each Steam user's existing local layout file. The old shared geometry is copied as the starting point for each role, then each is saved independently. Original legacy entries/files remain. Installation does not modify any local layout/preference JSON.
- New/default status selections start empty, and newly available statuses remain unselected. Explicit old choices (an existing nonempty HiddenStatus list) migrate once to a positive SelectedStatus set after the complete provider catalog arrives. The old all-selected default becomes empty; manually customized selections are retained. SHOW ALL/HIDE ALL and individual choices cancel pending legacy migration.
- The status picker groups parameters under collapsible + / - module entries with selected/total counts. Group expansion and selections persist locally; no extra selection/layout traffic is sent.

Source checkpoint: Backups/HUD-followups-20261020T005735078Z (also retains client layout JSON copies).
Installation/rollback receipt: Backups/SquadHUD-install-20261020T010049517Z/installation.json.

Gameplay DLLs/source and configuration are unchanged. Formation 2.15.1, Ranks 1.4.3, Unit Balance 7.7.1, Starter Units 1.0.0 and Diagnostics 0.1.0 retained. AntiGrief ownership/direct harvester-Shrimp control and the Centauri rendering repair remain intact. Major 1.0 extraction remains deferred.

Older sections below describe previous behavior; the 0.8.1 changes above supersede their status-selection, shared geometry and server-wide notification-hook details.

# Historical 0.8.0 overview

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

The older sections below describe the preceding releases; the 0.8.0 window/overlay behavior above supersedes their placement and sidebar lifecycle details.

# Historical 0.7.0 overview

Client and server companion **0.7.0** provide continuous scrollable catalogs, draggable full-menu placement, facility-based Buy, team-specific rank tables and authenticated administrator mod controls. The installer changes only these two HUD DLLs. Gameplay mods and server settings are retained. Both Release builds compile cleanly; live verification of the new behavior is pending.

## Window and navigation

The collapsed Mod menu header sits **96 pixels below the top edge**, leaving more room below the native menu. First F6 expands status; second F6 opens the full menu. F6, Escape or Close returns to collapsed. Existing F7 snapshots and Diagnostics F8/F9 remain; there are no new gameplay-action hotkeys.

Drag the **full menu's title bar** with the left mouse button. Its position is saved on release/close to client `UserData/Si_SquadHUD/window-<SteamID64>.json`. Each Steam user has a separate local profile. Normalized coordinates keep the window on-screen after resolution changes. The default is centered; removing that profile with the client closed restores the default. The collapsed/expanded status header retains its standard top-right position.

All catalogs use a side scrollbar and mouse wheel. There are no visible page numbers or Next/Previous controls. Lists arrive as bounded network chunks and become actionable only after the complete snapshot arrives. Refresh reloads the whole current list without resetting a still-valid scroll position. Root tabs have a distinct dark strip, bold centered labels and an accent underline for selection. Hover descriptions still appear after more than two seconds; formation-choice buttons intentionally have none.

## Tabs

- **Formations:** one scrollable list for FPS Follow/Move/Attack or commander layouts. Existing selection, Locked Formation and confirmed AI dismissal behavior is retained. Native disables all personal FPS formations. Formation algorithms and server defaults are unchanged.
- **Buy:** select a production facility, then browse all available faction units from it in one scrollable list. Production Facilities returns to the category list. Unit prices come from KoH's active server catalog. Buy first asks for confirmation; Confirm Purchase uses the original KoH purchase handler with its existing credit, cooldown, Shrimp unlock, location and spawn restrictions. Facility selection spends nothing. Structures remain outside this unit shop.
- **Ranks:** a non-clickable table showing only the player's faction: tier, required effective score, rank title, FPS cap including self, reward bonus, permitted categories/exclusions and seats per team. Traitor is included as Traitor / Deserter / Outcast for the faction, with an explanation of counted teamkills, the configured strike cooldown, first/second demotions, reward restrictions and the round-long promotion lock. Global recruitment bans and native validation still apply to the normal tier table.
- **Help:** non-clickable Command / Mod / Explanation tables under FPS Player, Commander and Admin. Registered mod commands and reviewed Formation/Rank variants are included. Numeric/back/close chat-menu navigation entries are excluded. Some mods publish no argument description; those are labelled explicitly rather than guessed. This is a mod chat-command reference, not a native console/cvar manual, and selecting the tab never executes commands.
- **Admin:** visible to an authenticated player with AdminMod **Generic** power. Lists every loaded server mod, current switch state where known, and verified live controls for **Formation, Logistics, KoH, Crab Cannon and Heavy Teleporter**. These use the existing public API or inspected on/off handlers, including their cleanup/save behavior. Other mods show **Loaded / No live switch**, rather than pretending that loaded means enabled or attempting to unload active hooks. Private adapters disable themselves when the reviewed DLL hash changes. Server permission and revision checks apply independently of the UI.

The full menu retains score, personal credits and FPS squad count/allowance plus AI follower count. Commander shows the FPS squad as inactive. Credits remain the shared KoH/Logistics wallet, separate from faction resources. No mod switch, credit or server setting changes merely by opening or refreshing.

## Appearance

SOL uses a neutral slate surface informed by the corrected screenshot. Alien olive and Centauri burgundy are retained with more opaque backgrounds and brighter near-white text. Centauri adds a subtle **procedural horizontal scan-line texture**, inspired by the native menu; it is not an extracted vanilla texture. Root tabs are distinct from options. Body text defaults to 18 px; FontSize in client `UserData/MelonPreferences.cfg`, category Si_SquadHUD, supports 16-28. The implementation uses custom Unity GUI graphics/fonts.

## Installation, logs and recovery

Prepared hashes: `Releases/0.7.0/manifest.json`. The installer checks game/reference hashes, refuses affected running processes, backs up replaced DLLs and verifies copies. It installs only the client HUD and server companion. It does not launch/stop either program, alter gameplay-mod DLLs or change server configuration. Client placement files are created only by the running mod when a user moves the full window.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ClientMods\Si_SquadHUD\Install-SquadHUD.ps1 -PlanOnly
powershell -NoProfile -ExecutionPolicy Bypass -File .\ClientMods\Si_SquadHUD\Install-SquadHUD.ps1
```

Use -Side client/server to limit installation. Program Files may require elevation. [INSTALLATION_STATUS.md](INSTALLATION_STATUS.md) identifies the receipt. With affected programs closed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ClientMods\Si_SquadHUD\Rollback-SquadHUD.ps1 -InstallRecord '<absolute installation.json path>'
```

Client logs: client `UserData/Si_SquadHUD/session-<timestamp>.log` and `MelonLoader/Latest.log`. Server: server `MelonLoader/Latest.log`, plus native Formation/KoH/admin-handler chat responses. No tests or game/server launches were performed for this release. Follow [TESTING.md](TESTING.md). Local hosting remains unsupported. Re-review private adapters, input hooks and networking after Silica 1.0; see [NETWORKING.md](NETWORKING.md).

## Client disappearance fix (0.4.1)

The 0.4.0 Centauri scan-line draw called GUI.DrawTextureWithTexCoords, a throwing Method unstripping failed stub in this installed IL2CPP binding. Selecting Centauri triggered the render error; the old handler disabled drawing for the rest of that client session, even across map changes/reconnects. Server status continued arriving. 0.4.1 draws decoration through the existing native GUI.Box path, disables only optional decoration if it fails, retries general drawing errors after two seconds, and restores drawing on scene changes/F6 recovery. Errors now log full stack traces. The client fix compiled cleanly; live retesting is pending. Companion/protocol/configuration are unchanged. See Releases/0.4.1/NOTES.md.

## Commander lock option (0.4.2 / Formation 2.15.1)

The commander Formation list includes **Lock formation / Locked or Unlocked** alongside its layout choices. Clicking Unlocked invokes the existing /formation lock handler and arms one commander move; clicking Locked invokes /formation unlock and releases owned groups/cancels the pending action. Locked means the next move is armed or the player owns a live locked group; it clears after arrival/release/cancellation. The menu refreshes commander state approximately once a second while open. The two-second tooltip includes the requested wording and explains next-move scope. At least three eligible ground AI units are required by the existing command. No placement algorithm or persistent locking mode was added. The 0.4.1 render repair is retained.

## Historical commander sidebar and roster update (0.5.0)

The commander Formations tab has a Sidebar button. It replaces the full menu with an 82 px symbol strip, initially at middle left. Drag its top grip; MENU or F6 returns to the full menu, and X closes it. Commander controls remain usable outside the sidebar; pointer interaction and dragging suppress underlying orders. Position is saved separately per Steam user in sidebar-<SteamID64>.json; existing full-menu placement is retained. Team, role, round and connection transitions close the scoped sidebar.

Native uses N; compact/large squares and hexagons, thin/thick lines and open shape outlines reflect the server's actual catalog. Alien Circle and Circle Large are filled discs; Open circle is an outline. Formation lock uses a padlock with authoritative selected state. Hovering for two seconds shows names or the lock explanation. Symbols use owned GUI.Box texture backgrounds, retaining the Centauri rendering repair.

The header says Mod menu and the UI is hidden outside a loaded dedicated-server match. Help and Ranks tables use faction surface/accent colors and thinner dividers. Help's Mod column identifies the callback assembly, with explicit Formation/Ranks labels for reviewed extra commands. Background formation polling retains the last complete list and its normal shading while actions wait for the reply, eliminating the periodic enabled/disabled color transition. Live verification remains pending.
## Unit Balance admin menu and Starter Units (0.7.0)

Both HUD client and server companion must be 0.7.0 for these features. The read-only UNITBALANCE tab retains its searchable category/tree/parameter layout. Its search input and other text inputs now use an owned opaque dark background with white text in every focus/hover state; faction styling and the Centauri GUI.Box repair are retained.

Admin -> Unit Balance -> CONFIGURE mirrors the installed mod's native /b menu: Sol/Centauri/Alien, JSON, HTP, Cap&Decay, Strategystyle and their actual submenus. Click menu entries; parameter rows provide a value input and SET. The original handler supplies validation, confirmations, saves and live application. Some switches apply immediately, exactly as in the chat menu. Parameter confirmation offers Save JSON, Cancel, and Save JSON and rebalance. JSON reset/load/save use the native prompts. BACK navigates up; ROOT cancels the UI's pending selection and starts again. RELOAD SAVED JSON has a separate confirmation and uses the existing rebalance handler. Native configured Discord auto-post behavior remains on reload. Private Discord configuration stays in native chat; its GUI entry is disabled, and webhook URLs are never included in public snapshots.

The companion sends only the current native menu entries/receipts for admin navigation, not an editable full JSON document. Native menu state is private to the authenticated UI session: the adapter temporarily installs it only during a handler call and restores that player's existing chat/stats state afterward. Other players and the same admin's ordinary chat navigation remain independent. Stale JSON/menu revisions are rejected or reopen Root without executing the stale choice. Requests execute on the server game thread, revalidate Generic power and native role/team/connection scope, and deduplicate retries.

Verified backups precede the reviewed native config-write helpers used by this UI: UserData/UnitBalance_cfg/SquadHUD_MenuBackups/. Native menu writes retain their original semantics; this is not the old batch editor. The native enabled switch and legacy 0.6.0 API retain their original verified backups in SquadHUD_Backups/. The installed Unit Balance 7.7.1 DLL/source are not edited or replaced. The existing OFF limitation remains: independent native rule hooks may stay active when its enabled flag is false.

Admin -> Starter Units -> CONFIGURE provides a similar mouse menu. The installed Starter Units 1.0.0 has no native chat editor, so the companion provides Root -> Sol/Centauri/Alien/Wildlife -> unit entries -> count/removal, with Add Unit using the loaded game's unit catalog. Root -> 4-way cross-spawns edits rules, From faction, At faction and their unit lists. Edits require confirmation, preserve unrelated JSON properties, create hash-verified backups in UserData/StarterUnits/SquadHUD_Backups/, and atomically replace the selected configuration (or create a previously absent file). Limits are 0..1000 units per entry and 5000 per configuration. Changes take effect at the next round's normal starter-spawn pass; there is no live respawn/reload button and no invented master switch.

Starter configuration uses only the five fixed JSON filenames documented by the author, never client-supplied file paths. The adapter is pinned to installed Starter Units SHA-256 048c449449f89955e4e2af6e21d75fe6ee466cfdfce523954affad48113412c2. Unit Balance remains pinned to 23e5f58f3337e081b784d4d940f46ec81451c630c900cc3610fac11f68609c66. Changed binaries require review. Author reference: https://github.com/DrMuck/Si_StarterUnits#configuration . No Starter Units source download or mod rebuild is required.

## Traffic and caching (0.7.0)

The public read-only balance cache remains shared gameplay configuration plus defaults/parameter metadata, delivered once per connection context and after observed native reloads. New admin clients receive that same public cache; separate saved full JSON is no longer pushed to them on each save. Admin navigation exchanges bounded current-node menus and selected values only. Existing 0.6.0 clients retain their legacy behavior until updated.

Catalogs now use content fingerprints: unchanged Formation/Admin/Buy/Rank/Help polling returns a small GU acknowledgement instead of repeating the whole list. The client retains complete catalogs across tab changes/close within the same connection/team/role context; changed data still arrives atomically as a complete bounded snapshot. Commander lock polling keeps its original latency. Large identical balance snapshots can reuse compression across recipients. Native Unit Balance's own game override synchronization remains controlled by that mod.

Read server MelonLoader/Latest.log directly:
- HUD transfer scheduled public-balance/admin-menu: packets and encoded bodyBytes for that transfer.
- HUD upload interval: accepted packet/body-byte totals for the previous interval, approximately once a minute while traffic exists.

These counters include HUD payloads accepted by the native send API, including UI request replays; they exclude native/Steam framing, transport retransmission, ordinary gameplay and other mods' own synchronization. They are not total network-interface bandwidth. No tests, game/server starts/stops or live configuration actions were performed for this release; follow TESTING.md for user live checks.