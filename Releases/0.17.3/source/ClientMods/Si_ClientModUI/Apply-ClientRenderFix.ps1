[CmdletBinding()]
param([string]$ClientRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Silica', [switch]$PlanOnly)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'Install-SquadHUD.ps1') -Side client -ClientRoot $ClientRoot -PlanOnly:$PlanOnly
