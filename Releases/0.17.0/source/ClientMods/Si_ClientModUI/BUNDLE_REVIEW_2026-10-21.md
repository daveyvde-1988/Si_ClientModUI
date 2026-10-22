# ModUI bundle review — 21 October 2026

## Recommendation

Adopt the friend's **structured server exports and generic data-driven controls**, selectively ported into our existing **Si_ClientModUI / Si_ServerModUI**. Keep our current client interface, local provider API, settings, standalone behavior, input isolation and performance work. Do not install the entire bundle or replace our Logistics/Formation sources.

This was a source/configuration comparison and local metadata review. No gameplay source or installed DLL was changed; no build, gameplay command, test, game or server was run. The report itself is the only new file.

## Compared inputs

- Friend: `../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/` (called `bundle/` below).
- Our client: `Client/`, version **0.16.0**; server: `../Si_ServerModUI/`, version **1.1.0**.
- Our authoritative Logistics: `../../Silica-1283/si_logistics/`, version **0.9.0**.
- Our authoritative Formation: `../../Silica-1283/si_formation/`, version **2.15.1**.
- Local Steam manifests: server **2738040 / build 25179400 / public**, client **1494420 / build 25179420 / public**. Different client/server build IDs are expected and are not evidence of a mismatch.
- The bundle describes the friend's PB2/test deployment; its game reference DLLs are **not included**. Its README explicitly identifies test-branch references for Elo/Leaderboard.

## What changed

| Area | Our current implementation | Friend's bundle | Assessment |
| --- | --- | --- | --- |
| Identity | Si_ClientModUI / Si_ServerModUI; namespace SiSquadHud | Si_ModUI / Si_ModUI_Server; namespace SiModUI | Keep our assembly names/API and config paths. |
| Transport | Packet 126, marker `SiSquadHUD/2` | Packet 126, marker `SiModUI/2` | Not compatible unchanged; deliberate migration/negotiation required. |
| Discovery | Registered roots, phrases, permitted admin commands | Same, plus reflective exports and manifest subcommands | Main improvement: previously invisible subcommands become explicit. |
| Export | No generic server ModUIData reader | `public static string ModUIData()` | Mod describes itself without referencing a UI server DLL. |
| Per-player export | No generic equivalent | `ModUIPlayerData(ulong steamId)` supplies command values | Useful, but limited to existing rows' `v` fields. |
| Command model | Text, descriptions, aliases, simple argument hints/choices | Adds current value `v`, order `o`, richer argument chains | Extend our models/validators and render these in our existing panels. |
| Structured content | Generic chat menu parser | Status lines, Buy catalog grid, UnitBalance browse tree | Useful optional views; do not replace our shell. |
| Config | Labels, owners, categories, priorities, hints | Adds `Subs`, `Choices`, `AdminOnly`, chains | Merge supported entries selectively; do not overwrite settings. |
| Refresh | Five-second change checks | Also invalidates/rebuilds after each command | Faster switch confirmation; exporters must be cheap/deterministic. |
| Menus | Explicit advertised navigation | Assumes Back/Close and silently sends `/0` on exit/switch | Unsafe for menus without that contract, including our Formation. |
| Client | Local providers, overlays, sidebar/F7, team/role layout, dividers, standalone fallback, caches | Separate server-driven UI implementation | Use as a behavioral/schema reference, not a replacement. |

The small per-mod file is mostly a **JSON builder**, not a second networking stack. `ModUI_Export.cs` defines `ModUI.ModUIExport` and depends on Newtonsoft.Json. The server finds the exact-case hook `ModUIData()` by reflection on types in the owning mod assembly. The filename can differ, but `ModUIdata()` with different capitalization will not match. It also supports the historical hook name `SquadHudData`.

Supported builder operations include `Action`, `OnOff`, `OnOffAuto`, `Switch`, `Value`, `Picker`, `Info`, `Enabled`, `Catalog`, `Browse` and `PlayerValues`. Controls send ordinary chat commands; the gameplay handler must still implement and authorize them.

`ModUIData()` is not unrestricted automatic discovery: the server first finds mods through registered commands. An assembly with only an export and no registered root is not automatically discovered by this implementation. The code also removes entries with no commands even if they contain data.

## Bundle mod inventory

| Bundled mod | Declared mod version | Relevant export |
| --- | --- | --- |
| Si_Buy | 1.3.1 | Direct purchase catalog, give/bounty pickers, admin settings. |
| Si_KingOfTheHill | 0.5.4 | Settings, switches and status. |
| Si_Leaderboard | 0.1.3 | Faction choices, lists, admin switches. |
| Si_EloMatchmaking | 0.4.4 | Player flex value, commander seat/pin controls. |
| Si_HeavyTeleporter | 1.3.2 | Settings. Some positional/player command forms remain unexported per its framework notes. |
| Si_CrabCannon | 2.8.4 | Aim/settings/unit switches and weapon controls. |
| Si_Logistics | **0.3.14** | Export built around an old Logistics command/config schema. |
| Si_RTS_AI | 0.94.7 | Master/config/faction/logging settings. |
| Si_UnitBalance | 7.10.3 | Read-only balance browser; admin editing still uses the chat menu. |
| Si_Formation | **Absent** | Some manually described commands exist in menu.json; no Formation source/DLL/export is bundled. |

These are mod metadata/source versions, not proof of public-branch compatibility. Core client/server MelonInfo attributes say 1.3.1, while project/file versions remain 1.2.1 / 1.2.0. This is inconsistent version labeling, not by itself proof that the DLLs are stale. Before deployment, rebuild from the chosen sources and record exact hashes/dependency versions.

## Logistics: use our current 0.9.0

The bundled 0.3.14 source is materially older. It compiles the main source and export builder. Our 0.9.0 also includes HarvesterFix, AITransport/AITransportAllocation, LogisticsPerformance, LogisticsRewards, LogisticsScore and CommanderScore.

Replacing ours would lose harvester recovery, AI transport, newer delivery/pickup rewards, player/commander scoring, purchase exclusions, air-specific rates and newer cleanup/performance behavior. It would also introduce the friend's config migration from `UserData/Logistics_cfg` to `UserData/cfg/Logistics`; preserve our current path and existing files.

The old export cannot be pasted unchanged:

- It reads `Cfg.Commands.PlayerCommand`, which our current config does not have; ours explicitly registers `/logistics` and `/logi`.
- It sends `/logistics enabled on|off`; ours implements `/logistics on|off` and has no `enabled` subcommand.
- It sends `/logistics bonus on|off`; that command is absent from our current handler.
- It omits `/logistics harvesterfix on|off`, `/logistics aitransport on|off`, `/logistics transfershare`, score settings and commander-score settings.

Proposed implementation: copy/adapt the JSON builder as a separate compiled file, and add a partial-class `ModUIData()` implementation against **our fields and real commands**. Initially expose read-only status/help, harvesterfix, AI transport, share/transfershare, capacities, existing rate/radius/cooldown controls, score/commander-score controls, debug and explicit reload. Inventory exact validation ranges before creating each row.

For the master switch, either expose explicit ON/OFF actions using existing commands, or add a narrowly scoped `enabled` alias that delegates to the existing master handler/cleanup. Do not duplicate enable/disable logic. Do not invent setters for config-only fields: use read-only display until a validated handler is deliberately added.

Keep shared exports deterministic: configuration summaries only, not live cargo, scores, timers or delivery progress. Personal/live state should remain on-demand through status replies unless a separately designed per-player feature is needed. The per-player hook still participates in periodic checks and can generate traffic if its values constantly change.

## Formation: build a new export for our 2.15.1

There is no bundled Formation version to merge. Build against our current sources and actual commands. Existing relevant hooks include `FormationApi.GetPlayerPanelPage` and `RequestPlayerPanelAction`, with team/role checks and a revision check. They are server-side APIs, not something the network client should call unauthenticated.

Two important compatibility gaps:

1. Our chat menu supports `/back` and a toggling `/formation` root. `/0` is treated as an invalid number. `/close` and `/exit` are not explicit close aliases. Therefore the friend's unconditional auto-close cannot safely be imported.
2. `ModUIPlayerData(steamId)` currently personalizes **values only**, not team-specific command lists, choices or arbitrary data. It cannot directly provide a different selectable Formation catalog to each faction and FPS/commander role.

Start with shared status/help and appropriate admin settings, plus compatible on-demand chat menus. Keep runtime selections, commander locks, movement and native Follow logic intact. Add idempotent close aliases/common cleanup before enabling automatic menu cleanup. Preserve permission requirements: some Formation commands are commander-or-admin, and others admin-only; a single export `a` flag does not express all of those distinctions.

A full faction/role-specific formation picker is a second step: extend the bounded player-scoped schema or add a narrow on-demand request mapped to the existing authenticated/revision-checked APIs. Do not put every player's selections in shared `data` or bypass stale-list checks. Global `enabled=false` must also be interpreted carefully: ours allows selections while the global service is off, so blindly hiding the mod would reduce current functionality.

## Problems to fix or preserve during the port

### Transport and validation protections

The bundle is not a strict superset of our safety/reliability work:

- Its server outbox has no 8192-packet bound; our addon does.
- Its synchronous reply capture lacks our message/line/aggregate limits and native overflow fallback.
- It removes our session-restart recovery for an unknown subscription nonce.
- It removes our stale queued-nonce check and send-failure resynchronization. It advances known frame hashes before successful enqueueing.
- Our client validates a full diff before committing and checks the resulting root. The friend's ApplyFrame merges directly without equivalent validation. Preserve our transactional validation and exact reply matching.
- Its protocol raises the expanded document cap from 2 MiB to 4 MiB and omits some existing checks. Do not raise limits just because the bundle does; the documented ~80 KB UnitBalance export fits our current expanded cap.

There is also a concrete schema incompatibility beyond the marker: our catalog validator requires **two elements per choice**. The friend's map choices carry **three**, including game modes. Just changing the marker would still reject those catalogs. Our current models also ignore `data`, `v` and `o` and do not implement picker chains.

### Export privacy, hashing and permissions

The following code paths need correction before reuse:

- If exported commands parsing fails, `Frame.ModData(id, admin, mod)` returns the original document. That document can still contain admin-marked commands under `data.commands`. Omit malformed data rather than returning unfiltered raw JSON.
- `ScrubIdentities` runs **after** the initial mod hash calculation; hashes must describe the final transmitted document. Per-player values are applied after shared scrubbing, so sanitization also needs to cover personalization.
- Exported admin entries are filtered primarily by `IsAdmin`, with Generic used as a label for fresh entries. They do not automatically inherit each gameplay handler's precise required power. Keep handler authorization and model/filter exact powers where needed; document the visibility distinction.
- Player-provided personalization must not insert another player's data. Everything outside properly filtered commands in shared `data` is public to recipients. A Steam-ID regex is only a fallback, not a complete privacy policy.
- Provider lookup caches misses and MethodInfo objects. Define invalidation for provider/load/session changes rather than letting hot changes remain invisible.

The bundle's framework notes themselves say the schema still needs an explicit version field and list incomplete exports/live verification. Treat this as a useful evolving contract, not a frozen, fully verified release.

### Menus and performance

Keep our advertised Back/Close parser behavior until each server menu implements those routes. A client's remembered `openMenuMod` is not a shared server-side menu-owner coordinator. Closing with `/0` and then sending a selection does not prove the previous mod released ownership. This matters for Formation, Buy and UnitBalance together.

The friend's client is a separate implementation with a 0.25-second housekeeping interval and repeated layout preparation in drawing paths. It does not contain our complete local provider/sidebar/F7/settings/overlay framework. Preserve our one-second housekeeping, 20 Hz preparation caches, hidden-panel skipping and immediate IMGUI/input/replies. Port data rendering into our panels, including existing feedback dividers and debug traffic counters.

Shared stable data should be cached by configuration/catalog revision. Rebuild after explicit actions so controls show confirmed state; other subscribers can receive the normal change push. Avoid uptime/live counters in shared exports, which would create repeated full-mod diffs. Verify deterministic hashes with unchanged state.

## Beta branch: what is known and unknown

Confirmed locally: both installed Steam manifests select **public**. Confirmed from the bundle: friend's deployment uses the test branch; reference DLLs are missing, Elo/Leaderboard projects select branch-specific includes, and the core server expects `src/include/*.dll`. Client project also contains a different machine's loader path.

The JSON builder itself has no game dependency, so beta use does **not** make the export idea inherently incompatible. Much of the network code uses the same API shape as our existing implementation. For example, the added map-mode field is present in our captured public client metadata (`LevelInfo.GameModes`); its presence alone is not a beta-only API.

However, no clean public-branch compilation/runtime validation of this bundle was performed. Exact compatibility of the supplied DLLs and all Harmony signatures remains **unproven**. Reflection/Harmony targets can fail at runtime even if a project compiles, especially commander lottery, role synchronization, kill attribution and UI EventSystem hooks. Do not infer compatibility from matching names or version numbers, and do not update Steam branches to make the bundle fit.

## Recommended order before implementation/deployment

1. **Freeze baselines.** Back up our source, installed DLLs and user configs; hash the bundle and selected inputs. Record public client/server build IDs and relevant Admin Mod/loader versions. Keep the friend's bundle untouched.
2. **Freeze a small contract.** Define schema version/capabilities, exact field limits, command kinds, argument chains, precise permission visibility, unknown-field handling and player-scoped data. Keep our names/config paths. Decide explicitly whether to support the friend's marker alongside our current marker; support each negotiated client separately if needed.
3. **Port server features selectively.** Add export discovery, safe parsing/filtering, final-document hashing, manifest additions and post-command refresh to our server addon. Preserve our limits, recovery and queue handling. Use the current public Mono references; do not run both addons to emulate compatibility.
4. **Extend our client.** Add bounded models/validation for values, order, data and 2/3-element map choices; render actions/switches/values/pickers in current panels. Add optional catalog/browser views without replacing local providers, UI, settings or native-chat fallback.
5. **Add our mod exports.** Adapt Logistics 0.9.0 first. Add a new Formation 2.15.1 export; separately resolve menu close/ownership and player-specific catalog requirements. Modify only thin export/command aliases needed for integration.
6. **Compile against public dependencies.** Fix project paths/references and consistent version labels. Build changed projects separately. Rebuild other bundled gameplay mods only when their own updates are actually wanted; audit their gameplay/config differences independently.
7. **Verify offline, then staged live.** For UI/transport, check malformed/oversize documents, complete diff validation, privacy filtering, capabilities, restart recovery, queues, deterministic exports and stale selections using appropriate existing checks. Logistics/Formation instructions reserve their tests/live verification for the user; do not run or add their tests without authorization.
8. **Deploy only after review.** Install one server companion and one client shell with a rollback receipt. Avoid replacing every mod in one operation.

## Focused live checks after an approved implementation

- Public-branch client/server: native chat with no UI, client without addon, addon with ordinary clients, and singleplayer local providers.
- Non-admin, limited-power admin, full admin; changes to permission/team/role while a menu or picker is open.
- Logistics: status, harvesterfix, AI transport, scoring and rewards continue working; settings survive reload without path migration or lost keys.
- Formation: FPS/commander and all teams; native/default choices, locks and disband; Back/Close aliases; stale catalog/round revisions; switching with Buy/UnitBalance.
- Switch/value commands execute once and show confirmed values. Picker chains prompt correctly, including optional stages, duplicate player names and map modes.
- No recurring hash changes/traffic with unchanged exported state. Measure large UnitBalance frames, multiple subscribers and explicit-command refreshes.
- Disconnect/reconnect/server restart, late replies, queue pressure and command timeouts without automatic action replay.
- Current overlays, F7 release, input isolation, window/sidebar positions, dividers and local providers remain intact.

## Main evidence

- [Bundle overview](../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/README.md)
- [Friend's contract](../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/Si_ModUI/src/README.md)
- [Framework and open items](../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/Si_ModUI/src/docs/CLIENT_UI_FRAMEWORK.md)
- [Export builder](../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/Si_ModUI/src/Export/ModUI_Export.cs)
- [Friend's frame builder](../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/Si_ModUI/src/Server/Frame.cs)
- [Friend's server transport](../ModUI_bundle_for_Davey_2026-10-21/ModUI_bundle_2026-10-21/Si_ModUI/src/Server/ServerMod.cs)
- [Our catalog validation](Client/CompanionCatalog.cs), [transport](../Si_ServerModUI/Server/ServerMod.cs), [capture bounds](../Si_ServerModUI/Server/Proxy.cs)
- [Current Logistics handler](../../Silica-1283/si_logistics/Si_Logistics.cs)
- [Current Formation menu](../../Silica-1283/si_formation/FormationMenu.cs), [ownership checks](../../Silica-1283/si_formation/MenuIsolation.cs), [scoped panel API](../../Silica-1283/si_formation/ClientPanelApi.cs)
