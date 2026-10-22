[CmdletBinding()]
param()
Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'
$checks=0
function Check([bool]$Value,[string]$Name){$script:checks++;if(-not $Value){throw $Name}}
function Hash([string]$Path){(Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()}
$fixture=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot ('fixtures\'+[Guid]::NewGuid().ToString('N'))))
$scripts=Join-Path $fixture 'package'
$client=Join-Path $fixture 'client'
$server=Join-Path $fixture 'server'
$release=Join-Path $scripts 'Releases\0.16.0'
New-Item -ItemType Directory (Join-Path $client 'Mods'),(Join-Path $server 'Mods'),(Join-Path $client 'UserData\Si_ModMenu'),(Join-Path $release 'client\Mods'),(Join-Path $release 'client\OptionalProviders'),(Join-Path $release 'server\Mods') -Force|Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot '..\Install-ModUI.ps1'),(Join-Path $PSScriptRoot '..\Rollback-ModUI.ps1') -Destination $scripts
'old-client'|Set-Content -LiteralPath (Join-Path $client 'Mods\Si_SquadHUD.dll')
'old-cheats'|Set-Content -LiteralPath (Join-Path $client 'Mods\cheats.dll')
'old-server'|Set-Content -LiteralPath (Join-Path $server 'Mods\Si_SquadHUD_Server.dll')
'friend-client'|Set-Content -LiteralPath (Join-Path $client 'Mods\Si_ModUI.dll')
'friend-server'|Set-Content -LiteralPath (Join-Path $server 'Mods\Si_ModUI_Server.dll')
'current-logistics'|Set-Content -LiteralPath (Join-Path $server 'Mods\Si_Logistics.dll')
'current-formation'|Set-Content -LiteralPath (Join-Path $server 'Mods\Si_Formation.dll')
New-Item -ItemType Directory -Path (Join-Path $release 'server\GameplayProviders') -Force|Out-Null
'{"keep":"settings"}'|Set-Content -LiteralPath (Join-Path $client 'UserData\Si_ModMenu\layout-test.json')
$settingsHash=Hash (Join-Path $client 'UserData\Si_ModMenu\layout-test.json')
'client-reference'|Set-Content -LiteralPath (Join-Path $client 'reference.dll')
'server-reference'|Set-Content -LiteralPath (Join-Path $server 'reference.dll')
$files=@(foreach($file in @('client\Mods\Si_ClientModUI.dll','client\OptionalProviders\cheats.dll','server\Mods\Si_ServerModUI.dll','server\GameplayProviders\Si_Logistics.dll','server\GameplayProviders\Si_Formation.dll')){
    $path=Join-Path $release $file
    ('new-'+$file)|Set-Content -LiteralPath $path
    [ordered]@{file=$file;sha256=Hash $path}
})
[ordered]@{schema=3;gameplayProviders=@('Si_Logistics.dll','Si_Formation.dll');files=$files;gameInputs=@(@{path='reference.dll';sha256=Hash (Join-Path $client 'reference.dll')});serverInputs=@(@{path='reference.dll';sha256=Hash (Join-Path $server 'reference.dll')})}|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $release 'manifest.json') -Encoding UTF8
$plan=(& (Join-Path $scripts 'Install-ModUI.ps1') -Side both -ClientRoot $client -ServerRoot $server -PlanOnly)|ConvertFrom-Json
Check ($plan.files.Count -eq 3 -and $plan.retire.Count -eq 4) 'Plan includes both renamed DLLs and installed provider'
$clientPlan=(& (Join-Path $scripts 'Install-ModUI.ps1') -Side client -ClientRoot $client -PlanOnly)|ConvertFrom-Json
Check ($clientPlan.files.Count -eq 2) 'Default server root evaluates correctly when omitted'
Check (-not(Test-Path -LiteralPath (Join-Path $client 'Mods\Si_ClientModUI.dll'))) 'Plan makes no installation changes'
$output=& (Join-Path $scripts 'Install-ModUI.ps1') -Side both -ClientRoot $client -ServerRoot $server
$receiptPath=(Get-ChildItem -LiteralPath (Join-Path $scripts 'installation') -Filter installation.json -Recurse -File).FullName
$receipt=Get-Content -LiteralPath $receiptPath -Raw|ConvertFrom-Json
Check $receipt.complete 'Complete receipt'
Check ((Test-Path -LiteralPath (Join-Path $client 'Mods\Si_ClientModUI.dll')) -and (Test-Path -LiteralPath (Join-Path $server 'Mods\Si_ServerModUI.dll'))) 'Both DLLs installed'
Check (-not(Test-Path -LiteralPath (Join-Path $client 'Mods\Si_SquadHUD.dll')) -and -not(Test-Path -LiteralPath (Join-Path $server 'Mods\Si_SquadHUD_Server.dll'))) 'Legacy DLLs retired'
foreach($item in $receipt.files){Check ((Hash $item.destination) -eq $item.sha256) 'Installed hash';if($item.backup){Check ((Hash $item.backup) -eq $item.oldSha256) 'Backup hash'}}
Check ((Hash (Join-Path $client 'UserData\Si_ModMenu\layout-test.json')) -eq $settingsHash) 'Settings preserved on install'
$changed=Join-Path $client 'Mods\Si_ClientModUI.dll'
'changed'|Set-Content -LiteralPath $changed
$rejected=$false
try{& (Join-Path $scripts 'Rollback-ModUI.ps1') -InstallRecord $receiptPath|Out-Null}catch{$rejected=$_.Exception.Message -like '*changed*'}
Check $rejected 'Rollback rejects a changed installed DLL'
Copy-Item -LiteralPath (Join-Path $release 'client\Mods\Si_ClientModUI.dll') -Destination $changed
& (Join-Path $scripts 'Rollback-ModUI.ps1') -InstallRecord $receiptPath|Out-Null
Check ((Get-Content -LiteralPath (Join-Path $client 'Mods\Si_SquadHUD.dll') -Raw).Trim() -eq 'old-client') 'Legacy client restored'
Check ((Get-Content -LiteralPath (Join-Path $server 'Mods\Si_SquadHUD_Server.dll') -Raw).Trim() -eq 'old-server') 'Legacy server restored'
Check ((Get-Content -LiteralPath (Join-Path $client 'Mods\cheats.dll') -Raw).Trim() -eq 'old-cheats') 'Provider restored'
Check (-not(Test-Path -LiteralPath (Join-Path $client 'Mods\Si_ClientModUI.dll')) -and -not(Test-Path -LiteralPath (Join-Path $server 'Mods\Si_ServerModUI.dll'))) 'New DLLs disabled on rollback'
Check ((Hash (Join-Path $client 'UserData\Si_ModMenu\layout-test.json')) -eq $settingsHash) 'Settings preserved on rollback'
Check ((Get-Content -LiteralPath (Join-Path $client 'Mods\Si_ModUI.dll') -Raw).Trim() -eq 'friend-client') 'Temporary friend client restored'
Check ((Get-Content -LiteralPath (Join-Path $server 'Mods\Si_ModUI_Server.dll') -Raw).Trim() -eq 'friend-server') 'Temporary friend companion restored'
$providerPlan=(& (Join-Path $scripts 'Install-ModUI.ps1') -Side both -ClientRoot $client -ServerRoot $server -IncludeGameplayProviders -PlanOnly)|ConvertFrom-Json
Check ($providerPlan.files.Count -eq 5) 'Explicit provider install includes Logistics and Formation'
& (Join-Path $scripts 'Install-ModUI.ps1') -Side both -ClientRoot $client -ServerRoot $server -IncludeGameplayProviders|Out-Null
$providerReceipt=(Get-ChildItem -LiteralPath (Join-Path $scripts 'installation') -Filter installation.json -Recurse -File|Sort-Object LastWriteTime -Descending|Select-Object -First 1).FullName
Check ((Hash (Join-Path $server 'Mods\Si_Logistics.dll')) -eq (Hash (Join-Path $release 'server\GameplayProviders\Si_Logistics.dll'))) 'Staged Logistics installed only when selected'
Check ((Hash (Join-Path $server 'Mods\Si_Formation.dll')) -eq (Hash (Join-Path $release 'server\GameplayProviders\Si_Formation.dll'))) 'Staged Formation installed only when selected'
& (Join-Path $scripts 'Rollback-ModUI.ps1') -InstallRecord $providerReceipt|Out-Null
Check ((Get-Content -LiteralPath (Join-Path $server 'Mods\Si_Logistics.dll') -Raw).Trim() -eq 'current-logistics') 'Logistics restored on provider rollback'
Check ((Get-Content -LiteralPath (Join-Path $server 'Mods\Si_Formation.dll') -Raw).Trim() -eq 'current-formation') 'Formation restored on provider rollback'
Write-Output ("PASS: $checks installer/rollback checks. Fixture: $fixture")
