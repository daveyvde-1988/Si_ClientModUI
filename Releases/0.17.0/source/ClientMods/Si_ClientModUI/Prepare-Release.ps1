[CmdletBinding()]
param(
    [string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica',
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')][string]$ClientVersion='0.17.0',
    [ValidatePattern('^[0-9]+\.[0-9]+\.[0-9]+$')][string]$ServerVersion='2.0.0'
)
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$server=Join-Path $PSScriptRoot '..\Si_ServerModUI'
$release=Join-Path $PSScriptRoot ('Releases\'+$ClientVersion)
$serverRelease=Join-Path $server ('Releases\'+$ServerVersion)
if(Test-Path -LiteralPath (Join-Path $release 'manifest.json')){throw 'Release already exists; preserve it and use a new version.'}
if(Test-Path -LiteralPath (Join-Path $serverRelease 'manifest.json')){throw 'Server release already exists; preserve it and use a new version.'}
$inputs=@(
 @{source=(Join-Path $PSScriptRoot 'Client\bin\Release\net6.0\Si_ClientModUI.dll');file='client\Mods\Si_ClientModUI.dll'},
 @{source=(Join-Path $server 'Server\bin\Release\netstandard2.1\Si_ServerModUI.dll');file='server\Mods\Si_ServerModUI.dll'},
 @{source=(Join-Path $PSScriptRoot '..\cheats\bin\Release\net6.0\cheats.dll');file='client\OptionalProviders\cheats.dll'},
 @{source=(Join-Path $workspace 'Silica-1283\si_logistics\bin\Release\netstandard2.1\Si_Logistics.dll');file='server\GameplayProviders\Si_Logistics.dll'},
 @{source=(Join-Path $workspace 'Silica-1283\si_formation\bin\Release\netstandard2.1\Si_Formation.dll');file='server\GameplayProviders\Si_Formation.dll'}
)
$files=@(foreach($item in $inputs){
 $dest=Join-Path $release $item.file
 New-Item -ItemType Directory -Path (Split-Path $dest) -Force|Out-Null
 Copy-Item -LiteralPath $item.source -Destination $dest
 if((Hash $dest) -ne (Hash $item.source)){throw 'Release copy hash mismatch'}
 [ordered]@{file=$item.file;sha256=Hash $dest;bytes=(Get-Item -LiteralPath $dest).Length}
})
$gameInputs=@(foreach($relative in @('GameAssembly.dll','Silica_Data\il2cpp_data\Metadata\global-metadata.dat','MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll')){
 [ordered]@{path=$relative;sha256=Hash (Join-Path $ClientRoot $relative)}
})
$serverInputs=@(foreach($relative in @('Silica_Data\Managed\SilicaCore.dll','Mods\Si_AdminMod.dll','MelonLoader\net35\MelonLoader.dll')){
 [ordered]@{path=$relative;sha256=Hash (Join-Path $workspace $relative)}
})
$sourceRoot=Join-Path $release 'source'
foreach($relative in @('ClientMods\Si_ClientModUI','ClientMods\Si_ServerModUI','ClientMods\cheats','Silica-1283\si_logistics','Silica-1283\si_formation')){
 $root=Join-Path $workspace $relative
 foreach($file in @(Get-ChildItem -LiteralPath $root -Recurse -File|Where-Object { $_.FullName -notmatch '\\(bin|obj|Releases|installation|fixtures|\.git)\\' -and $_.Extension -in '.cs','.csproj','.props','.md','.ps1','.json','.cfg' })){
  $tail=$file.FullName.Substring($workspace.Length+1)
  $dest=Join-Path $sourceRoot $tail
  New-Item -ItemType Directory -Path (Split-Path $dest) -Force|Out-Null
  Copy-Item -LiteralPath $file.FullName -Destination $dest
 }
}
$sources=@(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File|ForEach-Object {[ordered]@{file=$_.FullName.Substring($release.Length+1);sha256=Hash $_.FullName}})
$manifest=[ordered]@{schema=3;clientVersion=$ClientVersion;serverVersion=$ServerVersion;gameplayProviders=@('Si_Logistics.dll','Si_Formation.dll');companionRequired=$false;preparedUtc=[DateTime]::UtcNow.ToString('o');files=$files;gameInputs=$gameInputs;serverInputs=$serverInputs;sources=$sources;runtimeVerified=$false;installed=$false}
$manifest|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $release 'manifest.json') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $release 'client\README.md')
Copy-Item -LiteralPath (Join-Path $server 'README.md') -Destination (Join-Path $release 'server\README.md')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'STRUCTURED_INTEGRATION_0.17.0.md') -Destination (Join-Path $release 'REVIEW.md')
foreach($package in @(@{folder='client';name=('Si_ClientModUI-'+$ClientVersion+'-client.zip')},@{folder='server';name=('Si_ServerModUI-'+$ServerVersion+'-server.zip')},@{folder='source';name=('Si_ModUI-'+$ClientVersion+'-source.zip')})){
 $zip=Join-Path $release $package.name
 Compress-Archive -Path (Join-Path $release ($package.folder+'\*')) -DestinationPath $zip
 ((Hash $zip)+'  '+$package.name)|Set-Content -LiteralPath ($zip+'.sha256') -Encoding ASCII
}
New-Item -ItemType Directory -Path $serverRelease -Force|Out-Null
Copy-Item -LiteralPath (Join-Path $release 'server\Mods\Si_ServerModUI.dll') -Destination $serverRelease
Copy-Item -LiteralPath (Join-Path $release ('Si_ServerModUI-'+$ServerVersion+'-server.zip')) -Destination $serverRelease
[ordered]@{schema=3;serverVersion=$ServerVersion;preparedUtc=$manifest.preparedUtc;serverInputs=$serverInputs;runtimeVerified=$false;files=@([ordered]@{file='Si_ServerModUI.dll';sha256=Hash (Join-Path $serverRelease 'Si_ServerModUI.dll')})}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $serverRelease 'manifest.json') -Encoding UTF8
Write-Output ('Prepared only; nothing installed: '+$release)
