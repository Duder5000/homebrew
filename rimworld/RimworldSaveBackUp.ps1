$rimworldLocalLowPath = "C:\Users\Duder5000\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"

$savesSourcePath = "$rimworldLocalLowPath\Saves"
$savesDestinationPath = "D:\rimworldSavesBackUp"

$configSourcePath = "$rimworldLocalLowPath\Config"
$configDestinationPath = "D:\rimworldConfigsBackUp"

$rimPyModListsSourcePath = "C:\Users\Duder5000\AppData\LocalLow\RimPy Mod Manager\ModLists"
$rimPyModListsDestinationPath = "F:\GDrive\Misc\Rimworld\RimPyModLists"

##################################################################################

if (-not (Test-Path $savesDestinationPath)) {
	New-Item -ItemType Directory -Path $savesDestinationPath -Force
}
if (-not (Test-Path $configDestinationPath)) {
	New-Item -ItemType Directory -Path $configDestinationPath -Force
}
if (-not (Test-Path $rimPyModListsDestinationPath)) {
	New-Item -ItemType Directory -Path $rimPyModListsDestinationPath -Force
}

##################################################################################

Copy-Item -Path $savesSourcePath\* -Destination $savesDestinationPath -Force -Verbose -Recurse
Copy-Item -Path $configSourcePath\* -Destination $configDestinationPath -Force -Verbose -Recurse
Copy-Item -Path $rimPyModListsSourcePath\* -Destination $rimPyModListsDestinationPath -Force -Verbose -Recurse

##################################################################################

Remove-Item -Path "$savesSourcePath" -Force -Verbose -Recurse

$newestFiles = Get-ChildItem -Path $savesDestinationPath | Sort-Object -Property CreationTime -Descending | Select-Object -First 10
foreach ($file in $newestFiles) {
    Copy-Item -Path $file.FullName -Destination $savesSourcePath
}

##################################################################################

Start-Sleep -Seconds 1