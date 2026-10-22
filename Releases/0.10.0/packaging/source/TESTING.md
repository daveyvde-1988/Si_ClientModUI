# User-owned live checks: clean Mod menu 0.10.0

No automated tests were added/run, and no game/server was launched/stopped.

- Start the client as a new user of the menu. Confirm neutral default geometry, notifications off, no selected status values and no imported mod command catalog or header metrics.
- Open Help. Confirm only commands known for this user/server/current role appear. Use ADD ENTRY to save a command, restart, and confirm it is remembered for that Steam user and server.
- Click a command. It should only fill the draft. Edit arguments, then SEND or press Enter. Confirm one send, the menu remains open, and native chat does not open. Holding Enter should not repeat the command. Close the menu and confirm normal native chat input is restored.
- Check Player/Commander separation when changing roles. Admin and server debug controls should appear only with native admin access; losing admin access must remove them and reject an old prepared admin entry.
- The sidebar button should have no effect. Status should explain that no values exist until a mod registers them.
- Loaded client mods should have their own tabs. A mod with no exposed controls should show the empty state. Registered groups/choices/status/notifications should come from that mod's definition.
- Open debug. Empty is expected when no mod has supplied/advertised options; the UI sends no query to populate it.
- Change server/round/team/role and verify transient feedback/choices/drafts clear. Old UserData/Si_SquadHUD preferences are never read by this version.

Read accessible local UserData/Si_ModMenu session logs and MelonLoader logs directly when investigating findings.

