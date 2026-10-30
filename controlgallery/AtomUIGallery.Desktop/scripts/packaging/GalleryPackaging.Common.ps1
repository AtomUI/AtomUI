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

    $stampedVersion = "$Version.$BuildStamp"
    switch ($Kind) {
        "zip" { return "$Name-$stampedVersion-$Runtime.zip" }
        "appimage" { return "$Name-$Runtime-$stampedVersion.AppImage" }
        "dmg" { return "$Name-$stampedVersion-$Runtime.dmg" }
    }
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
    $fileName = [System.IO.Path]::GetFileName(([uri]$asset.url).AbsolutePath)
    $filePath = Join-Path $Destination $fileName
    Write-Host "Downloading $Name $($tool.version): $($asset.url)" -ForegroundColor Cyan
    Invoke-WebRequest -Uri $asset.url -OutFile $filePath

    $actualHash = (Get-FileHash -LiteralPath $filePath -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -ne $asset.sha256.ToLowerInvariant()) {
        Remove-Item -LiteralPath $filePath -Force
        throw "SHA256 mismatch for $Name $($tool.version) ($Architecture): expected $($asset.sha256), got $actualHash."
    }
    return $filePath
}
