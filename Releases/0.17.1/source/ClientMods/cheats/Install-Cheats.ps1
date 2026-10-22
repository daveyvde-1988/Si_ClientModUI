[CmdletBinding()]
param([string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica',[switch]$PlanOnly,[string]$RollbackReceipt='')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
if(-not $RollbackReceipt){
    & (Join-Path $PSScriptRoot '..\Si_ClientModUI\Install-ModUI.ps1') -Side client -ClientRoot $ClientRoot -IncludeCheats -PlanOnly:$PlanOnly
    exit 0
}
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$client=[IO.Path]::GetFullPath($ClientRoot)
if(@(Get-Process Silica -ErrorAction SilentlyContinue | Where-Object {$_.Path -eq (Join-Path $client 'Silica.exe')}).Count -gt 0){throw 'Close the Silica client before installation. No process will be stopped.'}
if($RollbackReceipt){
    $receipt=Get-Content -LiteralPath $RollbackReceipt -Raw | ConvertFrom-Json
    if($receipt.clientRoot -ne $client){throw 'Receipt targets a different client installation.'}
    foreach($entry in $receipt.files){
        $destination=Join-Path $client ('Mods\'+$entry.name)
        if($entry.name -notin @('cheats.dll','Si_SquadHUD.dll')){throw 'Unexpected receipt filename.'}
        if((Hash $destination) -ne $entry.sha256){throw 'Installed DLL changed since this receipt. Refusing rollback.'}
        if($entry.backup -and (Hash $entry.backup) -ne $entry.oldSha256){throw 'Backup hash mismatch.'}
    }
    if($PlanOnly){$receipt;exit 0}
    foreach($entry in $receipt.files){
        $destination=Join-Path $client ('Mods\'+$entry.name)
        if($entry.backup){Copy-Item -LiteralPath $entry.backup -Destination $destination}
        else{Remove-Item -LiteralPath $destination}
        if($entry.backup -and (Hash $destination) -ne $entry.oldSha256){throw 'Rollback verification failed.'}
    }
    Write-Output 'Restored previous client DLLs. User preferences are preserved.'
    exit 0
}
$release=if(Test-Path -LiteralPath (Join-Path $PSScriptRoot 'manifest.json')){$PSScriptRoot}else{Join-Path $PSScriptRoot 'Releases\0.3.0'}
$manifest=Get-Content -LiteralPath (Join-Path $release 'manifest.json') -Raw | ConvertFrom-Json
foreach($inputFile in $manifest.gameInputs){if((Hash (Join-Path $client $inputFile.path)) -ne $inputFile.sha256){throw 'Client game/reference build changed. Rebuild and review before installing.'}}
foreach($entry in $manifest.files){if((Hash (Join-Path $release ('Mods\'+$entry.name))) -ne $entry.sha256){throw 'Package hash mismatch.'}}
if($PlanOnly){[pscustomobject]@{ClientRoot=$client;Files=$manifest.files;PreserveSettings=$true}|ConvertTo-Json -Depth 8;exit 0}
$backup=Join-Path $release ('installation\'+[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ'))
New-Item -ItemType Directory -Path $backup -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $client 'Mods') -Force | Out-Null
$files=@(foreach($entry in $manifest.files){
    $destination=Join-Path $client ('Mods\'+$entry.name)
    $oldHash=$null;$oldFile=$null
    if(Test-Path -LiteralPath $destination){
        $oldHash=Hash $destination;$oldFile=Join-Path $backup $entry.name
        Copy-Item -LiteralPath $destination -Destination $oldFile
        if((Hash $oldFile) -ne $oldHash){throw 'Backup verification failed.'}
    }
    [ordered]@{name=$entry.name;sha256=$entry.sha256;oldSha256=$oldHash;backup=$oldFile}
})
$receipt=[ordered]@{createdUtc=[DateTime]::UtcNow.ToString('o');clientRoot=$client;complete=$false;files=$files}
$receiptPath=Join-Path $backup 'receipt.json'
$receipt|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receiptPath -Encoding UTF8
try{
    foreach($entry in $files){
        $destination=Join-Path $client ('Mods\'+$entry.name)
        if($entry.oldSha256 -and (Hash $destination) -ne $entry.oldSha256){throw 'Destination changed after backup.'}
        Copy-Item -LiteralPath (Join-Path $release ('Mods\'+$entry.name)) -Destination $destination
        if((Hash $destination) -ne $entry.sha256){throw 'Installed hash mismatch.'}
    }
}catch{
    foreach($entry in $files){
        $destination=Join-Path $client ('Mods\'+$entry.name)
        if($entry.backup){Copy-Item -LiteralPath $entry.backup -Destination $destination}
        elseif((Test-Path -LiteralPath $destination) -and (Hash $destination) -eq $entry.sha256){Remove-Item -LiteralPath $destination}
    }
    throw
}
$receipt.complete=$true
$receipt|ConvertTo-Json -Depth 8|Set-Content -LiteralPath $receiptPath -Encoding UTF8
Write-Output ('Installed cheats 0.3.0 and Mod Menu 0.14.0. Rollback receipt: '+$receiptPath)
