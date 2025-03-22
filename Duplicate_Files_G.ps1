# Define folder and log file paths
$folderPath = "F:\GDrive"
$logFile = "F:\GDrive\duplicate_files_g.log"

# Define exclusions
$excludedExtensions = @('.gitignore', '.lnk')
$excludedFileNames = @('app.css', 'base.css', 'css.css', 'About.xml', 'PublishedFileId.txt', 'preview.png')
$excludedSubfolder = "F:\GDrive\Notes\DND_TEMP"

Write-Output "Scanning folder: $folderPath..."
# Get all files recursively, excluding extensions, file names, and specific subfolder
$files = Get-ChildItem -Path $folderPath -Recurse -File | Where-Object { 
    $_.Extension -notin $excludedExtensions -and 
    $_.Name -notin $excludedFileNames -and  
    $_.FullName -notlike "$excludedSubfolder\*"
}

$totalFiles = $files.Count
Write-Output "Total files scanned: $totalFiles"

if ($totalFiles -eq 0) {
    Write-Output "No files found. Exiting."
    exit
}

# Group by file name (including extension)
Write-Output "Grouping files by name..."
$groupedFiles = $files | Group-Object -Property Name | Where-Object { $_.Count -gt 1 }

# Clear existing log file or create a new one
if (Test-Path $logFile) { Remove-Item -Path $logFile -Force }
New-Item -Path $logFile -ItemType File -Force | Out-Null

# Record duplicate file names and their paths
if ($groupedFiles) {
    "Duplicate files found:" | Add-Content -Path $logFile -Encoding UTF8
    $counter = 0
    foreach ($group in $groupedFiles) {
        $counter++
        $progress = [math]::Round(($counter / $groupedFiles.Count) * 100)
        Write-Progress -Activity "Recording duplicates" -Status "Processing group $counter of $($groupedFiles.Count)" -PercentComplete $progress

        "`nFile Name: $($group.Name)" | Add-Content -Path $logFile -Encoding UTF8
        
        # Compute hashes to check for true duplicates
        $hashes = @{}
        foreach ($file in $group.Group) {
            try {
                $hash = Get-FileHash -Path $file.FullName -Algorithm SHA256 -ErrorAction Stop
                "    $($file.FullName) (Hash: $($hash.Hash))" | Add-Content -Path $logFile -Encoding UTF8
                $hashes[$hash.Hash] += @($file.FullName)  # Directly append to the dictionary
            } catch {
                Write-Host "    Error processing file: $($file.FullName) - $_" -ForegroundColor Red
                continue
            }
        }
        
        # Log true duplicates
        foreach ($hash in $hashes.Keys | Where-Object { $hashes[$_].Count -gt 1 }) {
            "    True duplicate files (identical content):" | Add-Content -Path $logFile -Encoding UTF8
            $hashes[$hash] | ForEach-Object { "        $_" | Add-Content -Path $logFile -Encoding UTF8 }
        }
    }
    Write-Progress -Activity "Recording duplicates" -Status "Complete" -Completed
    Write-Output "Duplicates recorded in $logFile"
} else {
    "No duplicate files found." | Add-Content -Path $logFile -Encoding UTF8
    Write-Output "No duplicate files found."
}