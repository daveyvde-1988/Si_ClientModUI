# User-owned live checks: 0.11.0

No automated tests were added or run. No game/server was started/stopped.

- Open Mod menu, type ordinary text and unlisted commands in the bottom bar, and send with Enter/SEND. Confirm the menu remains open, native controls/menus do not react, holding Enter does not repeat and native input returns after closing/releasing held keys.
- Open sidebar, add a labeled custom command with a symbol/color and enable its persistent checkbox. Close the menu; click the icon and confirm one immediate submission. Restart and verify enable state, buttons and geometry remain.
- Check letters/digits, filled/open shapes, padlock and pictograms. Edit/remove/toggle individual buttons. Test a mod-provided action/icon only if one is actually registered; absence should produce an empty MOD SUPPORT list.
- Change transparency/text size in Settings. Verify persistence, scaled hit targets, readable text and team coloring across native factions. This is entirely client-side.
- As commander, browse Player and Commander Help. As admin, browse all three. Revoke admin status and confirm restricted controls stop appearing/invoking.
- Save a Help entry or custom sidebar command. Inspect UserData/Si_ModMenu/created-commands.json, copy it to another client and import/rejoin. Confirm portable references work without companion data. Typed chat and learned feedback should not be exported. A fresh profile with no definitions should create no command JSON.
- Change role/server/round. Sidebar enable state persists; unavailable provider actions remain disabled and old transient feedback clears.

Read local UserData/Si_ModMenu session logs and MelonLoader logs directly for findings. Menu-open logs include the isolated native action count.

