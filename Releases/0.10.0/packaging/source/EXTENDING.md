# Mod-owned client integrations

The stable public API remains in namespace SiSquadHud in Si_SquadHUD.dll. No named gameplay mod is recognized or required.

Call ClientModControls.Register(owner, definition) when the local mod and menu API are loaded. Unregister(owner) on teardown. Only currently loaded owners are shown. The older Register(owner, IReadOnlyList<Control>) overload remains supported.

Definition:
- Title: optional display name, otherwise the owning mod's MelonInfo name.
- Controls: IReadOnlyList<Control>.
- Statuses: IReadOnlyList<Status>.
- Commands: IReadOnlyList<ChatCommand>.

Control: Id, Label, Description, Group, Role, AdminOnly, Debug, Read (Func<string>), Apply (Action<string>), Choices (string[]). Groups are collapsible; explicit choices become buttons. Without choices, an editable value and APPLY/RUN button call the owning mod. Read should only inspect local data. Apply is called only on a user click, after role/admin validation. The mod validates values, performs cleanup and decides persistence. No generic unpatch/unload toggle exists.

Status: Id, Label, Group, Role, AdminOnly, Read. Values start unselected. Stable IDs retain later user selections. No game statistics or inferred mod states are injected by the shell.

ChatCommand: Command, Description, Group, Role, AdminOnly, Debug. Commands are local-scope definitions provided by the owning mod. Clicking only prepares the draft. SEND/Enter uses native Player.SendChatMessage so the normal mod interception path still owns effects.

Role is shared, player, commander or admin. AdminOnly can additionally restrict a player/commander-specific item. Admin controls appear under Admin, and Debug also respects current access. Local preferences whose visible category identifier exactly matches the normalized assembly/MelonInfo name are available generically; arbitrary private fields are never inspected.

ClientModControls.Notify(owner, text) publishes a bounded local notification when notifications are enabled. It uses the main-thread queue. User settings are under UserData/Si_ModMenu; previous HUD files are not imported.

Sidebar registration/rendering is intentionally deferred. Server-side structured catalogs, verified permission claims, status and notifications are also deferred. This release does not trust a chat string as a universal remote capability or permission declaration.

