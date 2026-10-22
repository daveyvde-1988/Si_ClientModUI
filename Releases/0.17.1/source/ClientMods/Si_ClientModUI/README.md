# Build candidate: Si_ClientModUI 0.17.0 / Si_ServerModUI 2.0.0
Installed locally for testing on 21 October 2026; the temporary friend UI pair is disabled. Live verification is pending. See [installation status](INSTALLATION_STATUS.md). See [changes, limitations and live checklist](STRUCTURED_INTEGRATION_0.17.0.md) and the [developer guide / AI prompt](README-DEVELOPERS.md). The candidate adds HELP / COMMANDS / MODS / SETTINGS, structured server controls, separate status settings, source-selectable notifications and adapted current Logistics/Formation exports. Historical release notes below describe earlier behavior.

# Current release: Si_ClientModUI 0.16.0

Mod developers: see the [developer integration README](README-DEVELOPERS.md) for client hooks, server integration, the Buy-style menu contract and an AI implementation prompt.

See FOLLOWUP_0.16.0.md for the current interaction/layout/optimization changes and live checklist. Menu actions now run immediately unless input is required. FPS hold-F7 sidebar mode, role/team window positions, adaptive sidebar grids, feedback/console dividers, custom-control deletion, passive network analytics and cached data preparation are included. The compatible cheats UI provider is 0.3.2. Si_ServerModUI remains 1.1.0. Use Install-ModUI.ps1 -Side client -ReleaseVersion 0.16.0 while Silica is closed; the current default release is 0.16.0.

The historical feature and installation notes below describe earlier releases; the current behavior above takes precedence.
# Si_ClientModUI 0.15.0
0.15.0 integrates the optional Si_ServerModUI 1.1.0 backend from DrMuck's redesign into the current 0.14.0 UI. Existing overlays, F6 behavior, input isolation, sidebar/team bindings, settings, provider panels, debug tools and saved data paths are retained. The shell starts empty and fills from actual local providers and confirmed server metadata. Singleplayer and servers without the addon keep native chat functionality.

The existing menu header and expanded status show checking, detected or not detected. Click the header indicator to recheck. No response means not detected; it does not prove the DLL is absent. Discovery is independent of opening the menu, bounded to three hellos per connection/scene/recheck, with no ongoing client polling. Server commands appear in existing Help/mod/Admin lists after confirmation. Captured output appears in RECEIVED FEEDBACK; numbered server choices and active vote options appear in existing command lists. Clicking still prepares a draft, then SEND/Enter submits. Server-provided map/ban arguments also appear as prepared command entries; other arguments use the existing draft bar.

Install client: close Silica, remove/disable the old Si_SquadHUD.dll and copy Mods/Si_ClientModUI.dll from the client ZIP. Keep UserData/Si_ModMenu unchanged. If using the separate cheats provider, replace it with the supplied OptionalProviders/cheats.dll (0.3.1), rebuilt for the new assembly name. Other binary providers referencing Si_SquadHUD.dll also require recompilation. Namespace/API signatures remain SiSquadHud.

Optional server: with the dedicated server closed, install Mods/Si_ServerModUI.dll from the separate server ZIP. Requires Si_AdminMod. Retire the previous Si_SquadHUD_Server.dll. Do not put the server DLL on clients. Configuration uses UserData/cfg/Si_ServerModUI/menu.json; the friend's old SquadHUD/menu.json can be copied manually before startup.

Verified workspace installer: Install-ModUI.ps1 -Side client (default), server or both; use -PlanOnly to inspect. An installed cheats provider is updated with its compatible rebuild. It backs up and verifies DLLs, retires legacy names, preserves preferences, and writes a receipt. Rollback-ModUI.ps1 -InstallRecord <receipt> restores the previous DLLs. No game/server is started or stopped by these scripts.

See INTEGRATION_ANALYSIS.md for the comparison and TESTING.md for validation and remaining live checks. The new artifacts are prepared, not deployed.

The following describes retained UI features from 0.14.0 and earlier.

0.14.0 hides every menu surface during native loading, intro video, cinematic camera playback and cinematic moments. It also closes the panel and releases captured input. The compact header, notifications and sidebar return when gameplay presentation is available; an interrupted open panel stays closed.

Sidebar bindings now save a TeamScope in the existing per-Steam-user client layout. Each team has its own buttons, symbol/color choices and per-button enabled state; spectator has a separate scope. The sidebar master visibility preference remains global. The first real team joined after upgrading receives existing unassigned buttons once, after a timestamped .before-team-sidebar backup of the old JSON. Switching teams does not reassign them. Provider saved-command definitions remain available for creating separate bindings on other teams.

The ADMIN tab appears only in multiplayer, for an administrator, when accessible admin commands or controls exist. Help's ADMIN filter requires actual admin command rows. Singleplayer folds accessible admin entries/controls into the normal mod/Help views.

Debug -> Controls now includes Enable client UI debug, saved in the client layout. When enabled, it collects up to 200 own events/errors, newest first, with search, clear and copy. Analytics include uptime, FPS, scene/team, UI update/draw/poll counts, errors, chat submissions, sidebar buttons, registered mods/commands and captured input actions. MOD DEBUG CONTROLS and the Commands subtab preserve provider debug options. It does not re-enable the separate Diagnostics mod. Detailed session file logging remains available even when this display is disabled. Events are captured only while enabled; the event list is not restored across restarts.

0.13.0 added an optional Definition.DrawCommands panel beside Definition.DrawPanel. Provider command views retain input isolation and do not submit the hidden chat draft. The cheats provider uses it for the native Help registry text list.

0.12.0 added a generic provider panel API (Definition.DrawPanel) with menu styling, scaling and input isolation. Providers can open the existing sidebar editor through ClientModControls.EditSidebar. Provider panels use the full content area; physical Enter does not submit the hidden chat draft. The symbol catalog now includes $ and common punctuation. The separate cheats client mod uses these APIs; the shell contains no cheat catalog.

Standalone client-side shell. No named gameplay mod, bundled command catalog or server companion is required. F6 expands the status header; F6 again opens Mod menu. Escape/F6 closes it. Opening or arranging UI does not query the server.

## Sidebar

Open sidebar in the toolbar. The Enable persistent sidebar checkbox saves whether it stays visible after the menu closes. New profiles start with no buttons and the sidebar disabled. Each button also has its own enable checkbox, Edit and Remove controls.

ADD CHAT BUTTON lets you enter a label and exact single-line chat command, select role/admin scope, a symbol and a color. Clicking its resulting sidebar icon submits that command immediately; it does not require a Help entry or a confirmation draft. Native server handlers still decide authorization and effects. Custom sidebar commands also appear in Help.

The symbol catalog contains A-Z/a-z, digits, filled/open circle/square/triangle/diamond/hexagon/star, padlock, line, plus, check/cross, smile/frown, heart, lightning and sun. Pictograms use owned bitmap masks instead of depending on emoji font coverage. Twelve color swatches and a six-digit RGB hex input are available.

MOD SUPPORT lists actions actually registered by loaded client mods. Add an action and customize its symbol/color. Runtime availability, role and administrator metadata are rechecked before invoking its delegate or command. Missing/unavailable provider actions remain disabled. Providers can also supply their own icon textures. No game mod is recognized by name or given an invented switch.

Sidebar geometry and choices save per Steam user in UserData/Si_ModMenu/layout-<SteamID>.json, alongside existing Mod menu preferences. Buttons remain across role/server changes; provider availability and access still gate execution.

## Settings and input

Settings is beside status/sidebar/notifications/debug. Transparency is 0..100% for UI surfaces (text/symbols remain readable). Text size is 75..150%; controls/spacing scale with it. Preferences persist on slider release or menu close. Team accent/background colors come from the current player's native TeamColor, entirely on the client.

The bottom entry bar sends any nonempty single-line text or command, up to the native 160-character limit, exactly as entered. SEND/physical Enter submits once and retains the draft. There is no saved-entry requirement and no automatic recording of typed chat. Clicking a Help command prepares the draft as before.

While the menu is open, both native input action assets are isolated, including UI/Global/Commander/Unit maps. Native action/map re-enabling, button polling and GameInput.ProcessInput are guarded. IMGUI text/mouse interaction and the menu's own raw Enter/Escape/F6 handling remain active. Previously enabled actions restore after closing and releasing held keys. This broadens the old chat-only guard to address typing that opened native menus. Live verification remains user-owned.

Help separates Player/Commander/Admin: commanders can access Player and Commander; admins can access all. Admin-only known controls remain hidden from others. The unrestricted chat bar behaves like native chat; it does not claim permission to execute what is typed.

## Sharing created commands

The mod writes UserData/Si_ModMenu/created-commands.json from explicitly saved Help entries and custom sidebar commands. It excludes server/Steam bindings, learned feedback, provider registrations and typed chat history. If no definitions were created/imported, it does not create a command file or seed one with known information.

Copy that file into another client's UserData/Si_ModMenu folder before joining, or use Settings -> IMPORT SHARED JSON. Imported entries appear in Help as portable references without needing server metadata or a companion. They never auto-execute and their presence does not prove server support. Role/admin flags remain. Recipients can bind entries to their own sidebar symbols; native provider delegates/textures are not serialized.

Private per-user commands-<SteamID>.json and layout files remain intact. Existing user-created 0.10.0 Help entries are eligible for export. JSON replacements retain verified backups. Old HUD namespaces and historical built-in catalogs are not imported.

## Client extensions

ClientModControls.Definition supports title, grouped controls, commands, status values, sidebar actions and sidebar symbols. Notify publishes local notices. Actual loaded client mods have tabs even if they expose no controls. Status still starts empty until supplied/selected. Optional structured server integration now fills the same command/feedback surfaces after discovery; the historical 0.8.1 companion remains separate.

Build: dotnet build Client/Si_ClientModUI.csproj -c Release --ignore-failed-sources
Prepare/install: Prepare-Release.ps1, then Install-ModUI.ps1 with the affected client/server closed.
Rollback: Rollback-ModUI.ps1 -InstallRecord <receipt>

Build passed with zero warnings/errors. 189 combined command and UI-policy checks passed. No game/server was started/stopped. Runtime verification is pending.




