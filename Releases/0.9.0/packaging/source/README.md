# Silica Mod Controller 0.9.0

Standalone IL2CPP client mod. The client requires MelonLoader, not a server companion. F6 expands status; a second F6 opens the full menu. F7 logs a local status snapshot. Multiplayer dedicated servers and singleplayer sessions are supported; runtime verification of this release remains user-owned and pending.

Opening a menu, changing tabs, arranging windows, scrolling and watching status generate no requests. There are no companion packets, subscriptions, keepalives, automatic chat commands, refresh polls or retries. Only clicking a command/SEND invokes the native Player.SendChatMessage entry point. The normal game/mod command path owns permissions, side effects and replies. Commands can affect gameplay or server settings exactly as if typed in chat.

## Commands and menus

Each command-owning module has a horizontally scrollable tab; HELP presents the same clickable catalog. ALL/PLAYER/ADMIN filters and local search narrow the list. The bundled catalog contains 88 commands across 23 module tabs, prepared from the installed DLLs, including the current Unit Balance 7.7.1. It describes known controls, not proof that the connected server runs those mods. Unrecognized commands may fall through to ordinary chat on servers without their handlers.

Click a complete command to send it once. Argument-taking entries open the editable draft. EDIT works for every row; SEND sends the visible draft. No placeholder or multiple-choice template is sent automatically. No click claims success or toggles a displayed server state optimistically.

Authenticated native server chat is retained as bounded, session-only feedback. Native player chat is excluded. Feedback mentioning additional command roots can extend the current session catalog. After a command is explicitly requested, received numbered menu rows become clickable for supported native menus: Formation, Unit Balance, KoH Buy and Heavy Teleporter (which uses /st N). Multiple numbered choices on a Unit Balance line are separated into rows; native Set prompts open argument entry. The source mod still owns global /1, /back, /0 navigation. Choices expire after 60 seconds; incoming choices are collected for 15 seconds after an explicit command. Manual command input invalidates cached numbered choices. Team/role/round/scene/connection changes clear feedback and observed values. Arbitrary chat transports or unknown menu formats may require typing their command in the draft.

UserData/Si_SquadHUD/commands.json can supply an additional array of CommandEntry objects (Mod, Command, Description, Role, Arguments). It is read locally at startup. Nothing from a remote server is automatically executed. No server mod list, server files or private preferences are queried.

## Status and notifications

Local values include role, score, kills/deaths, group size, native squad limit, FPS, faction, technology tier, stored faction resources and controlled-unit health. The native squad limit is not asserted to be a server override. Exact recognized Ranks/Formation/KoH replies can supply rank, squad allowance, formation descriptions and personal credits. Observed values include their age; no current rank/credit value is invented from local score or configuration. Missing information remains unknown. Complete server rank tables and the full server balance JSON are not available through this standalone transport unless supplied in normal feedback.

The commander sidebar sends Formation chat commands and can display received choices. It does not claim an authoritative continuously selected/locked state. Window geometry, default-empty positive status selections, role-specific placement, faction styling and renderer recovery remain local. Existing layout JSON is preserved; selections for unavailable former companion providers remain saved.

MOD NOTIFICATIONS optionally diverts authenticated native server chat into its bounded window on multiplayer; ordinary player chat remains native. In singleplayer, the native server-message presentation path is observed locally. No server-wide hooks or routing leases are installed.

## SP mods

The SP MODS tab appears in singleplayer when other Melon mods are loaded. It lists those actual local mods. Exact matching visible MelonPreferences categories expose supported primitive/enum preferences through their own value/validation/event path. APPLY changes the in-memory preference. SAVE PREFS asks the loader to persist it after copying and SHA-256-verifying existing files into UserData/Si_SquadHUD/PreferenceBackups. A mod may require reload/restart before a preference takes effect.

Mods can register custom controls through ClientModControls.Register, using their own Read/Apply delegates. There is no generic unpatch/unload switch; mods with no exposed controls are listed without an invented switch. The installed Diagnostics mod currently exposes no such preferences/API; it is retained unchanged.

## Separate companion and installation

The previous 0.8.1 server, wire models and paired client source are preserved under ../Si_SquadHUD_Companion. The standalone installer replaces only the client HUD and moves the old server companion outside Mods to that separate project's Disabled directory, retaining a hash-verified backup and receipt. It does not replace Formation, Ranks, Unit Balance, Starter Units or any gameplay mod, or edit settings/layout files. Historical 0.8.1 releases remain unchanged.

Build: dotnet build Client/Si_SquadHUD.csproj -c Release --ignore-failed-sources
Prepare: powershell -NoProfile -ExecutionPolicy Bypass -File Prepare-Release.ps1
Install with affected programs closed: powershell -NoProfile -ExecutionPolicy Bypass -File Install-SquadHUD.ps1
Rollback: powershell -NoProfile -ExecutionPolicy Bypass -File Rollback-SquadHUD.ps1 -InstallRecord <receipt>

Build success does not establish in-game behavior. No tests, game/server launches or process stops were performed.

