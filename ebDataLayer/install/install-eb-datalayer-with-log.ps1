# Import-Module WebAdministration

param($contextName, $appName, $communityName)

$base_path = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
& $base_path\install-iw-datalayer.ps1 $contextName $appName $communityName > $env:temp\install-eb-datalayer.log 2>&1