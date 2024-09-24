# Define paths
$savesSource = "C:\Users\Duder5000\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios\Saves"
$savesBackup = "F:\GDrive\Misc\RimRim\Saves Back-up"
$workshopContent = "E:\steamSSD\steamapps\workshop\content\294100"
$appDataRimworld = "C:\Users\Duder5000\AppData\LocalLow\Ludeon Studios\RimWorld by Ludeon Studios"
$rimworldFolder = "E:\steamSSD\steamapps\common\RimWorld"
$modTweaks = "F:\GDrive\Misc\RimRim\Rimworld Mod Tweaks\000"
$configBackup = "F:\GDrive\Misc\RimRim\Config Back-up\000"

# Steam Info
$steamPath = "C:\Program Files (x86)\Steam\Steam.exe"
$appID = 294100

# Step 1: Copy save files to backup (overwriting duplicates)
Write-Host "Copying save files to backup..."
Copy-Item -Path "$savesSource\*" -Destination "$savesBackup" -Recurse -Force

# Step 2: Delete all files in the workshop content folder
Write-Host "Deleting workshop content..."
Remove-Item -Path "$workshopContent\*" -Recurse -Force

# Step 3: Delete all files in the AppData RimWorld folder
Write-Host "Deleting RimWorld AppData files..."
Remove-Item -Path "$appDataRimworld\*" -Recurse -Force

# Step 4: Delete all files in the RimWorld installation folder
Write-Host "Deleting RimWorld installation files..."
Remove-Item -Path "$rimworldFolder\*" -Recurse -Force

# Step 5: Copy mod tweaks to RimWorld installation folder (overwriting duplicates)
Write-Host "Copying mod tweaks to RimWorld installation..."
Copy-Item -Path "$modTweaks\*" -Destination "$rimworldFolder" -Recurse -Force

# Step 6: Copy config backup to RimWorld AppData folder (overwriting duplicates)
Write-Host "Copying config backup to RimWorld AppData..."
Copy-Item -Path "$configBackup\*" -Destination "$appDataRimworld" -Recurse -Force

Write-Host "Deleting and copying complete"

#Start-Sleep -Seconds 3

# Steam validation requires manual initiation or SteamCMD, so we're skipping automated validation for now.
Write-Host "Manually validate the RimWorld game files through the Steam client."
# Uncomment the following line if using SteamCMD for validation, or manually validate:
# Start-Process -FilePath $steamPath -ArgumentList "://validate/$appID" (not valid)

Start-Sleep -Seconds 3