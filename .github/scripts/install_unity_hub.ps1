$hubInstallerUrl = "https://public-cdn.cloud.unity3d.com/hub/prod/UnityHubSetup.exe"
$downloadPath = "$env:TEMP\UnityInstaller"
if (!(Test-Path $downloadPath)) {
    New-Item -ItemType Directory -Path $downloadPath | Out-Null
}
Write-Host "Downloading Unity Hub installer"
& aria2c --quiet --max-connection-per-server=4 --dir $downloadPath $hubInstallerUrl
if ($LASTEXITCODE) {
    Write-Error "Failed to download Unity Hub installer"
    exit $LASTEXITCODE
}
Write-Host "Installing Unity Hub"
$process = Start-Process -FilePath "$downloadPath\UnityHubSetup.exe" -ArgumentList "/S" -NoNewWindow -PassThru
Wait-Process -Id $process.Id
if ($LASTEXITCODE) {
    Write-Error "Failed to install Unity Hub"
    exit $LASTEXITCODE
}
Write-Host "Unity Hub installed successfully"
