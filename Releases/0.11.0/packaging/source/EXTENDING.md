# Client-owned integrations: 0.11.0

The public namespace remains SiSquadHud in Si_SquadHUD.dll. Register(owner, Definition) and Unregister(owner) bind metadata to the actually loaded MelonMod. The older Register(owner, IReadOnlyList<Control>) overload remains compatible.

Definition fields: Title; Controls; Statuses; Commands; Sidebar; SidebarSymbols.

Control: Id, Label, Description, Group, Role, AdminOnly, Debug, Read (Func<string>), Apply (Action<string>), Choices. Groups/choices structure the owning mod's UI. Apply is an explicit click; the mod owns validation, gameplay effects and saving. Read should inspect local data only.

Status: Id, Label, Group, Role, AdminOnly, Read. No parameters are injected by the shell and values start unselected.

ChatCommand: Command, Description, Group, Role, AdminOnly, Debug. Clicking prepares a draft; SEND/Enter uses the native player chat entry point.

SidebarAction: Id, Label, Description, Symbol, Color, Role, Command, AdminOnly, Invoke (Action), Available (Func<bool>). Stable IDs identify user-selected bindings. Invoke takes precedence over Command. Available should be a pure local check; false disables the saved sidebar button without pretending the action succeeded. The menu never simulates gameplay keypresses. Users opt into each sidebar button and may override symbol/color.

SidebarSymbol: Id, Label, Texture (UnityEngine.Texture2D). Use a white/alpha mask for user-color tinting, preferably square. The owner retains texture lifetime. A sidebar action's Symbol can be a built-in ID (text:A, shape:circle-filled, shape:hexagon-open, icon:padlock, icon:smile, icon:heart, etc.) or its owner's custom symbol ID. The menu qualifies custom IDs with local owner scope. Missing symbols display a question mark.

Role is shared/player/commander/admin. Player entries are accessible to commanders; native admins can browse all roles. AdminOnly additionally restricts any item. Actions recheck access/availability on invocation. A local mod must supply accurate metadata; remote commands retain native server authorization.

ClientModControls.Notify(owner, text) queues a bounded local notice. Preferences with exactly matching visible loader categories remain generically editable. No arbitrary private fields, generic unpatch switches or named gameplay-mod adapters are introduced.

User-created Help/custom-sidebar commands export to created-commands.json. Registered provider commands/delegates/textures and learned feedback are not automatically exported. A provider must be installed on the recipient for its native action bindings to resolve. Server-owned structured integration remains deferred.

