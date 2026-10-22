# Temporary friend ModUI test installation — 21 October 2026

Active server UI: friend's `Si_ModUI_Server.dll` (MelonInfo 1.3.1).
Active client UI: friend's matching `Si_ModUI.dll` (MelonInfo 1.3.1).
Our Si_ServerModUI and Si_ClientModUI are disabled/backed up for this test. Client cheats.dll is also disabled because its provider references our client UI assembly. Our development sources are unchanged.

Updated server mods: Si_Buy, Si_CrabCannon, Si_EloMatchmaking, Si_HeavyTeleporter, Si_KingOfTheHill, Si_Leaderboard, Si_RTS_AI and Si_UnitBalance. Installed the bundle's server UI metadata at UserData/cfg/ModUI/menu.json. Existing gameplay settings were preserved; mods may migrate their configs on first startup.

Logistics, Formation and Ranks DLLs were excluded and hash-verified unchanged. All unrelated server DLLs and existing configs were verified unchanged. Backups include 584 server configuration files and 12 client configuration files.

The supplied DLLs came from the friend's beta/test environment; both local game installations remain on the public branch. Hash verification establishes installation integrity, not runtime compatibility. No server/game was started or stopped.

## Known integration interactions to watch

- Current Formation's menu ownership probe expects KingOfTheHill._buyStates. The bundle moves Buy into Si_Buy, so Formation's chat menu can fail closed.
- Current Logistics' commander-score purchase exclusion expects KingOfTheHill.TryPurchase. With purchases moved into Si_Buy, the hook may fail and commander scoring can be blocked.
- The current Formation does not support the friend's automatic /0 close behavior.

These are reasons to observe carefully during this temporary test; no gameplay source was modified to work around them.

## Receipts and rollback

- [Server installation receipt](../../Backups/Friend-ModUI-test-20261021T161350081/installation.json)
- [Client installation receipt](../../Backups/Friend-ModUI-client-test-20261021T161445678/installation.json)
- [Restore previous server DLLs](../../Backups/Friend-ModUI-test-20261021T161350081/Restore-Server.ps1)
- [Restore previous client UI and cheats provider](../../Backups/Friend-ModUI-client-test-20261021T161445678/Restore-Client.ps1)

With Silica and the server closed, run the matching Restore script using PowerShell with ExecutionPolicy Bypass if needed. Restore-Server also restores/removes the installed UI menu.json according to its pre-install state. Its optional -RestoreConfigs switch restores all snapshotted server configs, which can overwrite changes made during testing. It does not remove new config files created by mods during the test. Client rollback retains user settings.

For testing, open the matching client UI with F6. Verify initial server detection and mod load logs first. Test ordinary-player and admin menus separately. Do not interpret an installed DLL as a successful live test.
