Param (
    [Parameter(Mandatory=$true)]
    $BuildTargetFolder,
    [Parameter(Mandatory=$true)]
    $BuildTargetFile)

taskkill /IM "S4_Main.exe" /F > $null 2> $null

$S4Path = Get-ItemPropertyValue -Path Registry::HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs\11785 -Name InstallDir
$PluginPath = "${S4Path}\plugins"

$BuildTargetFile = "${BuildTargetFolder}${BuildTargetFile}"
$UIDependencyFile = "${BuildTargetFolder}S4-UIEngine.dll"
$APIDependencyFile = "${BuildTargetFolder}NetModAPI.asi"

Copy-Item -Path "${BuildTargetFile}.dll" -Destination "${BuildTargetFile}.nasi"
Copy-Item -Path "${BuildTargetFile}.nasi" -Destination "$PluginPath" -Force
Copy-Item -Path "${UIDependencyFile}" -Destination "$PluginPath" -Force
Copy-Item -Path "${APIDependencyFile}" -Destination "$PluginPath" -Force