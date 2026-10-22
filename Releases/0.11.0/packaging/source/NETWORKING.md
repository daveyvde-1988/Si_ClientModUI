# Communication/input: 0.11.0

All menu-origin chat goes through Player.CurrentPlayer.SendChatMessage following explicit SEND/physical Enter or a user click on a sidebar command. Text is passed unchanged (single line, <=160 characters); no catalog membership is required for raw chat. There are no automatic requests, retries, heartbeats or companion packets. Client provider Invoke callbacks belong to the owning mod and occur only after a click.

Native server-message feedback/optional notification routing retains authenticated peer, display scope, bounds, reader restoration and native fallback checks. Player chat stays native. Generic feedback only supplies transient references; it is not added to the export.

created-commands.json contains schema 1 and a Commands array (Mod, Command, Description, Role, AdminOnly, Debug, Group). Export sources are explicit saved Help definitions and custom sidebar commands, plus definitions explicitly imported from that file. Scope/Steam IDs/native delegates are omitted. Imports use portable scope, never auto-execute, and retain role/admin UI gating. No file is created when there are no user-created/imported definitions. Malformed import files are not overwritten automatically.

Input isolation now captures every action in CurrentPlayerInput.actions and CurrentAsset.currentActions.asset before entering the menu. It disables those actions, blocks their action/map Enable calls while open, suppresses native ProcessInput/button polls and reapplies guards after UpdateInput. The shell itself reads raw keyboard/mouse and IMGUI events. Original action states restore after closing and releasing held keys. No input bindings/preferences are rewritten.

Text size scales the GUI matrix and logical screen geometry; overlays convert raw mouse coordinates into logical units. Transparency only changes rendered surfaces. Team colors come from replicated Player.Team.TeamColor; this adds no traffic. API declarations/wrappers for GameInputAsset, GUI.HorizontalSlider/GUI.matrix and Matrix4x4.Scale were inspected in inspection/sidebar-api. Compilation/static inspection do not establish runtime behavior.

No game/server process was launched or stopped and no tests were run. Live typing, sidebar hit targets, scaling and native input restoration are pending user verification.

