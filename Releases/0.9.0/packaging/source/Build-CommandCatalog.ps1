$ErrorActionPreference='Stop'
$run=Join-Path $PSScriptRoot 'inspection\standalone-current'
$catalog=Join-Path $PSScriptRoot 'Catalog'
New-Item -ItemType Directory -Path $catalog -Force | Out-Null
$entries=[Collections.Generic.List[object]]::new()
function Add([string]$mod,[string]$command,[string]$description,[string]$role='shared',[bool]$arguments=$false) {
    if($command -match '^[/!](\d+|back)$'){return}
    $entries.Add([ordered]@{Mod=$mod.ToLowerInvariant();Command=$command;Description=$description;Role=$role;Arguments=$arguments;Source='catalog'})
}
foreach($dir in Get-ChildItem $run -Directory) {
    $body=(Get-ChildItem $dir.FullName -Filter '*.cs' | Get-Content -Raw) -join [Environment]::NewLine
    foreach($m in [regex]::Matches($body,'Register(Player|Admin)Command\(\s*"([^"]+)"([^;]+);')) {
        $admin=$m.Groups[1].Value -eq 'Admin'
        $desc='Registered chat command. Click to request the native response; use EDIT to supply arguments.'
        $strings=[regex]::Matches($m.Groups[3].Value,'"((?:\\.|[^"\\])*)"')
        if($admin -and $strings.Count -gt 0){$desc=$strings[$strings.Count-1].Groups[1].Value.Replace('\"','"')}
        $prefix=if($admin){'!'}else{'/'}
        Add $dir.Name ($prefix+$m.Groups[2].Value) $desc $(if($admin){'admin'}else{'shared'}) ($desc -match '<[^>]+>|\[optional')
    }
    foreach($m in [regex]::Matches($body,'_registerAdminCmdMethod\.Invoke\(null,\s*new object\[4\]\s*\{\s*"([^"]+)"')) {
        Add $dir.Name ('!'+$m.Groups[1].Value) 'Native administrator chat command. Server permission checks apply.' 'admin'
    }
}
Add 'si_unitbalance' '/b' 'Open the native Unit Balance editor. Requires server administrator rights.' 'admin'
Add 'si_unitbalance' '/stats' 'Open the native public unit-stat inspector.'
Add 'si_unitbalance' '/rebalance' 'Request the native balance reload. Requires administrator rights.' 'admin'
Add 'si_ranks' '/rank' 'Read your current rank, squad allowance and recruitment rules.'
Add 'si_ranks' '/promote' 'Promote the named player using Ranks. Enter a player name; Generic power required.' 'admin' $true
Add 'si_ranks' '/demote' 'Demote the named player using Ranks. Distinct from Commander Management !demote.' 'admin' $true
Add 'si_ranks' '/debug status' 'Read Ranks diagnostics. Requires Generic power.' 'admin'
Add 'si_formation' '/formation help' 'Show Formation commands for your role.'
Add 'si_formation' '/formation status' 'Read the current Formation state once.'
Add 'si_formation' '/formation lock' 'Arm the commander snapshot move, subject to Formation validation.' 'commander'
Add 'si_formation' '/formation unlock' 'Unlock commander formations and cancel pending lock/scout actions.' 'commander'
Add 'si_formation' '/followformation status' 'Read FPS formation lock state.' 'fps'
Add 'si_formation' '/followformation lock' 'Lock your FPS formation.' 'fps'
Add 'si_formation' '/followformation unlock' 'Unlock your FPS formation.' 'fps'
Add 'si_formation' '/disband all' 'Dismiss eligible AI followers using Formation rules.' 'fps'
Add 'si_formation' '/formation on' 'Enable Formation for this match. Requires Generic power.' 'admin'
Add 'si_formation' '/formation off' 'Disable Formation for this match using its own cleanup. Requires Generic power.' 'admin'
Add 'si_formation' '/formation rallyfix status' 'Read production parking state.'
foreach($name in @('Formationspeedcap','Formationtraveldistance','formationTravel','formationTolerance','FormationFallof','startdelayspeedlock','startdelaysoftlock','startdelaylock','startdelay')) {
    Add 'si_formation' ('/'+$name) 'Formation setting. Enter the value documented by /formation help; server permissions apply.' 'admin' $true
}
Add 'si_formation' '/scout' 'Arm the next commander scout order.' 'commander'
Add 'si_kingofthehill' '/koh status' 'Read King of the Hill capture/economy status once.'
Add 'si_heavyteleporter' '/st status' 'Read heavy teleporter status once.'
Add 'si_crabcannon' '/ccaim' 'Read current cannon aim; EDIT to provide angle/speed or range.'
$unique=@($entries | Group-Object { $_.Mod+"|"+$_.Command.ToLowerInvariant() } | ForEach-Object {$_.Group[-1]} | Sort-Object { $_.Mod },{ $_.Command })
$unique | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $catalog 'commands.json') -Encoding UTF8
Copy-Item (Join-Path $run 'inputs.json') (Join-Path $catalog 'installed-command-inputs.json')
Write-Output ("Catalog: "+$unique.Count+" commands, "+@($unique.Mod | Sort-Object -Unique).Count+" modules")

