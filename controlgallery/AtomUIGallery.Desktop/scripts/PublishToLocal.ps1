param (
    [string]$publishRootPath = "D:/publish",
    [string]$buildType = "Release",
    [string]$framework = "net10.0",
    [string]$runtime = "osx-arm64",
    [string]$buildTimestamp,
    [object]$publishAot = $true,
    [object]$publishReadyToRun = $true
)

if ([string]::IsNullOrEmpty($buildTimestamp)) {
    $buildTimestamp = Get-Date -Format 'yyyyMMddHHmm'
}

function Update-VersionInFile {
    param(
        [Parameter(Mandatory=$true)]
        [string]$XmlFilePath,

        [Parameter(Mandatory=$true)]
        [string]$TargetFilePath
    )

    try {
        [xml]$xmlContent = Get-Content -Path $XmlFilePath -ErrorAction Stop
        
        $version = $xmlContent.Project.PropertyGroup.AtomUIVersion

        if (-not $version) {
            throw "AtomUIVersion not found in XML file"
        }

        Write-Host "Extracted version: $version" -ForegroundColor Green
        
        $targetContent = Get-Content -Path $TargetFilePath -Raw -ErrorAction Stop
        $updatedContent = $targetContent -replace '__VERSION__', $version
        
        $updatedContent | Set-Content -Path $TargetFilePath -Force

        Write-Host "Successfully updated $TargetFilePath with version $version" -ForegroundColor Cyan
    }
    catch {
        Write-Error "Error occurred: $($_.Exception.Message)"
        return $false
    }

    return $true
}

function ConvertTo-Bool {
    param(
        [Parameter(Mandatory=$true)]
        [object]$Value
    )

    if ($Value -is [bool]) {
        return $Value
    }

    $normalizedValue = ([string]$Value).Trim().ToLowerInvariant()
    switch ($normalizedValue) {
        "true" { return $true }
        "1" { return $true }
        "yes" { return $true }
        "false" { return $false }
        "0" { return $false }
        "no" { return $false }
        default {
            throw "Invalid boolean value '$Value'. Expected true or false."
        }
    }
}

function Resolve-PublishPath {
    param([Parameter(Mandatory=$true)][string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path) -or $Path.IndexOfAny([char[]]'*?[]') -ge 0) {
        throw "Invalid publish root path '$Path'."
    }
    if (($Path -split '[/\\]') | Where-Object { $_ -eq '.' -or $_ -eq '..' }) {
        throw "Publish root path must not contain '.' or '..': '$Path'."
    }

    $fullPath = [System.IO.Path]::GetFullPath($Path)
    if (Test-Path -LiteralPath $fullPath) {
        $leaf = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
        if ($leaf.LinkType) {
            throw "Publish root must not be a symbolic link: '$Path'."
        }
    }
    $existing = $fullPath
    $suffix = [System.Collections.Generic.List[string]]::new()
    while (-not (Test-Path -LiteralPath $existing)) {
        $parent = [System.IO.Path]::GetDirectoryName($existing)
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $existing) {
            throw "Publish root path has no existing ancestor: '$Path'."
        }
        $suffix.Insert(0, [System.IO.Path]::GetFileName($existing))
        $existing = $parent
    }
    if (-not (Test-Path -LiteralPath $existing -PathType Container)) {
        throw "Publish root ancestor is not a directory: '$existing'."
    }

    # Resolve existing links one component at a time. Resolve-Path alone leaves
    # /tmp -> /private/tmp (and similar repository aliases) unresolved.
    $root = [System.IO.Path]::GetPathRoot($existing)
    $resolved = $root
    foreach ($part in ($existing.Substring($root.Length) -split '[/\\]' | Where-Object { $_ })) {
        $candidate = Join-Path $resolved $part
        $item = Get-Item -LiteralPath $candidate -Force -ErrorAction Stop
        if ($item.LinkType) {
            $target = $item.ResolveLinkTarget($true)
            if ($target -isnot [System.IO.DirectoryInfo] -or -not $target.Exists) {
                throw "Publish root ancestor link is not a directory: '$candidate'."
            }
            $resolved = $target.FullName
        } else {
            $resolved = $item.FullName
        }
    }
    foreach ($part in $suffix) {
        $resolved = Join-Path $resolved $part
    }
    return [System.IO.Path]::GetFullPath($resolved)
}

function Test-PathIsWithin {
    param([string]$Path, [string]$Parent)

    $comparison = if ($IsWindows) { [System.StringComparison]::OrdinalIgnoreCase } else { [System.StringComparison]::Ordinal }
    $prefix = $Parent.TrimEnd([char[]]@('/', '\')) + [System.IO.Path]::DirectorySeparatorChar
    return $Path.StartsWith($prefix, $comparison)
}

function Initialize-PublishRoot {
    param([Parameter(Mandatory=$true)][string]$Path)

    $resolved = Resolve-PublishPath -Path $Path
    $repositoryRoot = Resolve-PublishPath -Path ([System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..')))
    $homeRoot = Resolve-PublishPath -Path ([Environment]::GetFolderPath('UserProfile'))
    $fileSystemRoot = [System.IO.Path]::GetPathRoot($resolved)
    if ($resolved -eq $fileSystemRoot -or $resolved -eq $homeRoot -or
        $resolved -eq $repositoryRoot -or
        (Test-PathIsWithin -Path $repositoryRoot -Parent $resolved) -or
        (Test-PathIsWithin -Path $homeRoot -Parent $resolved)) {
        throw "Refusing unsafe publish root '$resolved'."
    }
    if ((Test-PathIsWithin -Path $resolved -Parent $repositoryRoot) -and
        -not (Test-PathIsWithin -Path $resolved -Parent (Join-Path $repositoryRoot '.artifacts/gallery_published')) -and
        -not (Test-PathIsWithin -Path $resolved -Parent (Join-Path $repositoryRoot 'binaries/gallery_published'))) {
        throw "Publish root inside the repository must be under .artifacts/gallery_published or binaries/gallery_published: '$resolved'."
    }

    $markerName = '.atomui-gallery-publish-root'
    $markerValue = 'AtomUIGallery.Desktop PublishToLocal.ps1 output v1'
    if (Test-Path -LiteralPath $resolved) {
        if (-not (Test-Path -LiteralPath $resolved -PathType Container)) {
            throw "Publish root is not a directory: '$resolved'."
        }
        $entries = @(Get-ChildItem -LiteralPath $resolved -Force -ErrorAction Stop)
        if ($entries.Count -gt 0) {
            $markerPath = Join-Path $resolved $markerName
            if (-not (Test-Path -LiteralPath $markerPath -PathType Leaf) -or
                (Get-Item -LiteralPath $markerPath -Force -ErrorAction Stop).LinkType -or
                (Get-Content -LiteralPath $markerPath -Raw -ErrorAction Stop).Trim() -cne $markerValue) {
                throw "Refusing to replace a nonempty directory without the Gallery publish marker: '$resolved'."
            }
            Remove-Item -LiteralPath $resolved -Recurse -Force -ErrorAction Stop
            if (Test-Path -LiteralPath $resolved) {
                throw "Publish root cleanup did not remove '$resolved'."
            }
        }
    }
    New-Item -Path $resolved -ItemType Directory -Force -ErrorAction Stop | Out-Null
    Set-Content -LiteralPath (Join-Path $resolved $markerName) -Value $markerValue -ErrorAction Stop
    return $resolved
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory=$true)]
        [string[]]$Arguments
    )

    Write-Host "dotnet $($Arguments -join ' ')" -ForegroundColor Cyan
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Test-NativeAotRestoreAssets {
    param(
        [Parameter(Mandatory=$true)]
        [string]$AssetsPath,

        [Parameter(Mandatory=$true)]
        [string]$Runtime
    )

    if (-not (Test-Path -Path $AssetsPath -PathType Leaf)) {
        throw "NativeAOT restore validation failed: assets file '$AssetsPath' was not found."
    }

    $assetsContent = Get-Content -Path $AssetsPath -Raw | ConvertFrom-Json -Depth 100
    $libraryNames = @($assetsContent.libraries.PSObject.Properties.Name)
    $targetProperties = @($assetsContent.targets.PSObject.Properties)
    $runtimeTarget = $targetProperties | Where-Object { $_.Name -like "*/$Runtime" } | Select-Object -First 1

    if (-not ($libraryNames | Where-Object { $_ -like "Microsoft.DotNet.ILCompiler/*" })) {
        throw "NativeAOT restore validation failed for runtime '$Runtime': Microsoft.DotNet.ILCompiler was not found in '$AssetsPath'."
    }

    if (-not $runtimeTarget) {
        throw "NativeAOT restore validation failed for runtime '$Runtime': no runtime-specific target was found in '$AssetsPath'."
    }

    $runtimeTargetLibraryNames = @($runtimeTarget.Value.PSObject.Properties.Name)
    if (-not ($runtimeTargetLibraryNames | Where-Object { $_ -like "Microsoft.DotNet.ILCompiler/*" })) {
        throw "NativeAOT restore validation failed for runtime '$Runtime': Microsoft.DotNet.ILCompiler was not found in target '$($runtimeTarget.Name)' in '$AssetsPath'."
    }

    Write-Host "NativeAOT restore assets validation passed for runtime '$Runtime'." -ForegroundColor Green
}

function Test-NativeAotOutput {
    param(
        [Parameter(Mandatory=$true)]
        [string]$PackagesPath,

        [Parameter(Mandatory=$true)]
        [string]$Runtime
    )

    $mainExecutable = if ($Runtime.StartsWith("win-")) {
        "AtomUIGallery.Desktop.exe"
    } else {
        "AtomUIGallery.Desktop"
    }

    $mainExecutablePath = Join-Path -Path $PackagesPath -ChildPath $mainExecutable
    if (-not (Test-Path -Path $mainExecutablePath -PathType Leaf)) {
        throw "NativeAOT validation failed for runtime '$Runtime': main executable '$mainExecutable' was not found in '$PackagesPath'."
    }

    $forbiddenFiles = @(
        "coreclr.dll",
        "clrjit.dll",
        "libcoreclr.so",
        "libcoreclr.dylib",
        "libclrjit.so",
        "libclrjit.dylib",
        "System.Private.CoreLib.dll",
        "AtomUIGallery.Desktop.dll"
    )

    foreach ($fileName in $forbiddenFiles) {
        $candidate = Join-Path -Path $PackagesPath -ChildPath $fileName
        if (Test-Path -Path $candidate -PathType Leaf) {
            throw "NativeAOT validation failed for runtime '$Runtime': unexpected self-contained runtime file '$fileName' was found in '$PackagesPath'."
        }
    }

    Write-Host "NativeAOT output validation passed for runtime '$Runtime'." -ForegroundColor Green
}

function Copy-InstallerAssets {
    param(
        [Parameter(Mandatory=$true)]
        [string]$PackagesPath
    )

    $installerAssetsPath = Join-Path -Path $PSScriptRoot -ChildPath "../Assets"
    $targetAssetsPath = Join-Path -Path $PackagesPath -ChildPath "Assets"

    if (-not (Test-Path -Path $installerAssetsPath -PathType Container)) {
        throw "Installer assets directory was not found: $installerAssetsPath"
    }

    if (Test-Path -Path $targetAssetsPath -PathType Container) {
        Remove-Item -Path $targetAssetsPath -Recurse -Force
    }

    New-Item -Path $targetAssetsPath -ItemType Directory -Force | Out-Null
    Copy-Item -Path (Join-Path -Path $installerAssetsPath -ChildPath "*") `
              -Destination $targetAssetsPath `
              -Recurse `
              -Force `
              -ErrorAction Stop

    Write-Host "Installer assets copied to $targetAssetsPath." -ForegroundColor Green
}

function Test-PackageFile {
    param(
        [Parameter(Mandatory=$true)]
        [string]$RelativePath
    )

    $packageFilePath = Join-Path -Path $packagesPath -ChildPath $RelativePath
    if (-not (Test-Path -Path $packageFilePath -PathType Leaf)) {
        throw "Required installer package file was not found: $packageFilePath"
    }
}

function Assert-PublishModeSegments {
    foreach ($segment in @($buildType, $framework, $runtime)) {
        if ([string]::IsNullOrWhiteSpace($segment) -or $segment -match '[/\\]' -or $segment -eq '.' -or $segment -eq '..') {
            throw "Invalid publish intermediate path segment '$segment'."
        }
    }
}

function Get-PublishIntermediateRoot {
    $repositoryRoot = Resolve-PublishPath -Path ([System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..')))
    $generatedRoot = Join-Path $repositoryRoot '.artifacts/AtomUIGallery.Desktop/obj'
    $candidate = Join-Path $generatedRoot "$buildType/$framework/$runtime"
    $resolved = Resolve-PublishPath -Path $candidate
    if (-not (Test-PathIsWithin -Path $resolved -Parent $generatedRoot)) {
        throw "Publish intermediate path escapes the Gallery generated output directory: '$resolved'."
    }
    return $resolved
}

function Clear-PublishModeIntermediates {
    param([Parameter(Mandatory=$true)][string]$IntermediateRoot)

    # The SDK does not clean linked/ or R2R/ when a subsequent publish switches
    # between trimmed NativeAOT, trimmed R2R and untrimmed R2R in the same RID obj.
    # A stale trimmed ReactiveUI.dll can otherwise be bundled with an untrimmed caller.
    foreach ($name in @('linked', 'R2R')) {
        $candidate = Join-Path $IntermediateRoot $name
        if (Test-Path -LiteralPath $candidate -ErrorAction Stop) {
            if ((Get-Item -LiteralPath $candidate -Force -ErrorAction Stop).LinkType) {
                throw "Publish intermediate '$candidate' must not be a symbolic link."
            }
            if (-not (Test-Path -LiteralPath $candidate -PathType Container -ErrorAction Stop)) {
                throw "Publish intermediate '$candidate' is not a directory."
            }
            Remove-Item -LiteralPath $candidate -Recurse -Force -ErrorAction Stop
            if (Test-Path -LiteralPath $candidate -ErrorAction Stop) {
                throw "Publish intermediate cleanup did not remove '$candidate'."
            }
        }
    }
}

$publishAotEnabled = ConvertTo-Bool -Value $publishAot
$publishReadyToRunEnabled = ConvertTo-Bool -Value $publishReadyToRun
$projectPath = Join-Path -Path $PSScriptRoot -ChildPath "../AtomUIGallery.Desktop.csproj"
$assetsPath = Join-Path -Path $PSScriptRoot -ChildPath "../../../.artifacts/AtomUIGallery.Desktop/obj/project.assets.json"
$configsPath = Join-Path -Path $PSScriptRoot -ChildPath "../configs"
$versionPropsPath = Join-Path -Path $PSScriptRoot -ChildPath "../../../build/Versions.props"

if ($publishAotEnabled -and $buildType -ne "Release") {
    throw "NativeAOT Gallery publishing requires buildType 'Release'. Current buildType is '$buildType'."
}
if (-not $publishAotEnabled -and $publishReadyToRunEnabled -and $buildType -ne "Release") {
    throw "ReadyToRun Gallery publishing requires buildType 'Release'. Pass -publishReadyToRun false for a Debug publish."
}

Assert-PublishModeSegments
$intermediateRoot = Get-PublishIntermediateRoot
$publishRootPath = Initialize-PublishRoot -Path $publishRootPath
$configPath = Join-Path $publishRootPath 'config'
$packagesPath = Join-Path $publishRootPath 'packages'
New-Item -Path $configPath, $packagesPath -ItemType Directory -Force -ErrorAction Stop | Out-Null

Clear-PublishModeIntermediates -IntermediateRoot $intermediateRoot

if ($publishAotEnabled) {

    Invoke-DotNet -Arguments @(
        "restore",
        $projectPath,
        "--runtime",
        $runtime,
        "-p:Configuration=$buildType",
        "-p:GalleryPublishAot=true",
        "--disable-parallel",
        "-m:1",
        "/nr:false",
        "-p:GalleryPublishTrimmed=true",
        "--nologo",
        "-v:minimal"
    )

    Test-NativeAotRestoreAssets -AssetsPath $assetsPath -Runtime $runtime

    Invoke-DotNet -Arguments @(
        "publish",
        $projectPath,
        "--output",
        $packagesPath,
        "--self-contained",
        "true",
        "--framework",
        $framework,
        "--runtime",
        $runtime,
        "--configuration",
        $buildType,
        "--no-restore",
        "-p:GalleryPublishTrimmed=true",
        "-p:GalleryPublishAot=true",
        "--disable-build-servers",
        "-m:1",
        "/nr:false",
        "--nologo",
        "-v:minimal"
    )

    Test-NativeAotOutput -PackagesPath $packagesPath -Runtime $runtime
} else {
    Invoke-DotNet -Arguments @(
        "publish",
        $projectPath,
        "--output",
        $packagesPath,
        "--self-contained",
        "--framework",
        $framework,
        "--runtime",
        $runtime,
        "--configuration",
        $buildType,
        "-p:GalleryPublishTrimmed=true",
        "-p:GalleryPublishAot=false",
        "-p:GalleryPublishReadyToRun=$($publishReadyToRunEnabled.ToString().ToLowerInvariant())",
        "-p:PublishSingleFile=true"
    )
}

Copy-InstallerAssets -PackagesPath $packagesPath

if ($IsMacOS) {
    Test-PackageFile -RelativePath "Assets/Images/AtomUIGallery.icns"
    Test-PackageFile -RelativePath "Assets/Images/DmgInstallerBg@2x.png"
    Copy-Item -Path (Join-Path -Path $configsPath -ChildPath "InstallerConfig.dmg.xml") -Destination $configPath/InstallerConfig.xml -Force -ErrorAction Stop
} elseif ($IsWindows) {
    Copy-Item -Path (Join-Path -Path $configsPath -ChildPath "InstallerConfig.wix.xml") -Destination $configPath/InstallerConfig.xml -Force -ErrorAction Stop
} elseif ($IsLinux) {
    Copy-Item -Path (Join-Path -Path $configsPath -ChildPath "InstallerConfig.appimage.xml") -Destination $configPath/InstallerConfig.xml -Force -ErrorAction Stop
} else {
    throw "Unsupported operating system for Gallery installer config generation."
}

if (-not (Update-VersionInFile -XmlFilePath $versionPropsPath -TargetFilePath "$configPath/InstallerConfig.xml")) {
    throw "Failed to update Gallery installer config version."
}
