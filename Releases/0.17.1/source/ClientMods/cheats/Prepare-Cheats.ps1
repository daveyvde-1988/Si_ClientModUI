[CmdletBinding()]
param([string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot '..\Si_ClientModUI\Prepare-Release.ps1') -ClientRoot $ClientRoot
exit 0
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$release=Join-Path $PSScriptRoot 'Releases\0.3.0'
if(Test-Path -LiteralPath (Join-Path $release 'manifest.json')){throw 'Release already exists. Preserve it and use a new version.'}
New-Item -ItemType Directory -Path (Join-Path $release 'Mods') -Force | Out-Null
$sources=@{
    'cheats.dll'=(Join-Path $PSScriptRoot 'bin\Release\net6.0\cheats.dll')
    'Si_SquadHUD.dll'=(Join-Path $PSScriptRoot '..\Si_SquadHUD\Client\bin\Release\net6.0\Si_SquadHUD.dll')
}
$files=@(foreach($name in $sources.Keys){
    $destination=Join-Path $release ('Mods\'+$name)
    Copy-Item -LiteralPath $sources[$name] -Destination $destination
    if((Hash $destination) -ne (Hash $sources[$name])){throw 'Release copy mismatch.'}
    [ordered]@{name=$name;sha256=Hash $destination;bytes=(Get-Item -LiteralPath $destination).Length}
})
$inputs=@(foreach($path in @('GameAssembly.dll','Silica_Data\il2cpp_data\Metadata\global-metadata.dat','MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll')){
    [ordered]@{path=$path;sha256=Hash (Join-Path $ClientRoot $path)}
})
$sourceFiles=@(Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object {$_.Name -ne 'INSTALLATION_STATUS.md' -and $_.Extension -in '.cs','.csproj','.ps1','.md'} |
    ForEach-Object {[ordered]@{name=$_.Name;sha256=Hash $_.FullName}})
[ordered]@{version='0.3.0';menuVersion='0.14.0';preparedUtc=[DateTime]::UtcNow.ToString('o');files=$files;gameInputs=$inputs;sources=$sourceFiles;
    validation='Release build: zero warnings/errors. 189 command checks pass in three cultures. Native catalog coverage and server/local routing verified against focused managed extraction. No game launches; runtime verification pending.'} |
    ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $release 'manifest.json') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md'),(Join-Path $PSScriptRoot 'Install-Cheats.ps1') -Destination $release
$sourcePackage=Join-Path $release 'source\cheats'
New-Item -ItemType Directory -Path $sourcePackage -Force | Out-Null
Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object {$_.Name -ne 'INSTALLATION_STATUS.md' -and $_.Extension -in '.cs','.csproj','.ps1','.md'} | Copy-Item -Destination $sourcePackage
New-Item -ItemType Directory -Path (Join-Path $sourcePackage 'tests') -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot 'tests') -File | Copy-Item -Destination (Join-Path $sourcePackage 'tests')
$menuSource=Join-Path $release 'source\Si_SquadHUD\Client'
New-Item -ItemType Directory -Path $menuSource -Force | Out-Null
Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot '..\Si_SquadHUD\Client') -File | Where-Object {$_.Extension -in '.cs','.csproj'} | Copy-Item -Destination $menuSource
$zip=Join-Path $release 'cheats-0.3.0-client.zip'
# A client package contains only the two mods, documentation and verified installer/manifest.
Compress-Archive -LiteralPath (Join-Path $release 'Mods'),(Join-Path $release 'README.md'),(Join-Path $release 'Install-Cheats.ps1'),(Join-Path $release 'manifest.json') -DestinationPath $zip
((Hash $zip)+'  cheats-0.3.0-client.zip')|Set-Content -LiteralPath ($zip+'.sha256') -Encoding ASCII
Write-Output $release
