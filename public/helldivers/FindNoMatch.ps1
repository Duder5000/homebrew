# Define the paths
$nexusPath = "C:\Users\Duder5000\Documents\homebrew\public\downloads\nexus"
$hd2Path = "C:\Users\Duder5000\Documents\homebrew\public\downloads\hd2"
$noMatchPath = "C:\Users\Duder5000\Documents\homebrew\public\downloads\noMatch"

# Create the noMatch directory if it doesn't exist
if (-not (Test-Path -Path $noMatchPath)) {
    New-Item -Path $noMatchPath -ItemType Directory | Out-Null
}

# Get all XML files in the nexus directory
$xmlFiles = Get-ChildItem -Path $nexusPath -Filter "*.xml" -File

# Loop through each XML file
foreach ($xmlFile in $xmlFiles) {
    $matchingFile = Get-ChildItem -Path $hd2Path -Filter $xmlFile.Name -File

    # If no matching file found, move the file to noMatch directory
    if (-not $matchingFile) {
        Move-Item -Path $xmlFile.FullName -Destination $noMatchPath -Force
        Write-Output "Moved $($xmlFile.Name) to $noMatchPath"
    }
}