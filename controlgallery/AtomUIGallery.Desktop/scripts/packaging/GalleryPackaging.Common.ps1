# Shared helpers for the Gallery installer packaging scripts (zip, AppImage, DMG).
# Dot-source this file; it depends only on pwsh 7 and the platform tools each script invokes.

Set-StrictMode -Version Latest

$script:GalleryPackagingToolManifestPath = Join-Path $PSScriptRoot "packaging-tools.json"

function Read-GalleryInstallerConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Installer config was not found: $Path"
    }

    [xml]$document = Get-Content -LiteralPath $Path -Raw
    $root = $document.Installer
    if ($null -eq $root) {
        throw "Installer config '$Path' has no <Installer> root element."
    }

    function Get-Element([string]$Name) {
        $node = $root.SelectSingleNode($Name)
        if ($null -eq $node) {
            return $null
        }
        $value = $node.InnerText.Trim()
        if ([string]::IsNullOrWhiteSpace($value)) {
            return $null
        }
        return $value
    }

    $config = [pscustomobject]@{
        Name                     = Get-Element "Name"
        AssemblyName             = Get-Element "AssemblyName"
        Version                  = Get-Element "Version"
        AppBundleIdentifier      = Get-Element "AppBundleIdentifier"
        Title                    = Get-Element "Title"
        Company                  = Get-Element "Company"
        CompanyFullName          = Get-Element "CompanyFullName"
        CompanyUrl               = Get-Element "CompanyUrl"
        Copyright                = Get-Element "Copyright"
        AppIcon                  = Get-Element "AppIcon"
        InstallerApplicationIcon = Get-Element "InstallerApplicationIcon"
        Background               = Get-Element "Background"
    }

    foreach ($required in "Name", "AssemblyName", "Version") {
        if ($null -eq $config.$required) {
            throw "Installer config '$Path' is missing <$required>."
        }
    }

    # Values are written into .desktop entries and Info.plist; a line break would inject extra keys.
    foreach ($property in $config.PSObject.Properties) {
        if ($null -ne $property.Value -and $property.Value -match '[\r\n]') {
            throw "Installer config '$Path' value <$($property.Name)> must be a single line."
        }
    }

    # The repository templates contain __VERSION__; PublishToLocal.ps1 replaces it in the published copy.
    if ($config.Version -notmatch '^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?$') {
        throw "Installer config '$Path' has an invalid version '$($config.Version)'; use the published config, not the repository template."
    }

    return $config
}

function Get-GalleryBuildStamp {
    param(
        [string]$BuildStamp
    )

    if ([string]::IsNullOrWhiteSpace($BuildStamp)) {
        # In CI every job must use the stamp computed once by the Prepare job.
        if ($env:GITHUB_ACTIONS -eq "true") {
            throw "A build stamp is required in CI; pass the Prepare job output."
        }
        return [DateTime]::UtcNow.ToString("MMdd")
    }
    if ($BuildStamp -notmatch '^(0[1-9]|1[0-2])(0[1-9]|[12]\d|3[01])$') {
        throw "Build stamp '$BuildStamp' must be a UTC month and day in MMdd form."
    }
    return $BuildStamp
}

function Get-GalleryArtifactName {
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet("zip", "appimage", "dmg")]
        [string]$Kind,
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Version,
        [Parameter(Mandatory = $true)]
        [string]$BuildStamp,
        [Parameter(Mandatory = $true)]
        [string]$Runtime
    )

    switch ($Kind) {
        "zip" { return "$(Add-GalleryBuildStamp "$Name-$Version" $BuildStamp)-$Runtime.zip" }
        "appimage" { return "$(Add-GalleryBuildStamp "$Name-$Runtime-$Version" $BuildStamp).AppImage" }
        "dmg" { return "$(Add-GalleryBuildStamp "$Name-$Version" $BuildStamp)-$Runtime.dmg" }
    }
}

# Inserts ".{stamp}" after the digits following the last dot, as the previous installer tool did:
# 6.2.10 -> 6.2.10.1030, 6.0.0-rc -> 6.0.0.1030-rc, 6.2.0-beta.1 -> 6.2.0-beta.1.1030.
function Add-GalleryBuildStamp([string]$Text, [string]$Stamp) {
    $position = $Text.LastIndexOf('.') + 1
    while ($position -lt $Text.Length -and [char]::IsDigit($Text[$position])) {
        $position++
    }
    return $Text.Insert($position, ".$Stamp")
}

function New-GalleryToolsDirectory {
    # A fresh directory per run: concurrent runs cannot replace each other's verified tools,
    # and nothing is executed from a predictable shared path. CI runners provide RUNNER_TEMP.
    $root = if ($env:RUNNER_TEMP) { $env:RUNNER_TEMP } else { [System.IO.Path]::GetTempPath() }
    $path = Join-Path $root "atomui-packaging-tools-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $path | Out-Null
    return $path
}

function Invoke-GalleryCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,
        [string[]]$ArgumentList = @(),
        [string]$WorkingDirectory = (Get-Location).Path
    )

    Write-Host "$FilePath $($ArgumentList -join ' ')" -ForegroundColor Cyan
    Push-Location $WorkingDirectory
    try {
        & $FilePath @ArgumentList
        if ($LASTEXITCODE -ne 0) {
            throw "$FilePath failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function Install-GalleryPackagingTool {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [string]$Architecture,
        [Parameter(Mandatory = $true)]
        [string]$Destination
    )

    $manifest = Get-Content -LiteralPath $script:GalleryPackagingToolManifestPath -Raw | ConvertFrom-Json
    $tool = $manifest.tools.$Name
    if ($null -eq $tool) {
        throw "Packaging tool '$Name' is not listed in $script:GalleryPackagingToolManifestPath."
    }
    $asset = $tool.assets.$Architecture
    if ($null -eq $asset) {
        throw "Packaging tool '$Name' $($tool.version) has no asset for '$Architecture'."
    }

    New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    # Multi-file tools list each file with its own checksum; the tool root directory is returned.
    if ($asset.PSObject.Properties.Name -contains "files") {
        $toolRoot = Join-Path $Destination $Name
        foreach ($file in $asset.files) {
            Save-GalleryVerifiedFile -Url $file.url -Sha256 $file.sha256 -Path (Join-Path $toolRoot $file.path) `
                -Description "$Name $($tool.version) $($file.path)"
        }
        return $toolRoot
    }

    $fileName = [System.IO.Path]::GetFileName(([uri]$asset.url).AbsolutePath)
    $filePath = Join-Path $Destination $fileName
    Save-GalleryVerifiedFile -Url $asset.url -Sha256 $asset.sha256 -Path $filePath `
        -Description "$Name $($tool.version) ($Architecture)"
    return $filePath
}

function Save-GalleryVerifiedFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Url,
        [Parameter(Mandatory = $true)]
        [string]$Sha256,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    New-Item -ItemType Directory -Path (Split-Path -Parent $Path) -Force | Out-Null
    Write-Host "Downloading $($Description): $Url" -ForegroundColor Cyan
    # Retry connection-level failures too (TLS resets, DNS); -MaximumRetryCount only covers HTTP status errors.
    for ($attempt = 1; ; $attempt++) {
        try {
            Invoke-WebRequest -Uri $Url -OutFile $Path
            break
        }
        catch {
            if ($attempt -ge 3) {
                throw
            }
            Write-Host "Download attempt $attempt failed: $($_.Exception.Message) Retrying." -ForegroundColor Yellow
            Start-Sleep -Seconds 5
        }
    }

    $actualHash = (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -ne $Sha256.ToLowerInvariant()) {
        Remove-Item -LiteralPath $Path -Force
        throw "SHA256 mismatch for $($Description): expected $Sha256, got $actualHash."
    }
}
