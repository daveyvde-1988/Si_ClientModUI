[CmdletBinding()]
param([ValidateSet('client','server','both')][string]$Side = 'both',
    [string]$ClientRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Silica', [switch]$PlanOnly)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$client = (Resolve-Path -LiteralPath $ClientRoot).Path
$release = Join-Path $PSScriptRoot 'Releases\0.8.1'
$manifest = Get-Content -LiteralPath (Join-Path $release 'manifest.json') -Raw | ConvertFrom-Json
function Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
$targets = @()
if ($Side -in @('server','both')) {
    if ((Hash (Join-Path $workspace 'Silica_Data\Managed\SilicaCore.dll')) -ne $manifest.serverCoreSha256) { throw 'Server build changed since preparation; rebuild first.' }
    $targets += @{ side = 'server'; root = $workspace; file = 'server\Si_SquadHUD_Server.dll'; destination = (Join-Path $workspace 'Mods\Si_SquadHUD_Server.dll') }
    $targets += @{ side = 'server'; root = $workspace; file = 'server\Si_Formation.dll'; destination = (Join-Path $workspace 'Mods\Si_Formation.dll') }
}
if ($Side -in @('client','both')) {
    if ((Hash (Join-Path $client 'GameAssembly.dll')) -ne $manifest.clientGameAssemblySha256 -or
        (Hash (Join-Path $client 'Silica_Data\il2cpp_data\Metadata\global-metadata.dat')) -ne $manifest.clientMetadataSha256 -or
        (Hash (Join-Path $client 'MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll')) -ne $manifest.clientInteropSha256) {
        throw 'Client build/references changed since preparation; regenerate/rebuild first.'
    }
    $targets += @{ side = 'client'; root = $client; file = 'client\Si_SquadHUD.dll'; destination = (Join-Path $client 'Mods\Si_SquadHUD.dll') }
}
$running = @(Get-Process Silica -ErrorAction SilentlyContinue)
foreach ($target in $targets) {
    if (-not $PlanOnly -and @($running | Where-Object { $_.Path -eq (Join-Path $target.root 'Silica.exe') }).Count -gt 0) { throw "Close the $($target.side) before installation. No process will be stopped automatically." }
    $entry = @($manifest.files | Where-Object { $_.file -eq $target.file })
    if ($entry.Count -ne 1) { throw 'Invalid package manifest.' }
    $target.source = Join-Path $release $target.file
    if ((Hash $target.source) -ne $entry[0].sha256) { throw 'Staged DLL hash mismatch.' }
    $target.sha256 = $entry[0].sha256
    $target.oldSha256 = if (Test-Path -LiteralPath $target.destination) { Hash $target.destination } else { $null }
}
if ($PlanOnly) {
    $targets | ForEach-Object { [PSCustomObject]@{ Side = $_.side; Destination = $_.destination; ReplacesExisting = ($null -ne $_.oldSha256) } } | ConvertTo-Json
    exit 0
}
$targets = @($targets | Where-Object { $_.oldSha256 -ne $_.sha256 })
if ($targets.Count -eq 0) { Write-Output 'Installed DLL hashes already match this release.'; exit 0 }
$stamp = [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssfffZ')
$backup = Join-Path $workspace ('Backups\SquadHUD-install-' + $stamp)
$null = New-Item -ItemType Directory -Path $backup
$records = @(foreach ($target in $targets) {
    $old = $null
    if ($null -ne $target.oldSha256) {
        $old = Join-Path $backup ($target.side + '-' + (Split-Path $target.destination -Leaf))
        Copy-Item -LiteralPath $target.destination -Destination $old
        if ((Hash $old) -ne $target.oldSha256) { throw 'Backup verification failed.' }
    }
    [PSCustomObject]@{ side = $target.side; root = $target.root; destination = $target.destination; source = $target.source; sha256 = $target.sha256; oldSha256 = $target.oldSha256; backup = $old }
})
$install = [ordered]@{ createdUtc = [DateTime]::UtcNow.ToString('o'); complete = $false; files = $records; gameLaunched = $false }
$recordPath = Join-Path $backup 'installation.json'
$install | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding UTF8
foreach ($file in $records) {
    if (Test-Path -LiteralPath $file.destination) {
        if ((Hash $file.destination) -ne $file.oldSha256) { throw 'Destination changed after backup; stop and inspect install record.' }
    } elseif ($null -ne $file.oldSha256) { throw 'Existing destination disappeared after backup.' }
    Copy-Item -LiteralPath $file.source -Destination $file.destination
    if ((Hash $file.destination) -ne $file.sha256) { throw 'Installed DLL verification failed.' }
}
$install.complete = $true
$install | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $recordPath -Encoding UTF8
$stateRoot = Join-Path $PSScriptRoot 'installation'
$null = New-Item -ItemType Directory -Path $stateRoot -Force
[ordered]@{ record = $recordPath; side = $Side; completedUtc = [DateTime]::UtcNow.ToString('o') } |
    ConvertTo-Json | Set-Content -LiteralPath (Join-Path $stateRoot ('latest-' + $Side + '.json')) -Encoding UTF8
Write-Output "Installed and verified $($records.Count) DLLs. Backup/rollback record: $recordPath"
