# Si_ClientModUI 0.17.1

MODS now has PLAYER and ADMIN categories for client and server panels. ADMIN is available only to current admins. Registered controls and discovered server choices are filtered by their actual admin/role metadata. Buy catalogs and read-only browsers remain in PLAYER; their original server authorization still applies.

Complete actions run by clicking the named option. Separate RUN buttons have been removed from generic controls. Controls that need a choice or value retain their input, picker or SET/APPLY confirmation. The editable console command submission is retained because it requires user input.

Diagnostics start off whenever a local profile is loaded, even if a previous session saved them enabled. SETTINGS > DEBUG can enable them during the current session. SETTINGS > GENERAL includes Show Mod menu button; F6 continues to open the menu when the button is hidden. Existing layouts default to showing it. Centauri backgrounds, accents and buttons are brighter; transparency still applies.

Custom client panels: Definition.PanelAdminOnly defaults to false. Set it to true for an admin-only DrawPanel/DrawCommands callback. The shell hides that callback in PLAYER and denies it without current admin access. Generic Controls are categorized individually using AdminOnly or Role="admin". Opaque custom callbacks cannot be subdivided automatically; providers own the contents and must continue enforcing authorization when executing actions. The compatible cheats provider 0.3.3 declares its custom panel admin-only. It otherwise retains its behavior.

Validation: client and cheats release builds pass without warnings or errors. All 270 offline network/catalog/lifecycle and preference checks pass, including category separation for admins/non-admins and backward-compatible button visibility. No game or server was started.

Live checks: compare Centauri visibility at your transparency setting; check PLAYER/ADMIN contents with both permission levels; click a complete option once and verify a single execution; check required-value prompts; hide the Mod menu button and reopen with F6; enable debug, restart and verify it starts off. Third-party panels depend on the permissions/category metadata their providers supply.

Source and client settings backup: Backups/ModUI-0.17.1-20261022T161705. Installation also makes verified backups of both replaced client DLLs. Server addon and gameplay mods require no update for this client patch.
