# Current update - 22 October 2026 (Europe/Brussels): UI 0.17.3 / cheats 0.4.1 installed

The active project is ClientMods/Si_ClientModUI; the old ClientMods/Si_SquadHUD path no longer exists. This update supersedes historical versions below. UI 0.17.3 and cheats 0.4.1 are installed and SHA-256 verified. Server addon remains Si_ServerModUI 2.0.0; all gameplay DLLs are preserved.

Both mod navigation groups sort alphabetically. Custom command removal now forgets matching saved copies across server scopes/imports and saves per-user RemovedCustomCommands deletion markers. Exports omit deleted definitions, including references from retained sidebar buttons. Explicit ADD ENTRY can restore a definition. Requested cleanup removed custom Ranks /rank, Ranks /promote and Yo mama /woops from both saved-command stores. Actual server-advertised Ranks functionality remains. Old removal only deleted the selected scope's saved copy, leaving duplicates that could be exported/reimported. Sidebar buttons retain separate removal controls.

Cheats retains ADMIN plus conditional PLAYER controls/reference while Game.CheatsEnabled reports true. Only admins can toggle cheats, including through typed/saved entries; native per-command authority remains. Generic provider additions: Definition.Description and PlayerPanelAvailable. Commands shows supplied descriptions/accessible command summaries or explicitly states absence. Centauri surfaces are darker; control rows and command buttons now measure content rather than reserving fixed 112/150 pixel cards. Default/vanilla details expand to fit.

Preserved prepared 0.17.2 / cheats 0.4.0 work: native Help reference under COMMANDS -> CLIENT MODS -> cheats; EXTRA PANEL removed from MODS; detailed cheats statuses; fresh-camera 50 metre dynamic spawning and legacy {view+100} compatibility. Fixed-coordinate shortcuts and IDs remain.

Both affected Release builds passed with zero warnings/errors. No tests added/run, game/server start/stop or delegation. Runtime visual/input/gameplay verification remains user-owned. All 753 unrelated DLL/configuration hashes matched. Other commands, layout values, sidebar bindings, status choices and cheats shortcut definitions were preserved; layout adds deletion markers only.

Source/settings checkpoint: Backups/ModUI-menu-followups-20261022T164103646Z. DLL receipt: ClientMods/Si_ClientModUI/installation/20261022T164708668Z/installation.json. Separate requested JSON cleanup receipt: checkpoint command-cleanup.json, with verified old JSON in cleanup-rollback/. Standard DLL rollback preserves cleaned settings. Release/friend ZIP: ClientMods/Si_ClientModUI/Releases/0.17.3/Si_ClientModUI-0.17.3-client.zip (UI DLL plus optional compatible cheats provider, requires MelonLoader). See FOLLOW_UP_0.17.3.md and deployment.json. Gameplay mods, Unit Balance settings, AntiGrief ownership/direct-control policies and deferred major extraction remain untouched.

Historical handoff follows.

# Silica modding project handoff
## Current update - 20 October 2026 (Europe/Brussels): sidebar/settings/portable commands 0.11.0

Client 0.11.0 is installed and hash-verified. This supersedes the 0.10.0 placeholder/sidebar, restricted draft, neutral theme and chat-only input guard. The generic empty-shell approach remains: no bundled mod command catalog, no named gameplay integration, no server queries/companion requirement. The companion stays separate and disabled.

Sidebar now has a persistent enable checkbox, per-button enabled/edit/remove state and a catalog of letters, digits, filled/open geometric shapes, padlock and simple owned pictograms, with swatches/RGB hex colors. A custom icon sends its user-chosen chat command immediately. Loaded mods can supply SidebarAction delegates/commands/availability and SidebarSymbol textures; users choose which bindings to display. No default buttons are created.

Settings controls 0..100% surface transparency and 75..150% text/control scale. Team accents/backgrounds read the local Player.Team.TeamColor, without server support. The bottom bar sends any nonempty native-length text/command on SEND/Enter; saving to Help is no longer required. Typed messages are not exported or auto-saved. Native handlers retain command authorization.

Keyboard isolation captures all actions in both native input assets, blocks action/map re-enable, native ProcessInput and button polling while menu-open, then restores previous states after held keys release. IMGUI text/mouse plus own raw Enter/Escape/F6 remain active. Menu-open logs include the isolated action count; context closures are logged. Live input/scaling verification is pending.

Commanders can access Player and Commander Help; admins can access all roles. User-created Help/custom-sidebar definitions export to UserData/Si_ModMenu/created-commands.json, schema 1 Commands array, with no Steam/server scope. It can be copied to another client/imported in Settings without companion information. No file/data is seeded when no definitions exist. Learned feedback/provider registrations/typed chat are not exported. Existing saved user entries remain eligible; no old known catalog is imported.

Build: zero warnings/errors. Installed/backup/friend ZIP/current source hashes matched; 645 non-HUD/settings checks and 11 prior source checkpoint checks passed. Receipt Backups/SquadHUD-standalone-install-20261020T131513136Z/installation.json; source/settings/JSON checkpoint Backups/ModMenu-sidebar-20261020T125910920Z; release ClientMods/Si_SquadHUD/Releases/0.11.0. No tests or game/server launches/stops. Runtime verification is user-owned. No delegation.


## Current update - 20 October 2026 (Europe/Brussels): clean Mod menu shell 0.10.0

This update supersedes all earlier pre-known mod catalogs, gameplay integrations, preference migrations and UI behavior below. The user now wants the menu to start with zero prior mod knowledge. Do not use old named-mod behavior/versions as implicit integration rules. The shell is client-side; structured server integration is deferred.

Client 0.10.0 is installed and verified. Name: Mod menu. Fresh UserData/Si_ModMenu namespace; no imported geometry/preferences, bundled commands or default status values. Old settings remain inactive for rollback. The old catalog/generator are archived outside the active project. Header metrics and all mod-specific parsers/sidebar controls were removed.

Loaded local mods get their own tabs. ClientModControls.Definition supplies Title, grouped Controls, ChatCommands, Statuses and local Notify. Matching visible local loader preferences can be edited generically. Sidebar is intentionally a no-op placeholder. Status/notifications have extension points, with nothing selected by default.

Commands prepare a draft on click; SEND/physical Enter sends once, retains the menu/draft, and never retries automatically. Native Chat/ChatChannel actions are guarded while the menu is open; original enabled states restore on close after held Enter releases. Admin controls are grouped under Admin, with current native admin checks at rendering and invocation. Help has Player/Commander/Admin separation plus persistent ADD ENTRY, per-Steam-user and server. si_ is removed from display names.

Debug lists options actually exposed or received; server debug is admin-only. There is no universal remote registry/permission feed. Unclassified hints are not silently promoted to available commands. Actual remote permissions belong to native handlers; server-side structured metadata will be designed later.

Build: zero warnings/errors. Installed DLL/backup/friend ZIP/source hashes verified; 643 non-HUD/settings checks matched. Receipt Backups/SquadHUD-standalone-install-20261020T115326193Z/installation.json; source/settings checkpoint Backups/ModMenu-shell-20261020T114347932Z; release ClientMods/Si_SquadHUD/Releases/0.10.0. The separate companion stays disabled/preserved. No tests or game/server launches/stops; runtime verification remains user-owned. Do not delegate unless explicitly requested or required by applicable instructions.


## Current update - 20 October 2026 (Europe/Brussels): standalone controller 0.9.0

This supersedes the historical paired 0.8.1 behavior below. Client 0.9.0 is installed and verified. The old server companion is disabled outside Mods, preserved with original Server/Shared and paired 0.8.1 Client sources in ClientMods/Si_SquadHUD_Companion. 26 preserved source hashes matched. The current client does not speak the companion protocol.

No server polling, custom packets, heartbeats, automatic chat commands or retries. Native chat sends happen only from command clicks/SEND. One tab per command-owning mod plus clickable Help: 88 commands across 23 catalog modules, extracted from current installed DLLs. EDIT supports arguments; normal received numbered menus become clickable for Formation, Unit Balance, KoH Buy and Heavy Teleporter. Unit Balance multi-choice lines and Set prompts are handled without rebuilding/modifying its DLL. Catalog presence is not proof of remote availability, and unknown formats require manual draft input.

Local status reads existing game state; recognized Ranks/Formation/KoH feedback supplies session-only age-labelled values. Unknown server-private data is not inferred. The commander sidebar now uses chat commands without continuous authoritative selection/lock highlights. SP MODS appears in singleplayer for loaded local mods, supporting matching visible MelonPreferences and explicit ClientModControls Read/Apply registrations. Unsupported mods stay listed without invented master switches. Diagnostics is unchanged and exposes no controller.

Build passed: zero warnings/errors. Installation, backup and disabled-companion hashes verified; all 643 non-HUD preservation checks matched. Receipt Backups/SquadHUD-standalone-install-20261020T105815659Z/installation.json; source/settings/layout checkpoint Backups/HUD-standalone-20261020T104606517Z; release ClientMods/Si_SquadHUD/Releases/0.9.0. No tests/game/server start/stop. Runtime verification pending. Current README/INSTALLATION_STATUS/NETWORKING/EXTENDING and TESTING describe the standalone behavior.

Gameplay mods/settings retained: Formation 2.15.1, Ranks 1.4.3, Unit Balance 7.7.1, Starter Units 1.0.0, Diagnostics 0.1.0. AntiGrief recruitment ownership and allowed direct harvester/Shrimp control preserved. No delegation. Major 1.0 extraction remains deferred.



## Current update - 20 October 2026 (Europe/Brussels): HUD 0.8.1

HUD client/server 0.8.1 are installed and SHA-256 verified. Exactly two HUD DLLs replaced; unchanged Formation skipped. Both Release builds passed with zero warnings/errors. No tests, config switches/saves/reloads or game/server start/stop actions. User runtime verification remains pending. Source checkpoint Backups/HUD-followups-20261020T005735078Z, with copied client layout files as well as pre-change source/settings hashes. Receipt Backups/SquadHUD-install-20261020T010049517Z/installation.json. Release ClientMods/Si_SquadHUD/Releases/0.8.1.

Fixes reported appearance blinking: Button no longer globally applies BackgroundRefresh to every disabled control; explicit keepAppearance is used for transient reads and tab visuals. Unsupported NO LIVE SWITCH remains permanently grey. Actual click guards/permissions stay intact. Read-only balance retains its published cache and no continuous successful-snapshot poll is added.

Native server-chat fallback now lives in the existing client ProcessMessage prefix. Only packets from the authenticated server, marked ChatMessage/serverMessage and matching native display scope, are diverted into the enabled notification window. Stream position restores in finally; ordinary player packets/unrecognized or malformed input remain native. This catches remaining Ranks/server notices without message-prefix classification. Reviewed server AdminMod delivery bridge remains. Removed all game SendChatMessage/SendServerPacket hooks from Server/ModNotifications.cs, avoiding the old every-packet hook seen in performance logs. AddNotification centralizes context clearing, rich-text/webhook sanitization, decorative separator removal and bounded 200-entry in-memory history; no drawn divider, only spacing. Long native notices are clipped at 8192 UTF-8 bytes; server UN retains its existing transport bounds.

Labels now Mod status / MOD MENU [F6] and MOD NOTIFICATIONS. Status readout shows selected Label: value only. WindowPlacement resolves every raw window key to role-suffixed -fps / -commander geometry; old shared entries seed both roles independently and remain intact. Client SelectedStatus is a positive allowlist, initially empty, so future providers start unselected. Explicit 0.8 choices (nonempty HiddenStatus) migrate once after the full provider list arrives; old all-selected defaults become empty. Existing explicit choices and all geometry/settings are retained. Status picker has + / - module groups with selected/total counts and locally persisted expansion.

No gameplay/source/settings modifications: Formation 2.15.1 authoritative source still Silica-1283/si_formation; Ranks 1.4.3, Unit Balance 7.7.1, Starter Units 1.0.0 and Diagnostics 0.1.0 retained. AntiGrief ownership/direct harvester-Shrimp control, Centauri GUI.Box repair and commander lock retained. Major 1.0 extraction deferred. Current README/INSTALLATION_STATUS/NETWORKING/TESTING/EXTENDING describe the changes; prior 0.8 instructions below are historical where superseded.
## Current update - 20 October 2026 (Europe/Brussels): HUD 0.8.0

HUD client/server 0.8.0 are installed and SHA-256 verified. Two HUD destinations changed; final consolidated receipt Backups/SquadHUD-install-final-20261019T235513780Z/installation.json contains verified original 0.7.0 backups and final 0.8.0 hashes. The three install step receipts are retained and referenced there. Source checkpoint Backups/Persistent-HUD-20261019T234525169Z. Release ClientMods/Si_SquadHUD/Releases/0.8.0. Both final affected Release builds have zero warnings/errors. All 618 recorded non-HUD preservation hashes matched. No tests, live config actions or game/server start/stop actions. User live verification pending.

Commander sidebar is now independent of panelOpen, with its own authenticated nonce/team/role-scoped commander catalog, conditional replies, serialized controls and roughly one-second status refresh. Its local enabled preference survives closing the full menu, chat/pause/alt-tab and context changes; visible only when a commander in a loaded match with Formation enabled. Reuses the main commander snapshot when that page is open. X explicitly disables it. No Formation gameplay/source changes.

Client/WindowPlacement.cs replaces two separate placement implementations with bounded draggable/resizable geometry and atomic local per-Steam-user layout-<id>.json saves. Legacy float[2] menu/sidebar positions migrate without altering originals. Menu and all tabs/editors share one frame; sidebar, collapsed header, expanded status, status picker and notification window have individual saved geometry. Resize with bottom-right // grip; drag title/grip. Header/status remain drawn alongside the full menu so FPS players can use the F6 cursor to arrange overlays; move the full menu aside if it covers one. Underlying orders are blocked over interactive overlays. Sidebar/notification choices and hidden status IDs are local; notification history is memory-only and clears with connection context.

Admin headers now use lowercase assembly filenames without .dll. Expandable + children use reviewed Logistics harvesterfix/aitransport and Crab Cannon native feature handlers (including original toggle/save semantics). Server Generic/revision validation and hash gates remain; parent OFF still gates its children. No arbitrary config toggles or unloading are introduced.

Client/OverlayWindows.cs, Shared/OverlayProtocol.cs and Server/StatusProviders.cs provide toolbar status selection left of sidebar, plus notification bar. Explicit status registry includes existing rank/caps/policy/recruitment/credits, local role/score/squad/FPS, loaded/master/subfeature states, Logistics settings and Heavy Teleporter settings/team charge/recharge. Crab Cannon extra details retain Generic-only access. Unknown states/values stay Loaded/Unavailable. Optional status lease is requested only with expanded status/picker; initial bounded snapshots, changed-value deltas, 10-second liveness and 30-second recovery refreshes. Module states are read once per server status tick. Notice sizing is cached; only visible notice rows draw.

Server/ModNotifications.cs hooks pinned AdminMod NetworkSendChat per recipient, plus pinned game NetworkLayer SendChatMessage scoped to registered-mod server-message origins and its synchronous per-recipient sends. No player chat prefix guessing, raw packet parsing, or new socket. Team/private recipients remain native. Suppression occurs only after accepted replacement delivery to an opted-in authenticated current peer/channel/team/nonce. Thread-local scope has finalizer restoration and recursion guard. Unknown custom transports, errors, expired 12-second option leases, oversized notices (>1100 UTF-8 bytes) or >60 notices/second keep native chat. Client bar holds 200 entries, strips rich text/redacts webhook URLs, CLEAR discards history, X turns routing off. Existing native Unit Balance menu capture takes precedence and remains separate.

No gameplay DLL/settings/source changed. Formation 2.15.1 source authority remains Silica-1283/si_formation; Ranks 1.4.3, Unit Balance 7.7.1, Starter Units 1.0.0 and Diagnostics 0.1.0 retained. AntiGrief recruitment ownership and allowed direct harvester/Shrimp control, native Unit Balance OFF caveat, Centauri GUI.Box rendering repair and authoritative commander lock retained. Major 1.0 extraction deferred. Current behavior supersedes older sidebar lifecycle/fixed header instructions below; see INSTALLATION_STATUS.md, TESTING.md, NETWORKING.md and EXTENDING.md.

## Current update - 20 October 2026 (Europe/Brussels): HUD 0.7.0

This supersedes the older HUD 0.6.0 admin batch-editor details below. HUD client/server 0.7.0 are installed and SHA-256 verified; exactly two HUD DLLs replaced. Receipt: Backups/SquadHUD-install-20261019T223635569Z/installation.json. Source checkpoint: Backups/Native-admin-menus-20261019T222658894Z (UTC timestamp; local date 20 October). Release: ClientMods/Si_SquadHUD/Releases/0.7.0. Both affected Release builds passed with zero warnings/errors. No tests, live config saves/reloads/switches or game/server start/stop actions. User runtime checks pending.

The public read-only Unitbalance tree/parameter browser is retained. Input backgrounds now use an owned opaque dark 1x1 texture with white text in all states (Client/FactionAppearance.cs inputStyle); previous white texture backgrounds made search unreadable. No shared game skin/assets are mutated.

Admin Configure now mirrors the INSTALLED Unit Balance /b chat handler hierarchy, not the old 0.6.0 graphical batch editor. Server/NativeAdminMenus.cs calls reviewed OnBalanceCommand/ProcessMenuInput/ShowCurrentMenu and captures only Unit Balance's chat/console output for that actor during the call. Per-subscription native menu state is temporarily installed, then the existing chat/stats state is restored in finally. Thus no persistent UI state hijacks ordinary chat. Actual Root: Sol, Centauri, Alien, JSON, HTP, Cap&Decay, Strategystyle. Original validation/confirmations/live behavior retained; some switches apply immediately as in native chat. Numeric/default/on/off inputs are bounded; menu rows are a server-built allowlist. Private Discord editing stays in chat; GUI row is disabled and URLs are redacted.

Reviewed GUI-triggered config writers, LoadSavedConfig and ResetToBlankConfig receive verified pre-write backups in UserData/UnitBalance_cfg/SquadHUD_MenuBackups. Native writes otherwise retain original semantics; do not claim the new GUI is the old atomic batch editor. Separate confirmed reload still uses native rebalance with its configured Discord auto-post behavior. The existing Unit Balance enabled-flag caveat remains: some independent rule hooks can remain active while OFF. Neither its 7.7.1 DLL nor source was modified.

Starter Units integration is now implemented (no longer deferred). Installed 1.0.0 SHA-256 048c449449f89955e4e2af6e21d75fe6ee466cfdfce523954affad48113412c2 has no native chat editor. Server/StarterUnitsMenus.cs supplies Root -> Sol/Centauri/Alien/Wildlife -> units -> count/remove/add; also 4-way rules -> From/At/Units/remove. Unit pickers use loaded game ObjectInfo prefabs. Only five fixed known JSON files are writable. Confirmed saves preserve other properties, verify backups in UserData/StarterUnits/SquadHUD_Backups and atomically replace/create the file. Limits: 0..1000 per entry, 5000 per file. Effects are the next round's existing spawn pass; never invoke SpawnAllStarters to simulate reload. No master switch is invented. Its DLL/settings remain unchanged by installation.

Client/AdminMenuPanel.cs + Shared/AdminMenuModels.cs render current-node responses and value inputs with BACK/ROOT, native confirmations and a separate Unit Balance reload button. UC operation=menu uses existing authenticated native peer/channel/team/role/nonce, Generic revalidation, opaque menu revisions, main-thread effects and last-32 response-array deduplication. Kind=admin-menu responses use the bounded GZip/SHA-256 UB envelope but are routed separately from the public cache. No full JSON is uploaded for navigation/edits.

Q4 clients receive the shared public-only snapshot (even admins); saved full JSON is no longer pushed to new admin clients on saves. Legacy Q3 clients retain their 0.6.0 behavior. P4 known catalog fingerprints and GU unchanged acknowledgements suppress repeated full Formation/Admin/Buy/Ranks/Help lists; complete client catalogs persist only within nonce/team/role context. Poll timing/commander lock latency retained. Large identical snapshot compression is reusable. Server logs HUD transfer scheduled packet/body-byte counts and about-minute HUD upload interval accepted-body-byte totals. They exclude native/Steam framing/retransmissions, ordinary gameplay and Unit Balance's own override-sync traffic.

All recorded preservation hashes (gameplay DLLs, Unit Balance current/default JSON, Starter Units JSON, preferences and Ranks config) matched after installation. Formation source of truth remains Silica-1283/si_formation; it was not edited. AntiGrief recruitment ownership, direct harvester/Shrimp control, sidebar/authoritative lock and Centauri rendering repair retained. Major 1.0 extraction remains deferred. See current INSTALLATION_STATUS.md/README.md/NETWORKING.md and the 0.7.0 user live checklist in TESTING.md; old 0.6.0 batch-editor instructions are historical.
## Current update - 19 October 2026: HUD 0.6.0 / Unit Balance UI

This supersedes historical HUD installation details below. Client/companion 0.6.0 are installed and SHA-256 verified; exactly two HUD DLLs replaced. Receipt: Backups/SquadHUD-install-20261019T213232405Z/installation.json. Source checkpoint: Backups/UnitBalance-UI-20261019T211915672Z. Release: ClientMods/Si_SquadHUD/Releases/0.6.0. Both affected Release builds have zero warnings/errors. No tests, live JSON saves/reloads/switches or game/server launches/stops. User verification pending.

Unit Balance UI is implemented without editing/replacing its installed 7.7.1 DLL or source. Its reviewed hash is 23e5f58f3337e081b784d4d940f46ec81451c630c900cc3610fac11f68609c66. The provided Silica-1283/Silica-UnitBalance-balance-beta-2026-04-06 source is older (7.0.0 attribute / 7.2.0 init text; missing newer rules). Do not build/install it as a replacement. Starter Units integration remains deferred.

New code: Server/UnitBalanceBackend.cs, Client/UnitBalancePanel.cs, Shared/BalanceProtocol.cs. Adapter reads installed parameter groups/captured vanilla cache and sanitizes current/default JSON. UNITBALANCE is a read-only category/tree/parameter browser for all players; admins separately open CONFIGURE from its Admin row, using the same layout. The public tab reads published configuration even for admins. Configure reads separately supplied saved JSON. Values are configured overrides/baselines, not measured live-object telemetry. No invented baseline values; unknowns show Default/unavailable.

SAVE JSON stages up to 256 allowed numeric/boolean/removal edits, requires matching complete batch count and revision, then atomically replaces the existing JObject while preserving other/private fields. Backups: UserData/UnitBalance_cfg/SquadHUD_Backups/Si_UnitBalance_Config-<UTC>-<id>.json. Original and replacement SHA-256 are verified. Reload is separate and confirmed. It invokes existing OnRebalanceCommand; a read-only Harmony postfix also observes native chat reloads/default restores. Confirmed reloads publish a new sanitized snapshot to all subscribed updated clients. Native configured Discord auto-post behavior is retained on reload; viewing/saving does not post.

Q3 subscriptions retain Q/Q2 compatibility. UM sends authoritative visibility/admin state independently of menu selection. UB is a complete GZip/SHA-256-bounded JSON transfer (2 MiB expanded, <=1024 chunks of <=1050 bytes); clients cache it in memory by connection context and retry incomplete transfers. UC/UR stage/save/reload/refresh requests use authenticated native peer/channel/team/role, Generic permission revalidation, opaque config+reload revisions, bounded payloads, main-thread effects and request-ID deduplication. Raw private settings/webhooks never go to clients. Client field edits are cleared with a message when revisions change; no stale ID rebasing.

Disabling Formation/KoH/Unit Balance hides its related tab on 0.6.0 clients, restored on enable. Admin Configure remains accessible when Unit Balance is off. IMPORTANT native limitation: several independent Unit Balance rule hooks do not consult its enabled flag. The switch follows native enabled=false/reload semantics; it does not unload hooks or guarantee fully vanilla live units/rules. This limitation is visible in the Admin row and documented. Do not claim full cleanup or silently add gameplay patches. Unsupported mod switches remain unsupported.

All 54 preservation hashes (other mod DLLs, Unit Balance current/default JSON/source, Ranks/preferences and Diagnostics) matched after installation. No balance settings, AntiGrief/Ranks ownership settings, gameplay algorithms or 1.0 extraction changed. Existing commander sidebar/lock and Centauri GUI.Box repair retained. Follow current INSTALLATION_STATUS.md and the 0.6.0 TESTING.md checklist; compilation/install verification is not runtime confirmation.
## Current update - 19 October 2026: HUD 0.5.0

This update supersedes the historical HUD 0.4.2 installation details below. Client and companion **0.5.0** are installed and SHA-256 verified; Formation **2.15.1**, Ranks **1.4.3**, Diagnostics **0.1.0** and settings were retained. Both affected HUD Release projects compiled with zero warnings/errors. No tests or game/server start/stop actions. Live verification remains user-owned and pending.

Commander Formations has a Sidebar button: narrow draggable symbol list, default middle left, with independent per-user sidebar-<SteamID64>.json placement. Native N, compact/large faction shapes, thin/thick lines, open outlines and padlock all invoke existing server-validated actions. Alien circles are filled except Open circle. MENU/F6 returns to full menu; X closes it; input outside the strip remains available. CommanderSidebar.cs owns rendering, procedural GUI.Box icon backgrounds, pointer capture and placement. Existing role/team/round context closes it.

Header now says Mod menu without faction prefix, and the UI is hidden outside loaded dedicated-server matches. Help/Ranks tables use faction colors and thinner dividers; Help's Mod column uses callback assembly provenance plus explicit reviewed Formation/Ranks entries. Formation polling preserves visual enabled state during background requests/cooldown while still ignoring clicks until ready; the previous once-per-second enabled/disabled shading caused the reported flicker. Existing complete-snapshot assembly, authentication and lock state remain.

Source checkpoint: Backups/Commander-sidebar-20261019T193500091Z. Installed exactly two DLLs; unchanged Formation is skipped by the installer. Receipt: Backups/SquadHUD-install-20261019T193739192Z/installation.json. Release: ClientMods/Si_SquadHUD/Releases/0.5.0. See current INSTALLATION_STATUS.md and the added user live checklist in TESTING.md. No gameplay algorithm, AntiGrief/Ranks setting, pinned adapter hash or 1.0 extraction changes.

Recorded 19 October 2026 (Europe/Brussels). This is a consolidated handoff, not a claim that untested releases work in game. Recheck installed files and applicable AGENTS.md before editing. The most recent user request was completed: add commander /formation lock to the graphical commander Formation list, including status and a two-second tooltip.

## Objective and scope

Build a reusable local reference library for both Silica client and dedicated-server modding, then develop useful client interfaces backed by authoritative server mods. The user originally worked mainly on server mods and explicitly expanded scope to client mods. Prepare thoroughly for the upcoming Silica 1.0 release, but defer the major extraction/reverse-engineering effort until that release. Do not treat 1.0 extraction as already completed or authorized to run now.

Game-reference inspection, client diagnostics and a tabbed faction mod menu are now implemented. Preserve captured game inputs, manifests, older releases and rollback backups. Routine source/release ZIPs exclude game binaries/reference assemblies.

## Paths and environment

- Workspace/dedicated server: `D:\SteamLibrary\steamapps\common\Silica Dedicated Server`
- Client game: `C:\Program Files (x86)\Steam\steamapps\common\Silica`
- Shell: PowerShell. Client Program Files installation generally requires elevated filesystem access; workspace is writable.
- Local tools: workspace `.tools`, including Il2CppDumper and `ilspycmd.exe`.
- Reference library: workspace `GameReference`.
- Client diagnostics source: `ClientMods\Si_ClientDiagnostics`.
- Main client feature and companion: `ClientMods\Si_SquadHUD` (the project name remains SquadHUD despite the broader mod menu).
- Authoritative Formation source: `Silica-1283\si_formation`.
- Ranks source: `Silica-1283\si_ranks`.
- Logistics source: `Silica-1283\si_logistics`.
- Installed server DLLs: workspace `Mods`; client DLLs: client `Mods`.
- Server preferences: `UserData\MelonPreferences.cfg`; Ranks configuration: `UserData\Ranks_cfg\Si_Ranks_Config.json`; KoH configuration: `UserData\KingOfTheHill_cfg\Si_KingOfTheHill_Config.json`.

Multiple other source/archive folders exist. Do not switch to another Formation copy based on timestamps. That folder's AGENTS.md explicitly identifies it as authoritative.

## Last installed versions

| Component | Version | Location |
| --- | --- | --- |
| Client SquadHUD/mod menu | 0.4.2 | Client Mods/Si_SquadHUD.dll |
| Dedicated-server companion | 0.4.2 | Server Mods/Si_SquadHUD_Server.dll |
| Formation | 2.15.1 | Server Mods/Si_Formation.dll |
| Ranks | 1.4.3 | Server Mods/Si_Ranks.dll; retained during recent changes |
| Client Diagnostics | 0.1.0 | Client Mods/Si_ClientDiagnostics.dll |

Last observed client: Silica Beta 0.9.47, Unity 6000.0.81f1, IL2CPP x64, MelonLoader 0.7.3-ci.2446 with net6 runtime. Server is managed Mono, not IL2CPP. Current recorded reference builds: client Steam build 25179420, server build 25179400. Verify hashes/build IDs again after any update.

Latest install replaced exactly three DLLs: HUD client, companion and Formation. All were SHA-256 verified. No settings, Ranks DLL, or other gameplay-mod DLLs were changed by that install. Neither game nor server was launched/stopped by the task.

## Reference-library work and what the tools actually reveal

Start with GameReference/README.md, STATUS.md, shared/RELEASE_DAY.md and applicable GameReference/AGENTS.md. Some STATUS.md entries are historical; current menu status is in ClientMods/Si_SquadHUD/INSTALLATION_STATUS.md.

Preserved client snapshot:
`GameReference/client/build-25179420-20261019T124741058Z`

Preserved server snapshot:
`GameReference/server/build-25179400-20261019T124743376Z`

Each has manifests and trial runs. Client smoke run `smoke-20261019T124800914Z` generated metadata declarations/dump.cs (~40 MB); server smoke `smoke-20261019T124809853Z` exported managed samples including Player. Initial trial runs took about 2.84s and 6.70s respectively. These are actual small-run timings, not estimates for full future extraction. Input archives are under GameReference/archives; originals/results/hashes are retained.

Client inputs are GameAssembly.dll plus Silica_Data/il2cpp_data/Metadata/global-metadata.dat (metadata version 31 in this capture). IL2CPP metadata dumps expose declarations, names and structure, not complete reconstructed C# implementations. Native implementation analysis/full asset extraction remains deferred. Managed server assemblies, especially Silica_Data/Managed/SilicaCore.dll, can be decompiled for logic. Generated client interop references under client MelonLoader/Il2CppAssemblies are used to compile actual client hooks.

Sources of knowledge in this project are local mod sources/configuration, targeted managed decompilation, generated client bindings, preserved metadata, logs and user live tests. Do not claim full native source is readable from a metadata dump. Old `.inspection` exports can be research leads with uncertain provenance; verify current binaries when an API detail matters.

## Verified user observations and earlier fixes

The user live-tested Diagnostics 0.1.0 successfully: READY, upper-left panel, F8 toggle and F9 snapshots; recruitment/dismissal, movement and ordinary input worked. A recorded session has twelve snapshots without diagnostic errors.

Observed in those tests:

- Native FPS group limit 9 includes the player: one controlled player unit plus eight AI.
- Menu Redeploy retained the squad in the observed test; player death dismissed it.
- Commander without a controlled unit has no meaningful FPS squad count; show inactive/n/a.
- A Ranks harvester-control/commander handoff bug was corrected, and the user confirmed that correction worked.
- Direct FPS control of harvesters and Shrimp should remain allowed under game rules. They should not be recruitable/orderable as FPS AI followers.

The user explicitly assigned recruitment restrictions to AntiGrief, not Ranks. Current recorded settings:

- Ranks JSON: EnforceHarvesterRecruitmentBan=false.
- AntiGrief preferences: Grief_BlockShrimpTakeOver=false; Grief_BlockShrimpFPSCommanding=true; Grief_BlockHarvesterFPSCommanding=true.
- [SAM] denial messages were from AntiGrief. Do not reintroduce the duplicate Ranks recruitment restriction.

Formation 2.14.2 fixed custom FPS Follow for one or two AI followers; the old <=2 cutoff must not return there. Normal Move/Attack/commander small-selection behavior was retained. Explicit commander snapshot locks still require at least three eligible ground AI units.

## Client menu: current behavior and user preferences

The user prefers mouse controls and no new gameplay-action shortcuts because of vanilla key conflicts. Keep existing keys:

- F6: collapsed header -> expanded status -> full menu. F6/Escape/Close closes the full menu back to collapsed.
- F7: HUD status snapshot.
- Diagnostics F8/F9: separate diagnostic toggle/snapshot.

Collapsed header says Sol/Centauri/Alien mod menu (F6), fixed top-right at 96px below the top edge to clear native controls. The full menu is draggable by its title bar and saves normalized placement per Steam user in client UserData/Si_SquadHUD/window-<SteamID64>.json. Keep it on-screen after resolution changes. The status header itself remains fixed.

All menus use continuous scrollable lists, not visible pages/Next/Previous. Root tabs are distinct from option rows. Fonts default to 18px, configurable FontSize 16-28 in client MelonPreferences.cfg/Si_SquadHUD. An earlier default-20 -> 18 migration is tracked by StyleRevision. Use brighter text with good contrast.

Styling: native-inspired custom Unity GUI, not actual vanilla menu prefabs/font assets. SOL is neutral slate, Centauri burgundy with subtle procedural scan lines, Alien olive with rounded panels; human panels are beveled. Earlier screenshots were amber-lit, and the user supplied corrected neutral-light references. Avoid reintroducing amber tint into SOL. Do not mutate shared vanilla assets/styles.

Hover descriptions appear after more than two seconds, including disabled controls. Ordinary formation-layout choices intentionally have no descriptions yet: visual examples are a later task. Special lock controls do have descriptions.

Full menu includes score, personal credits, FPS squad count/effective allowance and AI follower count. Counts include the player; commanders show FPS squad inactive. Unknown/stale data must be labelled unavailable rather than invented zero.

Tabs:

1. Formations: FPS Follow/Move/Attack or commander layouts; existing native option, lock and confirmed AI dismissal. Native option 1 disables all personal FPS formations; selecting a custom layout re-enables retained function selections. Do not change this established command behavior silently.
2. Buy: named Buy, not KoH Buy. Choose production facility, then all faction units from that facility with buy buttons in a scrollable list. Buying requires a separate confirmation; category selection/cancel/refresh spends nothing. Structures are outside this unit shop.
3. Ranks: non-clickable table of this player's faction only. Tier, effective score requirement, title, FPS cap, reward bonus, permitted categories/exclusions and seats. Include the faction's traitor title/rules.
4. Help: non-clickable Command/Who/Explanation tables under FPS Player, Commander and Admin. Registered server mod commands plus reviewed variants. Exclude numeric/back/close navigation entries such as /2 and /back. Missing descriptions are explicitly labelled. This is not an exhaustive native console/cvar reference. Exact slash /demote is Ranks; !demote is Commander Management.
5. Admin: visible to AdminMod Generic-power admins. Lists every loaded server mod. Reviewed live switches exist for Formation, Logistics, KoH, Crab Cannon and Heavy Teleporter. Other mods show Loaded / No live switch. Do not fake enabled state or generically unload mods with active hooks. Live controls call the mod's own handler/API with its cleanup/save behavior and server permission validation.

## Formation locks: distinguish FPS from commander

FPS control was renamed from Facing Lock to Locked Formation. Requested explanation: units try to keep formation even while moving, and movement will be slow. It retains the existing FPS lock/speed-coordination behavior; no new movement algorithm was introduced. Dismiss AI requires two clicks and protects player-controlled units.

Latest feature (0.4.2 / Formation 2.15.1): commander Formation list includes **Lock formation / Locked or Unlocked**, alongside square/line/etc choices.

- Unlocked -> click invokes existing /formation lock, arming the next commander snapshot move.
- Locked -> click invokes /formation unlock, releasing owned locked groups and cancelling a pending action.
- Locked means Options(player).Pending==1 OR a same-team owned LockedGroup with members remains active.
- Unlocked follows arrival/release/cancellation when neither condition remains.
- This is a one-move snapshot command, not a permanent global lock mode. Existing minimum-three eligible ground AI and native order/ownership checks remain.
- Commander menu state refreshes approximately once per second while open.
- Tooltip starts with the user's exact requested wording: "units attempt to stay in formation trough travel, formation will move slow." It also clarifies next-move/unlock scope.

Implementation: Formation GetPlayerPanelPage advertises commanderLock=true and returns commander lock state in locked. RequestPlayerPanelAction accepts lock option 1/0 in commander scope and calls existing LockCommand. Companion adds a distinct group=commander-lock UI row with an unused display-row number. Client sends lock action with desired boolean, not a formation-select action for that display number. No catalog option number is reserved.

## Score, credits, rewards and rank semantics

Authoritative display score comes from server Player.Score, with local client score fallback. Credits use optional Si_KingOfTheHill.KingOfTheHill.GetCreditsBySteamId(long), only when ModeBuyActive is true. Logistics shares that wallet. Absent/inactive/stale wallet displays Unavailable. Faction treasury is separate.

Buy reconstructs server-side choices from KoH's current faction/catalog/prices, checks active round/mode, admin-only and balance-editor gates, then invokes the existing TryPurchase handler. Existing funds, cooldown, Shrimp unlock, prefab, proximity/spawn rules remain authoritative. Native KoH chat explains success/refusal. Do not implement a duplicate wallet or spawn path.

Rank thresholds use effective rank score, recovery/penalties and limited seats, not a simple raw-score-to-title mapping. Rank title/bonus remain while commanding, but FPS squad recruitment is inactive. Third counted teamkill enters the faction's hidden tier: Traitor (Sol), Deserter (Centauri), Outcast (Alien). Counted strikes obey configured cooldown; first/second strikes demote one/two tiers. Traitor promotions are locked until next round; no AI recruits, combat/structure reward restrictions and configured other-reward penalty apply. The table reads the loaded RankEngine.Config, not a copied client config.

A follow-up compatibility concern was verified while preparing this handoff, without modifying Ranks: Rank source RewardPatches.cs pins LogisticsHash to C5799FD0CDB2D1B5330CD165FDC5F5088118565E4D51712A06B17C586BFF3329, while installed Mods/Si_Logistics.dll hashes to 9BA61FE9ACF1E9A34C6773BC9DE9C4D0B7E23CE44C1E99E48D34F27741EFE20E. That source explicitly disables its Logistics reward patch on a hash mismatch. Review the installed Ranks binary/status before assuming its rank bonus still applies to current Logistics rewards. The HUD/admin adapter's reviewed Logistics hash is the newer 9BA... hash. Do not simply change a security/provenance hash without reviewing changed reward sites. This concern was not fixed in this chat.

## Critical rendering incident and repair

0.4.0 introduced a disappearing-menu bug. User associated it with an AFK kick and reconnect/map/round transitions. Client logs showed NotSupportedException: Method unstripping failed when Centauri was selected, while authoritative server updates and F7 snapshots continued.

Confirmed cause: generated UnityEngine.GUI.DrawTextureWithTexCoords(Rect,Texture,Rect,bool) is a throwing stub in the installed client binding. GUI.DrawTexture overloads also ultimately reach a throwing stub. Compiling against exposed IL2CPP signatures does NOT prove those methods work.

The Centauri decoration call triggered OnGUI's drawingEnabled=false path. That old handler never recovered for the rest of the client session, including across scenes/reconnects. Server restarts could not fix it.

Client 0.4.1 repaired this; repair is retained in 0.4.2:

- Use GUI.Box with an owned GUIStyle/background, the native path already used by working surfaces.
- Decorative failure disables only decoration.
- Other drawing errors retry after two seconds; scene changes and F6 can recover drawing.
- Error logs include full exception stack traces instead of just type/message.
- Related drag-control and scroll entry points were inspected and resolve to native methods.

Incident evidence/source checkpoint: Backups/Client-render-fix-20261019T184628772Z. Relevant client errors were at 18:33:19 and 18:39:13 UTC on 19 October. No user-confirmed post-repair runtime result has been reported yet. Do not claim the repair or latest commander feature has passed live tests merely because builds pass.

## Architecture and implementation map

ClientMods/Si_SquadHUD:

- Client/SquadHud.cs: lifecycle, status polling, packet reception, logging, render recovery.
- Client/FormationPanel.cs: menu/input focus, actions, complete-snapshot assembly, lifecycle clearing.
- Client/FactionAppearance.cs: styles, GUI, lists/tables/facilities, tooltips and commander-lock row.
- Client/WindowPlacement.cs: per-Steam-user normalized position, drag capture/release.
- Server/ServerHud.cs: authenticated subscriptions, queued main-thread actions, status and replies.
- Server/PanelBackend.cs: optional Formation API bridge, full catalog assembly, commander row.
- Server/MenuCatalog.cs: Buy, Ranks, Help and Admin catalogs/actions.
- Server/AdminControls.cs: loaded-mod inventory and reviewed live-switch adapters.
- Server/HudProviders.cs: score/credits/rank/cap/recruitment status.
- Shared/HudProtocol.cs, FormationProtocol.cs, CatalogProtocol.cs: bounded parsing and snapshot framing.
- README.md, NETWORKING.md, TESTING.md, INSTALLATION_STATUS.md: main operational docs.

Formation additions are primarily ClientPanelApi.cs, reusing existing handlers in CommanderOptions.cs, CommanderLocks.cs, FormationMenu.cs and FpsMoveTask.cs. Main-thread APIs: GetPlayerPanelPage(ulong,string,int), RequestPlayerPanelAction(ulong,string,int,string,string). Connected-player, role, revision and service checks remain in Formation as well as the companion.

Network bridge uses native packet byte 126 and exact SiSquadHUD/1 marker over the existing authenticated game connection, not chat/HTTP/new ports. Refuses activation if native enum claims 126. Resolve actors from authenticated remote peer/channel; never trust client-supplied account IDs, rank, credits or privileges. Effectful actions are queued to OnUpdate after packet dispatch unwinds, then revalidated. Cache last 32 action IDs/results to avoid duplicate purchases/switches.

Status: Q/Q2 requests, S/S2 replies; nonce/team/sequence/lifecycle scoping and expiry. Full-list requests use internal page sentinel 2048. G snapshots are split into bounded chunks (<=1800 body characters, <=2100 packet bytes), not UI pages. At most 512 rows/chunks; <=6 rows per chunk, packed adaptively. Client assembles all matching chunks before making rows actionable. Scene/player/server/team/role/round changes invalidate caches and pending confirmations. Generic-admin flag is presentation only; server rechecks authority for each action.

Input: use native UI map/menu-focus guards while full menu is open; suppress underlying aim/fire/camera/orders; restore cursor/control state on close, native menus/chat, alt-tab and context changes. Game continues running. Preserve player-controlled exclusions, ordinary manual-order precedence and scoped cleanup.

Private KoH/admin adapters are pinned to reviewed DLL hashes; changed DLLs require review. KoH SHA256: 5377B71A034E065DF76D5A3F595DE733DAF24D9A875DEF9D0C092BCCAAABF8BC. Full adapter hashes are in AdminControls.cs and release manifests. Never replace pinned hashes blindly after updates.

## Builds, installation and rollback

Read applicable AGENTS.md. Formation rules: no tests added/run, no game/server start/stop unless explicitly requested; user owns live verification. Compile after source changes and stop after a successful appropriate build. Do not keep broadening/repeating checks without a new reason. Do not run old fixtures as proof of current behavior.

From workspace:

```powershell
dotnet build .\ClientMods\Si_SquadHUD\Client\Si_SquadHUD.csproj -c Release --ignore-failed-sources
dotnet build .\ClientMods\Si_SquadHUD\Server\Si_SquadHUD_Server.csproj -c Release --ignore-failed-sources
```

From Silica-1283/si_formation:

```powershell
dotnet build Si_Formation.csproj -c Release --ignore-failed-sources "-p:ServerRoot=D:\SteamLibrary\steamapps\common\Silica Dedicated Server"
```

Prepare-Release.ps1 stages current DLLs/manifests. Install-SquadHUD.ps1 supports -PlanOnly and -Side client/server/both, checks game/reference and staged hashes, refuses affected running processes, backs up existing DLLs and verifies copies. Do not kill user processes to deploy. Client Program Files writes need appropriate permission. Source/checkpoint and installation backups are distinct: preserve installation receipts and binaries for rollback.

Latest release:

- ClientMods/Si_SquadHUD/Releases/0.4.2/manifest.json and NOTES.md.
- ClientMods/Si_SquadHUD/Si_SquadHUD-0.4.2-preparation.zip and .sha256.
- Source checkpoint: Backups/Commander-menu-lock-20261019T185512405Z.
- Installed backup receipt: Backups/SquadHUD-install-20261019T185657151Z/installation.json.

Rollback with affected programs closed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\ClientMods\Si_SquadHUD\Rollback-SquadHUD.ps1 -InstallRecord '.\Backups\SquadHUD-install-20261019T185657151Z\installation.json'
```

Earlier archives/backups remain intact. 0.4.1 repair receipt: Backups/SquadHUD-install-20261019T184756100Z/installation.json. 0.4.0 receipt: Backups/SquadHUD-install-20261019T180810538Z/installation.json. The dynamic Apply-ClientRenderFix.ps1 helper installs the current staged client only; use the full installer for features requiring server/Formation updates.

## Logs and next work

Read logs directly; the user need not locate/upload them:

- Client: C:\Program Files (x86)\Steam\steamapps\common\Silica\MelonLoader\Latest.log.
- HUD sessions: client UserData/Si_SquadHUD/session-<UTC timestamp>.log.
- Diagnostics: client UserData/Si_ClientDiagnostics/session-<timestamp>.log.
- Server: workspace MelonLoader/Latest.log.

Preserve failing sessions before replacement/restarts. F7 records HUD status; F9 records diagnostics. Note faction, role, action, resolution and local time when reproducing. Full stack traces are now available. Private IL2CPP bindings may compile yet throw; inspect actual generated method bodies for Method unstripping failed before using new GUI helpers.

Immediate next checks are USER LIVE TESTING, not automatic execution:

1. Confirm client/companion 0.4.2 and Formation 2.15.1 load.
2. Join Centauri, spawn/start round and reconnect/change map: header/menu should survive the prior render trigger. F6 twice opens full menu.
3. Commander lock row: two-second tooltip, arm -> move with >=3 eligible ground AI -> Locked while pending/travelling -> Unlocked after completion; click again to cancel/release. Check team/role/round scoping and another commander's ownership isolation.
4. Follow TESTING.md for full lists, Buy outcomes, drag recall, input focus and admin switches. Restore intended settings if the user chooses to exercise switches in a spare test round.
5. Review the recorded Ranks/Logistics reward-adapter hash mismatch if reward integration is the next requested topic.
6. Defer full 1.0 extraction until requested after release. Then capture new client/server separately, preserve old snapshots, regenerate references and review signatures/adapters before deploying old code.

Older Diagnostics and initial HUD behavior were user-tested. Latest 0.4.2 changes have compilation/install verification only; no live confirmation has been reported for the current render repair/commander feature. Historical isolated test counts do not validate the current protocol, IL2CPP GUI, gameplay or networking.
