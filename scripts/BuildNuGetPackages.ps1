[CmdletBinding()]
param(
    [ValidateSet("Debug", "Release")]
    [string]$BuildType = "Release",
    [string]$PackageOutputDir = "",
    [string]$PackageValidationBaselineVersion = "",
    [string]$NativeCompilerSourceRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

. "$PSScriptRoot/NuGetPackageProjects.ps1"
. "$PSScriptRoot/verification/PackageBaseline.ps1"

if ([string]::IsNullOrWhiteSpace($PackageOutputDir)) {
    $PackageOutputDir = Join-Path $repositoryRoot ".artifacts/Nuget/$BuildType"
}
$PackageOutputDir = [System.IO.Path]::GetFullPath($PackageOutputDir)
New-Item -Path $PackageOutputDir -ItemType Directory -Force | Out-Null

function Invoke-AtomUIDotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host "dotnet $($Arguments -join ' ')"
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments[0]) failed with exit code $LASTEXITCODE"
    }
}

$buildArguments = @(
    "--configuration", $BuildType,
    "--disable-build-servers",
    "-m:1",
    "/nr:false"
)
if (-not [string]::IsNullOrWhiteSpace($NativeCompilerSourceRoot)) {
    $buildArguments += "-p:AtomUINative8CompilerSourceRoot=$NativeCompilerSourceRoot"
}

# When a baseline version is supplied, the packages are validated against that released version:
# ApiCompat compares the C# public API (build/PackageValidation.props) and verify-package-layout.ps1
# compares the consumer-visible package layout. The baseline packages are fetched by the build step
# below, whose implicit restore carries the validation property; packing then reuses that restore
# because it runs with --no-build. Do not add a separate `dotnet restore` here: a restore without
# the Release configuration would overwrite the assets file with the Debug target frameworks.
$validationArgumentsByProject = @{}
foreach ($package in $AtomUIReleasePackages) {
    $baselineVersion = Get-AtomUIPackageValidationBaselineVersion -Package $package `
        -BaselineVersion $PackageValidationBaselineVersion
    $validationArgumentsByProject[$package.ProjectPath] = @()
    if ($baselineVersion) {
        $validationArgumentsByProject[$package.ProjectPath] = @("-p:AtomUIPackageValidationBaselineVersion=$baselineVersion")
    }
}

# Prerequisite tool projects are not published packages, so they must never be validated against a
# released baseline (build-only tool projects have no published package).
foreach ($project in $AtomUIReleaseBuildPrerequisiteProjects) {
    Invoke-AtomUIDotNet -Arguments (@("build", $project, "-t:BuildToolchain") + $buildArguments)
}

foreach ($project in $AtomUIReleasePackageProjects) {
    Invoke-AtomUIDotNet -Arguments (@("build", $project) + $buildArguments + $validationArgumentsByProject[$project])
}

foreach ($project in $AtomUIReleasePackageProjects) {
    Invoke-AtomUIDotNet -Arguments (@(
        "pack",
        $project,
        "--no-build",
        "--output", $PackageOutputDir
    ) + $buildArguments + $validationArgumentsByProject[$project])
}

[xml]$versionProps = Get-Content -LiteralPath (Join-Path $repositoryRoot "build/Versions.props")
$version = [string]$versionProps.Project.PropertyGroup.AtomUIVersion
if ([string]::IsNullOrWhiteSpace($version)) {
    throw "Unable to read AtomUIVersion from build/Versions.props"
}

$expectedPackageNames = @($AtomUIExpectedPackageIds | ForEach-Object { "$($_).$version.nupkg" })
$actualPackageNames = @(
    Get-ChildItem -LiteralPath $PackageOutputDir -Filter "*.nupkg" -File |
        Select-Object -ExpandProperty Name
)
$missingPackages = @($expectedPackageNames | Where-Object { $_ -notin $actualPackageNames })
$unexpectedPackages = @($actualPackageNames | Where-Object { $_ -notin $expectedPackageNames })

if ($missingPackages.Count -gt 0) {
    throw "Missing NuGet packages: $($missingPackages -join ', ')"
}
if ($unexpectedPackages.Count -gt 0) {
    throw "Unexpected NuGet packages: $($unexpectedPackages -join ', ')"
}

. "$PSScriptRoot/verification/PackageFrameworks.ps1"
$expectedProductFrameworks = if ($BuildType -eq "Release") { @("net8.0", "net10.0") } else { @("net10.0") }
foreach ($package in $AtomUIReleasePackages) {
    $packagePath = Join-Path $PackageOutputDir "$($package.PackageId).$version.nupkg"
    Assert-AtomUIPackageFrameworks -PackagePath $packagePath -Package $package -BuildType $BuildType -Version $version
}

Write-Output "Verified $($actualPackageNames.Count) AtomUI NuGet packages for version $version"
Write-Output "Verified $BuildType product target frameworks: $($expectedProductFrameworks -join ', ')"

# Consumer-visible package layout (tools/, buildTransitive/, build/, target-framework set). ApiCompat
# does not see these, yet they are the class of break that shipped undocumented in 6.1.9. Run this
# whenever a baseline is supplied; acknowledged changes live in package-layout-allowlist.json.
if (-not [string]::IsNullOrWhiteSpace($PackageValidationBaselineVersion)) {
    $layoutVerification = Join-Path $PSScriptRoot "verification/verify-package-layout.ps1"
    & pwsh -NoLogo -NoProfile -File $layoutVerification `
        -PackageDirectory $PackageOutputDir `
        -BaselineVersion $PackageValidationBaselineVersion
    if ($LASTEXITCODE -ne 0) {
        throw "Package layout verification failed against $PackageValidationBaselineVersion."
    }
}
