# Current 0.8.1 user live verification is pending

Both builds and installation hash checks passed. No tests or game/server starts/stops were performed. During your next normal session, the changed behavior to verify is:

- NO LIVE SWITCH stays grey during admin background refresh. Tabs retain steady styling in read-only Unitbalance, while server actions and permissions remain guarded.
- Enable notifications: Ranks announcements and native server messages should enter MOD NOTIFICATIONS; ordinary player chat remains in chat. X restores ordinary notice delivery. There should be small gaps between entries and no divider rules/decorative separator-only lines.
- Header says Mod status, its expanded navigation button says MOD MENU [F6], and selected rows show only Label: value.
- Resize/move windows separately as FPS and commander, switch back and restart: each role should retain its own geometry. Existing shared placement seeds both roles without resetting other preferences.
- A new/default profile starts with every status unselected; existing explicit choices should migrate. Expand + module groups in the selector, choose values, collapse and reopen; counts, choices and group expansion should persist. Future providers remain unselected.

Read logs directly if anything differs. Compilation/installation does not confirm runtime behavior. Older checklists below are historical where superseded.
# Current 0.8.0 live verification is pending (user-owned)

Compilation and installation/hash checks passed; no tests or game/server launches/stops were performed. The earlier checklists remain historical. The 0.8.0 behavior to verify during your next normal session is:

- Keep a commander sidebar enabled across full-menu close, chat, pause, alt-tab, team/role changes and reconnect. It should return only in a valid commander context; X is the explicit off control. Compare selection/lock with the main Formation page, without alternating shading during polls.
- Drag titles and bottom-right // grips for the main menu, sidebar, collapsed header, expanded status, status selector and notices. Close/reopen/restart client and change resolution; saved geometry should stay reachable. F6 provides FPS users a cursor while overlays remain visible; move the main menu aside if needed. Overlay clicks/drags should not issue underlying orders.
- Admin headers should match DLL basenames. Expand + for Logistics and Crab Cannon, check native states/permissions, and restore any deliberately changed switches. Parent OFF should still prevent child operation; unsupported modules still show Loaded / no switch.
- Select status parameters (toolbar left of sidebar), inspect expanded status and saved choices. Ordinary players should never receive Crab Cannon's admin-only detail rows. Unavailable values should not appear as invented zeroes.
- Enable notification bar, observe private/team/broadcast mod notices with two users where convenient, and ordinary player chat in chat. CLEAR should clear history; X should restore native mod chat. Notice fallbacks and the native Unit Balance editor's separate capture are described in NETWORKING.md. Position/size should persist; notice text should not persist on disk or cross connection contexts.

Read client UserData/Si_SquadHUD/session-*.log and MelonLoader/Latest.log, plus server MelonLoader/Latest.log directly. No automatic/live verification is implied by these expected behaviors.

## First: disappearance regression

- [ ] Start the client and confirm Silica Squad HUD 0.5.0. Select Centauri, the confirmed triggering faction; the header must remain visible after spawning/round start.
- [ ] Press F6 twice, inspect the full menu, close it, then reconnect/change map. Check Centauri again, plus Sol/Alien if convenient.
- [ ] If a menu error recurs, note faction/time/action and press F7 once. The client log now includes full stack traces and renderer-recovery messages; logs can be read directly from this PC. No configuration reset or new extraction is required.

# 0.4.0 live checklist

Launch the dedicated server and client normally and start a genuine new round. Confirm HUD client **0.5.0** and companion **0.5.0** without patch errors. The build/install task did not start/stop programs or toggle mods.

- [ ] Collapsed header is below the native top-right menu with a gap. First F6 expands status, second opens the full menu; F6/Escape/Close collapses it again. F7 and Diagnostics F8/F9 still work.
- [ ] Drag the full window by its title bar. Controls must not fire while dragging. Close/reopen and restart the client: position should persist for this Steam user. Resize the window/change resolution and verify the menu remains on-screen; Close stays reachable.
- [ ] Check distinct root tabs, selection underlines and brighter text. Inspect SOL slate, Centauri burgundy with subtle scan lines, and Alien olive in neutral lighting. Check opacity on bright/dark terrain and no clipped text at the normal resolution.
- [ ] Every list uses a side scrollbar/mouse wheel; no page controls remain. Scroll to the very last Formation, rank, command and admin entry. Refresh and check lists are complete, without duplicates or mixed old/new snapshots.
- [ ] Buy initially shows production facilities. Choose one and compare all units/prices with the existing KoH menu. Return to facilities and pick another. Selecting facilities spends nothing; changing facility cancels an unconfirmed purchase.
- [ ] Buy/Confirm an inexpensive unit at a valid location. Expect one native KoH receipt, one spawn and correct credit deduction. Cancel spends nothing. Check insufficient funds, cooldown, early Shrimp lock and location refusal still behave normally. No new numeric chat-menu session should open.
- [ ] Ranks is a non-clickable table with only this team's titles, all normal tiers and traitor/Deserter/Outcast. Compare score, cap, bonus, category/exclusion and seat values with active config. Read the traitor explanation and verify its cooldown/reward rules. Opening it cannot promote, demote or award credits.
- [ ] Help is a non-clickable command table under FPS Player, Commander and Admin. No /2, /back or similar navigation rows. Check Formation subcommands, rank commands and the distinction between slash /demote (Ranks) and !demote (Commander Management).
- [ ] With a Generic admin account, Admin appears and lists loaded server mods. Reviewed switches show ON/OFF; unsupported mods say Loaded / No live switch. Ordinary players must not see the Admin tab or obtain switch authority.
- [ ] If appropriate in a spare test round, toggle a reviewed mod off and back on. Check the native response, actual behavior and status update. Formation is match-scoped; other reviewed switches follow their existing command persistence. Restore the intended state. Do not toggle core/server infrastructure or expect unsupported mods to unload.
- [ ] Locked Formation, confirmed AI dismissal, rank/status, squad quantity, score and credits still work. Check team/role/round transitions, reconnect, chat/pause, alt-tab and close during dragging/loading. Controls/cursor must restore; old lists and confirmations must not cross scopes.

Capture faction, role, resolution, action and session time for issues. Client logs: UserData/Si_SquadHUD and MelonLoader/Latest.log; server: MelonLoader/Latest.log. Full-list transfer, dragging, visuals and live switches remain user-owned verification; compilation is not runtime evidence.

## Commander lock option - 0.4.2

- [ ] Confirm HUD client/server 0.5.0 and Formation 2.15.1. As commander, find Lock formation in the list with square/line/etc.
- [ ] Hover over it for more than two seconds: requested movement/slow-speed description appears. Ordinary layout choices still have no tooltip.
- [ ] Click when Unlocked: it becomes Locked and arms the next move, matching /formation lock. Close menu, select at least three eligible ground AI and issue a normal move. Reopen and check Locked while travelling, then Unlocked after arrival/release.
- [ ] Arm and click again before issuing a move: pending lock cancels. Unlock an active group and check native cleanup/restored speed behavior. Other commanders' groups must remain unaffected.
- [ ] Check team/role/round transitions clear scoped state; FPS lock, layout selection and Centauri rendering remain intact.

## Sidebar and roster update - 0.5.0 (user live verification)

- [ ] Main menu/disconnected/loading: no mod header or menu. In a loaded match: header says Mod menu, with no faction prefix.
- [ ] Commander Formations -> Sidebar: narrow symbol strip starts at middle left. Drag the top grip, return with MENU/F6, reopen and verify placement. Restart client and check this Steam user's placement; resize and keep it on-screen.
- [ ] Compare every symbol/tooltip/selected state with the full commander list for Sol, Centauri and Alien. Native N, small/large filled shapes, thin/thick lines, open outlines, padlock. Alien Circle is filled and Open circle is hollow. Hover descriptions take two seconds.
- [ ] Choose layouts and arm/cancel/release commander lock from the sidebar; verify authoritative state updates. Outside it, select units/move camera/issue orders normally. Clicking/scrolling/dragging sidebar must not send underlying orders. FPS players have no Sidebar control.
- [ ] Watch commander Formations and sidebar for at least ten seconds: background polling must not alternate light/dark or flash loading text. Lock status must still update. Check scope cleanup on role/team/round/reconnect and native menus, chat and alt-tab.
- [ ] Inspect Help/Ranks in every faction: themed roster surfaces, thin dividers, readable wrapped text. Help says Mod and names the registering mod; verify Formation and Ranks extras and the /demote versus !demote attribution.
## Historical Unit Balance batch editor - 0.6.0 (superseded for new clients)

- [ ] Confirm HUD client/server 0.6.0 and unchanged Unit Balance 7.7.1. No adapter/hash errors. With two updated clients, join and inspect UNITBALANCE without issuing any commands: categories, every unit/structure, global rules, projectile settings and available vanilla baselines. Public values are locked, including on the admin account.
- [ ] As Generic admin, Admin -> Unit Balance -> CONFIGURE. Compare the JSON override values and grouped tree with the public view/reference editor. Edit a numeric and a boolean field, browse another unit, return, and DISCARD EDITS: server JSON must not change.
- [ ] In a spare round, save a small reviewed edit batch using both confirmation clicks. Verify the timestamped backup in UserData/UnitBalance_cfg/SquadHUD_Backups, unchanged private fields/other settings, and pending status in Configure. The public views must retain the previously published configuration until reload.
- [ ] Confirm RELOAD SAVED JSON. Check native logs/behavior, both clients' new cached values, and receipt while the second client's menu is closed. Native Discord auto-post settings still apply on reload. The implementation/build task did not exercise this action.
- [ ] Test invalid numeric inputs, fractional integer fields, stale concurrent-admin edits and close/role/team/round/disconnect during staging. No partial batches, unauthorized saves, duplicate commits or silently applied stale field IDs. Unsaved edits clear on a changed revision with an explanatory message.
- [ ] Confirm native !rebalance is observed and refreshes clients. If intentionally using !rebalance default, public configuration must show defaults with the native-state explanation; understand that independent native hooks may still apply rules.
- [ ] In a spare round, test Unit Balance's native enabled switch and restore intended state. Public Unitbalance tab hides/restores; admin Configure remains. OFF does not promise all native hooks/live units are vanilla. Similarly check Formation/KoH switches hide/restore only their related tabs on both clients, including while that tab/sidebar is open.
- [ ] Check long names, info strings, smaller resolutions, every faction style, search/scroll, editable focus, DEFAULT behavior, and cursor/control restoration. Existing commander sidebar/lock, Centauri rendering, Buy, Ranks and Help must remain intact.
- [ ] Ordinary accounts cannot access Configure or save/reload/switch. Revoke admin power during editing and verify controls/requests lose authority. A new server/team/round must not inherit the previous connection's snapshot or staged batch.

Restore intended balance settings after your chosen live checks. No automated tests or game/server launches are authorized by this checklist.
## Current 0.7.0 user live checklist

The old 0.6.0 batch editor checklist above is historical. Current admin editing uses the native menu, not the old SAVE JSON batch buttons.

- [ ] Confirm client/server 0.7.0, Unit Balance 7.7.1 and Starter Units 1.0.0 load without adapter/hash/Harmony errors. In Sol/Centauri/Alien, search the unchanged read-only Unitbalance browser: dark input, white text, readable focus/selection. Public values stay locked.
- [ ] Generic admin -> Unit Balance -> CONFIGURE. Compare Root and submenus with native /b: factions/categories/unit groups, JSON, HTP, Cap&Decay and Strategystyle. Navigate with clicks, BACK and ROOT. No duplicate menu chatter should be printed to normal chat. Native chat/stats state must remain independent.
- [ ] In a spare test round, set one small parameter with SET; inspect the native Save/Cancel/Save+rebalance confirmation. Cancel first, then save a reviewed edit. Inspect the verified pre-write backup in UnitBalance_cfg/SquadHUD_MenuBackups. Check native validation/refusal and immediate toggles match chat behavior. Restore intended values afterward.
- [ ] Confirm reload and compare public read-only caches on two updated clients, including a closed menu. Native configured Discord auto-post behavior is retained. Private Discord GUI configuration is disabled; no webhook URL should appear in client data/logs.
- [ ] Change JSON through another admin/chat while a confirmation is open. A stale choice must not execute; the menu should reject it or reopen Root. Close/role/team/round/reconnect/admin-revocation must not cause delayed commits or inherit old UI state.
- [ ] Admin -> Starter Units -> CONFIGURE: inspect all faction/wildlife entries and 4-way rules. Browse Add Unit catalog; cancel an add/count/removal and confirm JSON is unchanged. In a spare round, save a reviewed change and verify backup, unrelated keys and file hash. New units should only appear through the mod's normal next-round spawn pass. Restore intended settings.
- [ ] Check Starter Units From/At factions, add/remove rules, per-rule units, zero counts, invalid/fractional/large counts, missing faction files, concurrent admin changes, and pagination when needed. There is no live respawn or master switch.
- [ ] Watch stable commander lists/sidebar and other tabs: no flashing/loading; unchanged polls should produce small acknowledgements, while lock/selection/admin state changes still appear promptly. Cached lists must not cross connection/team/role scopes.
- [ ] Read server log HUD transfer scheduled and HUD upload interval lines during your normal play. Compare menu browsing with the old full snapshot pattern. Counters exclude native/Steam overhead, normal game traffic and Unit Balance's native override sync.
- [ ] Verify ordinary accounts cannot open/configure/save either admin menu, and existing Buy/Ranks/Help/sidebar/input behavior remains intact. All live actions are user-owned; this checklist does not authorize automated tests or game/server control.