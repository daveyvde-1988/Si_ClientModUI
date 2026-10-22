# Validation: 0.15.0

Build the client, optional server and compatible cheats provider with dotnet build -c Release --ignore-failed-sources against the installed client/server references. All three currently compile with zero warnings/errors.

Offline results: 185 protocol/catalog/client lifecycle checks, 189 existing command/UI policy checks, and 17 installer/rollback fixture checks passed. Eleven of the fifteen original client source files are byte-for-byte unchanged, including settings, input guards, sidebar, persistence, provider API, and overlay windows. The four edited files contain lifecycle/command hooks and the discovery text in existing UI surfaces.

Offline checks:

```powershell
dotnet run --project ClientMods/Si_ClientModUI/tests/NetworkChecks.csproj -c Release
dotnet run --project ClientMods/cheats/tests/CommandChecks.csproj -c Release
```

Network checks exercise the shared protocol, bounded UTF-8/gzip transfers, atomic catalog diffs/cache, and production CompanionNetwork.cs against lightweight native/loader stand-ins. They cover singleplayer/no-server discovery behavior, peer/nonce authentication, reader restoration, command/reply correlation, menu/vote lifecycle, timeout without replay, cache confirmation and disconnect cleanup. They do not launch the game or validate Harmony interception, Unity GUI, Steam delivery or actual Admin Mod callbacks. The existing command/UI policy suite has 189 checks across three cultures.

Installer checks use temporary workspace fixtures, never the live game: plan, install renamed DLLs, retire old names, preserve settings, verify receipt/backup hashes, reject modified DLL rollback, and restore the old files.

Live checks still required:

- Singleplayer: use existing provider controls, preferences, cheats, free chat and sidebar commands; verify the local status and zero companion traffic.
- Vanilla multiplayer: wait about 13 seconds for not detected; verify native chat/unlisted commands and existing UI remain available. Recheck manually using the header indicator.
- Server with Si_ServerModUI plus Si_AdminMod: confirm detected, empty shell fills only from actual providers/registry, and Help/mod/Admin command lists respect the caller's powers. Test a non-admin, restricted admin, full admin and power revocation while connected.
- Run a safe registered command: one explicit SEND/Enter should produce one invocation and a correlated feedback reply. Test a menu with numbered options, Back/Close entries, free-text arguments and a supplied map/ban choice. Other player chat stays native. Check asynchronous/broadcast output remains native.
- Start a vote: verify its question, command options and removal on end/expiry. Commands and votes still require the existing prepare-then-SEND interaction.
- Switch role/team/round/server and reconnect/scene-load. Confirm current sidebar team scope, cleared old navigation, confirmed cache reuse, updated access and absence of stale replies. Simulate a missed reply: no automatic command retry or native fallback.
- Check F6 flow, overlays, loading/video/cinematic hiding, input capture/restoration, provider panels, debug controls, window placement, symbols, transparency and text scaling against 0.14.0.
- Restart and verify UserData/Si_ModMenu layouts, user/imported commands and created-commands.json remain intact. Server metadata must not be automatically exported.

No real game/server has been started, stopped or modified by these checks. Live gameplay and visual verification remain pending.


