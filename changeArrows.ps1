# Check if the script was run with a valid parameter
if ($args.Count -ne 1 -or ($args[0] -ne 'laptop' -and $args[0] -ne 'desktop')) {
    Write-Host "Usage: script.ps1 <laptop|desktop>"
    exit
}

# Specify the path to the HTML file based on the parameter
if ($args[0] -eq 'laptop') {
    $htmlFilePath = "C:\Users\Duder5000\Documents\homebrew\public\helldivers\index.html"
} elseif ($args[0] -eq 'desktop') {
    $htmlFilePath = "D:\homebrew\public\helldivers\index.html"
}

# Read the content of the HTML file
$htmlContent = Get-Content -Path $htmlFilePath -Raw

# Define a hashtable to map arrows to their HTML hexadecimal codes
$arrowMap = @{
    '▲' = '&#x2191;'
    '▼' = '&#x2193;'
    '◄' = '&#x2190;'
    '►' = '&#x2192;'
}

# Iterate through each arrow in the map and replace them in the HTML content
foreach ($arrow in $arrowMap.GetEnumerator()) {
    $htmlContent = $htmlContent -replace [regex]::Escape($arrow.Key), $arrow.Value
}

# Write the modified content back to the HTML file
$htmlContent | Set-Content -Path $htmlFilePath
