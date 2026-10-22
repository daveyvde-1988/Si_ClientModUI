# Mod menu 0.10.0

No companion protocol or catalog resource is compiled. There are no predefined mod commands, mod names, native command prefix adapters or status parsers. The only menu-origin chat send is Player.CurrentPlayer.SendChatMessage, reached from SEND or a physical Enter press on a prepared, currently permitted entry. There are no automatic queries/retries.

Native ChatMessage reception checks the authenticated connected server and native server-message/team display flags, bounds packet size, restores the reader position and falls back on malformed data. Optional notifications divert the recognized server-message presentation; ordinary player chat retains the native path. Singleplayer server-message logging can be observed locally. Local owner-published notices use a bounded main-thread queue.

Received text remains memory-only. Generic command hints, explicitly advertised alternative tokens and explicit slash-number rows may supply inert draft choices. No bare-number prefix is invented. Unclassified remote access is kept unknown. Explicit admin labels and server debug entries are admin-only; a server-supplied structured permission protocol remains future work.

Native Player.AdminLevel (NONE versus STANDARD/ROOT/HOST) supplies current admin visibility; singleplayer is local-host access. This is UI gating, with fresh checks before sends/Apply and status-cache invalidation on role/admin changes. Remote handlers retain actual command authorization. User Help definitions are local, per-Steam-user, server-scoped references.

Input isolation uses the current interop GameInput.GetAction(Global, Chat/ChatChannel) APIs. The menu records their enabled states before switching input context, disables them while open, reapplies suppression after GameInput.UpdateInput and restores states on close. Restoration waits for held Enter to release. Keyboard wasPressedThisFrame submits once per press; IMGUI Return/KeypadEnter is consumed before text fields. Native invocation wrappers were inspected under inspection/shell-api; actual game behavior remains user-owned runtime verification.

No gameplay mod was inspected to obtain a built-in catalog for this release. The earlier catalog is archived and excluded from source/resources. The old companion stays separate and disabled.

