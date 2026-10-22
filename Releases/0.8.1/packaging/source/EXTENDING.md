# 0.8.1 adapter/UI notes (supersede older details below)

Use raw window keys when calling LayoutRect, PlaceWindow and OverlayContains; RoleWindowKey resolves them consistently to -fps / -commander keys. Never pass an already suffixed key. Geometry migrates by copying the old shared entry once per role. Positive SelectedStatus is default-empty; legacy HiddenStatus exists only for one-time explicit-choice migration. ExpandedStatusGroups stores local module expansion. Keep new provider IDs stable and unselected until chosen.

Button keepAppearance is explicit for transient state. Never use it for permanently unsupported controls. Tab visuals stay stable while their actual allowed guard still prevents actions during pending work. Notification native fallback is in the client's authenticated ProcessMessage prefix and uses the native server-message flag, not player-text prefixes. Restore stream position in finally and fail back on unknown/malformed input. Do not reintroduce a hook on every gameplay packet. Keep the reviewed server AdminMod recipient bridge separate from UI text formatting.
# Extending the mod menu

The client renders data; the companion owns permissions, native handler adapters and published state. Gameplay mods remain independent. Keep new adapters narrow and review the installed binary before exposing private interfaces. Do not allow arbitrary client-supplied command strings, reflection paths or JSON filenames.

## Windows

Client/WindowPlacement.cs owns LocalLayout and named LocalWindow geometry. Use LayoutRect with a unique stable key, bounded minimum dimensions and normalized defaults; draw WindowSurface then PlaceWindow. Reserve the title's drag area from actual buttons, and keep a bottom-right 18px resize grip clear. Event capture must respect the active movingWindow, pointer focus and window stacking so one overlay cannot steal another's drag or issue native orders. Render all tab/editor contents inside the main menu frame unless they genuinely need a separate window. Ensure the F6 cursor can reach persistent overlays. Mark placementDirty and SaveWindowPosition after changing a local preference. Never send geometry to the server or mutate native GUI skins/textures.

## Admin controls

Server/AdminControls.cs lists DLL basenames and explicit Item adapters. A child has a unique Key and Parent equal to its root DLL title. MenuCatalog maps Parent to child:<title>; the client handles + expansion locally while retaining server row numbers. Both root and child actions require current Generic permission/revision, and known handlers must be pinned through Verified. Invoke original cleanup/save behavior. For toggle-only commands, read/recheck the current state before toggling toward a requested boolean. Never treat Loaded as ON, unload arbitrary mods, or create unsupported switches. Parent OFF still governs a child's effective behavior.

## Status providers

Server/StatusProviders.cs holds read-only allowlisted Provider entries, stable IDs, module names, display labels and player-scoped reads. RegisterFields is for reviewed numeric/boolean fields only; avoid private endpoints/secrets. Add custom providers for derived/team/player values. Mark AdminOnly when the original status command requires Generic. Report Unavailable or Not active when a value is unknown, rather than inventing zero. Ordinary players receive only public providers; no client choice grants access. Existing base HUD providers remain authoritative for rank/squad/recruitment/credits.

US status messages support <=128 parameter rows, bounded chunked sequences, full snapshots and changed-value deltas. The client commits only complete sequences and keeps local hidden IDs. Do not create a poll per selected checkbox or send full balance JSON to refresh a status value. New provider IDs should be stable across label changes so users retain choices.

## Notifications

Server/ModNotifications.cs owns reviewed delivery interception, scoped native broadcast capture and origin detection. ServerHud owns authenticated option leases, per-recipient delivery and counters. Only accepted replacement delivery suppresses the original. Preserve native recipient/team/private scope, fail back to native chat, restore nested/thread-local scopes in finally/finalizer, and guard recursion. Unknown transports stay untouched until reviewed; never classify ordinary player chat solely by text prefix. Do not capture console messages as chat or bypass NativeAdminMenus' private menu capture. New private hooks need a reviewed binary hash and updated reference/install checks. UI notice history is capped and memory-only.

## Build/release

Follow the user's instructions: compile affected projects; do not add/run tests or control the game/server unless explicitly requested. Prepare-Release.ps1 stages binaries/source hashes. Install-SquadHUD.ps1 backs up, refuses affected running programs and verifies copies; unchanged gameplay DLLs are skipped. Preserve gameplay/config hashes, client preferences, historical releases and rollback receipts. Compile success is not live verification. Authoritative Formation remains Silica-1283/si_formation; do not move AntiGrief's recruitment ownership into this UI. Major 1.0 extraction is deferred.
