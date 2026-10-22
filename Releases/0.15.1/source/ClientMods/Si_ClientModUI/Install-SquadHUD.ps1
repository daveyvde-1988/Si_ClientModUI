# Compatibility entry point for existing local instructions.
[CmdletBinding()]
param([ValidateSet('client','server','both')][string]$Side='client',
    [string]$ClientRoot='C:\Program Files (x86)\Steam\steamapps\common\Silica',[switch]$PlanOnly)
& (Join-Path $PSScriptRoot 'Install-ModUI.ps1') -Side $Side -ClientRoot $ClientRoot -PlanOnly:$PlanOnly
