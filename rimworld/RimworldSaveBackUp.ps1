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

if (-not (Test-Path $savesSourcePath)) {
	New-Item -ItemType Directory -Path $savesSourcePath -Force
}

$newestFiles = Get-ChildItem -Path $savesDestinationPath -Filter *.rws | Sort-Object -Property CreationTime -Descending | Select-Object -First 10
foreach ($file in $newestFiles) {
    Copy-Item -Path $file.FullName -Destination $savesSourcePath
}

##################################################################################

Get-ChildItem -Path "$savesDestinationPath" -Filter *.txt | Remove-Item

$currentDateTime = Get-Date
$formattedDateTimeName = $currentDateTime.ToString("yyyy-MM-dd")
$formattedDateTimeContent = $currentDateTime.ToString("yyyy-MM-dd at HH:mm:ss")

$fileName = "_Last Ran $formattedDateTimeName.txt"
$content = "Current date and time: $formattedDateTimeContent"

$filePath = Join-Path -Path $savesDestinationPath -ChildPath $fileName
$content | Out-File -FilePath $filePath

Write-Host "File '$fileName' created with content:"
Get-Content $filePath

##################################################################################

Start-Sleep -Seconds 1