# Server maintenance patch - 19 October 2026

Client 0.1.0 and protocol 1 remain unchanged. Installed server DLLs:

- Formation 2.14.2: selected custom Follow works with one or two eligible AI followers. Zero followers still clean up state. Move/Attack/commander small-selection behavior is unchanged.
- Ranks 1.4.3: `EnforceHarvesterRecruitmentBan=false` disables both recruitment guards and built-in harvester/Shrimp exclusions. Direct control remains native. Explicit rank/category restrictions remain.
- HUD Server 0.1.1: reads the optional Ranks guard state and AntiGrief's actual recruitment preferences, so the HUD continues to show externally enforced exclusions accurately.

AntiGrief's existing `Grief_BlockShrimpFPSCommanding` and `Grief_BlockHarvesterFPSCommanding` remain true. `Grief_BlockShrimpTakeOver` remains false. `[SAM]` messages were traced to its invitation check through Admin Mod's chat helper; server log lines 2317/2318 recorded rejected Shrimp invitations.

All three builds passed with zero warnings/errors. No tests were added/run and no process was started/stopped for this patch. Gameplay verification remains with the user, in accordance with Formation's working instructions.

DLL backup/rollback record: `Backups/SquadHUD-install-20261019T151244883Z/installation.json`.
Previous Ranks config: `Backups/Ranks-recruitment-owner-config-20261019T151246016Z/Si_Ranks_Config.json`.
Additional source/log backups: `Backups/Formation-small-follow-20261019T150704348Z` and `Backups/Harvester-rule-owner-20261019T150839586Z`.

Live checklist after starting the server: confirm all three versions; use a distinctive Follow formation with one AI follower, two, three, then dismiss down to zero; check lock/unlock and native Follow selection; verify harvesters/Shrimp remain recruit-blocked by AntiGrief but usable by direct control. The original live HUD/transport test was confirmed working by the user; these two fixes still need a focused retest.
