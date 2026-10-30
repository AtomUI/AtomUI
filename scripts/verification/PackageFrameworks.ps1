# Shared absolute package contract, independent of a previous (possibly broken) release baseline.
function Assert-AtomUIPackageFrameworks {
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath,
        [Parameter(Mandatory = $true)]$Package,
        [Parameter(Mandatory = $true)][ValidateSet("Debug", "Release")][string]$BuildType,
        [Parameter(Mandatory = $true)][string]$Version
    )

    if ($Package.PackageRole -notin @("RuntimeProduct", "Analyzer", "Language", "Template", "BuildTool")) {
        throw "Unknown package role for $($Package.PackageId): $($Package.PackageRole)"
    }
    $archive = [System.IO.Compression.ZipFile]::OpenRead($PackagePath)
    try {
        $names = @($archive.Entries | ForEach-Object { $_.FullName })
        if (@($names | Group-Object | Where-Object Count -gt 1).Count -ne 0) {
            throw "Package '$PackagePath' has duplicate ZIP paths."
        }
        $nuspecs = @($archive.Entries | Where-Object { $_.FullName -match '^[^/]+\.nuspec$' })
        if ($nuspecs.Count -ne 1) { throw "Package '$PackagePath' requires exactly one nuspec." }
        $reader = [System.IO.StreamReader]::new($nuspecs[0].Open())
        try { [xml]$nuspec = $reader.ReadToEnd() } finally { $reader.Dispose() }
        $metadata = $nuspec.package.metadata
        if ([string]$metadata.id -cne $Package.PackageId -or [string]$metadata.version -cne $Version) {
            throw "Package '$PackagePath' has an unexpected nuspec identity/version."
        }
        . (Join-Path $PSScriptRoot 'PackageBuildContract.ps1')
        Assert-AtomUIPackageBuildContract -Archive $archive -Nuspec $nuspec -Package $Package -BuildType $BuildType -Version $Version

        $libraries = @($archive.Entries | Where-Object { $_.FullName -match '^lib/[^/]+/[^/]+\.dll$' })
        if ($Package.PackageRole -ne "RuntimeProduct") {
            if ($libraries.Count -ne 0) {
                throw "$($Package.PackageId) ($($Package.PackageRole)) must not acquire runtime lib/ assemblies."
            }
            return
        }

        [string[]]$expectedFrameworks = if ($BuildType -eq "Release") { @("net8.0", "net10.0") } else { @("net10.0") }
        $actualFrameworks = @($libraries | ForEach-Object { $_.FullName.Split('/')[1] } | Sort-Object -Unique)
        if ($actualFrameworks.Count -ne $expectedFrameworks.Count -or
            @($expectedFrameworks | Where-Object { $_ -cnotin $actualFrameworks }).Count -gt 0) {
            throw "$($Package.PackageId) $BuildType package must contain lib/ assets for $($expectedFrameworks -join ', '); actual: $($actualFrameworks -join ', ')."
        }

        if ($null -eq ("AtomUI.PackageVerification.AssemblyMetadata" -as [type])) {
            Add-Type -Path (Join-Path $PSScriptRoot "PackageAssemblyMetadata.cs")
        }
        $assemblyVersions = @()
        $versionComponents = @((($Version -split '[-+]')[0]) -split '\.')
        while ($versionComponents.Count -lt 4) { $versionComponents += '0' }
        $expectedAssemblyVersion = ([Version]($versionComponents -join '.')).ToString()
        foreach ($framework in $expectedFrameworks) {
            $entries = @($libraries | Where-Object { $_.FullName.Split('/')[1] -ceq $framework })
            if ($entries.Count -ne 1 -or $entries[0].FullName -cne "lib/$framework/$($Package.PackageId).dll") {
                throw "$($Package.PackageId) must have exactly its product assembly under lib/$framework."
            }
            $stream = $entries[0].Open()
            try { $info = [AtomUI.PackageVerification.AssemblyMetadata]::Read($stream) } finally { $stream.Dispose() }
            $expectedTarget = ".NETCoreApp,Version=v$($framework.Substring(3))"
            if ($info[0] -cne $Package.PackageId -or $info[1] -cne $expectedAssemblyVersion -or $info[2] -cne $expectedTarget) {
                throw "$($entries[0].FullName) has assembly identity/version/target '$($info -join ' / ')', expected '$($Package.PackageId) / $expectedAssemblyVersion / $expectedTarget'."
            }
            $assemblyVersions += $info[1]
        }
        if (@($assemblyVersions | Sort-Object -Unique).Count -ne 1) {
            throw "$($Package.PackageId) target frameworks contain different assembly versions."
        }

        $groups = @($metadata.SelectNodes("*[local-name()='dependencies']/*[local-name()='group']"))
        $groupFrameworks = @($groups | ForEach-Object { [string]$_.targetFramework })
        if ($groupFrameworks.Count -ne $expectedFrameworks.Count -or
            @($expectedFrameworks | Where-Object { $_ -cnotin $groupFrameworks }).Count -gt 0) {
            throw "$($Package.PackageId) nuspec dependency groups must match $($expectedFrameworks -join ', ')."
        }
        foreach ($group in $groups) {
            $dependencies = @($group.SelectNodes("*[local-name()='dependency']"))
            if (@($dependencies | Group-Object id | Where-Object Count -gt 1).Count -gt 0) {
                throw "$($Package.PackageId) has duplicate dependencies in $($group.targetFramework)."
            }
            foreach ($dependency in $dependencies) {
                if ([string]::IsNullOrWhiteSpace([string]$dependency.id) -or [string]::IsNullOrWhiteSpace([string]$dependency.version)) {
                    throw "$($Package.PackageId) has an incomplete dependency in $($group.targetFramework)."
                }
            }
        }
    }
    finally { $archive.Dispose() }
}
