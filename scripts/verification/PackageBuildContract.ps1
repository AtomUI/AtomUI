# Compare the package with the SDK's evaluated pack contract, in addition to absolute role/TFM rules.
function Get-AtomUICanonicalPackageNode {
    param([System.Xml.XmlNode]$Node)
    if ($null -eq $Node) { return "" }
    $attributes = @($Node.Attributes | Where-Object { $_.NamespaceURI -ne 'http://www.w3.org/2000/xmlns/' } |
        Sort-Object LocalName -CaseSensitive | ForEach-Object { "$($_.LocalName)=$($_.Value)" })
    $children = @($Node.ChildNodes | Where-Object NodeType -eq ([System.Xml.XmlNodeType]::Element) |
        ForEach-Object { Get-AtomUICanonicalPackageNode $_ } | Sort-Object -CaseSensitive)
    return ConvertTo-Json -Compress -Depth 20 -InputObject ([ordered]@{ name = $Node.LocalName; attributes = $attributes; children = $children })
}

function Assert-AtomUIPackageBuildContract {
    param($Archive, [xml]$Nuspec, $Package, [string]$BuildType, [string]$Version)
    $requestedPackage = $Package
    . (Join-Path $PSScriptRoot '../NuGetPackageProjects.ps1')
    $Package = $requestedPackage
    $definition = @($AtomUIReleasePackages | Where-Object PackageId -ceq $Package.PackageId)
    if ($definition.Count -ne 1 -or $definition[0].PackageRole -cne $Package.PackageRole) {
        throw "Package '$($Package.PackageId)' does not match the authoritative package-role manifest."
    }
    $project = $definition[0].ProjectPath
    $projectName = [IO.Path]::GetFileNameWithoutExtension($project)
    $expectedPath = Join-Path $repositoryRoot ".artifacts/$projectName/obj/$BuildType/$($Package.PackageId).$Version.nuspec"
    $basePath = [IO.Path]::GetDirectoryName($project)
    if (Test-Path -LiteralPath $expectedPath -PathType Leaf) {
        [xml]$expected = Get-Content -LiteralPath $expectedPath -Raw
    }
    else {
        # Explicit nuspec projects (the aggregate language package) have no generated nuspec.
        # Ask MSBuild for its evaluated source/base instead of keeping a second package list here.
        $evaluation = & dotnet msbuild $project "-p:Configuration=$BuildType" '-getProperty:NuspecFile,NuspecBasePath'
        if ($LASTEXITCODE -ne 0) { throw "Cannot read evaluated pack contract for $project." }
        $properties = ($evaluation -join "`n" | ConvertFrom-Json).Properties
        if ([string]::IsNullOrWhiteSpace($properties.NuspecFile)) {
            throw "Missing generated pack contract '$expectedPath'. Build/pack the project before verification."
        }
        $expectedPath = if ([IO.Path]::IsPathFullyQualified($properties.NuspecFile)) { $properties.NuspecFile } else { Join-Path ([IO.Path]::GetDirectoryName($project)) $properties.NuspecFile }
        $basePath = if ([IO.Path]::IsPathFullyQualified($properties.NuspecBasePath)) { $properties.NuspecBasePath } else { Join-Path ([IO.Path]::GetDirectoryName($project)) $properties.NuspecBasePath }
        $expectedPath = [IO.Path]::GetFullPath($expectedPath)
        $basePath = [IO.Path]::GetFullPath($basePath)
        [xml]$expected = (Get-Content -LiteralPath $expectedPath -Raw).Replace('$version$', $Version)
    }
    if ([string]$expected.package.metadata.id -cne $Package.PackageId -or [string]$expected.package.metadata.version -cne $Version) {
        throw "Evaluated pack contract '$expectedPath' has an unexpected package identity/version."
    }
    foreach ($section in @('dependencies', 'frameworkReferences', 'contentFiles')) {
        $xpath = "/*[local-name()='package']/*[local-name()='metadata']/*[local-name()='$section']"
        if ((Get-AtomUICanonicalPackageNode $expected.SelectSingleNode($xpath)) -cne
            (Get-AtomUICanonicalPackageNode $Nuspec.SelectSingleNode($xpath))) {
            throw "$($Package.PackageId) packaged $section differs from its evaluated build contract."
        }
    }

    $expectedFiles = @{}
    foreach ($file in $expected.SelectNodes("/*[local-name()='package']/*[local-name()='files']/*[local-name()='file']")) {
        $source = [string]$file.src
        if (-not [IO.Path]::IsPathFullyQualified($source)) { $source = Join-Path $basePath $source }
        $target = ([string]$file.target).Replace('\', '/').TrimStart('/')
        if ($target.Length -eq 0 -or $target.EndsWith('/')) { $target += [IO.Path]::GetFileName($source) }
        if ($source.Contains('*') -or $target.Contains('*')) {
            throw "Unexpanded pack file rule in '$expectedPath'; supply the evaluated concrete pack contract."
        }
        $expectedFiles[$target] = $source
        if ($null -eq $Archive.GetEntry($target)) { throw "$($Package.PackageId) is missing required pack asset '$target'." }
    }
    foreach ($entry in $Archive.Entries) {
        if ($entry.FullName -match '^(lib|ref|runtimes|analyzers|tools|build|buildTransitive|content|contentFiles)/' -and
            -not $expectedFiles.ContainsKey($entry.FullName)) {
            throw "$($Package.PackageId) contains an undeclared consumer asset '$($entry.FullName)'."
        }
        if ($entry.FullName -match '^build(?:Transitive)?/.*\.(props|targets)$') {
            $reader = [IO.StreamReader]::new($entry.Open())
            try { [xml]$buildAsset = $reader.ReadToEnd() } finally { $reader.Dispose() }
            foreach ($import in $buildAsset.SelectNodes("//*[local-name()='Import']")) {
                $projectPath = [string]$import.Project
                # Static package-local imports form a deliverable closure even when the branch is
                # not used by the current framework. Dynamic SDK/user imports are outside this rule.
                if ($projectPath -match '^\$\(MSBuildThisFileDirectory\)([^$%@?*]+)$') {
                    $relative = $Matches[1].Replace('\', '/')
                    $resolved = [Uri]::new([Uri]::new("https://atomui.invalid/$($entry.FullName)"), $relative)
                    $target = [Uri]::UnescapeDataString($resolved.AbsolutePath.TrimStart('/'))
                    if ($resolved.Host -ne 'atomui.invalid' -or $null -eq $Archive.GetEntry($target)) {
                        throw "$($Package.PackageId) build asset '$($entry.FullName)' imports missing package asset '$target'."
                    }
                }
            }
        }
    }
    foreach ($target in $expectedFiles.Keys) {
        $stream = $Archive.GetEntry($target).Open()
        $sha = [Security.Cryptography.SHA256]::Create()
        try { $actualHash = [Convert]::ToHexString($sha.ComputeHash($stream)) }
        finally { $stream.Dispose(); $sha.Dispose() }
        if ($actualHash -cne (Get-FileHash -LiteralPath $expectedFiles[$target] -Algorithm SHA256).Hash) {
            throw "$($Package.PackageId) contains stale or mismatched bytes for '$target'."
        }
    }
}
