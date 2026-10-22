[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$InstallRecord,
    [ValidateSet('client','server','both')][string]$Side = 'both')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$record = Get-Content -LiteralPath $InstallRecord -Raw | ConvertFrom-Json
$files = @($record.files | Where-Object { $Side -eq 'both' -or $_.side -eq $Side })
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
Write-Output 'Rollback complete. Updated files were preserved under .disabled names; previous DLLs restored where present.'
