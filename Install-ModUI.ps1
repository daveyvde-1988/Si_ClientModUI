[CmdletBinding()]
param([ValidateSet('client','server','both')][string]$Side='client',
    [string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica',
    [string]$ServerRoot='',
    [switch]$IncludeCheats,[switch]$IncludeGameplayProviders,[switch]$PlanOnly,
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')][string]$ReleaseVersion='0.16.0')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if([string]::IsNullOrWhiteSpace($ServerRoot)){$ServerRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))}
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$release=Join-Path $PSScriptRoot ('Releases\'+$ReleaseVersion)
$manifest=Get-Content -LiteralPath (Join-Path $release 'manifest.json') -Raw|ConvertFrom-Json
$plan=@();$retire=@()
foreach($targetSide in @('client','server')){
    if($Side -ne 'both' -and $Side -ne $targetSide){continue}
    $root=[IO.Path]::GetFullPath($(if($targetSide -eq 'client'){$ClientRoot}else{$ServerRoot}))
    $checks=if($targetSide -eq 'client'){$manifest.gameInputs}else{$manifest.serverInputs}
    foreach($inputFile in $checks){if((Hash (Join-Path $root $inputFile.path)) -ne $inputFile.sha256){throw "$targetSide game/reference build changed; rebuild first."}}
    $names=if($targetSide -eq 'client'){@('Si_ClientModUI.dll')}else{@('Si_ServerModUI.dll')}
    if($targetSide -eq 'client' -and ($IncludeCheats -or (Test-Path -LiteralPath (Join-Path $root 'Mods\cheats.dll')))){$names=@($names)+@('cheats.dll')}
    if($targetSide -eq 'server' -and $IncludeGameplayProviders -and $manifest.PSObject.Properties.Name -contains 'gameplayProviders'){$names=@($names)+@($manifest.gameplayProviders)}
    foreach($name in $names){
        $relative=if($name -eq 'cheats.dll'){'client\OptionalProviders\cheats.dll'}elseif($name -in @('Si_Logistics.dll','Si_Formation.dll')){'server\GameplayProviders\'+$name}else{$targetSide+'\Mods\'+$name}
        $entry=@($manifest.files|Where-Object file -eq $relative)[0]
        $source=Join-Path $release $relative
        if((Hash $source) -ne $entry.sha256){throw 'Release hash mismatch'}
        $plan+=@{side=$targetSide;root=$root;source=$source;destination=(Join-Path $root ('Mods\'+$name));sha256=$entry.sha256}
    }
    $legacyNames=if($targetSide -eq 'client'){@('Si_SquadHUD.dll','Si_ModUI.dll')}else{@('Si_SquadHUD_Server.dll','Si_ModUI_Server.dll')}
    foreach($legacy in $legacyNames){
        $oldPath=Join-Path $root ('Mods\'+$legacy)
        if(Test-Path -LiteralPath $oldPath){$retire+=@{original=$oldPath;sha256=Hash $oldPath;side=$targetSide;root=$root}}
    }
}
if($PlanOnly){[ordered]@{files=$plan;retire=$retire;preserveSettings=$true}|ConvertTo-Json -Depth 8;exit 0}
foreach($root in @($plan.root|Select-Object -Unique)){
    if(@(Get-Process Silica -ErrorAction SilentlyContinue|Where-Object {$_.Path -eq (Join-Path $root 'Silica.exe')}).Count -gt 0){throw 'Close the affected client/server before installing. No process will be stopped.'}
}
$stamp=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
$backup=Join-Path $PSScriptRoot ('installation\'+$stamp)
New-Item -ItemType Directory $backup -Force|Out-Null
foreach($item in $plan){
    New-Item -ItemType Directory (Split-Path $item.destination -Parent) -Force|Out-Null
    $item.oldSha256=$null;$item.backup=$null
    if(Test-Path -LiteralPath $item.destination){
        $item.oldSha256=Hash $item.destination;$item.backup=Join-Path $backup ($item.side+'-'+(Split-Path $item.destination -Leaf))
        Copy-Item -LiteralPath $item.destination -Destination $item.backup
        if((Hash $item.backup) -ne $item.oldSha256){throw 'Backup verification failed'}
    }
}
foreach($item in $retire){
    $item.disabled=$item.original+'.disabled-'+$stamp
    $item.backup=Join-Path $backup ($item.side+'-legacy-'+(Split-Path $item.original -Leaf))
    Copy-Item -LiteralPath $item.original -Destination $item.backup
    if((Hash $item.backup) -ne $item.sha256){throw 'Legacy backup verification failed'}
}
$record=[ordered]@{schema=3;createdUtc=[DateTime]::UtcNow.ToString('o');complete=$false;files=$plan;retired=$retire}
$receipt=Join-Path $backup 'installation.json'
$record|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receipt -Encoding UTF8
foreach($item in $plan){
    if($item.oldSha256 -and (Hash $item.destination) -ne $item.oldSha256){throw 'Destination changed after backup'}
    Copy-Item -LiteralPath $item.source -Destination $item.destination
    if((Hash $item.destination) -ne $item.sha256){throw 'Installed DLL verification failed'}
}
foreach($item in $retire){
    if((Hash $item.original) -ne $item.sha256){throw 'Legacy DLL changed after backup'}
    Move-Item -LiteralPath $item.original -Destination $item.disabled
}
$record.complete=$true;$record|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receipt -Encoding UTF8
Write-Output ('Installed '+$Side+'. Preferences preserved. Receipt: '+$receipt)
