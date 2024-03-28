$sourceDirectory = "C:\Users\Duder5000\Downloads\Stratagem Macros for Razer Synapse 3"
$destinationDirectory = "C:\Users\Duder5000\Downloads\Edited"

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