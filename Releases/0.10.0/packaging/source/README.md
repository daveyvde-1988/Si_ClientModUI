# Silica Mod Menu 0.10.0

A standalone client UI shell with no bundled mod catalog, gameplay adapters, preselected status values or imported HUD settings. It uses what loaded client mods expose, received native server feedback and entries the user explicitly saves. It sends no discovery queries, polling requests, keepalives or companion packets.

F6 expands the small header; a second F6 opens Mod menu. Click a chat command to prepare its draft. SEND or a physical Enter press sends the draft once through native player chat. There is no EDIT button and no automatic command retry. The native Global Chat/ChatChannel actions are suppressed while the menu is open and restored on close; Enter is consumed by the menu. This input behavior still needs user live verification.

The header contains only status, sidebar, notifications, debug and Close. It has no score, credit, rank or squad metrics. The sidebar button is intentionally without action; there is no sidebar overlay or formation logic. Status has no built-in parameters; mod-supplied values can be selected later. Notifications start off and can display native server messages or locally published mod notices.

## Discovery, roles and Help

Each actually loaded client mod gets its own tab, even when it exposes no controls. Matching visible loader preference categories can supply generic editable local preferences. Explicit integrations can define their own title, groups, controls, commands, status and notifications. All display names remove the si_ prefix; binary/internal API names stay stable.

Help lists known applicable chat controls under Player, Commander and Admin. Player/Commander entries follow the current game role; shared entries appear in either. Admin commands and controls are placed under Admin and the Help Admin category, visible only when the local player has a native non-NONE admin level (or is the singleplayer host). Access is rechecked before invoking a command/control; the native command handler still owns permissions and effects. Local client public debug controls are available normally. Server debug entries require admin status.

The debug button shows only supplied/discovered debug commands and controls for current access. A remote server does not provide a universal mod command registry or permission schema to this client. Therefore it cannot enumerate every remote debug option or establish arbitrary command permissions without metadata. Advertised debug option alternatives can be expanded when received. Unclassified hints are not silently treated as available commands; users can add explicit Help entries. Server integration is deferred.

ADD ENTRY in Help saves mod name, command, description, role, admin/debug classification and server scope. Entries are remembered per Steam user and server; singleplayer has its own scope. Saving does not send a command. Existing entries with the same identity are replaced. Saved definitions remain user-authored references; their existence is not proof that the server still supports them.

Native authenticated server chat supplies bounded, memory-only feedback; ordinary player chat is excluded. Explicit slash-number menu rows may be prepared as drafts after a user request. Bare-number menus are not assigned a guessed command prefix. There are no special parsers or routing rules for particular mods. Scene/team/role/round/connection boundaries clear transient feedback, choices and drafts.

## Clean defaults and settings

UserData/Si_ModMenu is a new namespace:
- layout-<SteamID>.json is created only after a layout/selection/notification choice.
- commands-<SteamID>.json contains entries explicitly saved in Help.
- session logs record lifecycle/errors and local menu counts, not a server mod inventory.

Old UserData/Si_SquadHUD files and old MelonPreferences values are ignored. No layouts or command definitions migrate. Old files/releases are preserved for rollback; the previous catalog generator/catalog are archived outside the active project. The server companion stays separate and disabled.

Local preference APPLY changes the owning mod's in-memory preference through its loader value API. Persistence and live behavior belong to that mod; explicit registered controls can implement their own saving.

Build: dotnet build Client/Si_SquadHUD.csproj -c Release --ignore-failed-sources
Prepare: powershell -NoProfile -ExecutionPolicy Bypass -File Prepare-Release.ps1
Install (client closed): powershell -NoProfile -ExecutionPolicy Bypass -File Install-SquadHUD.ps1
Rollback with the installation receipt: Rollback-SquadHUD.ps1 -InstallRecord <receipt>

The client build passed with zero warnings/errors. No tests, game/server starts or stops were performed. Runtime verification remains user-owned.

