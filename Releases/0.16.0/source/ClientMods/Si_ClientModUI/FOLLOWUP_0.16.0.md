# Si_ClientModUI 0.16.0 follow-up release

This release includes the prepared generic-menu fixes from 0.15.1 and adds the requested interactions, layout scopes, passive analytics and caching. Si_ServerModUI 1.1.0 remains compatible and requires no change.

## Command interaction and deletion

Clicking a complete command or menu option submits it immediately, once. Known argument metadata, literal command templates and advertised numeric-editor instructions instead focus an INPUT REQUIRED draft. Append the value and use SEND/Enter; an empty value is blocked. CANCEL clears the prompt. Stale menu options, removed custom controls and unavailable provider actions are rechecked before invocation. Sidebar chat buttons needing known/template input open this same prompt. Arbitrary typed chat still follows the existing explicit submission path. Remote commands retain native server authorization and are never replayed after a timeout.

REMOVE is available on user-created/imported Help rows and sidebar-derived chat-control rows. It updates private definitions, applicable sidebar bindings and shared exports, with verified JSON backups. Removing the final definition writes an empty existing export so it cannot reappear after restarting. An untouched empty profile still creates no export. Provider/server definitions remain owned by their mods.

## Sidebar and layout

The sidebar settings no longer contain MOD SUPPORT. Existing provider-backed buttons and the public provider API still work, and the cheats provider can still open the sidebar editor for a saved native action.

FPS players have a saved Hold F7 option, enabled by default. While F7 is held, the sidebar appears, native game actions are isolated and the cursor is available for one or more button clicks. Releasing F7 hides the temporary sidebar and restores the previous cursor mode. Native actions remain isolated until held keyboard/mouse buttons are released, preventing a click from passing into gameplay. Focus loss, loading, native menus, role/session changes or opening the main panel also end hold mode. A cancelled hold requires releasing F7 before reacquiring it. The old F7 debug snapshot shortcut is retired. The sidebar tooltip reads: hold F7 to open sidebar.

With the FPS hold option enabled, the persistent-sidebar preference does not keep the FPS sidebar visible after release. Disable hold mode to use an always-visible FPS sidebar. Commander persistent-sidebar behavior is retained. Sidebar settings can show it for arrangement independently of the persistent preference.

Window geometry now saves separately for FPS/Commander and each individual team, including native team index and spectator scope. Existing role-only geometry is copied to each scope on its first use; legacy keys remain in the JSON. Sidebar buttons retain their existing team/access definitions. Narrow sidebars remain vertical, wide sidebars arrange icons horizontally, and square/wider sidebars form a grid with scrolling if needed.

Drag the = divider between the generic command/control area and RECEIVED FEEDBACK to resize their relative areas. The split saves per team and role. The bundled cheats 0.3.2 provider uses the same divider API for its bottom console-entry/feedback section; short sections scroll instead of clipping controls. Cheat gameplay commands, authorization and saved shortcut formats are unchanged. Its old Si_SquadHUD dependency declaration was also corrected to Si_ClientModUI.

## Optimization and debug

Housekeeping, loaded-mod discovery, status reads and companion timeout/expiry maintenance run at approximately one-second intervals. Session/role changes, physical input, menu isolation, drawing, incoming packets and action authorization remain immediate. There is no game FPS cap and no skipped IMGUI callbacks.

Command snapshots and provider-value reads use a 50 ms refresh gate; command lists, tab widths and text measurements are reused until their inputs change. Context changes invalidate immediately. Provider metadata changes are checked at most 20 Hz while relevant panels are visible; registration changes notify the shell directly. Hidden status/command panels and offscreen control rows avoid preparation/reads. The native cheats Help registry and its row measurements are also cached. Custom provider drawing still follows native GUI events; the shell does not suppress an owning mod's draw callback.

Debug shows observed native packet TX/RX counts, application bytes and one-second byte rates; separate companion body traffic; catalogs, replies, events, response latency, rejected/stale packets, send failures, timeouts and transfers assembling. Counts are session totals. Native byte counts exclude Steam/UDP/encryption overhead. Reply latency includes command execution and queueing and is not a pure network ping. Analytics observes existing traffic and sends no additional requests. Cache-work counters help compare actual preparation with raw update/GUI callback counts.

## Validation and limitations

Client 0.16.0 and cheats 0.3.2 build with zero warnings/errors. Offline checks cover shared transport/catalog/menu lifecycle, refresh gating, input/hold policies, role/team keys, layout serialization, traffic accounting, real custom-command JSON deletion/export persistence and verified installation/rollback fixtures. Game-side Harmony interception, cursor behavior and visuals still require live verification. No game/server process is launched or stopped by installation.

Some gameplay mods do not declare argument requirements. The shell uses supplied metadata and explicit editor instructions, rather than inventing a command schema. Such mods can still return their own usage/help through feedback. Existing private chat-menu ownership restrictions between mods also remain.

Live checklist: test immediate Buy/Formation choices; a UnitBalance value prompt and cancellation; saved/imported control removal across restart; hold/release F7 with multiple clicks and with the mouse still held; focus loss, native menus and role/team changes; horizontal/square sidebar resizing; independent window/divider positions across teams and roles; generic feedback and cheats console dividers; debug traffic while connected and idle; singleplayer and a server without the addon. Check typing and restored native input after closing. No live FPS gain has been measured yet.

Install from this workspace with Install-ModUI.ps1 -Side client -ReleaseVersion 0.16.0 while Silica is closed. The receipt records verified backups and installed hashes. Rollback-ModUI.ps1 -InstallRecord <receipt> restores the previous DLLs. Existing user-data JSON is preserved by installation; a separate verified settings snapshot is taken before deployment.
