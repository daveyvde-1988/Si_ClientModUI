[CmdletBinding()]
param([string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica')
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$server=Join-Path $PSScriptRoot '..\Si_ServerModUI'
$release=Join-Path $PSScriptRoot 'Releases\0.15.0'
$serverRelease=Join-Path $server 'Releases\1.1.0'
if(Test-Path (Join-Path $release 'manifest.json')){throw 'Release already prepared. Preserve it; use a new version for changes.'}
if(Test-Path (Join-Path $serverRelease 'manifest.json')){throw 'Server release already prepared. Preserve it; use a new version for changes.'}
$clientPackage=Join-Path $release 'client'
$serverPackage=Join-Path $release 'server'
New-Item -ItemType Directory (Join-Path $clientPackage 'Mods'),(Join-Path $clientPackage 'OptionalProviders'),(Join-Path $serverPackage 'Mods') -Force | Out-Null
$inputs=@(
    @{source=(Join-Path $PSScriptRoot 'Client\bin\Release\net6.0\Si_ClientModUI.dll');file='client\Mods\Si_ClientModUI.dll'},
    @{source=(Join-Path $server 'Server\bin\Release\netstandard2.1\Si_ServerModUI.dll');file='server\Mods\Si_ServerModUI.dll'},
    @{source=(Join-Path $PSScriptRoot '..\cheats\bin\Release\net6.0\cheats.dll');file='client\OptionalProviders\cheats.dll'}
)
$files=@(foreach($item in $inputs){
    $dest=Join-Path $release $item.file
    Copy-Item -LiteralPath $item.source -Destination $dest
    if((Hash $dest) -ne (Hash $item.source)){throw 'Release copy failed verification'}
    [ordered]@{file=$item.file;sha256=Hash $dest;bytes=(Get-Item -LiteralPath $dest).Length}
})
$gameInputs=@(foreach($path in @('GameAssembly.dll','Silica_Data\il2cpp_data\Metadata\global-metadata.dat','MelonLoader\Il2CppAssemblies\Il2CppSilicaCore.dll')){
    [ordered]@{path=$path;sha256=Hash (Join-Path $ClientRoot $path)}
})
$serverRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..'))
$serverInputs=@(foreach($path in @('Silica_Data\Managed\SilicaCore.dll','Mods\Si_AdminMod.dll','MelonLoader\net35\MelonLoader.dll')){
    [ordered]@{path=$path;sha256=Hash (Join-Path $serverRoot $path)}
})
$sourceRoot=Join-Path $release 'source\ClientMods'
foreach($name in @('Si_ClientModUI','Si_ServerModUI','cheats')){
    $root=Join-Path $PSScriptRoot ('..\'+$name)
    foreach($folder in @('','Client','Server','Shared','tests')){
        $from=Join-Path $root $folder
        if(-not(Test-Path -LiteralPath $from)){continue}
        $to=Join-Path $sourceRoot (Join-Path $name $folder)
        New-Item -ItemType Directory $to -Force | Out-Null
        Get-ChildItem -LiteralPath $from -File | Where-Object {$_.Extension -in '.cs','.csproj','.md','.ps1'} | Copy-Item -Destination $to
    }
}
$sources=@(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File | ForEach-Object {
    [ordered]@{file=$_.FullName.Substring($release.Length+1);sha256=Hash $_.FullName}
})
$manifest=[ordered]@{schema=3;clientVersion='0.15.0';serverVersion='1.1.0';companionRequired=$false;preparedUtc=[DateTime]::UtcNow.ToString('o');
    files=$files;gameInputs=$gameInputs;serverInputs=$serverInputs;sources=$sources;runtimeVerified=$false}
$manifest|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $release 'manifest.json') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $clientPackage 'README.md')
Copy-Item -LiteralPath (Join-Path $server 'README.md') -Destination (Join-Path $serverPackage 'README.md')
foreach($pair in @(@{folder=$clientPackage;name='Si_ClientModUI-0.15.0-client.zip'},@{folder=$serverPackage;name='Si_ServerModUI-1.1.0-server.zip'},@{folder=(Join-Path $release 'source');name='Si_ModUI-0.15.0-source.zip'})){
    $zip=Join-Path $release $pair.name
    Compress-Archive -Path (Join-Path $pair.folder '*') -DestinationPath $zip
    ((Hash $zip)+'  '+$pair.name)|Set-Content -LiteralPath ($zip+'.sha256') -Encoding ASCII
}
New-Item -ItemType Directory $serverRelease -Force|Out-Null
Copy-Item -LiteralPath (Join-Path $release 'server\Mods\Si_ServerModUI.dll'),(Join-Path $release 'Si_ServerModUI-1.1.0-server.zip'),(Join-Path $release 'Si_ServerModUI-1.1.0-server.zip.sha256') -Destination $serverRelease
[ordered]@{schema=3;serverVersion='1.1.0';preparedUtc=$manifest.preparedUtc;files=@($files|Where-Object file -like 'server\*');serverInputs=$serverInputs;runtimeVerified=$false}|
    ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $serverRelease 'manifest.json') -Encoding UTF8
Write-Output $release
