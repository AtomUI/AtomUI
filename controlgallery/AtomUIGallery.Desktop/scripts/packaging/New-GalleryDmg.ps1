# Packages a published macOS Gallery as a .app bundle inside a DMG.
# Signing follows the previous installer tool: each file in Contents/MacOS (main executable last),
# then the bundle, then a strict deep verification. No identity means an unsigned DMG.
param(
    [Parameter(Mandatory = $true)]
    [string]$ConfigPath,
    [Parameter(Mandatory = $true)]
    [string]$PackagesPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [Parameter(Mandatory = $true)]
    [ValidateSet("osx-x64", "osx-arm64")]
    [string]$Runtime,
    [string]$BuildStamp,
    [string]$CertIdentity,
    # Skips create-dmg's Finder (AppleScript) window layout; for local runs without Finder automation access.
    [switch]$SkipFinderLayout
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "GalleryPackaging.Common.ps1")

if (-not $IsMacOS) {
    throw "DMG packaging is only supported on macOS."
}

$config = Read-GalleryInstallerConfig -Path $ConfigPath
$stamp = Get-GalleryBuildStamp -BuildStamp $BuildStamp
if (-not (Test-Path -LiteralPath $PackagesPath -PathType Container)) {
    throw "Packages directory was not found: $PackagesPath"
}
if (-not $config.AppBundleIdentifier) {
    throw "Installer config must define <AppBundleIdentifier>."
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path
$PackagesPath = (Resolve-Path -LiteralPath $PackagesPath).Path

$bundleName = "$($config.Name).app"
$bundlePath = Join-Path $OutputPath $bundleName
$contentsPath = Join-Path $bundlePath "Contents"
$macOSPath = Join-Path $contentsPath "MacOS"
$resourcesPath = Join-Path $contentsPath "Resources"
if (Test-Path -LiteralPath $bundlePath) {
    Remove-Item -LiteralPath $bundlePath -Recurse -Force
}
foreach ($directory in $macOSPath, (Join-Path $contentsPath "PlugIns"), $resourcesPath) {
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
}

$executable = Join-Path $PackagesPath $config.AssemblyName
if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
    throw "Executable was not found: $executable"
}
Copy-Item -LiteralPath $executable -Destination $macOSPath
Invoke-GalleryCommand -FilePath "chmod" -ArgumentList @("+x", (Join-Path $macOSPath $config.AssemblyName))
Get-ChildItem -LiteralPath $PackagesPath -Filter "*.dylib" -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination $macOSPath
}

$iconRelativePath = if ($config.AppIcon) { $config.AppIcon } else { $config.Name }
if (-not [System.IO.Path]::HasExtension($iconRelativePath)) {
    $iconRelativePath = "$iconRelativePath.icns"
}
$iconPath = Join-Path $PackagesPath $iconRelativePath
$bundleIconName = $null
if (Test-Path -LiteralPath $iconPath -PathType Leaf) {
    $bundleIconName = [System.IO.Path]::GetFileNameWithoutExtension($iconRelativePath)
    Copy-Item -LiteralPath $iconPath -Destination (Join-Path $resourcesPath "$bundleIconName.icns")
}
elseif ($config.AppIcon) {
    throw "Configured macOS app icon was not found: $iconPath"
}

function ConvertTo-PlistString([string]$Value) {
    return [System.Security.SecurityElement]::Escape($Value)
}

$entries = [System.Collections.Generic.List[string]]::new()
function Add-PlistString([string]$Key, [string]$Value) {
    $entries.Add("   <key>$Key</key>")
    $entries.Add("   <string>$(ConvertTo-PlistString $Value)</string>")
}
Add-PlistString "CFBundleDevelopmentRegion" "English"
Add-PlistString "CFBundleExecutable" $config.AssemblyName
Add-PlistString "CFBundleGetInfoString" $config.Copyright
if ($bundleIconName) {
    Add-PlistString "CFBundleIconFile" $bundleIconName
}
Add-PlistString "CFBundleIdentifier" $config.AppBundleIdentifier
Add-PlistString "CFBundleInfoDictionaryVersion" "6.0"
Add-PlistString "CFBundleLongVersionString" "$($config.Name) $($config.Version)"
Add-PlistString "CFBundleName" $config.Name
Add-PlistString "CFBundlePackageType" "APPL"
Add-PlistString "CFBundleShortVersionString" $config.Version
Add-PlistString "CFBundleSignature" "????"
Add-PlistString "CFBundleVersion" $config.Version
$entries.Add("   <key>CSResourcesFileMapped</key>")
$entries.Add("   <true/>")
Add-PlistString "NSHumanReadableCopyright" $config.Copyright
Add-PlistString "NSPrincipalClass" "NSApplication"
Add-PlistString "NSHighResolutionCapable" "True"

$plistPath = Join-Path $contentsPath "Info.plist"
$plist = @(
    '<?xml version="1.0" encoding="utf-8"?>'
    '<!DOCTYPE plist PUBLIC "-//Apple Computer//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">'
    '<plist version="1.0">'
    '<dict>'
) + $entries + @('</dict>', '</plist>')
Set-Content -LiteralPath $plistPath -Value ($plist -join "`n") -Encoding utf8NoBOM
Invoke-GalleryCommand -FilePath "plutil" -ArgumentList @("-lint", $plistPath)

if (-not [string]::IsNullOrWhiteSpace($CertIdentity)) {
    # Ordinal path order with the main executable last, matching the previous installer tool.
    $codeFiles = [System.Collections.Generic.List[string]]::new()
    Get-ChildItem -LiteralPath $macOSPath -File -Recurse |
        Where-Object { $_.Name -cne $config.AssemblyName } |
        ForEach-Object { $codeFiles.Add($_.FullName) }
    $codeFiles.Sort([System.StringComparer]::Ordinal)
    $codeFiles.Add((Join-Path $macOSPath $config.AssemblyName))
    foreach ($file in $codeFiles) {
        Invoke-GalleryCommand -FilePath "codesign" -ArgumentList @("--force", "-s", $CertIdentity, $file)
    }
    Invoke-GalleryCommand -FilePath "codesign" -ArgumentList @("--force", "-s", $CertIdentity, $bundlePath)
    Invoke-GalleryCommand -FilePath "codesign" -ArgumentList @("--verify", "--deep", "--strict", $bundlePath)
}
else {
    Write-Host "No signing identity was provided; the bundle is left unsigned." -ForegroundColor Yellow
}

$toolsPath = Join-Path ([System.IO.Path]::GetTempPath()) "atomui-packaging-tools"
$createDmgArchive = Install-GalleryPackagingTool -Name "create-dmg" -Architecture any -Destination $toolsPath
$createDmgRoot = Join-Path $toolsPath "create-dmg"
if (Test-Path -LiteralPath $createDmgRoot) {
    Remove-Item -LiteralPath $createDmgRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $createDmgRoot | Out-Null
Invoke-GalleryCommand -FilePath "tar" -ArgumentList @("-xzf", $createDmgArchive, "-C", $createDmgRoot, "--strip-components", "1")
$createDmg = Join-Path $createDmgRoot "create-dmg"

$dmgName = Get-GalleryArtifactName -Kind dmg -Name $config.Name -Version $config.Version -BuildStamp $stamp -Runtime $Runtime
$dmgPath = Join-Path $OutputPath $dmgName
if (Test-Path -LiteralPath $dmgPath) {
    Remove-Item -LiteralPath $dmgPath -Force
}
$volumeName = if ($config.Title) { $config.Title } else { $config.Name }
$arguments = @("--volname", $volumeName)
if ($config.Background) {
    $background = if ([System.IO.Path]::IsPathFullyQualified($config.Background)) { $config.Background } else { Join-Path $PackagesPath $config.Background }
    if (Test-Path -LiteralPath $background -PathType Leaf) {
        $arguments += @("--background", $background)
    }
}
$arguments += @(
    "--window-pos", "200", "120",
    "--window-size", "800", "530",
    "--icon-size", "100",
    "--icon", $bundleName, "230", "230",
    "--app-drop-link", "600", "230"
)
if ($SkipFinderLayout) {
    $arguments += "--skip-jenkins"
}
$arguments += @(
    $dmgName,
    $bundlePath
)
Invoke-GalleryCommand -FilePath $createDmg -ArgumentList $arguments -WorkingDirectory $OutputPath

Remove-Item -LiteralPath $bundlePath -Recurse -Force
Write-Host "Created $dmgPath" -ForegroundColor Green
