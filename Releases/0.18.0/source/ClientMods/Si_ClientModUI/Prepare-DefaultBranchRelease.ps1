[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$release=Join-Path $PSScriptRoot 'Releases/0.18.0'
if(Test-Path -LiteralPath (Join-Path $release 'manifest.json')){throw 'Release already prepared; preserve it'}
$package=Join-Path $PSScriptRoot '../default-branch_0.9.47_2026-10-23'
New-Item -ItemType Directory -Force (Join-Path $release 'client/Mods'),(Join-Path $release 'server/Mods') | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Client/bin/Release/net6.0/Si_ClientModUI.dll') -Destination (Join-Path $release 'client/Mods/Si_ClientModUI.dll')
Get-ChildItem (Join-Path $package 'Mods') -Filter *.dll | Where-Object Name -ne 'Si_Logistics.dll' | Copy-Item -Destination (Join-Path $release 'server/Mods')
foreach($side in @('client','server')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'FOLLOW_UP_0.18.0.md') -Destination (Join-Path $release ($side+'/README.md'))}
$source=Join-Path $release 'source/ClientMods'
foreach($folder in @('Client','tests')){
    $dest=Join-Path $source ('Si_ClientModUI/'+$folder)
    New-Item -ItemType Directory -Force $dest | Out-Null
    Get-ChildItem (Join-Path $PSScriptRoot $folder) -File | Where-Object {$_.Extension -in '.cs','.csproj','.props'} | Copy-Item -Destination $dest
}
$shared=Join-Path $source 'Si_ServerModUI/Shared'
New-Item -ItemType Directory -Force $shared | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot '../Si_ServerModUI/Shared/Protocol.cs') -Destination $shared
foreach($name in @('Install-DefaultBranch.ps1','Rollback-DefaultBranch.ps1','Prepare-DefaultBranchRelease.ps1','FOLLOW_UP_0.18.0.md')){
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination (Join-Path $source ('Si_ClientModUI/'+$name))
}
$files=@(foreach($side in @('client','server')){Get-ChildItem (Join-Path $release ($side+'/Mods')) -File | ForEach-Object {[ordered]@{file=$side+'/Mods/'+$_.Name;sha256=Hash $_.FullName;bytes=$_.Length}}})
$clientRoot='C:/Program Files (x86)/Steam/steamapps/common/Silica'
$inputs=@(foreach($relative in @('MelonLoader/net6/MelonLoader.dll','MelonLoader/net6/0Harmony.dll','MelonLoader/net6/Il2CppInterop.Runtime.dll','MelonLoader/net6/Il2CppInterop.Common.dll','MelonLoader/Il2CppAssemblies/Il2CppSilicaCore.dll','MelonLoader/Il2CppAssemblies/Il2Cppmscorlib.dll','MelonLoader/Il2CppAssemblies/Il2Cppcom.rlabrecque.steamworks.net.dll','MelonLoader/Il2CppAssemblies/UnityEngine.CoreModule.dll','MelonLoader/Il2CppAssemblies/UnityEngine.IMGUIModule.dll','MelonLoader/Il2CppAssemblies/UnityEngine.TextRenderingModule.dll','MelonLoader/Il2CppAssemblies/Unity.InputSystem.dll')){[ordered]@{side='client';path=$relative;sha256=Hash (Join-Path $clientRoot $relative)}})
$inputs+=@([ordered]@{side='server';path='Silica_Data/Managed/SilicaCore.dll';sha256=Hash (Join-Path $root 'Silica_Data/Managed/SilicaCore.dll')})
$sources=@(Get-ChildItem (Join-Path $release 'source') -Recurse -File | ForEach-Object {[ordered]@{file=$_.FullName.Substring($release.Length+1);sha256=Hash $_.FullName}})
[ordered]@{schema=4;clientVersion='0.18.0';serverName='Si_ModUI_Server';serverVersion='1.4.4';serverOriginal=$true;marker='SiModUI/2';excluded='Si_Logistics.dll';files=$files;gameInputs=$inputs;sources=$sources;runtimeVerified=$false;validation='Client Release build: zero warnings/errors. 292 offline checks pass. Live in-game verification pending.'} | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $release 'manifest.json') -Encoding UTF8
foreach($item in @(@('client','Si_ClientModUI-0.18.0-client.zip'),@('server','DefaultBranch-0.9.47-server-without-Logistics.zip'),@('source','Si_ClientModUI-0.18.0-source.zip'))){
    $zip=Join-Path $release $item[1]
    Compress-Archive -Path (Join-Path $release ($item[0]+'/*')) -DestinationPath $zip
    ((Hash $zip)+'  '+$item[1]) | Set-Content ($zip+'.sha256') -Encoding ASCII
}
Write-Output ('Prepared '+$files.Count+' DLLs at '+$release)
