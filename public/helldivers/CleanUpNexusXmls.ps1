#https://www.nexusmods.com/helldivers2/mods/25?tab=files

$sourceDirectory = "C:\Users\Duder5000\Downloads\Stratagem Macros for Razer Synapse 3"
$destinationDirectory = "C:\Users\Duder5000\Documents\homebrew\public\downloads\Edited"

if (-not (Test-Path -Path $destinationDirectory)) {
    New-Item -ItemType Directory -Path $destinationDirectory | Out-Null
}

$xmlFiles = Get-ChildItem -Path $sourceDirectory -Filter *.xml -Recurse

foreach ($file in $xmlFiles) {
    Write-Host "Processing file: $($file.FullName)"

    $content = Get-Content -Path $file.FullName -Raw
    $editedContent = $content -creplace '<Delay>(\d+)</Delay>', '<Delay>60</Delay>'
    $editedFilePath = Join-Path -Path $destinationDirectory -ChildPath $file.Name
    $editedContent | Set-Content -Path $editedFilePath
}

Write-Host "Replacement complete. Edited files are located in: $destinationDirectory"

#########################################################################

$folderPath = "C:\Users\Duder5000\Documents\homebrew\public\downloads\nexus"
$xmlFiles = Get-ChildItem -Path $folderPath -Filter "*.xml"

foreach ($file in $xmlFiles) {
    $newFileName = $file.Name -replace '[^a-zA-Z0-9.]', ''
    $newFilePath = Join-Path -Path $file.Directory.FullName -ChildPath $newFileName
    Rename-Item -Path $file.FullName -NewName $newFileName -Force
}

Write-Host "File renaming process complete."

#########################################################################
#https://github.com/SublimeText/PowerShell

$nexusPath = "C:\Users\Duder5000\Documents\homebrew\public\downloads\nexus"
$hd2Path = "C:\Users\Duder5000\Documents\homebrew\public\downloads\hd2"
$noMatchPath = "C:\Users\Duder5000\Documents\homebrew\public\downloads\noMatch"

if (-not (Test-Path -Path $noMatchPath)) {
    New-Item -Path $noMatchPath -ItemType Directory | Out-Null
}

Get-ChildItem -Path $noMatchPath -File | Move-Item -Destination $nexusPath -Force

$xmlFiles = Get-ChildItem -Path $nexusPath -Filter "*.xml" -File

foreach ($xmlFile in $xmlFiles) {
    $matchingFile = Get-ChildItem -Path $hd2Path -Filter $xmlFile.Name -File

    if (-not $matchingFile) {
        Move-Item -Path $xmlFile.FullName -Destination $noMatchPath -Force
        Write-Output "Moved $($xmlFile.Name) to $noMatchPath"
    }
}

Write-Host "File moving complete."