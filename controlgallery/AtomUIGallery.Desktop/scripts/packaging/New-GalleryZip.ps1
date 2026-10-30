# Packages a published Windows Gallery into a zip archive containing its executables and DLLs.
param(
    [Parameter(Mandatory = $true)]
    [string]$ConfigPath,
    [Parameter(Mandatory = $true)]
    [string]$PackagesPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [Parameter(Mandatory = $true)]
    [string]$Runtime,
    [string]$BuildStamp
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "GalleryPackaging.Common.ps1")

$config = Read-GalleryInstallerConfig -Path $ConfigPath
$stamp = Get-GalleryBuildStamp -BuildStamp $BuildStamp
if (-not (Test-Path -LiteralPath $PackagesPath -PathType Container)) {
    throw "Packages directory was not found: $PackagesPath"
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$stagePath = Join-Path $OutputPath $config.Name
$zipPath = Join-Path $OutputPath (Get-GalleryArtifactName -Kind zip -Name $config.Name -Version $config.Version -BuildStamp $stamp -Runtime $Runtime)
if (Test-Path -LiteralPath $stagePath) {
    Remove-Item -LiteralPath $stagePath -Recurse -Force
}
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

# Only the executables and their DLLs are distributed; symbols and publish metadata stay out of the archive.
New-Item -ItemType Directory -Path $stagePath | Out-Null
$files = @(Get-ChildItem -LiteralPath $PackagesPath -File | Where-Object { $_.Extension -in ".exe", ".dll" })
if (-not ($files | Where-Object { $_.Extension -eq ".exe" })) {
    throw "No executable was found in $PackagesPath."
}
foreach ($file in $files) {
    Copy-Item -LiteralPath $file.FullName -Destination $stagePath
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagePath, $zipPath,
    [System.IO.Compression.CompressionLevel]::Optimal, $false)
Remove-Item -LiteralPath $stagePath -Recurse -Force

Write-Host "Created $zipPath" -ForegroundColor Green
