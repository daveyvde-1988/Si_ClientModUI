# Silica Mod Menu 0.11.0

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

ClientModControls.Definition supports title, grouped controls, commands, status values, sidebar actions and sidebar symbols. Notify publishes local notices. Actual loaded client mods have tabs even if they expose no controls. Status still starts empty until supplied/selected. Structured server-side integration remains future work; the old companion is preserved separately and disabled.

Build: dotnet build Client/Si_SquadHUD.csproj -c Release --ignore-failed-sources
Prepare/install: Prepare-Release.ps1, then Install-SquadHUD.ps1 with the client closed.
Rollback: Rollback-SquadHUD.ps1 -InstallRecord <receipt>

Build passed with zero warnings/errors. No tests were added/run, and no game/server was started/stopped. Runtime verification is pending.

