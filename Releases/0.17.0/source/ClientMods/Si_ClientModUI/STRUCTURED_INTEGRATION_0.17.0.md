# Structured integration build — 0.17.0 / 2.0.0

**Built and staged, not installed.** The temporary friend client/server remain the installed test versions until you confirm the server and game are closed. No game/server process was launched or stopped.

## Delivered layout

- Top tabs: HELP, COMMANDS, MODS, SETTINGS. F6 opens HELP directly unless Remember last menu is enabled; that option defaults off.
- HELP explains the mod, versions, navigation, F6/F7, permissions and missing-companion fallback.
- COMMANDS has ALL plus collapsible CLIENT MODS / SERVER MODS lists. Selected mods have PLAYER / ADMIN views; ADMIN is unavailable to non-admins. Search, custom entries/removal, normal chat input and a draggable history divider are retained. Observed player/server chat and provider notices enter history independently of notification overlay settings.
- MODS separates local providers from server modules. Local custom panels, registered controls and visible preference categories remain supported. Server panels use declared controls and fall back to known commands; loaded server mods without exports still appear.
- SETTINGS has GENERAL, STATUS, SIDEBAR, NOTIFICATIONS and DEBUG. General retains appearance controls and adds remembered menus. Status is a separate optional draggable window with per-mod selection. Sidebar retains F7 and team/role positions and accepts text symbols of up to eight letters/numbers. Notifications have a master switch and identifiable source selection. Debug retains passive traffic/cache diagnostics.

## Structured controls and server integration

The server reads the friend's ModUIData()/ModUIPlayerData() exports and supports command order, current values, choices, status lines, argument chains, catalogs and read-only balance browsers. The reusable builder adds group, control-kind, range/step/unit and known default/vanilla hints. Current values remain visible; default/vanilla values use expandable DETAILS, as requested.

Controls include action buttons, switches, checkboxes, text/numeric value boxes, bounded sliders, dropdowns, headers, explanations and read-only values. Categories collapse. Slider dragging and text editing remain local; SET sends one command. Picker stages collect arguments locally and send the completed command once. Large choice sets use dropdowns. Unknown kinds use basic control behavior.

The server declares frame schema 2/capabilities while retaining our SiSquadHUD/2 marker and assembly/config names. This build does not communicate with the temporary friend's SiModUI/2 addon; install our matching pair together. There is no continuing client catalog polling. Post-command updates confirm state; shared changes use hashed diffs. Limits, bounded queues/capture, exact replies, stale-nonce handling and restart recovery remain in place.

Optional embedded hints add missing command descriptions/subcommands for older mods while preserving existing explicit labels/argument/choice settings. This is metadata, not handler registration or authority. The current visible-player document is sanitized before hashing. Malformed exports do not fall back to unfiltered command documents. Exported metadata cannot lower registry-required admin powers.

## Our current gameplay mods

- Logistics **0.9.1**, based on our 0.9.0: new export for existing settings, harvesterfix, AI transport, rewards and score controls. Added `/logistics enabled on|off` as a thin alias to existing validated master behavior. No old Logistics source/config migration was imported. Purchase exclusions recognize compatible Si_Buy and legacy KoH implementations; unknown signatures fail closed.
- Formation **2.15.2**, based on our 2.15.1: new shared export and player-scoped formation choices. Selections include the current revision and call the existing role/team/revision-checked API. Added namespaced close/exit and `/close`, `/exit`, `/0` navigation cleanup; added a thin master alias. Ownership checks recognize the separate Buy mod. Native movement/Follow logic was retained.
- Ranks is unchanged.
- Compatible client cheats provider **0.3.2** rebuilt for packaging; installation can restore it alongside our UI.

## Validation and difficulties

- Client 0.17.0, server addon 2.0.0, Formation 2.15.2 and the compatible cheats provider build against the installed references with no warnings/errors.
- Logistics 0.9.1 builds successfully. Its existing package setup reports NU1701 (MelonLoader package framework compatibility) and NU1900 (NuGet vulnerability service unavailable). No tools/dependencies were installed to suppress these warnings.
- **259** client/network/catalog/cache/persistence checks, **18** server export permission/privacy/personalization/hash checks, and **24** installer/rollback fixture checks passed: **301 checks total**. Fixture tests operate on dummy files, not installed DLLs.
- The server tests caught and fixed an export reintroduction path for a registry command hidden by required admin power.
- Logistics/Formation tests and live verification remain user-owned under their AGENTS.md instructions; none were run or added. Their builds do not establish gameplay correctness.
- No live IMGUI/game session was opened for visual verification. Short/wide labels, small resolutions and large dropdowns need an in-game pass.

## Practical limits to review

1. Native messages without an identifiable mod use the Server category; source selection cannot reliably identify every arbitrary chat line. The UI does not invent ownership. Asynchronous output remains native and enters history when observed as chat; it is not captured as a synchronous structured reply.
2. Existing third-party mods still own their command grammar and permissions. Unknown internal subcommands/ranges/vanilla values cannot be discovered reliably. Exports must describe these accurately.
3. There is no universal server-side owner coordinator for every unmodified third-party chat menu. This client avoids blind automatic `/0` cleanup; Formation retains scoped compatibility checks. Prefer direct structured controls where possible.
4. Catalogs currently render grouped purchase buttons, not a dense tile grid. Buy purchase decisions still run in Buy's handler. The balance browser remains read-only, as in the friend's design.
5. Team/player pickers rely on current local match data, including active empty teams. Unambiguous name tokens are required; ambiguous targets remain available through manual input. No guessed targets are sent.
6. Defaults/vanilla values appear only when supplied. No arbitrary server status polling or speculative menu crawling was added.
7. Only changed UI/Logistics/Formation components were rebuilt against our public branch. The already-installed friend's other beta-built gameplay DLLs still need their own public-branch live verification.

## Before installing

Close both Silica and the dedicated server and confirm readiness. Installation is intentionally pending. The verified installer can then use:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ClientMods/Si_ClientModUI/Install-ModUI.ps1 -Side both -ReleaseVersion 0.17.0 -IncludeCheats -IncludeGameplayProviders -PlanOnly
```

Remove `-PlanOnly` only for the authorized installation. It verifies game/reference hashes, backs up DLLs, retires the temporary Si_ModUI / Si_ModUI_Server, preserves preferences, and writes a rollback receipt. The gameplay-provider switch includes our current Logistics and Formation builds; unrelated gameplay mods/Ranks are excluded.

Source backup: `Backups/ModUI-structured-integration-20261021T162209268`.

## First live pass and feedback requested

- Check HELP readability, left navigation widths and COMMANDS scrolling at your normal resolution/text size.
- Confirm F6 first-open/default behavior, remembered selections, native chat/Enter, the divider and history with notifications on/off.
- Check status/notification windows independently, per-mod selections, multi-character sidebar symbols, F7 cursor release and saved team/role positions.
- Try complete actions, value SET/cancel, checkboxes, dropdowns, collapsed categories and default/vanilla DETAILS. Confirm one action per click and authoritative values after replies.
- Check admin/non-admin and limited-power admin views. Switch team/role with a picker open and verify stale Formation choices are rejected.
- Switch between Formation, Buy and UnitBalance chat menus. Confirm Back/Close routes, native Follow/commander locks, Logistics harvesterfix/AI transport/scoring/reward behavior, and purchase score exclusions.
- Observe debug traffic with unchanged state, then a setting change; verify no recurring hash churn or network messages from merely browsing.

Please report which navigation/control sizes feel awkward, whether grouped purchase buttons should become a denser grid, and any messages attributed to the wrong notification category. Runtime feedback is still required before calling this release fully verified.

See [developer integration guide and AI prompt](README-DEVELOPERS.md).
