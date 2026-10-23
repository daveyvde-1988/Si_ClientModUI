[CmdletBinding()]
param(
    [ValidateSet('client','server','both')][string]$Side='both',
    [string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica',
    [string]$ServerRoot='',
    [switch]$PlanOnly
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
if(!$ServerRoot){$ServerRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))}
$ClientRoot=[IO.Path]::GetFullPath($ClientRoot);$ServerRoot=[IO.Path]::GetFullPath($ServerRoot)
$release=Join-Path $PSScriptRoot 'Releases/0.18.0'
$manifest=Get-Content -LiteralPath (Join-Path $release 'manifest.json') -Raw | ConvertFrom-Json
if($manifest.schema -ne 4 -or $manifest.marker -ne 'SiModUI/2'){throw 'Unexpected release manifest'}
$serverNames=@('Si_AntiCheat.dll','Si_Buy.dll','Si_CrabCannon.dll','Si_EloMatchmaking.dll','Si_HeavyTeleporter.dll','Si_KingOfTheHill.dll','Si_Leaderboard.dll','Si_MapBalance.dll','Si_ModUI_Server.dll','Si_PacketChunker.dll','Si_RTS_AI.dll','Si_Spawn.dll','Si_StarterUnits.dll','Si_UnitBalance.dll')
$plan=@();$retire=@()
foreach($targetSide in @('client','server')){
    if($Side -ne 'both' -and $Side -ne $targetSide){continue}
    $root=if($targetSide -eq 'client'){$ClientRoot}else{$ServerRoot}
    foreach($inputFile in @($manifest.gameInputs | Where-Object side -eq $targetSide)){
        if((Hash (Join-Path $root $inputFile.path)) -ne $inputFile.sha256){throw "$targetSide game/reference changed; rebuild and review first"}
    }
    $names=if($targetSide -eq 'client'){@('Si_ClientModUI.dll')}else{$serverNames}
    foreach($name in $names){
        $relative=$targetSide+'/Mods/'+$name
        $entry=@($manifest.files | Where-Object file -eq $relative)
        if($entry.Count -ne 1){throw "Missing or duplicate release file: $relative"}
        $source=Join-Path $release $relative
        if((Hash $source) -ne $entry[0].sha256){throw "Release hash mismatch: $relative"}
        $plan+=@{side=$targetSide;root=$root;source=$source;destination=(Join-Path $root ('Mods/'+$name));sha256=$entry[0].sha256;oldSha256=$null;backup=$null}
    }
    $oldNames=if($targetSide -eq 'client'){@('Si_ModUI.dll','Si_SquadHUD.dll')}else{@('Si_ServerModUI.dll','Si_SquadHUD_Server.dll')}
    foreach($name in $oldNames){
        $path=Join-Path $root ('Mods/'+$name)
        if(Test-Path -LiteralPath $path){$retire+=@{side=$targetSide;root=$root;original=$path;sha256=Hash $path;disabled=$null;backup=$null}}
    }
}
function CheckProcesses {
    foreach($root in @($plan.root | Select-Object -Unique)){
        if(@(Get-Process Silica,SilicaServerCommander -ErrorAction SilentlyContinue | Where-Object {$_.Path -and (Split-Path $_.Path -Parent) -eq $root}).Count){throw 'Close the affected client/server before installing. No process was stopped.'}
    }
}
CheckProcesses
if($PlanOnly){[ordered]@{side=$Side;files=$plan;retire=$retire;logistics='preserved';formation='preserved';settings='preserved'} | ConvertTo-Json -Depth 6;exit 0}
$stamp=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
$backup=Join-Path $PSScriptRoot ('installation/'+$stamp+'-default-branch')
New-Item -ItemType Directory -Path $backup | Out-Null
foreach($item in $plan){
    if(Test-Path -LiteralPath $item.destination){
        $item.oldSha256=Hash $item.destination
        $item.backup=Join-Path $backup ($item.side+'-'+(Split-Path $item.destination -Leaf))
        Copy-Item -LiteralPath $item.destination -Destination $item.backup
        if((Hash $item.backup) -ne $item.oldSha256){throw 'Backup verification failed'}
    }
}
foreach($item in $retire){
    $item.disabled=$item.original+'.disabled-'+$stamp
    $item.backup=Join-Path $backup ($item.side+'-retired-'+(Split-Path $item.original -Leaf))
    Copy-Item -LiteralPath $item.original -Destination $item.backup
    if((Hash $item.backup) -ne $item.sha256){throw 'Companion backup verification failed'}
}
$receipt=Join-Path $backup 'installation.json'
$record=[ordered]@{schema=4;createdUtc=[DateTime]::UtcNow.ToString('o');complete=$false;rolledBack=$false;marker='SiModUI/2';files=$plan;retired=$retire}
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $receipt -Encoding UTF8
$applied=@();$retired=@()
try {
    CheckProcesses
    foreach($item in $plan){
        if($item.oldSha256 -and (Hash $item.destination) -ne $item.oldSha256){throw 'Destination changed after backup'}
        if(!$item.oldSha256 -and (Test-Path -LiteralPath $item.destination)){throw 'A new destination appeared after preflight'}
        $applied+=,$item
        Copy-Item -LiteralPath $item.source -Destination $item.destination
        if((Hash $item.destination) -ne $item.sha256){throw 'Installed DLL verification failed'}
    }
    foreach($item in $retire){
        if((Hash $item.original) -ne $item.sha256){throw 'Companion changed after backup'}
        Move-Item -LiteralPath $item.original -Destination $item.disabled
        $retired+=,$item
    }
    $record.complete=$true
}
catch {
    $installError=$_
    foreach($item in $applied){
        if($item.backup){Copy-Item -LiteralPath $item.backup -Destination $item.destination}
        elseif(Test-Path -LiteralPath $item.destination){Move-Item -LiteralPath $item.destination -Destination ($item.destination+'.failed-'+$stamp)}
    }
    foreach($item in $retired){Copy-Item -LiteralPath $item.backup -Destination $item.original}
    $record.rolledBack=$true
    throw $installError
}
finally {$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $receipt -Encoding UTF8}
Write-Output ('Installed and hash-verified '+$plan.Count+' DLLs; retired '+$retire.Count+' overlapping UI DLL(s). Receipt: '+$receipt)
