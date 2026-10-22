# cheats 0.3.2 installed with Si_ClientModUI 0.16.0

Adds resizable console-entry/feedback UI and cached native Help measurements, with the corrected client dependency. Gameplay commands and shortcut files are preserved. Verified receipt: ClientMods/Si_ClientModUI/installation/20261020T175713587Z/installation.json. See ../Si_ClientModUI/FOLLOWUP_0.16.0.md. Live verification pending.

# cheats 0.3.0 installed - 20 October 2026

Installed with Mod Menu 0.14.0. Installed DLLs, both previous-DLL backups, source manifests and package checksum were verified. Existing shortcut definitions and layouts were preserved. Diagnostics remains disabled.

- cheats.dll SHA-256: b8a83686916bbf4b0217ec7736165a894838e2908b270bceca72874eaf9924dd
- Si_SquadHUD.dll SHA-256: 07abaa2b122ccaa8be9660bdeca9757d51c6ee40465f2cc22cb6a99a18746e9b

The reported native Spawn FormatException was found in the client's Latest.log and preserved in Backups/ModMenu-team-debug-camera-20261020T144952073Z/client-Latest.log. Native spawn uses culture-sensitive float.Parse, while Camera exports dot decimals. Numeric dispatch now uses integer mantissas and exponents, tested for exact float round trips in English and Belgian locales. Dynamic current view continues to resolve a fresh Camera snapshot plus 100 metres on every invocation. Full failures are logged; the menu shows a concise failure notice.

SunScale, TimeScale, DayScale and MoonScale now use -1/+1. NetTickMult retains -0.1/+0.1.

Release builds: zero warnings/errors. 189 command/UI-policy checks passed, including locale-sensitive native numeric parsing, repeated dynamic view resolution, overlay visibility, admin-section visibility, legacy sidebar migration, team isolation and persisted scopes. Live verification is pending. No game/server process was launched or stopped. No server mods were changed.

Receipt: Releases/0.3.0/installation/20261020T145601218Z/receipt.json
Package: Releases/0.3.0/cheats-0.3.0-client.zip
Source/settings checkpoint: Backups/ModMenu-team-debug-camera-20261020T144952073Z

Rollback with the client closed:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/cheats/Install-Cheats.ps1 -RollbackReceipt ClientMods/cheats/Releases/0.3.0/installation/20261020T145601218Z/receipt.json
```

This restores cheats 0.2.0 and Mod Menu 0.13.0. User settings are retained. If the new menu has already assigned old sidebar bindings to a team, the older menu ignores the extra TeamScope field; the original layout also has a timestamped .before-team-sidebar backup next to it.

Previous installation record follows.

# cheats 0.2.0 installed - 20 October 2026

Installed with Mod Menu 0.13.0 in the Silica client's Mods folder. Both installed DLL hashes and rollback backups were verified, along with current source manifests and the client package checksum. Existing shortcut JSON and sidebar layouts were preserved.

- cheats.dll: 023c4dafe59a192b3dd56391bfa584030419e7a8089911e38d54ad4219e571ba
- Si_SquadHUD.dll: 898d61b7e4a758fc3d3637218e2cf585391af1dc6a95548eed8433ea176f4557

Dynamic current view saves a spawn template. Every invocation executes native Camera, reads its new clipboard pose, moves 100 metres along camera forward including pitch, and dispatches the resolved spawn command. The same button uses a new view after repositioning. Existing fixed-coordinate shortcuts remain fixed until edited or recreated.

Enable/Disable and numeric/resource +/- controls execute immediately. The editable used-console-entry bar, session history and Create sidebar button are available in Controls. Commands shows the complete native Help registry as searchable text.

Release builds passed with zero warnings/errors; 100 checks passed across three cultures, including repeated dynamic execution and yaw/pitch offsets. Live game verification is pending. No client/server process was launched or stopped, and no server mod was changed.

Receipt: Releases/0.2.0/installation/20261020T141824066Z/receipt.json
Source checkpoint: Backups/Cheats-dynamic-view-20261020T141121066Z
Package: Releases/0.2.0/cheats-0.2.0-client.zip

With the client closed, rollback from the workspace:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/cheats/Install-Cheats.ps1 -RollbackReceipt ClientMods/cheats/Releases/0.2.0/installation/20261020T141824066Z/receipt.json
```

This restores cheats 0.1.0 and Mod Menu 0.12.0. User settings are preserved. Diagnostics remains disabled separately.

Previous installation record follows.

# Installed - 20 October 2026

Installed in `C:\Program Files (x86)\Steam\steamapps\common\Silica\Mods`:

- `cheats.dll` 0.1.0: `5b33222958b7a62b67002d601c96b51e287233bcbf06904f1a753c84313d5554`
- `Si_SquadHUD.dll` 0.12.0: `10c08617258b702e6f5fd1cc96d9d0f32d94201e8232319fd55f2be7050d8bd1`

Verified installed hashes, the previous menu DLL backup, package contents, and all recorded current cheats/menu source hashes. The install receipt is [receipt.json](Releases/0.1.0/installation/20261020T132923056Z/receipt.json). The receipt records removal of the newly installed cheats DLL and restoration of the previous menu DLL for rollback.

Source checkpoint: `Backups/Cheats-client-20261020T132203168Z`.

Release builds passed with zero warnings/errors. All 64 command checks passed across three cultures. All 31 catalog entries match the focused managed game extraction for coverage and server/local routing. The installer checked the current client game/metadata/interop hashes against its manifest before copying.

No server mods, user command JSON, layouts, game inputs, or loader configuration were changed. No game/server process was launched or stopped. Live verification remains pending; use the checklist in [README.md](README.md).

Open F6 twice -> cheats -> Controls. Use Saved commands and sidebar shortcuts to create a console action; choose its symbol in the existing sidebar editor and enable the persistent sidebar there.

With the client closed, rollback from the workspace:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ClientMods/cheats/Install-Cheats.ps1 -RollbackReceipt ClientMods/cheats/Releases/0.1.0/installation/20261020T132923056Z/receipt.json
```

The client package is [cheats-0.1.0-client.zip](Releases/0.1.0/cheats-0.1.0-client.zip). It includes both required DLLs, manifest, installer and instructions. User-created shortcuts/settings remain separate from DLL rollback.

