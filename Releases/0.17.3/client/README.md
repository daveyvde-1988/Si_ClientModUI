# Mod menu follow-ups: 0.17.3 / cheats 0.4.1

Client-only update, 22 October 2026. The installed server addon and gameplay mods require no change. Includes the previously prepared 0.17.2 / cheats 0.4.0 changes: native command reference in COMMANDS, 50 metre dynamic spawning and detailed cheats statuses. Existing fixed coordinates and shortcut IDs remain intact.

- Client and server mod navigation sorts alphabetically by displayed name, ignoring case.
- Removing a custom command forgets every saved server copy and its portable copy, and records a per-user deletion marker. Older JSON/imports and sidebar exports cannot silently restore that command-list entry. Explicitly adding the same command again clears its marker. Sidebar buttons retain their own separate removal controls.
- The old removal code only removed saved copies for the current server. Another saved copy could remain and be exported again on profile load; shared exports are automatically read back into the command list. This is why duplicate custom entries could return.
- Requested cleanup removes the three custom definitions: Ranks `/rank`, Ranks `/promote` and Yo mama `/woops`. It cleans both saved-command files and the shared export, with verified backups. Actual server-advertised Ranks functionality is preserved.
- Cheats appears under MODS -> CLIENT MODS -> cheats -> PLAYER while the game reports cheats enabled. Administrators retain ADMIN access and the enable/disable control. Non-admin attempts to toggle cheats are rejected, including saved/typed console entries. Access is rechecked on invocation and revoked when cheats turn off. Commands still use the native console dispatcher, including its individual privilege checks.
- The native command reference appears under COMMANDS -> CLIENT MODS -> cheats, using the same access categories. EXTRA PANEL does not appear under MODS. The interactive controls stay under MODS.
- Commands displays a short mod description from the client's provider/assembly metadata, supplied server information, or accessible command descriptions. Missing descriptions are stated explicitly; no guessed mod catalog is added.
- Centauri surfaces are darker. Command buttons no longer stretch to the height of their explanations. Action, read-only and editable control rows measure the content they display instead of reserving 112/150 pixels for every row. Default/vanilla details expand with enough room for their text.

## Validation

Both affected Release builds succeeded with zero warnings and zero errors. No tests were added or run. No game/server was started or stopped. Runtime/visual verification remains user-owned.

Source/settings checkpoint: `Backups/ModUI-menu-followups-20261022T164103646Z`. Release DLLs, source copies, ZIP and installed/rollback copies are SHA-256 verified during packaging/deployment. Installation status and receipts are recorded in INSTALLATION_STATUS.md.

## User live checks

Check alphabetic navigation; confirm the custom Ranks/Yo mama entries stay absent after reconnect/restart/importing an old shared file; remove another custom entry and revisit it. Existing sidebar geometry, bindings, status selections and unrelated custom commands should remain.

With a normal player and admin, enable cheats, open PLAYER controls/reference, execute a permitted command, then disable cheats and verify panel/sidebar access ends. Only administrators should see or invoke the enable/disable control. Native permissions may reject particular commands even while cheats is enabled.

Check the darker Centauri background, compact action rows, long descriptions/labels, editable controls, and expanded DETAILS at different menu widths/text sizes. Confirm the Commands reference handles Enter without submitting hidden chat text.
