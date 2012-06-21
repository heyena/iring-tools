# Import-Module WebAdministration

param($contextName, $appName, $communityName)

# =============================================================================
# Check for deployment package
# =============================================================================
$base_path = Split-Path -Path $MyInvocation.MyCommand.Path -Parent
$zip_file = Get-Item $base_path\*DataLayer*.zip

if ($zip_file -is [System.Array]) {
  $zip_file_name = $zip_file[$zip_file.Count - 1].name
}
else {
  $zip_file_name = $zip_file.name
}

if (!$zip_file_name) {
  Write-Output "DataLayer deployment package not found."
  Exit 1
}

Write-Output "Package file: $zip_file_name"

# =============================================================================
# Find the iRINGTools installation
# =============================================================================
$irt_path = "\inetpub\iringtools"

if ((Test-Path -path e:) -eq $True) { 
  $irt_path = "e:" + $irt_path
}
elseif ((Test-Path -path d:) -eq $True) { 
  $irt_path = "d:" + $irt_path
}
else { 
  $irt_path = "c:" + $irt_path
}

Write-Output "iRINGTools installation path: $irt_path"

if ((Test-Path -path "$irt_path") -eq $False) {
  Write-Output "iRINGTools web services not found."
  Exit 1
}

# =============================================================================
# Deploy binaries if no params provided and config files otherwise
# =============================================================================
$zip_bin = "$base_path\$zip_file_name\bin"
$irt_bin = "$irt_path\services\bin"

$shell_app = New-Object -com shell.application
$zip_bin_folder = $shell_app.namespace($zip_bin)
  
if ($contextName -eq $Null -or $appName -eq $Null -or $communityName -eq $Null) {
  $irt_bin_folder = $shell_app.namespace($irt_bin)
  $irt_bin_folder.CopyHere($zip_bin_folder.items(), 0x14)
  Write-Output "Binary deployed successfully."
}
else {
  $unzipTemp = $env:temp + "\ebDatalayer"
  $tempDir = New-Item $unzipTemp -type directory
  $zip_conf = "$base_path\$zip_file_name\conf"
  $zip_conf_folder = $shell_app.namespace($zip_conf)
  $temp_folder = $shell_app.NameSpace($unzipTemp)
  $temp_folder.CopyHere($zip_conf_folder.items())
  
  $temp_folder.items() | foreach {
    $new_file_name = $_.name
    $new_file_name = $new_file_name -replace "{context}", $contextName
    $new_file_name = $new_file_name -replace "{app}", $appName
    $new_file_name = $new_file_name -replace "{community}", $communityName
    Rename-Item $_.path $new_file_name
  }

  $irt_data = "$irt_path\services\app_data"
  Copy-Item $unzipTemp\* $irt_data
  Write-Output "Configurations deployed successfully."
  Remove-Item $unzipTemp -recurse
}

Write-Output "done."
