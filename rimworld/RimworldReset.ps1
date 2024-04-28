$rimworldLocalLowPath = "C:\Users\Duder5000\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"
$rimworldSteamPath = "E:\steamSSD\steamapps\common\RimWorld"
$rimworldWorkshopPath = "E:\steamSSD\steamapps\workshop\content\294100"
$prefsSourcePath = "F:\GDrive\Misc\Rimworld\Shortcuts\Prefs.xml"
$prefsDestinationPath = "$configFolderPath\Prefs.xml"

##################################################################################

#Invoke-Expression -Command "F:\GDrive\Misc\Rimworld\PowerShell\RimworldSaveBackUp.ps1"

##################################################################################

Remove-Item -Path "$rimworldLocalLowPath" -Recurse -Force  -Verbose
Remove-Item -Path "$rimworldSteamPath" -Recurse -Force  -Verbose
Remove-Item -Path "$rimworldWorkshopPath" -Recurse -Force  -Verbose

##################################################################################

if (-not (Test-Path $prefsDestinationPath)) {
	New-Item -ItemType Directory -Path $prefsDestinationPath -Force
}
Copy-Item -Path $prefsSourcePath -Destination $prefsDestinationPath -Force -Verbose -Recurse

##################################################################################

Start-Process "steam://validate/294100"

Start-Sleep -Seconds 1