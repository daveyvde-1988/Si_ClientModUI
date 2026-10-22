[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$InstallRecord)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$record=Get-Content -LiteralPath $InstallRecord -Raw|ConvertFrom-Json
if($record.schema -ne 3){throw 'Use the historical rollback script for this receipt.'}
foreach($item in $record.files){
    $mods=[IO.Path]::GetFullPath((Join-Path $item.root 'Mods'))
    $path=[IO.Path]::GetFullPath($item.destination)
    if((Split-Path $path -Parent) -ne $mods -or (Split-Path $path -Leaf) -notin @('Si_ClientModUI.dll','Si_ServerModUI.dll','cheats.dll','Si_Logistics.dll','Si_Formation.dll')){throw 'Unexpected rollback destination'}
    if(@(Get-Process Silica -ErrorAction SilentlyContinue|Where-Object {$_.Path -eq (Join-Path $item.root 'Silica.exe')}).Count -gt 0){throw 'Close the affected client/server before rollback'}
    if((Hash $path) -ne $item.sha256){throw 'Installed DLL changed; inspect before rollback'}
    if($item.backup -and (Hash $item.backup) -ne $item.oldSha256){throw 'Backup hash mismatch'}
}
foreach($item in $record.retired){
    $mods=[IO.Path]::GetFullPath((Join-Path $item.root 'Mods'))
    $original=[IO.Path]::GetFullPath($item.original)
    $allowed=if($item.side -eq 'client'){@('Si_SquadHUD.dll','Si_ModUI.dll')}elseif($item.side -eq 'server'){@('Si_SquadHUD_Server.dll','Si_ModUI_Server.dll')}else{throw 'Unexpected rollback side'}
    if((Split-Path $original -Parent) -ne $mods -or (Split-Path $original -Leaf) -notin $allowed){throw 'Unexpected legacy restore destination'}
    if(Test-Path -LiteralPath $item.original){throw 'Legacy destination already exists'}
    if((Hash $item.disabled) -ne $item.sha256){throw 'Disabled legacy DLL changed'}
}
$stamp=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
foreach($item in $record.files){
    Move-Item -LiteralPath $item.destination -Destination ($item.destination+'.disabled-'+$stamp)
    if($item.backup){Copy-Item -LiteralPath $item.backup -Destination $item.destination;if((Hash $item.destination) -ne $item.oldSha256){throw 'Restore hash mismatch'}}
}
foreach($item in $record.retired){Copy-Item -LiteralPath $item.disabled -Destination $item.original;if((Hash $item.original) -ne $item.sha256){throw 'Legacy restore hash mismatch'}}
Write-Output 'Previous DLLs restored; preferences preserved.'
