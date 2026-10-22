[CmdletBinding()]
param([string]$ClientRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Silica')
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$release = Join-Path $PSScriptRoot 'Releases\0.8.1'
$null = New-Item -ItemType Directory -Path $release -Force
function Hash([string]$Path) { (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant() }
$inputs = @(
    @{ source = (Join-Path $PSScriptRoot 'Client\bin\Release\net6.0\Si_SquadHUD.dll'); file = 'client\Si_SquadHUD.dll' },
    @{ source = (Join-Path $PSScriptRoot 'Server\bin\Release\netstandard2.1\Si_SquadHUD_Server.dll'); file = 'server\Si_SquadHUD_Server.dll' },
    @{ source = (Join-Path $workspace 'Silica-1283\si_formation\bin\Release\netstandard2.1\Si_Formation.dll'); file = 'server\Si_Formation.dll' }
)
$files = @(foreach ($inputFile in $inputs) {
    $destination = Join-Path $release $inputFile.file
    $null = New-Item -ItemType Directory -Path (Split-Path $destination -Parent) -Force
    Copy-Item -LiteralPath $inputFile.source -Destination $destination
    [PSCustomObject]@{ file = $inputFile.file; bytes = (Get-Item -LiteralPath $destination).Length; sha256 = Hash $destination }
})
$sources = @(foreach ($folder in @('Client','Server','Shared')) {
    Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot $folder) -File | Where-Object { $_.Extension -in @('.cs','.csproj') } |
        ForEach-Object { [PSCustomObject]@{ file = $_.FullName.Substring($PSScriptRoot.Length + 1); sha256 = Hash $_.FullName } }
})
$record = [ordered]@{
    schema = 1; preparedUtc = [DateTime]::UtcNow.ToString('o'); hudVersion = '0.8.1'; clientVersion = '0.8.1'; ranksVersion = '1.4.3'; formationVersion = '2.15.1'
    serverCoreSha256 = Hash (Join-Path $workspace 'Silica_Data\Managed\SilicaCore.dll')
    clientGameAssemblySha256 = Hash (Join-Path $ClientRoot 'GameAssembly.dll')
    clientMetadataSha256 = Hash (Join-Path $ClientRoot 'Silica_Data\il2cpp_data\Metadata\global-metadata.dat')
    clientInteropSha256 = Hash (Join-Path $ClientRoot 'MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll')
    kohSha256 = Hash (Join-Path $workspace 'Mods\Si_KingOfTheHill.dll')
    adminModSha256 = Hash (Join-Path $workspace 'Mods\Si_AdminMod.dll')
    providers = @(foreach($name in @('Si_Formation','Si_Ranks','Si_Logistics','Si_CrabCannon','Si_HeavyTeleporter','Si_UnitBalance','Si_StarterUnits')) { @{ file = ('Mods\'+$name+'.dll'); sha256 = Hash (Join-Path $workspace ('Mods\'+$name+'.dll')) } })
    files = $files; sources = $sources
    validation = 'Client/server 0.8.1 compile cleanly. Stable disabled/tab appearance, native server-chat notification fallback, spacing-only notifications, role-specific geometry, default-empty positive status selection with explicit legacy-choice migration and expandable groups. Gameplay mods and settings retained; no tests or game/server start/stop actions. User live verification pending.'
}
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $release 'manifest.json') -Encoding UTF8
Write-Output $release
