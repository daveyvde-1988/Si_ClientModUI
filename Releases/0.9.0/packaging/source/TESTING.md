# User-owned live verification for standalone 0.9.0

No automated tests were added or run. No game/server was launched or stopped.

With the standalone client installed and the preserved companion disabled:
- Join the existing server, use F6, and confirm the mod tabs and Help are clickable. Opening/scrolling tabs should not send chat commands.
- Open Unit Balance through /b or !b in its tab. Confirm all numbered choices on a single native response line appear, /back works, and EDIT/SEND supports a value such as /1 1.5 or a native .N prompt.
- Use /rank, /formation status and /buy; compare feedback and age-labelled status with native responses. No value should become current just from clicking a command.
- Confirm player chat stays native, notifications toggle locally, and geometry/status selections survive restart separately for commander/FPS.
- Check the commander sidebar's command buttons and received choices. It intentionally has no continuously authoritative lock/selection highlight.
- In singleplayer, confirm the SP MODS tab lists the unchanged Diagnostics mod. It has no exposed controller; future mods can expose preferences or register controls.
- Change role/team/round/server and confirm received choices and old status clear. Manually entering a native chat command should invalidate old numbered choices.

If anything fails, read the accessible client HUD session log and MelonLoader log directly. Runtime findings take precedence over source-based expectations.

