# Define folder and log file paths
$folderPath = "F:\GDrive"
$logFile = "F:\GDrive\duplicate_files_old.log"

# Define excluded extensions
$excludedExtensions = @('.gdoc', '.gsheet', '.gslides', '.lnk', '.xml', '.css')

# Get all files recursively, excluding certain file types
Write-Output "Scanning folder: $folderPath..."

$files = Get-ChildItem -Path $folderPath -Recurse -File | Where-Object { $_.Extension -notin $excludedExtensions }
$excludedSubfolder = "F:\GDrive\Misc v2\SFU_Archive\Terms\2019-spring\iat334\A3\copy-of-shared"
$files = Get-ChildItem -Path $folderPath -Recurse -File | Where-Object { 
   $_.Extension -notin $excludedExtensions -and !$_.FullName.StartsWith($excludedSubfolder)
}

$totalFiles = $files.Count
Write-Output "Total files scanned: $totalFiles"

# Check if there are files to process
if ($totalFiles -eq 0) {
    Write-Output "No files found. Exiting."
    exit
}

# Initialize progress bar
$counter = 0
foreach ($file in $files) {
    $counter++
    $progress = [math]::Round(($counter / $totalFiles) * 100)
    Write-Progress -Activity "Scanning files" -Status "Processing file $counter of $totalFiles" -PercentComplete $progress
}
Write-Progress -Activity "Scanning files" -Status "Complete" -Completed

# Group by file name (including extension)
Write-Output "Grouping files by name..."
$groupedFiles = $files | Group-Object -Property Name | Where-Object { $_.Count -gt 1 }

# Clear existing log file or create a new one
if (Test-Path $logFile) { Remove-Item -Path $logFile -Force }
New-Item -Path $logFile -ItemType File -Force | Out-Null

# Initialize progress bar for duplicates
$counter = 0

# Record duplicate file names and their paths
if ($groupedFiles) {
    "Duplicate files found:" | Add-Content -Path $logFile -Encoding UTF8
    foreach ($group in $groupedFiles) {
        $counter++
        $progress = [math]::Round(($counter / $groupedFiles.Count) * 100)
        Write-Progress -Activity "Recording duplicates" -Status "Processing group $counter of $($groupedFiles.Count)" -PercentComplete $progress

        "`nFile Name: $($group.Name)" | Add-Content -Path $logFile -Encoding UTF8
        
        # Compute hashes to check for true duplicates
        $hashes = @{ }
        foreach ($file in $group.Group) {
            try {
                $hash = Get-FileHash -Path $file.FullName -Algorithm SHA256 -ErrorAction Stop
                "    $($file.FullName) (Hash: $($hash.Hash))" | Add-Content -Path $logFile -Encoding UTF8
                
                if ($hashes.ContainsKey($hash.Hash)) {
                    $hashes[$hash.Hash] += @($file.FullName)
                } else {
                    $hashes[$hash.Hash] = @($file.FullName)
                }
            } catch {
                Write-Host "    Error processing file: $($file.FullName) - $_" -ForegroundColor Red
                continue
            }
        }
        
        # Log true duplicates
        foreach ($hash in $hashes.Keys) {
            if ($hashes[$hash].Count -gt 1) {
                "    True duplicate files (identical content):" | Add-Content -Path $logFile -Encoding UTF8
                foreach ($filePath in $hashes[$hash]) {
                    "        $filePath" | Add-Content -Path $logFile -Encoding UTF8
                }
            }
        }
    }
    Write-Progress -Activity "Recording duplicates" -Status "Complete" -Completed
    Write-Output "Duplicates recorded in $logFile"
} else {
    "No duplicate files found." | Add-Content -Path $logFile -Encoding UTF8
    Write-Output "No duplicate files found."
}