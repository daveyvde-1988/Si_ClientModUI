[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$InstallRecord,
    [ValidateSet('client','server','both')][string]$Side = 'both')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$record = Get-Content -LiteralPath $InstallRecord -Raw | ConvertFrom-Json
$files = @($record.files | Where-Object { $Side -eq 'both' -or $_.side -eq $Side })
$retired=$null
if($Side -in @('server','both') -and $record.PSObject.Properties.Name -contains 'retiredCompanion'){$retired=$record.retiredCompanion}
if($retired){
    if(Test-Path -LiteralPath $retired.original){throw 'Companion destination already exists; inspect before rollback.'}
    if((Get-FileHash -LiteralPath $retired.disabled -Algorithm SHA256).Hash.ToLowerInvariant() -ne $retired.sha256){throw 'Disabled companion changed; inspect before rollback.'}
    $serverRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
    if([IO.Path]::GetFullPath($retired.original) -ne (Join-Path $serverRoot 'Mods\Si_SquadHUD_Server.dll')){throw 'Invalid companion restore destination.'}
    if(@(Get-Process Silica -ErrorAction SilentlyContinue | Where-Object {$_.Path -eq (Join-Path $serverRoot 'Silica.exe')}).Count -gt 0){throw 'Close the server before rollback.'}
}
foreach ($file in $files) {
    $root = (Resolve-Path -LiteralPath $file.root).Path
    $path = [IO.Path]::GetFullPath($file.destination)
    if (-not $path.StartsWith((Join-Path $root 'Mods') + '\', [StringComparison]::OrdinalIgnoreCase) -or
        (Split-Path $path -Leaf) -notin @('Si_SquadHUD.dll','Si_SquadHUD_Server.dll','Si_Ranks.dll','Si_Formation.dll')) { throw 'Invalid rollback destination.' }
    if (@(Get-Process Silica -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq (Join-Path $root 'Silica.exe') }).Count -gt 0) { throw 'Close the affected game/server first.' }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $file.sha256) { throw 'Installed DLL changed; inspect it before rollback.' }
    if ($null -ne $file.backup -and (Get-FileHash -LiteralPath $file.backup -Algorithm SHA256).Hash.ToLowerInvariant() -ne $file.oldSha256) { throw 'Backup hash mismatch.' }
}
$stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
foreach ($file in $files) {
    Move-Item -LiteralPath $file.destination -Destination ($file.destination + '.disabled-' + $stamp)
    if ($null -ne $file.backup) {
        Copy-Item -LiteralPath $file.backup -Destination $file.destination
        if ((Get-FileHash -LiteralPath $file.destination -Algorithm SHA256).Hash.ToLowerInvariant() -ne $file.oldSha256) { throw 'Restored DLL verification failed.' }
    }
}
if($retired){
    Copy-Item -LiteralPath $retired.disabled -Destination $retired.original
    if((Get-FileHash -LiteralPath $retired.original -Algorithm SHA256).Hash.ToLowerInvariant() -ne $retired.sha256){throw 'Restored companion hash mismatch.'}
}
Write-Output 'Rollback complete. Updated files were preserved under .disabled names; previous DLLs restored where present.'
