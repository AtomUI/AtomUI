#requires -Version 7.0
<#
.SYNOPSIS
    Fails a release when a NuGet package's consumer-visible layout changes without being acknowledged.

.DESCRIPTION
    Package-version validation (EnablePackageValidation / ApiCompat) compares assemblies under
    lib/ only. It does not see changes to tools/, buildTransitive/, build/ or the set of target
    frameworks, which is exactly the class of break that shipped undocumented in 6.1.9
    (tools/netstandard2.0/AtomUI.Build.Tasks.dll moved to tools/net10.0/).

    This gate compares the layout of the packages about to be released with the previously
    released baseline packages and reports added and removed entries. Any difference fails the
    run unless the package id is listed in the allow list with a reason, and every allow list
    entry must be used by the current comparison so the allow list cannot rot.

    Layout entries are the consumer-visible payload paths: lib/, ref/, runtimes/, analyzers/,
    tools/, build/ and buildTransitive/. Packaging plumbing such as _rels/, [Content_Types].xml,
    the .nuspec and top-level notice files is ignored.

.PARAMETER PackagePath
    A single package to compare. Use with -BaselinePackage.

.PARAMETER BaselinePackage
    The previously released package to compare against. Use with -PackagePath.

.PARAMETER PackageDirectory
    A directory of packages to compare, typically the release output directory.

.PARAMETER BaselineVersion
    The released version to compare against in -PackageDirectory mode. The baseline package is
    resolved from -BaselineDirectory, the local NuGet cache, or nuget.org.

.PARAMETER BaselineDirectory
    Optional directory holding baseline packages named <id>.<version>.nupkg.

.PARAMETER AllowListPath
    Optional JSON file keyed by baseline version and package id:
    { "<baselineVersion>": { "<id>": { "allowed": ["<entry or glob>"], "reason": "<why>" } } }
    When omitted, the ATOMUI_PACKAGE_LAYOUT_ALLOWLIST environment variable is used, and failing
    that the allow list next to this script. Acknowledgements are scoped to the baseline version
    they were made against.

.EXAMPLE
    pwsh -NoProfile -File scripts/verification/verify-package-layout.ps1 `
        -PackageDirectory .artifacts/Nuget/Release -BaselineVersion 6.1.8

.EXAMPLE
    pwsh -NoProfile -File scripts/verification/verify-package-layout.Tests.ps1
#>

[CmdletBinding(DefaultParameterSetName = 'Pair')]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Pair', Position = 0)]
    [string]$PackagePath,

    [Parameter(Mandatory = $true, ParameterSetName = 'Pair')]
    [string]$BaselinePackage,

    [Parameter(Mandatory = $true, ParameterSetName = 'Directory')]
    [string]$PackageDirectory,

    [Parameter(ParameterSetName = 'Directory')]
    [string]$BaselineVersion,

    [Parameter(ParameterSetName = 'Directory')]
    [string]$BaselineDirectory,

    [string]$AllowListPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$layoutPrefixes = @('lib/', 'ref/', 'runtimes/', 'analyzers/', 'tools/', 'build/', 'buildTransitive/')

if (-not $AllowListPath) {
    if ($env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST) {
        $AllowListPath = $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST
    }
    else {
        $defaultAllowList = Join-Path $PSScriptRoot 'package-layout-allowlist.json'
        if (Test-Path -LiteralPath $defaultAllowList -PathType Leaf) {
            $AllowListPath = $defaultAllowList
        }
    }
}

function Get-PackageIdentity {
    param([Parameter(Mandatory = $true)][string]$Path)
    $zip = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $nuspec = $zip.Entries |
            Where-Object { $_.FullName.EndsWith('.nuspec', [StringComparison]::OrdinalIgnoreCase) } |
            Select-Object -First 1
        if (-not $nuspec) { throw "Package '$Path' has no .nuspec." }
        $reader = [IO.StreamReader]::new($nuspec.Open())
        try { [xml]$document = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $document.package.metadata
        return [PSCustomObject]@{
            Id      = [string]$metadata.id
            Version = [string]$metadata.version
        }
    }
    finally { $zip.Dispose() }
}

function Get-LayoutEntries {
    param([Parameter(Mandatory = $true)][string]$Path)
    $zip = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        $entries = foreach ($entry in $zip.Entries) {
            $name = $entry.FullName.Replace('\', '/')
            if ($entry.Length -eq 0 -and $name.EndsWith('/')) { continue }
            foreach ($prefix in $layoutPrefixes) {
                if ($name.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
                    $name
                    break
                }
            }
        }
    }
    finally { $zip.Dispose() }
    return @($entries | Sort-Object -Unique)
}

function Read-AllowList {
    param([string]$Path)
    if (-not $Path) { return @{} }
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Allow list '$Path' does not exist."
    }
    $raw = Get-Content -LiteralPath $Path -Raw
    if ([string]::IsNullOrWhiteSpace($raw)) {
        throw "Allow list '$Path' is empty."
    }
    $document = $raw | ConvertFrom-Json -AsHashtable
    if ($null -eq $document) {
        throw "Allow list '$Path' must contain a JSON object keyed by baseline version."
    }
    foreach ($baselineVersion in $document.Keys) {
        foreach ($packageId in $document[$baselineVersion].Keys) {
            $entry = $document[$baselineVersion][$packageId]
            if (-not $entry.ContainsKey('reason') -or [string]::IsNullOrWhiteSpace([string]$entry['reason'])) {
                throw "Allow list entry '$baselineVersion / $packageId' must declare a non-empty 'reason'."
            }
            if (-not $entry.ContainsKey('allowed')) {
                throw "Allow list entry '$baselineVersion / $packageId' must declare 'allowed' patterns."
            }
        }
    }
    return $document
}

function Resolve-BaselinePackage {
    param(
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$Version
    )
    $fileName = "$Id.$Version.nupkg"
    if ($BaselineDirectory) {
        $candidate = Join-Path $BaselineDirectory $fileName
        if (Test-Path -LiteralPath $candidate -PathType Leaf) { return $candidate }
    }
    $cachePath = Join-Path (Join-Path (Join-Path $HOME '.nuget/packages') $Id.ToLowerInvariant()) $Version
    $cached = Join-Path $cachePath $fileName
    if (Test-Path -LiteralPath $cached -PathType Leaf) { return $cached }

    $downloadRoot = Join-Path ([IO.Path]::GetTempPath()) ('atomui-layout-baseline-' + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $downloadRoot -Force | Out-Null
    $target = Join-Path $downloadRoot $fileName
    $url = "https://api.nuget.org/v3-flatcontainer/$($Id.ToLowerInvariant())/$Version/$($Id.ToLowerInvariant()).$Version.nupkg"
    try {
        Invoke-WebRequest -Uri $url -OutFile $target -UseBasicParsing
    }
    catch {
        [Console]::Error.WriteLine("Could not resolve baseline package '$Id' $Version from '$BaselineDirectory', the NuGet cache, or nuget.org. Restore the baseline with EnablePackageValidation=true first.")
        exit 2
    }
    return $target
}

function Compare-Layout {
    param(
        [Parameter(Mandatory = $true)][string]$CurrentPackage,
        [Parameter(Mandatory = $true)][string]$BaselinePackagePath,
        [Parameter(Mandatory = $true)]$AllowList,
        [Parameter(Mandatory = $true)][ref]$UsedAllowListEntries
    )
    $identity = Get-PackageIdentity -Path $CurrentPackage
    $baselineIdentity = Get-PackageIdentity -Path $BaselinePackagePath

    $current = Get-LayoutEntries -Path $CurrentPackage
    $baseline = Get-LayoutEntries -Path $BaselinePackagePath

    $added = @($current | Where-Object { $baseline -notcontains $_ })
    $removed = @($baseline | Where-Object { $current -notcontains $_ })

    # Allow list entries are scoped to the baseline version they acknowledge, so an older
    # acknowledgement does not silently cover a later release.
    $patterns = @()
    if ($AllowList.ContainsKey($baselineIdentity.Version) -and
        $AllowList[$baselineIdentity.Version].ContainsKey($identity.Id)) {
        $patterns = @($AllowList[$baselineIdentity.Version][$identity.Id]['allowed'])
    }

    $changed = New-Object System.Collections.Generic.List[string]
    foreach ($path in $added) { $changed.Add("+ $path") }
    foreach ($path in $removed) { $changed.Add("- $path") }

    $unacknowledged = New-Object System.Collections.Generic.List[string]
    foreach ($line in $changed) {
        $matched = $false
        for ($index = 0; $index -lt $patterns.Count; $index++) {
            if ($line.Substring(2) -like $patterns[$index]) {
                $matched = $true
                $UsedAllowListEntries.Value["$($baselineIdentity.Version)|$($identity.Id)"][$index] = $true
            }
        }
        if (-not $matched) { $unacknowledged.Add($line) }
    }

    if ($unacknowledged.Count -eq 0) {
        Write-Host "$($identity.Id): layout matches $($baselineIdentity.Version)"
        return 0
    }

    Write-Host "$($identity.Id): layout differs from $($baselineIdentity.Version)"
    foreach ($line in $unacknowledged) { Write-Host "  $line" }
    Write-Host "  $($unacknowledged.Count) unacknowledged layout change(s)."
    return 1
}

$allowList = Read-AllowList -Path $AllowListPath
$usedAllowListEntries = @{}
foreach ($allowListBaselineVersion in $allowList.Keys) {
    foreach ($packageId in $allowList[$allowListBaselineVersion].Keys) {
        $key = "$allowListBaselineVersion|$packageId"
        $usedAllowListEntries[$key] = @{}
        for ($index = 0; $index -lt @($allowList[$allowListBaselineVersion][$packageId]['allowed']).Count; $index++) {
            $usedAllowListEntries[$key][$index] = $false
        }
    }
}

$pairs = @()
if ($PSCmdlet.ParameterSetName -eq 'Pair') {
    if (-not (Test-Path -LiteralPath $PackagePath -PathType Leaf)) {
        [Console]::Error.WriteLine("Package '$PackagePath' does not exist.")
        exit 2
    }
    if (-not (Test-Path -LiteralPath $BaselinePackage -PathType Leaf)) {
        [Console]::Error.WriteLine("Baseline package '$BaselinePackage' does not exist.")
        exit 2
    }
    $pairs += [PSCustomObject]@{ Current = $PackagePath; Baseline = $BaselinePackage }
}
else {
    if (-not (Test-Path -LiteralPath $PackageDirectory -PathType Container)) {
        [Console]::Error.WriteLine("Package directory '$PackageDirectory' does not exist.")
        exit 2
    }
    if ([string]::IsNullOrWhiteSpace($BaselineVersion)) {
        [Console]::Error.WriteLine('-BaselineVersion is required with -PackageDirectory.')
        exit 2
    }
    . (Join-Path $PSScriptRoot '../NuGetPackageProjects.ps1')
    . (Join-Path $PSScriptRoot 'PackageBaseline.ps1')
    $packageCount = 0
    foreach ($package in (Get-ChildItem -LiteralPath $PackageDirectory -Filter '*.nupkg' -File | Sort-Object Name)) {
        $packageCount++
        $identity = Get-PackageIdentity -Path $package.FullName
        $releasePackage = $AtomUIReleasePackages | Where-Object PackageId -eq $identity.Id
        if ($null -ne $releasePackage -and
            -not (Get-AtomUIPackageValidationBaselineVersion -Package $releasePackage -BaselineVersion $BaselineVersion)) {
            continue
        }
        $pairs += [PSCustomObject]@{
            Current  = $package.FullName
            Baseline = (Resolve-BaselinePackage -Id $identity.Id -Version $BaselineVersion)
        }
    }
    if ($packageCount -eq 0) {
        # An empty directory means the preceding pack step produced nothing; silently passing would
        # turn a broken release build into a green layout check.
        [Console]::Error.WriteLine("No *.nupkg found in '$PackageDirectory'; pack the release packages before running layout verification.")
        exit 2
    }
}

$failureCount = 0
$comparedBaselineVersions = @{}
foreach ($pair in $pairs) {
    $failureCount += Compare-Layout -CurrentPackage $pair.Current -BaselinePackagePath $pair.Baseline `
        -AllowList $allowList -UsedAllowListEntries ([ref]$usedAllowListEntries)
    $comparedBaselineVersions[(Get-PackageIdentity -Path $pair.Baseline).Version] = $true
}

# Only entries scoped to a baseline version compared in this run can be checked for staleness.
$unusedEntries = @()
foreach ($key in $usedAllowListEntries.Keys) {
    $separator = $key.IndexOf('|', [StringComparison]::Ordinal)
    $allowListBaselineVersion = $key.Substring(0, $separator)
    $packageId = $key.Substring($separator + 1)
    if (-not $comparedBaselineVersions.ContainsKey($allowListBaselineVersion)) { continue }
    for ($index = 0; $index -lt $usedAllowListEntries[$key].Count; $index++) {
        if (-not $usedAllowListEntries[$key][$index]) {
            $unusedEntries += "$allowListBaselineVersion / $packageId -> $(@($allowList[$allowListBaselineVersion][$packageId]['allowed'])[$index])"
        }
    }
}
if ($unusedEntries.Count -gt 0) {
    Write-Host 'Unused allow list entries (remove them so the allow list stays honest):'
    foreach ($entry in $unusedEntries) { Write-Host "  $entry" }
    $failureCount += $unusedEntries.Count
}

if ($failureCount -gt 0) {
    Write-Host "Package layout verification FAILED ($failureCount finding(s)). Document the change in docs/releases/<version>-api-changes.md and acknowledge it in scripts/verification/package-layout-allowlist.json."
    exit 1
}

Write-Host 'Package layout verification passed.'
exit 0
