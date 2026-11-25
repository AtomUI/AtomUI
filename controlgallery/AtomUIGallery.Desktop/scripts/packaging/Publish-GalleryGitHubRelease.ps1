# Publishes the six verified Gallery artifacts from a single Actions run.
param(
    [Parameter(Mandatory = $true)][ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')][string]$Repository,
    [Parameter(Mandatory = $true)][ValidatePattern('^v[0-9]+\.[0-9]+\.[0-9]+(-[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$')][string]$Tag,
    [Parameter(Mandatory = $true)][ValidatePattern('^[0-9a-f]{40}$')][string]$Commit,
    [Parameter(Mandatory = $true)][string]$BuildStamp,
    [Parameter(Mandatory = $true)][string]$ArtifactDirectory
)

$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'GalleryPackaging.Common.ps1')

function Invoke-GitHub {
    param([Parameter(Mandatory = $true)][string[]]$Arguments)
    $result = & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "gh $($Arguments -join ' ') failed (exit $LASTEXITCODE)."
    }
    return $result
}

function Get-ReleaseAssets([long]$ReleaseId) {
    Invoke-GitHub -Arguments @('api', '--paginate', "repos/$Repository/releases/$ReleaseId/assets?per_page=100", '--jq', '.[]') |
        ForEach-Object { ConvertFrom-Json -InputObject $_ }
}

$version = $Tag.Substring(1)
$stamp = Get-GalleryBuildStamp -BuildStamp $BuildStamp
$matrix = @(
    @{ Artifact = 'windows-zip-x86_64'; Kind = 'zip'; Runtime = 'win-x64'; Platform = 'Windows x64' }
    @{ Artifact = 'windows-zip-arm64'; Kind = 'zip'; Runtime = 'win-arm64'; Platform = 'Windows Arm64' }
    @{ Artifact = 'dmg-packages-osx-x64'; Kind = 'dmg'; Runtime = 'osx-x64'; Platform = 'macOS Intel' }
    @{ Artifact = 'dmg-packages-osx-arm64'; Kind = 'dmg'; Runtime = 'osx-arm64'; Platform = 'macOS Apple Silicon' }
    @{ Artifact = 'linux-appimage-x86_64'; Kind = 'appimage'; Runtime = 'linux-x64'; Platform = 'Linux x86_64' }
    @{ Artifact = 'linux-appimage-arm64'; Kind = 'appimage'; Runtime = 'linux-arm64'; Platform = 'Linux Arm64' }
)

$installers = @(
    foreach ($platform in $matrix) {
        $name = Get-GalleryArtifactName -Kind $platform.Kind -Name 'AtomUIGallery' -Version $version `
            -BuildStamp $stamp -Runtime $platform.Runtime
        $folder = Join-Path $ArtifactDirectory $platform.Artifact
        $files = @(Get-ChildItem -LiteralPath $folder -Force)
        if ($files.Count -ne 1 -or $files[0].PSIsContainer -or $files[0].Name -cne $name -or $files[0].Length -eq 0) {
            throw "Expected exactly one non-empty $name in $folder."
        }
        $files[0]
    }
)

$tagObject = (Invoke-GitHub -Arguments @('api', "repos/$Repository/git/ref/tags/$Tag", '--jq', '.object')) | ConvertFrom-Json
while ($tagObject.type -eq 'tag') {
    $tagObject = (Invoke-GitHub -Arguments @('api', "repos/$Repository/git/tags/$($tagObject.sha)", '--jq', '.object')) |
        ConvertFrom-Json
}
if ($tagObject.type -ne 'commit' -or $tagObject.sha -cne $Commit) {
    throw "Tag $Tag no longer points to the commit used to build the Gallery artifacts."
}

$marker = '<!-- atomui-gallery-managed-release -->'
$releaseFilter = '.[] | select(.tag_name == "' + $Tag + '")'
$release = Invoke-GitHub -Arguments @('api', '--paginate', "repos/$Repository/releases?per_page=100", '--jq', $releaseFilter) |
    ForEach-Object { ConvertFrom-Json -InputObject $_ }
if ($release -and $release.draft -and -not ([string]$release.body).StartsWith($marker, [StringComparison]::Ordinal)) {
    throw "Release $Tag already has a draft maintained outside this workflow."
}

$readme = Join-Path $ArtifactDirectory 'Gallery-README.md'
$rows = @(
    for ($index = 0; $index -lt $matrix.Count; $index++) {
        $file = $installers[$index]
        "| $($matrix[$index].Platform) | [$($file.Name)](https://github.com/$Repository/releases/download/$Tag/$($file.Name)) |"
    }
)
@(
    $marker
    "## Gallery $Tag"
    ''
    'Choose the NativeAOT Gallery build for your system and architecture. 请按操作系统和处理器架构下载。'
    ''
    '| Platform | Download |'
    '| --- | --- |'
    $rows
    ''
    '- Windows — Extract the ZIP and run `AtomUIGallery.Desktop.exe`. 解压整个 ZIP 后运行程序。'
    '- macOS — Open the DMG and drag AtomUIGallery to Applications. 打开 DMG 后将应用拖到「应用程序」。'
    '- Linux — Run `chmod +x <file>.AppImage`, then launch the AppImage. 添加执行权限后运行。'
    ''
    "[Changelog](https://github.com/$Repository/blob/$Tag/CHANGELOG.md) · [更新日志](https://github.com/$Repository/blob/$Tag/CHANGELOG.zh-CN.md)"
) | Set-Content -LiteralPath $readme -Encoding utf8

$localFiles = $installers + (Get-Item -LiteralPath $readme)
$byName = @{}
foreach ($file in $localFiles) {
    $byName[$file.Name] = 'sha256:' + (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
}

if (-not $release) {
    $createArguments = @('release', 'create', $Tag, '--repo', $Repository, '--verify-tag', '--draft',
        '--title', "AtomUI $Tag", '--notes-file', $readme)
    if ($version.Contains('-')) {
        $createArguments += '--prerelease'
    }
    Invoke-GitHub -Arguments $createArguments | Write-Host
    $release = Invoke-GitHub -Arguments @('api', '--paginate', "repos/$Repository/releases?per_page=100", '--jq', $releaseFilter) |
        ForEach-Object { ConvertFrom-Json -InputObject $_ }
    if (-not $release -or -not $release.draft) {
        throw "Could not locate the draft release for $Tag."
    }
}

$assets = @(Get-ReleaseAssets -ReleaseId $release.id)
$expectedNames = @($localFiles.Name)
foreach ($asset in $assets) {
    if ($asset.name -match '^AtomUIGallery-.*\.(zip|dmg|AppImage)$' -and $asset.name -cnotin $expectedNames) {
        throw "Release $Tag contains a different Gallery build: $($asset.name)."
    }
    if ($asset.name -cin $expectedNames -and $asset.digest -cne $byName[$asset.name]) {
        throw "Release asset $($asset.name) already exists with different content."
    }
}

$existingNames = @($assets | ForEach-Object { $_.name })
$pending = @($localFiles | Where-Object { $_.Name -cnotin $existingNames })
if ($pending.Count -gt 0) {
    Invoke-GitHub -Arguments (@('release', 'upload', $Tag, '--repo', $Repository) + @($pending.FullName)) | Write-Host
}

$assets = @(Get-ReleaseAssets -ReleaseId $release.id)
foreach ($file in $localFiles) {
    $matches = @($assets | Where-Object { $_.name -ceq $file.Name })
    if ($matches.Count -ne 1 -or $matches[0].state -ne 'uploaded' -or $matches[0].digest -cne $byName[$file.Name]) {
        throw "Could not verify the uploaded asset $($file.Name)."
    }
}

if ($release.draft) {
    Invoke-GitHub -Arguments @('release', 'edit', $Tag, '--repo', $Repository, '--verify-tag', '--draft=false') | Write-Host
    $published = (Invoke-GitHub -Arguments @('api', "repos/$Repository/releases/$($release.id)")) | ConvertFrom-Json
    if ($published.draft -or $published.tag_name -cne $Tag) {
        throw "Release $Tag is not public after the publish step."
    }
}
Write-Host "Verified six Gallery installers and the README for $Tag."
