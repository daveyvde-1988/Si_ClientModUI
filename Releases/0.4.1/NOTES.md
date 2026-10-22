# 0.4.1 client render repair

The client disappeared when selecting Centauri because the new decorative scan-line renderer called GUI.DrawTextureWithTexCoords. The current generated IL2CPP binding contains only a NotSupportedException("Method unstripping failed") stub. Client log errors at 18:33:19 and 18:39:13 UTC correspond to Centauri selection. After the error, server status and F7 snapshots continued, but drawingEnabled stayed false across map/reconnect events.

The repair draws the pattern through native GUI.Box with an owned GUIStyle background, isolates decorative failures, retries other render errors after two seconds, restores drawing on scene changes/F6 and logs full exception traces. Related scrolling and drag-control entry points were inspected and resolve to native calls rather than the offending draw stub.

Only client 0.4.1 was built and installed; companion 0.4.0, networking schemas and server settings are unchanged. Build passed with zero warnings/errors. No tests or runtime launches were performed. Live checks: select Centauri, open/close F6 menu, spawn/start a round, reconnect/change map; confirm the header persists. If another error occurs, note time/faction and use F7; new full stack traces support a precise next diagnosis.

Incident evidence/source checkpoint: Backups/Client-render-fix-20261019T184628772Z. Client rollback receipt: Backups/SquadHUD-install-20261019T184756100Z/installation.json. Earlier 0.4.0 archive remains intact.
