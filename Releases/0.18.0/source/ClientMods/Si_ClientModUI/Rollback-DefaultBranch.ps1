[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$InstallRecord)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$record=Get-Content -LiteralPath $InstallRecord -Raw | ConvertFrom-Json
if($record.schema -ne 4 -or !$record.complete){throw 'Expected a completed default-branch installation receipt'}
$allowed=@('Si_ClientModUI.dll','Si_AntiCheat.dll','Si_Buy.dll','Si_CrabCannon.dll','Si_EloMatchmaking.dll','Si_HeavyTeleporter.dll','Si_KingOfTheHill.dll','Si_Leaderboard.dll','Si_MapBalance.dll','Si_ModUI_Server.dll','Si_PacketChunker.dll','Si_RTS_AI.dll','Si_Spawn.dll','Si_StarterUnits.dll','Si_UnitBalance.dll')
foreach($item in $record.files){
    $path=[IO.Path]::GetFullPath($item.destination);$mods=[IO.Path]::GetFullPath((Join-Path $item.root 'Mods'))
    if((Split-Path $path -Parent) -ne $mods -or (Split-Path $path -Leaf) -notin $allowed){throw 'Unexpected rollback destination'}
    if((Hash $path) -ne $item.sha256){throw 'Installed DLL changed; inspect before rollback'}
    if($item.backup -and (Hash $item.backup) -ne $item.oldSha256){throw 'Backup hash mismatch'}
}
foreach($root in @($record.files.root | Select-Object -Unique)){
    if(@(Get-Process Silica,SilicaServerCommander -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq $root}).Count){throw 'Close the affected client/server before rollback'}
}
foreach($item in $record.retired){
    $path=[IO.Path]::GetFullPath($item.original);$mods=[IO.Path]::GetFullPath((Join-Path $item.root 'Mods'))
    if((Split-Path $path -Parent) -ne $mods -or (Split-Path $path -Leaf) -notin @('Si_ServerModUI.dll','Si_SquadHUD_Server.dll','Si_ModUI.dll','Si_SquadHUD.dll')){throw 'Unexpected retired companion path'}
    if(Test-Path -LiteralPath $path){throw 'Companion destination already exists'}
    if((Hash $item.backup) -ne $item.sha256){throw 'Retired companion backup hash mismatch'}
}
$stamp=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
foreach($item in $record.files){
    Move-Item -LiteralPath $item.destination -Destination ($item.destination+'.disabled-rollback-'+$stamp)
    if($item.backup){Copy-Item -LiteralPath $item.backup -Destination $item.destination;if((Hash $item.destination) -ne $item.oldSha256){throw 'Restore verification failed'}}
}
foreach($item in $record.retired){Copy-Item -LiteralPath $item.backup -Destination $item.original;if((Hash $item.original) -ne $item.sha256){throw 'Companion restore verification failed'}}
Write-Output 'Previous DLLs restored. Configuration files and UI settings preserved.'
