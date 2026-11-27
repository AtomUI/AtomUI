param (
    [string]$localSourcesDir = "D:/nuget.local",
    [string]$buildType = "Release"
)

$ErrorActionPreference = "Stop"
$packageOutputDir = Join-Path $PSScriptRoot "../.artifacts/Nuget/$buildType"

function Push-NuGetPackages {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [string]$PackagePath = "../.artifacts/Nuget",
        [Parameter(Mandatory = $true)]
        [string]$Source
    )

    # Collects every NuGet package
    $packages = Get-ChildItem -LiteralPath $PackagePath -Filter "*.nupkg" -File |
        Sort-Object Name

    if (-not $packages) {
        Write-Warning "未找到任何.nupkg文件"
        return
    }

    $pushedPackages = @()
    foreach ($pkg in $packages) {
        if ($PSCmdlet.ShouldProcess($pkg.Name, "推送")) {
            dotnet nuget push $pkg.FullName --source $Source --skip-duplicate
            if ($LASTEXITCODE -ne 0) {
                throw "推送失败: $($pkg.Name) (退出码: $LASTEXITCODE)"
            }

            $pushedPackages += $pkg
            Write-Host "成功: $($pkg.Name)" -ForegroundColor Green
        }
    }

    foreach ($pkg in $pushedPackages) {
        Remove-Item $pkg.FullName -Force
    }
}

& "$PSScriptRoot/BuildNuGetPackages.ps1" -BuildType $buildType -PackageOutputDir $packageOutputDir

Push-NuGetPackages -PackagePath $packageOutputDir -Source $localSourcesDir
