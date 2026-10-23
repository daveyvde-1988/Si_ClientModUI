# ModUI compatibility analysis — 23 October 2026

Keep **Si_ClientModUI 0.17.3** and **Si_ServerModUI 2.0.0** as the baseline. Add compatibility to their transport/data handling and extend the existing PLAYER / ADMIN panel interiors. Replacing our client with Si_ModUI would replace the UI shell and lose features. Replacing our server wholesale would discard validation, permission filtering, restart recovery and Formation integration.

**No mod source, installed DLL, settings, game branch or gameplay behavior was changed for this analysis.** New files are this report and static inspection artifacts. No game/server was started, stopped or sent commands. No live compatibility or visual test was performed.

## Inputs and version findings

Inspected the actual DLLs in `ClientMods/default-branch_0.9.47_2026-10-23`, our active source, installed UI DLLs, release DLL metadata, and the retained friend's 21 October UI DLLs. Decompiled both friend's UI generations and our installed server. Inspected all 14 other packaged server DLLs for UI exports and game references. Full gameplay algorithm comparison of those 14 independent mods is outside this UI compatibility analysis.

| Component | Current installed version | New friend's package | Finding |
|---|---|---|---|
| Our client | Si_ClientModUI **0.17.3** | Si_ModUI **1.5.0** | Separate development lines; numbers are not directly comparable. |
| Our server | Si_ServerModUI **2.0.0** | Si_ModUI_Server **1.4.4** | Same underlying command-proxy design; different wire identity and extensions. |
| Retained older friend client | Si_ModUI **1.3.1** | 1.5.0 | Actual older DLL available and compared. |
| Retained older friend server | Si_ModUI_Server **1.3.1** | 1.4.4 | Actual older DLL available and compared. |

The friend's **client MelonInfo says 1.5.0, but AssemblyVersion/FileVersion still say 1.2.1.0**, just as his older 1.3.1 client did. The new server consistently reports 1.4.4.0 / MelonInfo 1.4.4; its older copy reported assembly 1.2.0.0 / MelonInfo 1.3.1. Identify builds by MelonInfo plus SHA-256, not assembly version alone.

Our installed client is byte-identical to the 0.17.3 release; our installed server is byte-identical to its 2.0.0 release:

| DLL | SHA-256 |
|---|---|
| Installed Si_ClientModUI.dll | `6204cbc10c1b72e7daaf3a62ad2053c04ec49ab30a7c0a360f9dc605062484a1` |
| Installed Si_ServerModUI.dll | `4341745f35b82b461c217a17b43452ee08ebf2ec50e63170356455992cfcf1a2` |

The historical release folders span 0.1.0, 0.1.1, 0.2.0, 0.3.0, 0.4.0–0.4.2, 0.5.0, 0.6.0, 0.7.0, 0.8.0–0.8.1, 0.9.0, 0.10.0–0.14.0, 0.15.0–0.15.1, 0.16.0, 0.17.0–0.17.1 and 0.17.3. Earlier integration notes identify the friend's original redesign as based on 0.8.1. That supports an older common starting point; it does **not** prove the exact commit ancestry of the new DLLs.

The relevant progression is:

- 0.1–0.9: specialized SquadHUD era; those adapters/layouts are historical.
- 0.10–0.14: generic client shell, local providers, custom commands, settings/sidebar/overlays and input protections.
- 0.15 / server 1.1: friend's registry/proxy transport integrated into our shell.
- 0.15.1: generic chat-menu parsing; 0.16: subsequent client improvements.
- 0.17 / server 2.0: structured exports, controls, catalogs, browse trees and personalized Formation commands integrated into our shell.
- 0.17.1 / 0.17.3: client refinements, descriptions, alphabetical navigation and saved-command deletion behavior.
- Friend 1.3.1 → client 1.5.0 / server 1.4.4: multi-select, tables, richer/admin catalogs, ordering and native-console fixes, detailed below.

The complete binary inventory, including older releases' actual embedded versions and hashes, is [inventory.json](../../.inspection/modui-default-20261023/inventory.json). A release can deliberately reuse an older unchanged side: e.g. the 0.1.1 bundle contains client 0.1.0, and 0.4.1 contains server 0.4.0.

## Game branch compatibility is already largely present

The **installed** `Silica_Data/Managed/SilicaCore.dll` contains `GameInfo.GAME_VERSION = Beta:0.9.47`. Our server is already built against this installation; this is not a migration from a 1.0-only server UI.

Read-only Mono.Cecil resolution found:

| DLL | Direct game member references checked | Unresolved/missing |
|---|---:|---:|
| Our installed server 2.0.0 | 30 | 0 |
| Friend server 1.4.4 | 30 | 0 |
| Our installed client 0.17.3 | 54 | 0 |
| Friend client 1.5.0 | 36 | 0 |

All **15** packaged server DLLs also resolved their direct SilicaCore member references against our installed 0.9.47 assembly. Client checks used the installed IL2CPP interop assemblies. This checks direct metadata references, not every reflected Harmony target, native implementation, dynamically loaded dependency or gameplay path. The package says its client was compiled against 1.0.4 interop and reused for default branch; its RTS AI still references a 1.0.4 pathfinding dependency. Those qualifications remain relevant to live testing.

Evidence and repeatable read-only check: [game-reference-check.json](../../.inspection/modui-default-20261023/game-reference-check.json), [check-game-references.ps1](../../.inspection/modui-default-20261023/check-game-references.ps1).

## Compatibility differences and required treatment

| Area | Our implementation | Friend's implementation | Needed change |
|---|---|---|---|
| Packet identity | Packet 126, `SiSquadHUD/2` | Packet 126, `SiModUI/2` | **Required for mixed pairs:** support both markers deliberately. Same packet number alone is insufficient. |
| Hello / command / chunks | `H`, `C`, `T`; nonce, sequence, base64, gzip | Same basic grammar | Reuse the transport; no second networking stack. |
| Frame schema | `v:2`, capabilities, `ui`, `role`, personalized commands | Legacy frame without explicit schema/capabilities; adds command `m` | Recognize legacy schema and normalize optional fields. Preserve our extensions. |
| Command `m` | Missing end-to-end | Multi-select switch, used by `/flex` | Add model, server/export parsing, validation and selected-set rendering. Click sends one selected token, not the entire set. |
| Tables | Player export merges values/commands, ignores tables; renderer ignores tables | Shared/per-player `tables:[{n,h,r}]` | Merge per-player tables, validate dimensions/text, render using existing panel styles. |
| Catalog audience | Catalog shown only in PLAYER | `admin:true` allows ADMIN catalogs | Route by declared audience and current authority; keep Spawn out of PLAYER. |
| Catalog factions | Current team only | `all:true` offers all factions/groups | Add an internal faction selector for Spawn, including wildlife/other groups. |
| Catalog labels | `[token,cost]`; token displayed | `[token,cost,label?]` | Display the label, send the exact token. Needed for prefab names. |
| Catalog state | Does not enforce `enabled` in catalog button creation | Disabled actions plus `offText` | Respect declared enabled state and show explanatory text inside the panel. |
| Catalog currency | No dedicated balance handling | `credits:false` hides currency; Buy can show balance | Honor currency metadata; no credits/0cr on Spawn. Optional explicit Buy status action can show confirmed balance. |
| Basic controls | Actions, switches, values, pickers, sliders, groups, defaults/vanilla | Actions, switches, values, pickers | Already substantially supported; retain ours. |
| Browse tree | Read-only sections/entries/groups with changes-only | Same basic tree | Already supported. Verify the new UnitBalance document fits bounds. |
| Map/mode picker | Client and map choice third element already supported; embedded defaults advertise `map,mode?` | Adds `!map` and `/nominate` seed upgrade to `map,mode?` | Update our seed upgrade too, so saved explicit `map` values can gain the optional stage. |
| Player/team picker | Sequential stages | Matrix for two-stage player/team choices | Optional convenience inside panel; not required for command compatibility. |
| Commanded state | Server-confirmed updates | Optimistic highlight for up to ten seconds | Retain our confirmation behavior; multi-select can show pending state distinctly if desired. |
| Refresh | Five-second change checks; post-command invalidate/push | Same, plus 60-second per-player export cache cleared after commands | Add bounded caching for expensive tables without making Formation choices stale. |
| Native console replies | Bounded proxy capture; uncaptured messages use AdminMod delivery | UTF-8 byte-based splitting before `NetworkSendConsole_Netcode` | Port the useful splitting fix while retaining our capture bounds. |
| Diagnostics | `!hudtest` | `!moduitest` | Optional alias for support instructions. |
| Extra server command | No bundled Slay implementation | Registers `!slay`, requires Slay power, kills controlled unit | Optional feature parity; not needed for UI interoperability. Prefer its own gameplay/admin owner if adopted. |
| Config | `UserData/cfg/Si_ServerModUI/menu.json` | `UserData/cfg/ModUI/menu.json`; migrates old SquadHUD folder | Keep ours. Offer selective metadata import, preserving existing overrides. |
| Client persistence | `UserData/Si_ModMenu`, local providers, portable definitions, per-user/team layout | Separate frame/layout/order storage | Keep ours; include wire dialect/schema in server cache identity. |
| Shell/order | HELP / COMMANDS / MODS / SETTINGS; alphabetical navigation | Separate Player/Admin/Info shell with draggable mod ordering | Keep our shell and alphabetical ordering. No compatibility dependency on his layout. |

The friend's client also automatically sends a silent `/<0>`-style close using the previous module's prefix when switching menus. Our parser uses advertised navigation and avoids guessed close commands. Keep our behavior; cross-mod menu ownership is not solved by copying his `AutoClose` method.

The native-console patch matters beyond GUI replies: the installed AdminMod chunks around 2400 bytes but does not split one oversized string. His patch budgets 1800 bytes including overhead and splits strings on UTF-8 character boundaries. It applies to uncaptured console output; it does not require changing UI visuals.

Our existing 2 MiB expanded-document limit differs from his 4 MiB. Other existing bounds include 512 KiB JSON text per module, 2048 commands/choices per module/row, 8192 aggregate catalog entries and JSON depth checks. Do not blindly remove bounds. Choose a documented common bounded envelope, check representative UnitBalance/Spawn/table payloads, and paginate or raise compatible limits where needed. The two markers ending `/2` are not evidence that all size/schema behavior matches.

## Specific functional issues to fix

**Elo multi-select.** His 0.6.2 exports `/flex` with `m:true` and personal values such as `sol,alien`. Our exact single-value comparison cannot highlight both; selecting an already active faction must remain possible to remove it. Retain the existing button appearance, compute membership, and submit `/flex sol` once. Confirm the returned personal value before presenting the change as authoritative.

**Elo duplicate text.** Its export declares `/elo balance` twice: a player status action and an admin ON/OFF switch. Our current merge promotes the existing row to admin and removes the player's status row; his merge rejects the conflicting admin decoration. Prefer distinct exported command texts: `/elo balance status` for the player action and `/elo balance` for the admin switch. The inspected handler treats non-on/off text as a status request, so `status` already works in this binary. This is a small exporter/metadata correction, not a gameplay rewrite. Alternatively introduce separate row IDs with distinct access metadata, which is a larger contract change.

**Leaderboard / AntiCheat tables.** Leaderboard 0.1.4 exports personal Commander, FPS and Activity tables. AntiCheat 0.3.0 exports running audits and report tables only after its own admin check. Our server currently discards `tables` from personalized exports, and our panel has no table renderer. Both ends need changes. Preserve recipient-only data, bound cells/rows/columns, recheck authority and hash the final filtered document. Refresh expensive/live tables explicitly or with bounded provider-aware caching; do not cause every subscriber to receive constantly changing report data.

**Spawn catalog.** Spawn 1.1.0 declares `admin`, `all`, `credits:false`, `offText`, friendly item labels and per-admin placement/team values. Our present renderer would place its catalog in PLAYER for an admin, omit it from ADMIN, use only the admin's own faction, and show raw prefab tokens with zero costs. Fix audience, faction selection, labels and enabled state together. Catalog actions must inherit the registered `!spawn` power (**Cheat**, not a guessed Generic power); keep the authoritative dispatcher. Filter admin catalog data server-side as well as hiding controls client-side. A catalog-level `admin:true` is not a declaration that every unrelated information field/table is public or private.

**Map seed migration.** Our embedded defaults already contain optional modes. `EnrichDefaults` only fills null Arg values, so an older saved `Arg:"map"` can remain stuck. His newer SeedArg adds both `!map` and `/nominate` with `map,mode?`, allowing the existing upgrade mechanism to handle this.

## Preserve the stronger parts of our branch

- Current shell, faction colors, transparency/scaling, geometry, feedback divider/history, input isolation, F6/F7, sidebar, local providers, standalone fallback, status/notifications, saved/imported commands and deletion markers.
- Atomic full/diff validation, root consistency, authenticated current-server packets, nonce/sequence checks and restart recovery without replaying actions.
- UTF-8 commands accepted up to the native 160-character limit. His parser still rejects an encoded command body over 512 characters; changing only our client's marker does not fix that restriction on his server.
- Outbox/capture bounds, dispatch-time validation and final permission/role filtering. His outbox and reply capture lack our corresponding caps.
- Registry-required powers cannot be lowered by export metadata. Personal exports must not reintroduce hidden commands.
- Our personalized Formation commands/revision checks, provider refresh handling and listing of installed modules without registered commands. His server personalizes values/tables only and removes empty command modules.
- Our hashing after final filtering/sanitization. His shared frame hashes modules before the later identity scrub.

Do not wholesale replace our Frame.cs, Proxy.cs, Protocol.cs or client controller with the friend's files.

## Compatibility targets and implementation order

Three different outcomes should be kept distinct:

| Pairing | Today | Required outcome |
|---|---|---|
| Our client + our server + his gameplay exports | Ordinary existing exports largely work; new features incomplete | Add multi-select, tables, catalog semantics, seed migration and exporter fixes. Marker can remain ours. |
| Our client + his unchanged server | Custom UI transport does not connect | Add bounded marker discovery/fallback on our client plus legacy schema adapter. His server's own limits/restart behavior remain unless he updates it. |
| His client + our server | Custom UI transport does not connect | Our server accepts both markers and replies using the subscriber's marker; preserve legacy fields. His client will ignore our extra presentation hints. |

Recommended implementation order:

1. Extend our common models/export helper with `m`, tables and catalog metadata. Retain existing fields and legacy defaults. Resolve `/elo balance` export duplication.
2. Extend our server's export merge/personalization/filtering; add native-console byte splitting and bounded table refresh/caching. Add the map/mode seed upgrade.
3. Add table/multi-select/admin-catalog rendering within the existing PLAYER / ADMIN content area, reusing current fonts, colors, buttons, scaling, scrolling and feedback area. Preserve browser/controls support.
4. For mixed UI pairs, implement dual-marker support. Server records marker per subscriber and uses it for every frame/reply/event. Client tries our dialect first and a bounded friend fallback; once authenticated, pin the dialect for that session. Separate cache identity and pending transfers; refresh catalogs on dialect changes. Never resend gameplay commands to probe compatibility.
5. Keep one server companion installed. A second companion is not a bridge and can duplicate patches/commands. Package a single updated client/server pair and clear build identity documentation; do not require gameplay DLL replacement just to gain transport compatibility.
6. Validate fixtures for both dialects, legacy/no-version frames, multi-select toggling, tables, admin catalog visibility/powers, malformed/oversized input, unchanged hashes, complete diffs and reconnects. Then verify the unchanged shell and new panel interiors in a controlled game session.

Primary files expected to change: server `Shared/Protocol.cs`, `Server/ServerMod.cs`, `Server/Frame.cs`, `Server/Proxy.cs`, `Server/MenuConfig.cs`, `Export/ModUI_Export.cs`; client `CompanionNetwork.cs`, `CompanionCatalog.cs`, `StructuredPanels.cs`, and server frame cache handling. Existing shell/appearance/layout/provider files need no redesign. Updating the export helper is for future provider builds; already-built provider DLLs do not start using a new helper automatically.

## Packaged gameplay versions and UI relevance

Every matching packaged gameplay DLL differs in SHA-256 from its installed counterpart, even when its displayed version is unchanged. Equal version numbers therefore do not mean equal builds or equal references.

| Mod | Installed here | Package | UI relevance / decision |
|---|---|---|---|
| AntiCheat | Absent | 0.3.0 | New admin controls and personal audit tables; optional independent gameplay addition. |
| Buy | 1.3.1 | 1.3.2 | Existing catalog/control schema mostly supported; verify disabled/currency behavior. |
| CrabCannon | 2.8.4 | 2.8.4 | Shared controls; recompiled/different binary. |
| EloMatchmaking | 0.4.4 | 0.6.2 | Multi-select `/flex`, new controls/reservations/tier settings; duplicate balance export needs correction. |
| HeavyTeleporter | 1.3.2 | 1.3.2 | Shared controls; different binary. |
| KingOfTheHill | 0.5.4 | 0.5.4 | Existing controls; different binary. KoH 0.5.x expects separate Buy for its former economy. |
| Leaderboard | 0.1.3 | 0.1.4 | Adds personal tables. |
| Logistics | **0.9.3** | **0.3.14** | Different, older code line. Preserve ours and its exports/features/configuration. |
| MapBalance | 3.2.0 | 3.3.0 | No ModUI export hook found; registry/metadata fallback. Package moves config folder. |
| PacketChunker | 1.1.0 | 1.2.0 | No ModUI export; config/dependency update is independent of UI. |
| RTS AI | 0.94.7 | 0.94.7 | Existing structured controls; different build; pathfinding dependency caveat remains. |
| Spawn | Absent | 1.1.0 | New admin/all-faction catalog; bare !spawn takeover can conflict with SpawnConfigs. |
| StarterUnits | 1.0.0 | 1.1.0 | No ModUI export; registry/metadata fallback; config move. |
| UnitBalance | 7.10.3 | 7.10.3 | Existing read-only browse schema, different build; retain chat editing as supported. |
| Friend UI server | Our Si_ServerModUI 2.0.0 | Si_ModUI_Server 1.4.4 | Alternative companion, not an additional gameplay dependency. |

Formation is **not included** in this package. Preserve our Formation provider/API. Si_AdminMod, Si_CommManagement and Si_BasicTeamBalance are also not supplied as replacements. The package's `fix_cfg_layout.ps1` moves multiple gameplay configuration locations; none of that is necessary to make our UI understand its exports. Do not run it as part of a UI-only update.

## Inspection artifacts

- [Binary inventory and SHA-256 values](../../.inspection/modui-default-20261023/inventory.json)
- [Static game-member reference results](../../.inspection/modui-default-20261023/game-reference-check.json)
- [Diff summary](../../.inspection/modui-default-20261023/diffs/summary.json)
- [Friend server changes since 1.3.1: Frame](../../.inspection/modui-default-20261023/diffs/friend-server-1.3.1-to-1.4.4-Frame.cs.diff), [Proxy](../../.inspection/modui-default-20261023/diffs/friend-server-1.3.1-to-1.4.4-Proxy.cs.diff), [ServerMod](../../.inspection/modui-default-20261023/diffs/friend-server-1.3.1-to-1.4.4-ServerMod.cs.diff), [MenuConfig](../../.inspection/modui-default-20261023/diffs/friend-server-1.3.1-to-1.4.4-MenuConfig.cs.diff)
- [Friend client changes since 1.3.1](../../.inspection/modui-default-20261023/diffs/friend-client-1.3.1-to-1.5.0-ClientMod.cs.diff)
- [Our installed server versus his: Frame](../../.inspection/modui-default-20261023/diffs/our-server-2.0.0-vs-friend-1.4.4-Frame.cs.diff), [Protocol](../../.inspection/modui-default-20261023/diffs/our-server-2.0.0-vs-friend-1.4.4-Protocol.cs.diff), [Proxy](../../.inspection/modui-default-20261023/diffs/our-server-2.0.0-vs-friend-1.4.4-Proxy.cs.diff), [ServerMod](../../.inspection/modui-default-20261023/diffs/our-server-2.0.0-vs-friend-1.4.4-ServerMod.cs.diff)

Diffs normalize the two namespaces only. Decompiler-generated variable names, casts, nullable annotations and unresolved dependency attribute formatting are not themselves behavior changes. Protocol/Chunk/Events in the friend's old and new server compare equal after namespace normalization; vote Events also compare equal between our server and his new server. No new vote transport is needed.
