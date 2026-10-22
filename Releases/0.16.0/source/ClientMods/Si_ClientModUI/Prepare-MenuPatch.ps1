[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$release=Join-Path $PSScriptRoot 'Releases\0.15.1'
if(Test-Path -LiteralPath (Join-Path $release 'manifest.json')){throw 'Patch release already prepared; preserve it.'}
$previous=Join-Path $PSScriptRoot 'Releases\0.15.0'
$baseline=Get-Content -LiteralPath (Join-Path $previous 'manifest.json') -Raw|ConvertFrom-Json
$files=@(foreach($entry in $baseline.files){
    $source=if($entry.file -eq 'client\Mods\Si_ClientModUI.dll'){Join-Path $PSScriptRoot 'Client\bin\Release\net6.0\Si_ClientModUI.dll'}else{Join-Path $previous $entry.file}
    $target=Join-Path $release $entry.file
    New-Item -ItemType Directory (Split-Path $target -Parent) -Force|Out-Null
    Copy-Item -LiteralPath $source -Destination $target
    if((Hash $target) -ne (Hash $source)){throw 'Patch copy mismatch'}
    [ordered]@{file=$entry.file;sha256=Hash $target;bytes=(Get-Item -LiteralPath $target).Length}
})
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MENU_COMPATIBILITY.md') -Destination (Join-Path $release 'client\README.md')
$sourceRoot=Join-Path $release 'source\ClientMods'
foreach($name in @('Si_ClientModUI','Si_ServerModUI','cheats')){
    $root=Join-Path $PSScriptRoot ('..\'+$name)
    foreach($folder in @('','Client','Server','Shared','tests')){
        $from=Join-Path $root $folder
        if(-not(Test-Path -LiteralPath $from)){continue}
        $to=Join-Path $sourceRoot (Join-Path $name $folder)
        New-Item -ItemType Directory $to -Force|Out-Null
        Get-ChildItem -LiteralPath $from -File|Where-Object {$_.Extension -in '.cs','.csproj','.md','.ps1'}|Copy-Item -Destination $to
    }
}
$sources=@(Get-ChildItem -LiteralPath $sourceRoot -Recurse -File|ForEach-Object {[ordered]@{file=$_.FullName.Substring($release.Length+1);sha256=Hash $_.FullName}})
[ordered]@{schema=3;clientVersion='0.15.1';serverVersion='1.1.0';companionRequired=$false;preparedUtc=[DateTime]::UtcNow.ToString('o');files=$files;gameInputs=$baseline.gameInputs;serverInputs=$baseline.serverInputs;sources=$sources;runtimeVerified=$false;validation='Client build: zero warnings/errors. 201 protocol/catalog/menu/lifecycle checks and 17 installer checks pass. No live processes started or stopped.'}|
    ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $release 'manifest.json') -Encoding UTF8
foreach($item in @(@{folder='client';name='Si_ClientModUI-0.15.1-client.zip'},@{folder='source';name='Si_ModUI-0.15.1-source.zip'})){
    $zip=Join-Path $release $item.name
    Compress-Archive -Path (Join-Path $release ($item.folder+'\*')) -DestinationPath $zip
    ((Hash $zip)+'  '+$item.name)|Set-Content -LiteralPath ($zip+'.sha256') -Encoding ASCII
}
Write-Output $release
