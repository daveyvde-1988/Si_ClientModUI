# Extending the standalone controller

Add server command definitions locally in UserData/Si_SquadHUD/commands.json, an array such as:
[
  {"Mod":"example","Command":"/example status","Description":"Read status","Role":"shared","Arguments":false},
  {"Mod":"example","Command":"/example","Description":"Enter an argument","Role":"admin","Arguments":true}
]
Catalog presence is informational. All server permissions stay with the native command handler. Do not add automatic polling, arbitrary remote execution or server file reads.

For local SP mods, public ClientModControls.Register(MelonMod owner, IReadOnlyList<Control>) accepts explicit controls:
Control.Id, Label, Description, Read (Func<string>), Apply (Action<string>, optional), Choices (string[], optional).
The mod owns callback validation, persistence and any live toggle cleanup. Register after the HUD API is available; Unregister on teardown. Use local Read delegates; do not hide network requests inside them. The UI only invokes Apply after an explicit user click and only in singleplayer. The API is an opt-in contract; existing mods are not modified to implement it.

Visible MelonPreferences categories whose identifier exactly matches the normalized owning mod assembly name or MelonInfo name are exposed automatically. No ownership is guessed from arbitrary fields, no hidden entries are shown and no generic master switch unpatches a mod. APPLY updates memory via BoxedValue; SAVE PREFS uses native category persistence with verified file backups. A mod may need to opt in if it uses different category identifiers.

The old companion integration APIs are preserved in ../Si_SquadHUD_Companion and are not dependencies of this client. Major 1.0 extraction remains deferred.

