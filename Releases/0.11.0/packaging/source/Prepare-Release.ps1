[CmdletBinding()]
param([string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$release=Join-Path $PSScriptRoot 'Releases\0.11.0'
if(Test-Path (Join-Path $release 'manifest.json')){throw 'Release already prepared. Preserve it; use a new version for changes.'}
New-Item -ItemType Directory (Join-Path $release 'client') -Force | Out-Null
$source=Join-Path $PSScriptRoot 'Client\bin\Release\net6.0\Si_SquadHUD.dll'
$destination=Join-Path $release 'client\Si_SquadHUD.dll'
Copy-Item -LiteralPath $source -Destination $destination
if((Hash $source) -ne (Hash $destination)){throw 'Release copy failed verification'}
$files=@([ordered]@{file='client\Si_SquadHUD.dll';sha256=Hash $destination;bytes=(Get-Item $destination).Length})
$sources=@(foreach($folder in @('Client')){
    Get-ChildItem (Join-Path $PSScriptRoot $folder) -File | Where-Object {$_.Extension -in '.cs','.csproj','.json'} |
        ForEach-Object {[ordered]@{file=$_.FullName.Substring($PSScriptRoot.Length+1);sha256=Hash $_.FullName}}
})
$record=[ordered]@{
    schema=2;clientVersion='0.11.0';preparedUtc=[DateTime]::UtcNow.ToString('o');companionRequired=$false
    clientGameAssemblySha256=Hash (Join-Path $ClientRoot 'GameAssembly.dll')
    clientMetadataSha256=Hash (Join-Path $ClientRoot 'Silica_Data\il2cpp_data\Metadata\global-metadata.dat')
    clientInteropSha256=Hash (Join-Path $ClientRoot 'MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll')
    files=$files;sources=$sources;validation='Release build: zero warnings/errors. No tests or game/server launches. Runtime verification pending.'
}
$record | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $release 'manifest.json') -Encoding UTF8
$package=Join-Path $release 'packaging\client'
New-Item -ItemType Directory (Join-Path $package 'Mods') -Force | Out-Null
Copy-Item $destination (Join-Path $package 'Mods\Si_SquadHUD.dll')
@'
Silica Mod Menu 0.11.0 - standalone client
Install MelonLoader for the Windows Silica client first.
With Silica closed, copy Mods/Si_SquadHUD.dll into your Silica/Mods folder.
No server companion is required. Do not install the old server DLL on the client.
F6 expands status; F6 again opens Mod menu. The bottom bar sends arbitrary text/commands using SEND or Enter without saving them.
Sidebar builds persistent symbol/color buttons for user commands or mod-provided actions. Settings controls transparency and text size.
There is no bundled command catalog or mod-specific parser. Loaded client mods can expose their own controls.
Help saves user entries; commanders can browse Player and Commander, and admins can browse all roles.
UI settings use UserData/Si_ModMenu. created-commands.json shares only user-created definitions; it is never prefilled.
No discovery requests are sent. Remote command access cannot be determined without metadata. Live verification is pending.
'@ | Set-Content (Join-Path $package 'README-INSTALL.txt') -Encoding UTF8
((Hash $destination)+'  Mods/Si_SquadHUD.dll') | Set-Content (Join-Path $package 'SHA256SUMS.txt') -Encoding ASCII
$zip=Join-Path $release 'Si_SquadHUD-0.11.0-client.zip'
Compress-Archive -Path (Join-Path $package '*') -DestinationPath $zip
((Hash $zip)+'  '+(Split-Path $zip -Leaf)) | Set-Content ($zip+'.sha256') -Encoding ASCII
Write-Output $release


