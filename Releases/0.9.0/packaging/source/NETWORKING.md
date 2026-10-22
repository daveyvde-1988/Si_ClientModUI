# Standalone 0.9.0 communication

There is no HUD wire protocol in the client build. No files from the former Shared or Server projects are compiled. The only outgoing entry point is Player.CurrentPlayer.SendChatMessage(command, false), invoked directly from explicit command/SEND clicks. No lifecycle, OnUpdate, OnGUI layout, tab, status or sidebar maintenance path sends requests. There are no retry queues or automatic refreshes.

The native authenticated connection supplies server chat. ProcessMessage peeks at native ChatMessage packets, requires the connected server's remote Steam ID, bounds packet size, checks the native server-message and team flags, and always restores reader position. Player messages and other native packets retain their normal paths. Malformed data falls back to native handling. Server messages are copied into feedback; enabling notifications diverts recognized native server-message deliveries as before, without a companion lease. Singleplayer server-message logging is observed locally.

Feedback parsing never executes commands. It can add inert command rows and recognize exact status strings. Numeric menu rows are scoped to a recent explicit request and expire; native mod handlers retain numeric menu ownership, authorization and state. Sending any new UI command clears the old numeric choices. Native manually typed command input also invalidates them. No optimistic server success/state is inferred.

Known client metadata: GameReference/client/build-25179420-20261019T124741058Z, smoke-20261019T124800914Z. Managed game counterpart and the current client interop declarations provide the native chat signatures; this is source/API evidence, not runtime verification. Installed mod command extraction is recorded in Catalog/installed-command-inputs.json; decompiled command evidence is in inspection/standalone-current. The newer installed Unit Balance binary was inspected directly; the older April source was not rebuilt.

Server-private values unavailable to normal chat/local state remain unknown. No client filesystem read of this machine's dedicated-server settings is used at runtime. Observed values are session-only and age-labelled. Generic chat parsing cannot discover arbitrary remote code or authenticate a mod's identity beyond the server transport.

