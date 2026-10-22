# Si_ClientModUI: developer integration guide

## Quick guide

Si_ClientModUI provides a consistent menu. Your mod supplies its commands, controls and values, and remains responsible for gameplay and permissions. Local features work without a server companion.

- **Client settings, actions or sidebar buttons:** [Client integration](#client-integration).
- **Server mod panels:** [Server exports](#server-exports) and [control types](#control-types).
- **Player-specific choices:** [Per-player exports](#per-player-exports).
- **Older mods without an export:** [Discovery and fallback](#discovery-and-fallback).
- **Chat menus with numbered choices:** [Navigation contract](#navigation-contract).
- **Low traffic and safe actions:** [Performance and permissions](#performance-and-permissions).
- **Ask an AI to integrate a mod:** [Copy the implementation prompt](#ai-implementation-prompt).

This guide describes the **0.17.0 client / 2.0.0 server build candidate**. It is built, staged and awaiting installation/live verification. See [implementation and testing notes](STRUCTURED_INTEGRATION_0.17.0.md).

## Client integration

Reference `Si_ClientModUI.dll` with `Private=false`; the public namespace remains `SiSquadHud`. A direct reference requires that DLL at runtime. Put the bridge in a separate optional assembly if the gameplay mod must load without the UI. Use `[assembly: MelonAdditionalDependencies("Si_ClientModUI")]` for load order.

Register an owner-bound definition once after initialization:

```csharp
using SiSquadHud;

ClientModControls.Register(this, new ClientModControls.Definition
{
    Title = "Example",
    Controls = new[]
    {
        new ClientModControls.Control
        {
            Id = "opacity", Label = "Overlay opacity", Group = "Appearance",
            Description = "Local overlay opacity; does not change server state.",
            Kind = "slider", Minimum = 0, Maximum = 100, Step = 1, Unit = "%",
            DefaultValue = "100",
            Read = () => opacity.ToString(System.Globalization.CultureInfo.InvariantCulture),
            Apply = text => ApplyValidatedOpacity(text)
        }
    },
    Commands = new[]
    {
        new ClientModControls.ChatCommand
        { Command = "/example status", Description = "Request status." },
        new ClientModControls.ChatCommand
        { Command = "/example limit", Description = "Enter a limit.", RequiresInput = true }
    }
});
// Unload:
ClientModControls.Unregister(this);
```

This is a fragment for your existing MelonMod, not a complete project. `opacity` and `ApplyValidatedOpacity` belong to your mod. Registering commands here does not create server handlers or prove their availability.

`Control` retains `Id`, `Label`, `Description`, `Group`, `Role`, `AdminOnly`, `Debug`, `Read`, `Apply`, `Choices`, and adds `Kind`, `Minimum`, `Maximum`, `Step`, `Unit`, `DefaultValue`, `VanillaValue`. Leaving `Kind` empty preserves the previous generic control behavior. Choices are strings; the owning handler must validate values and persist changes.

`Definition.Statuses` contains `Status { Id, Label, Group, Role, AdminOnly, Read }`. Values appear as opt-in checkboxes under SETTINGS → STATUS. `Read` must inspect cached local data, not issue a network request.

`Definition.Sidebar` contains `SidebarAction { Id, Label, Description, Symbol, Color, Role, Command, AdminOnly, Invoke, Available }`. `Invoke` takes precedence over `Command`. Users choose bindings through `ClientModControls.EditSidebar(this, actionId)`. Stable IDs preserve bindings. `Available` must be a cheap local check.

`SidebarSymbols` supplies owner-scoped `Texture2D` symbols; the provider retains texture lifetime. Built-ins include `shape:circle-filled`, `shape:hexagon-open`, `icon:padlock`, and `text:A`. `text:AB12` supports up to eight letters/numbers; users can edit this in sidebar settings.

`ClientModControls.Notify(this, text)` records a local notice. COMMANDS history retains notices independently of whether the notification overlay is enabled. Notification settings select identifiable mod sources; unidentified server messages use the Server category.

`DrawPanel` and `DrawCommands` support custom IMGUI panels. The latter is accessible through EXTRA PANEL when supplied. `PanelContext` provides `View`, `LabelStyle`, `InputStyle`, `Label`, `Button` and `Divider(key, region, minimumAbove, minimumBelow)`. Reuse shell scaling/input isolation, balance GUI groups and cache expensive preparation. The divider position is owner/team/role scoped.

Re-register changed metadata; do not register every frame. Unregister on unload. Roles are `shared`, `player`, `commander`, `admin`; player entries also work for commanders and local admin presentation permits all roles. These are presentation rules; the actual action handler must enforce access.

## Server exports

Server gameplay mods do not need to reference either UI DLL. Copy [ModUI_Export.cs](../Si_ServerModUI/Export/ModUI_Export.cs) into their Mono/netstandard2.1 project and include it explicitly if default compile items are disabled. It depends on Newtonsoft.Json.

Expose the exact-case method **`public static string ModUIData()`** on a type in the loaded mod assembly. The optional server addon finds it by reflection. It describes existing command handlers; it does not install them.

```csharp
public static string ModUIData() => ModUI.ModUIExport.New()
    .Info("Example settings")
    .Status("enabled", "Example service", Cfg.Enabled ? "ON" : "OFF")
    .Action("/example status", "Status", "Request current status.")
    .OnOff("/example enabled", "Enabled", "Change the master setting.", true, Cfg.Enabled)
        .Presentation("checkbox", "General")
    .Value("/example share", "Reward share", "Fraction between zero and one.", true,
        Cfg.Share.ToString(System.Globalization.CultureInfo.InvariantCulture))
        .Presentation("slider", "Rewards", 0, 1, .01, "fraction", defaultValue: "0.05")
    .Picker("/example assign", "Assign player", "Choose team, then player.", true, "team,player")
    .Build();
```

Adapt every command and field to your real source. The example requires `/example enabled`, `/example share`, etc. to exist. Default values must come from known mod defaults; vanilla values must be supplied only when actually known.

The exported document looks like:

```json
{
  "schema": 2,
  "info": ["Example settings"],
  "stats": [{"id":"enabled","label":"Example service","value":"ON","group":"General"}],
  "commands": [{
    "t":"/example share", "l":"Reward share", "d":"Fraction between zero and one.",
    "a":true, "p":"Generic", "v":"0.05",
    "ui":{"kind":"slider","group":"Rewards","min":0,"max":1,"step":0.01,"unit":"fraction","default":"0.05"}
  }]
}
```

Command fields: `t` text, `l` label, `d` description, `a` admin flag, `p` required power, `v` current string value, `ch` `[value,label]` choices, `arg` picker chain, `o` display order, `role` presentation role, `ui` presentation hints. Exports preserve declaration order by default. Server processing removes raw `commands` from shared `data` and merges permitted entries into the command list.

`Presentation(kind, group, min, max, step, unit, defaultValue, vanilla, role, power)` decorates the preceding builder row. `Status(id,label,value,group)` publishes optional status-window values. Keep all IDs stable.

## Control types

| Description | How to declare it | Interaction |
| --- | --- | --- |
| Action | `Action(...)` | Executes on click. |
| Switch | `OnOff`, `OnOffAuto`, or `Switch` | Choice buttons show the confirmed current value. |
| Checkbox | Switch plus `Presentation("checkbox")` | Sends one explicit on/off choice. Put ON first, OFF second. |
| Value box | `Value(...)` | Edit locally, then SET. |
| Percentage/range slider | Value plus `Presentation("slider", ..., min, max, step, unit)` | Drag locally, then SET once. Units must match the command's grammar. |
| Dropdown | Switch/choices plus `Presentation("dropdown")` | Opens a local choice list; sends the selected value. |
| Read-only value | Value plus `Presentation("readonly")` | Displays without sending. |
| Header/explanation | Action metadata plus `Presentation("header")` / `"explanation"` | Text only; no action is executed. |
| Categories | `Presentation(..., group: "Economy / Rewards")` | Repeated group headings become collapsible sections. |
| Default/vanilla references | `defaultValue` / `vanilla` hints | Current value stays visible; reference values are under expandable DETAILS. |

Large choice sets automatically use a dropdown. Unknown control kinds fall back to the basic value/choice/action behavior rather than executing a new type of instruction. Missing ranges use value boxes; the UI does not guess valid limits.

`Picker` supports comma-separated `player`, `team`, `map`, `mode`, `ban`, `choice` and `text` stages. A trailing `?` makes the final stage optional. The client collects tokens locally and sends the complete command once. BACK returns one stage. A map choice can have a third element containing comma-separated mode names. If player names cannot be addressed unambiguously, use manual input rather than guessing a target.

`Catalog(command)` supplies faction → category → item/price data for direct purchases. The current renderer presents grouped purchase buttons; it does not open chat-menu pages to discover the catalog. `Browse(title)` supplies read-only sections → entries → groups → stat rows, with optional vanilla/note columns and a changes-only filter. See the friend's Buy/UnitBalance exports and our builder for the shapes.

## Per-player exports

`public static string ModUIPlayerData(ulong steamId)` can personalize a frame for the authenticated recipient. The server chooses the ID from the connection; the client cannot name another player.

The friend's existing shape remains supported:

```json
{"v":{"/flex":"on"}}
```

Schema 2 additionally supports a bounded `commands` array for player-specific choices. Those commands must remain under a published root. Each row uses the same fields as shared commands and inherits its parent command's permission restriction. This lets Formation send only the current faction/role's options. Shared exports remain cached; personalized changes produce ordinary mod diffs.

See [Formation's export](../../Silica-1283/si_formation/ModUIData.cs): its selection command includes the existing catalog revision, and execution validates the actual player's role/team/revision through `ApiPanelAction`. Do not bypass those checks. Avoid including credits, timers or rapidly changing cargo in periodic exports unless that traffic is explicitly intended.

## Discovery and fallback

The server lists loaded Melon mods, discovers registered player/admin commands and phrases, and reads optional exports. A mod without any export still appears in the server list; its panel falls back to known commands or a clear no-information message.

For mods providing little information, the shell uses actual registry data, supplied usage descriptions, server menu metadata and recognized feedback received during normal use. It does not execute commands speculatively to crawl menus. An internal subcommand is not discoverable merely because its root is registered.

Server configuration remains `UserData/cfg/Si_ServerModUI/menu.json`. Besides labels/owners/order/categories, it supports `Commands[text].Subs`, `Choices`, `Arg` chains and `AdminOnly`. Optional bundled fallback hints fill missing metadata; existing explicit labels, arguments and choices are retained. Configuration does not create gameplay command handlers or grant authority.

Local client mods can expose matching visible MelonPreferences without a dedicated panel. No arbitrary private fields are read by the generic UI.

## Navigation contract

For legacy chat menus, use numbered, clearly labeled options and advertise supported navigation on every page:

```text
[Example] Main menu
1. Units
2. Settings
/back to return · /close to close · /0 to close
```

Use shared handlers for `/back`, canonical `/close`, and `/0` / `/exit` close aliases. A namespaced `/example close` route should be idempotent. Complete actions run immediately. Value pages must advertise `Set: /N <value>` or `Enter: /N <value>` so numbered choices prompt for input; keep immediate actions on a separate page.

Menu state is per player. Resolve choices against the displayed snapshot and clear stale state on session/team/role/permission changes. Only the owner should consume bare numeric/navigation input. Si_ServerModUI does not create a universal owner coordinator for unmodified third-party menus; do not claim separate IsMenuOpen flags guarantee exclusivity. Our Formation retains scoped ownership checks, supports the split Buy implementation, and adds explicit close routes. The client does not blindly send `/0` when leaving arbitrary third-party panels.

## Performance and permissions

Shared metadata should be deterministic and cheap or cached by config/catalog revision. No uptime/tick counters in shared `info`. Discovery is bounded; the server checks changes about every five seconds and refreshes the invoking player's frame after a command. Opening tabs, browsing, expanding details and editing values does not send requests. SET, switches, selected actions and completed pickers send explicit commands.

Client housekeeping/status work runs about once per second; expensive visible preparation is cached at up to 20 Hz, with command/layout rebuilding on changed inputs. Hidden panels skip preparation. Native IMGUI drawing, input, authorization checks and incoming replies remain immediate; game FPS is not capped.

The server's actual command handlers always authorize actions. Registry-required admin powers cannot be weakened by export decoration. Exports may tighten player-registered tuning commands' visibility. Unregistered internal permissions cannot be inferred: declare accurate flags/powers and keep handler checks. All shared non-command data is public to recipients; do not export tokens, paths, private identities or another player's data. Malformed exports fail closed, and final personalized documents are sanitized before hashing.

Synchronous player-targeted replies through Si_AdminMod's chat/console helpers appear in received feedback. Async/coroutine/broadcast output remains native. Capture overflow remains native. The UI preserves native chat fallback without the companion, including singleplayer/local providers.

Wire transport intentionally retains our `SiSquadHUD/2` marker on packet 126, with frame `v:2` and capability fields. It does not automatically communicate with the friend's renamed `SiModUI/2` companion. Install our matching client/server pair together when switching away from the temporary friend test. Limits, queue bounds, exact reply matching and restart recovery are retained.

## AI implementation prompt

```text
Integrate [MOD SOURCE PATH] with Si_ClientModUI 0.17.0 / Si_ServerModUI 2.0.0.
Read applicable AGENTS.md files and this README, then inspect actual commands,
config fields, permissions and lifecycle before editing. Preserve gameplay and
standalone/native-chat behavior. Never replace our source with an older bundled mod.

For server panels, copy the shared ModUI_Export.cs builder and add the exact-case
public static string ModUIData(). Describe real commands using Action, Switch,
OnOff, Value, Picker, Status, Catalog or Browse. Use Presentation for kind, group,
ranges, units and known default/vanilla values. Declare precise admin powers.
Never invent handlers or defaults. Validate every action in its existing server handler.

Use ModUIPlayerData(ulong steamId) for authenticated player-specific values/choices.
Keep shared JSON public and deterministic. Cache expensive catalog preparation.
Preserve stale-revision/team/role checks. Do not export another player's state.

For client providers, Register(this, Definition), keep IDs stable, use cheap Read
and Available callbacks, and Unregister on unload. Use explicit RequiresInput for
value-taking commands. Preserve shell input isolation, layout/settings and F7.
Default/vanilla details should be expandable. Keep optional UI dependencies isolated.

Preserve native chat fallback and generic discovery when exports are absent.
For legacy menus, use advertised /back, /close, /0 and /exit with shared cleanup,
per-player ownership and snapshot-safe selections. Do not blindly send close commands
or claim unmodified third-party menus share an owner coordinator.

Back up affected sources, build against the installed public Mono/IL2CPP references,
and run relevant authorized checks. Respect mod instructions reserving live tests
for the user. Do not install, start or stop anything unless authorized. Report
changed behavior, actual limitations, build results and focused live-test requirements.
```

## Source references

- [Client API](Client/LocalModControls.cs), [menu structure](Client/ControllerMenu.cs), [reusable controls](Client/StructuredPanels.cs)
- [Catalog validation](Client/CompanionCatalog.cs), [control hints](Client/ControlPresentation.cs), [network lifecycle](Client/CompanionNetwork.cs)
- [Server export parsing](../Si_ServerModUI/Server/Frame.cs), [metadata config](../Si_ServerModUI/Server/MenuConfig.cs), [bounded proxy](../Si_ServerModUI/Server/Proxy.cs)
- [Logistics export](../../Silica-1283/si_logistics/ModUIData.cs), [Formation export](../../Silica-1283/si_formation/ModUIData.cs)
