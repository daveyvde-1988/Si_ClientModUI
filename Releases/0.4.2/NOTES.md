# 0.4.2 commander lock option

Adds Lock formation to the commander's scrollable Formation list, next to layout choices. Locked is authoritative when a one-move lock is armed or an owned locked group is active; Unlocked otherwise. Click toggles through the existing commander /formation lock and /formation unlock handlers, with normal role, catalog revision, main-thread and active-service checks. Existing movement algorithms, minimum three eligible AI, one-move scope and cleanup remain unchanged.

The two-second hover description begins: "units attempt to stay in formation trough travel, formation will move slow." It also explains next-move/unlock scope. Ordinary formation choices remain without descriptions. A separate commander-lock group marker distinguishes this UI control from numeric formation choices without reserving a catalog option number. Old Formation APIs do not advertise the control. Commander state polls once per second while open.

Client/companion 0.4.2 and Formation 2.15.1 built with zero warnings/errors. Client rendering fix 0.4.1 is retained. Installed with verified backups; no tests or game/server launches performed. See ../../INSTALLATION_STATUS.md and ../../TESTING.md for live checks and rollback receipt.
