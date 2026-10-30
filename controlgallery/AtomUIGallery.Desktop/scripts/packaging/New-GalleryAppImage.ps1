# Packages a published Linux Gallery as an AppImage.
# The AppDir is complete (AppRun, desktop entry, .DirIcon, permissions) before appimagetool runs,
# and the AppImage is built exactly once with pinned, checksum-verified tools.
param(
    [Parameter(Mandatory = $true)]
    [string]$ConfigPath,
    [Parameter(Mandatory = $true)]
    [string]$PackagesPath,
    [Parameter(Mandatory = $true)]
    [string]$OutputPath,
    [Parameter(Mandatory = $true)]
    [ValidateSet("linux-x64", "linux-arm64")]
    [string]$Runtime,
    [string]$BuildStamp
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "GalleryPackaging.Common.ps1")

if (-not $IsLinux) {
    throw "AppImage packaging is only supported on Linux."
}

$config = Read-GalleryInstallerConfig -Path $ConfigPath
$stamp = Get-GalleryBuildStamp -BuildStamp $BuildStamp
$architecture = if ($Runtime -eq "linux-x64") { "x86_64" } else { "aarch64" }
if (-not (Test-Path -LiteralPath $PackagesPath -PathType Container)) {
    throw "Packages directory was not found: $PackagesPath"
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$OutputPath = (Resolve-Path -LiteralPath $OutputPath).Path
$toolsPath = New-GalleryToolsDirectory
# The tools run in this process; restore the variables they need so the calling workflow step is unaffected.
$previousExtractAndRun = $env:APPIMAGE_EXTRACT_AND_RUN
$previousArch = $env:ARCH
try {
    $linuxDeploy = Install-GalleryPackagingTool -Name linuxdeploy -Architecture $architecture -Destination $toolsPath
    $appImageTool = Install-GalleryPackagingTool -Name appimagetool -Architecture $architecture -Destination $toolsPath
    # appimagetool otherwise downloads the unpinned continuous runtime that becomes the AppImage ELF header.
    $runtimeFile = Install-GalleryPackagingTool -Name "type2-runtime" -Architecture $architecture -Destination $toolsPath
    Invoke-GalleryCommand -FilePath "chmod" -ArgumentList @("+x", $linuxDeploy, $appImageTool)

    $appDir = Join-Path $OutputPath "$($config.Name).AppDir"
    $binPath = Join-Path $appDir "usr/bin"
    $libPath = Join-Path $appDir "usr/lib"
    $applicationsPath = Join-Path $appDir "usr/share/applications"
    $iconsPath = Join-Path $appDir "usr/share/icons"
    if (Test-Path -LiteralPath $appDir) {
        Remove-Item -LiteralPath $appDir -Recurse -Force
    }
    foreach ($directory in $binPath, $libPath, $applicationsPath, $iconsPath) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $executable = Join-Path $PackagesPath $config.AssemblyName
    if (-not (Test-Path -LiteralPath $executable -PathType Leaf)) {
        throw "Executable was not found: $executable"
    }
    Copy-Item -LiteralPath $executable -Destination $binPath
    Get-ChildItem -LiteralPath $PackagesPath -Filter "*.so" -File | ForEach-Object {
        Copy-Item -LiteralPath $_.FullName -Destination $libPath
    }

    if ($config.AppIcon) {
        $iconSet = Join-Path $PackagesPath $config.AppIcon
        if (-not (Test-Path -LiteralPath $iconSet -PathType Container)) {
            throw "Configured application icon directory was not found: $iconSet"
        }
        Copy-Item -Path (Join-Path $iconSet "*") -Destination $iconsPath -Recurse
        # linuxdeploy resolves the desktop entry's Icon from the hicolor theme and fails when it is absent.
        Get-ChildItem -LiteralPath $iconSet -Filter "icon_*.png" -File |
            Where-Object { $_.BaseName -match '^icon_(\d+)x\1$' } |
            ForEach-Object {
                $size = $Matches[1]
                $themeDir = Join-Path $iconsPath "hicolor/${size}x${size}/apps"
                New-Item -ItemType Directory -Path $themeDir -Force | Out-Null
                Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $themeDir "$($config.Name).png")
            }
    }
    if (-not $config.InstallerApplicationIcon) {
        throw "Installer config must define <InstallerApplicationIcon> for the AppImage .DirIcon."
    }
    $applicationIcon = Join-Path $PackagesPath $config.InstallerApplicationIcon
    if (-not (Test-Path -LiteralPath $applicationIcon -PathType Leaf)) {
        throw "Configured installer application icon was not found: $applicationIcon"
    }
    # Regular files, not symlinks: appimagetool validates .DirIcon as a real file.
    Copy-Item -LiteralPath $applicationIcon -Destination (Join-Path $appDir ".DirIcon")
    Copy-Item -LiteralPath $applicationIcon -Destination (Join-Path $appDir "$($config.Name).png")

    $desktopEntry = @(
        "[Desktop Entry]"
        "Type=Application"
        "Version=1.0"
        "Name=$($config.Name)"
        "Icon=$($config.Name)"
        "Exec=$($config.AssemblyName)"
        "Categories=Utility"
        "X-Version=$($config.Version)"
        "[X-Company Info]"
        "X-Company=$($config.Company)"
        "X-CompanyFullName=$($config.CompanyFullName)"
        "X-CompanyUrl=$($config.CompanyUrl)"
        "X-Copyright=$($config.Copyright)"
    ) -join "`n"
    $desktopFileName = "$($config.Name).desktop"
    Set-Content -LiteralPath (Join-Path $appDir $desktopFileName) -Value $desktopEntry -Encoding utf8NoBOM
    Set-Content -LiteralPath (Join-Path $applicationsPath $desktopFileName) -Value $desktopEntry -Encoding utf8NoBOM

    $appRun = @'
#!/bin/sh
HERE="$(dirname "$(readlink -f "$0")")"
export LD_LIBRARY_PATH="$HERE/usr/lib${LD_LIBRARY_PATH:+:$LD_LIBRARY_PATH}"
exec "$HERE/usr/bin/__EXECUTABLE_NAME__" "$@"
'@.Replace("__EXECUTABLE_NAME__", $config.AssemblyName)
    $appRunPath = Join-Path $appDir "AppRun"
    Set-Content -LiteralPath $appRunPath -Value $appRun -Encoding utf8NoBOM -NoNewline

    # linuxdeploy runs as an AppImage itself; extract-and-run avoids requiring FUSE on the runner.
    $env:APPIMAGE_EXTRACT_AND_RUN = "1"
    Invoke-GalleryCommand -FilePath $linuxDeploy -ArgumentList @("--appdir", $appDir) -WorkingDirectory $OutputPath

    # linuxdeploy replaces the root icon with a symlink to a small theme size; keep the full-size application icon.
    $rootIcon = Join-Path $appDir "$($config.Name).png"
    Remove-Item -LiteralPath $rootIcon -Force -ErrorAction SilentlyContinue
    Copy-Item -LiteralPath $applicationIcon -Destination $rootIcon

    # Everyone must be able to read the AppDir; launchers and shared libraries must be executable.
    Invoke-GalleryCommand -FilePath "find" -ArgumentList @($appDir, "-type", "d", "-exec", "chmod", "755", "{}", "+")
    Invoke-GalleryCommand -FilePath "find" -ArgumentList @($appDir, "-type", "f", "-exec", "chmod", "644", "{}", "+")
    $executables = @($appRunPath, (Join-Path $binPath $config.AssemblyName)) +
        @(Get-ChildItem -LiteralPath $libPath -Filter "*.so*" -File | ForEach-Object { $_.FullName })
    Invoke-GalleryCommand -FilePath "chmod" -ArgumentList (@("755") + $executables)
    Invoke-GalleryCommand -FilePath "sh" -ArgumentList @("-n", $appRunPath)

    $appImageName = Get-GalleryArtifactName -Kind appimage -Name $config.Name -Version $config.Version -BuildStamp $stamp -Runtime $Runtime
    $appImagePath = Join-Path $OutputPath $appImageName
    if (Test-Path -LiteralPath $appImagePath) {
        Remove-Item -LiteralPath $appImagePath -Force
    }
    $env:ARCH = $architecture
    Invoke-GalleryCommand -FilePath $appImageTool -ArgumentList @("--runtime-file", $runtimeFile, $appDir, $appImageName) -WorkingDirectory $OutputPath
    Invoke-GalleryCommand -FilePath "chmod" -ArgumentList @("755", $appImagePath)

    # Verify the packed image rather than the AppDir: extraction needs no FUSE.
    $verifyPath = Join-Path ([System.IO.Path]::GetTempPath()) "atomui-appimage-verify-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $verifyPath | Out-Null
    try {
        Invoke-GalleryCommand -FilePath $appImagePath -ArgumentList @("--appimage-extract") -WorkingDirectory $verifyPath
        $root = Join-Path $verifyPath "squashfs-root"
        foreach ($required in "AppRun", $desktopFileName, ".DirIcon", "usr/bin/$($config.AssemblyName)") {
            $item = Get-Item -LiteralPath (Join-Path $root $required) -Force -ErrorAction SilentlyContinue
            if ($null -eq $item -or $item.PSIsContainer) {
                throw "AppImage verification failed: '$required' is missing from $appImageName."
            }
        }
        if ((Get-Item -LiteralPath (Join-Path $root ".DirIcon") -Force).LinkType) {
            throw "AppImage verification failed: .DirIcon must be a regular file."
        }
        Invoke-GalleryCommand -FilePath "sh" -ArgumentList @("-n", (Join-Path $root "AppRun"))
    }
    finally {
        Remove-Item -LiteralPath $verifyPath -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Created $appImagePath" -ForegroundColor Green
}
finally {
    $env:APPIMAGE_EXTRACT_AND_RUN = $previousExtractAndRun
    $env:ARCH = $previousArch
    Remove-Item -LiteralPath $toolsPath -Recurse -Force -ErrorAction SilentlyContinue
}
