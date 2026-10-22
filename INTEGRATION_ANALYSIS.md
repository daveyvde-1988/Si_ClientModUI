# Comparison and integration: 20 October 2026

The starting client was the active 0.14.0 source in `ClientMods/Si_SquadHUD/Client`, not any historical release. Its source and scripts were copied to `Backups/ModUI-integration-20261020T153019699Z` before edits. The active folder is now `ClientMods/Si_ClientModUI`; historical releases moved with it. The supplied `Si_SquadHUD_knecht/Si_ModUI` tree is unchanged.

| Area | Current 0.14.0 | Friend's redesign | Integrated result |
|---|---|---|---|
| UI | Current F6 flow, overlays, sidebar, team scopes, settings, provider panels, Debug, loading/cinematic guards | Replacement Player/Admin/Info/reply/vote UI based on 0.8.1 | Current UI retained; discovery indicator added to existing menu header/status |
| Data | Empty shell, local providers, user/imported commands, native feedback | Server Admin Mod registry catalog, per-player powers, module hashes/diffs | Server commands enter existing Help/mod/Admin lists only after authenticated discovery |
| Dispatch | Unrestricted explicit SEND/Enter/sidebar through native chat | Registered commands/phrases and navigation through a proxy; captured synchronous caller output | Confirmed server entries use proxy; other chat remains native; singleplayer uses existing path |
| Replies/navigation | Native feedback; only explicitly prefixed choices learned | Structured reply, numbered lines combined with server-supplied navigation prefix | Existing feedback area plus temporary choices in existing command lists; prepare then SEND preserved |
| Votes | No structured events | Push vote question/options/end | Existing Vote command tab, feedback and notifications; expires on end/duration |
| Network | No companion discovery | Packet 126, marker `SiSquadHUD/2`, nonce, reliable UTF-8/gzip chunks | Same wire format, bounded retries/assembly, authenticated peer checks, bounded gzip expansion |
| Availability | Singleplayer/multiplayer without companion | Companion required | Standalone unchanged; detected/checking/not detected/offline/transport unavailable displayed |
| Persistence | `UserData/Si_ModMenu`, per-user layouts/commands, portable export | Separate older layout/cache paths; cache keyed by sanitized server name | Existing preferences untouched; isolated catalog cache keyed by server Steam ID, local user and channel; cache is never proof of support |

Imported server files are `Frame.cs`, `MenuConfig.cs`, `Proxy.cs`, `Events.cs`, `ServerMod.cs` and shared `Protocol.cs`. The old `Client/Ui.cs` was not imported. Client transport was adapted into partial files beside the current UI, with separate catalog models.

Server behavior retained: registry attribution, permitted admin commands, sidecar labels/priorities/hidden commands/argument hints, match-control group, catalog diffs, authoritative native dispatcher, synchronous per-recipient reply capture, vote push, command queue/rate limit, global outbox token bucket and `!hudtest`. No mod-specific client catalog was reintroduced. Server-supplied map/ban choices become ordinary command entries. The current draft bar remains the argument editor; the old pickers and automatic menu-close commands were not imported because they change current UI/interaction behavior. Navigation commands are explicit user submissions.

Corrections made while porting: UTF-8 commands up to 160 characters no longer hit an incorrect 512-character encoded-packet cap; gzip expansion and transfer concurrency are bounded; frame merges validate before committing; stale nonce packets are dropped; frame cache distinguishes administrator status as well as powers; queued dispatch revalidates registration; capture/outbox memory is bounded; server restart recovery does not replay commands. Frames that change permissions clear old server navigation. No command automatically retries or falls back after network submission.

Assembly names are `Si_ClientModUI` (0.15.0) and `Si_ServerModUI` (1.1.0). Namespace `SiSquadHud`, class `SquadHud`, `ClientModControls` and all user-data paths stay stable. Source providers remain compatible. Binary providers referencing assembly `Si_SquadHUD` must be rebuilt; the separate cheats provider has been rebuilt as 0.3.1 and is supplied optionally in the client package. The server builds against the installed server/Admin Mod references, not the friend's bundled DLL copies.

The integration is built and checked offline. Native Harmony patch execution, Steam transport, gameplay and visual behavior still need the live checks in `TESTING.md`.

