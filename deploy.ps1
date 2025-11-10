$folder = "D:\homebrew"
D:

cd $folder
firebase deploy

$sleeptime = 5
Write-Output "Start $sleeptime second sleep"
Start-Sleep -Seconds $sleeptime

exit