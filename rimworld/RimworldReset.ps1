$rimworldLocalLowPath = "C:\Users\Duder5000\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"
$rimworldSteamPath = "E:\steamSSD\steamapps\common\RimWorld"
$rimworldWorkshopPath = "E:\steamSSD\steamapps\workshop\content\294100"
$prefsSourcePath = "F:\GDrive\Misc\Rimworld\Shortcuts\Prefs.xml"
$prefsDestinationPath = "$configFolderPath\Prefs.xml"

#$localModsPath = "E:\steamSSD\steamapps\common\RimWorld\Mods"
#$localModsBackUpPath = "D:\rimworldLocalModsBackUp"

##################################################################################

#if (-not (Test-Path $localModsBackUpPath)) {
#	New-Item -ItemType Directory -Path $localModsBackUpPath -Force
#}

#Copy-Item -Path $localModsPath\* -Destination $localModsBackUpPath -Force -Verbose -Recurse

#$savesFilesCount = (Get-ChildItem -Path "$rimworldLocalLowPath\Saves" | Measure-Object).Count

#Invoke-Expression -Command "D:\homebrew\rimworld\RimworldSaveBackUp.ps1"

#Write-Host "Sleep time: $savesFilesCount"
#Start-Sleep -Seconds $savesFilesCount

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