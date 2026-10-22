[CmdletBinding()]
param([string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica',[switch]$PlanOnly)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$release=Join-Path $PSScriptRoot 'Releases\0.10.0'
$manifest=Get-Content (Join-Path $release 'manifest.json') -Raw | ConvertFrom-Json
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
foreach($inputFile in @(
    @{path='GameAssembly.dll';hash=$manifest.clientGameAssemblySha256},
    @{path='Silica_Data\il2cpp_data\Metadata\global-metadata.dat';hash=$manifest.clientMetadataSha256},
    @{path='MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll';hash=$manifest.clientInteropSha256}
)){if((Hash (Join-Path $ClientRoot $inputFile.path)) -ne $inputFile.hash){throw 'Client build/references changed; rebuild first'}}
$source=Join-Path $release 'client\Si_SquadHUD.dll'
$destination=Join-Path $ClientRoot 'Mods\Si_SquadHUD.dll'
$expected=@($manifest.files | Where-Object file -eq 'client\Si_SquadHUD.dll')[0].sha256
if((Hash $source) -ne $expected){throw 'Release hash mismatch'}
$companion=Join-Path $workspace 'Mods\Si_SquadHUD_Server.dll'
if($PlanOnly){
    [pscustomobject]@{ClientSource=$source;ClientDestination=$destination;DisableCompanion=(Test-Path $companion);PreserveSettings=$true}|ConvertTo-Json
    exit 0
}
foreach($root in @($workspace,$ClientRoot)){
    if(@(Get-Process Silica -ErrorAction SilentlyContinue | Where-Object {$_.Path -eq (Join-Path $root 'Silica.exe')}).Count -gt 0){throw 'Close the affected client/server before installation. No process will be stopped.'}
}
$stamp=[DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
$backup=Join-Path $workspace ('Backups\SquadHUD-standalone-install-'+$stamp)
New-Item -ItemType Directory $backup | Out-Null
$oldHash=if(Test-Path $destination){Hash $destination}else{$null}
$old=if($null -ne $oldHash){Join-Path $backup 'client-Si_SquadHUD.dll'}else{$null}
if($old){Copy-Item -LiteralPath $destination -Destination $old;if((Hash $old) -ne $oldHash){throw 'Client backup mismatch'}}
$retired=$null
if(Test-Path $companion){
    $hash=Hash $companion
    $serverBackup=Join-Path $backup 'server-Si_SquadHUD_Server.dll'
    Copy-Item -LiteralPath $companion -Destination $serverBackup
    if((Hash $serverBackup) -ne $hash){throw 'Companion backup mismatch'}
    $disabled=Join-Path $workspace ('ClientMods\Si_SquadHUD_Companion\Disabled\Si_SquadHUD_Server-0.8.1-'+$stamp+'.dll')
    New-Item -ItemType Directory (Split-Path $disabled -Parent) -Force | Out-Null
    $retired=[ordered]@{original=$companion;disabled=$disabled;backup=$serverBackup;sha256=$hash}
}
$record=[ordered]@{
    schema=2;createdUtc=[DateTime]::UtcNow.ToString('o');complete=$false;gameLaunched=$false
    files=@([ordered]@{side='client';root=$ClientRoot;destination=$destination;source=$source;sha256=$expected;oldSha256=$oldHash;backup=$old})
    retiredCompanion=$retired
}
$recordPath=Join-Path $backup 'installation.json'
$record | ConvertTo-Json -Depth 8 | Set-Content $recordPath -Encoding UTF8
if($oldHash -and (Hash $destination) -ne $oldHash){throw 'Installed client changed after backup'}
Copy-Item -LiteralPath $source -Destination $destination
if((Hash $destination) -ne $expected){throw 'Installed client mismatch'}
if($retired){
    if((Hash $retired.original) -ne $retired.sha256){throw 'Companion changed after backup'}
    Move-Item -LiteralPath $retired.original -Destination $retired.disabled
    if((Hash $retired.disabled) -ne $retired.sha256){throw 'Disabled companion mismatch'}
}
$record.complete=$true
$record | ConvertTo-Json -Depth 8 | Set-Content $recordPath -Encoding UTF8
New-Item -ItemType Directory (Join-Path $PSScriptRoot 'installation') -Force | Out-Null
[ordered]@{record=$recordPath;side='client';completedUtc=[DateTime]::UtcNow.ToString('o')} |
    ConvertTo-Json | Set-Content (Join-Path $PSScriptRoot 'installation\latest-standalone.json') -Encoding UTF8
Write-Output ('Installed standalone client and preserved disabled companion. Receipt: '+$recordPath)

