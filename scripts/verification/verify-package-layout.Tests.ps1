#requires -Version 7.0
<#
.SYNOPSIS
    Tests for verify-package-layout.ps1.

.DESCRIPTION
    Builds synthetic .nupkg files in a temp directory and asserts that the layout gate
    detects package-layout breaks (the class of change that shipped undocumented in
    6.1.9) and that it accepts identical layouts and explicitly acknowledged changes.

.EXAMPLE
    pwsh -NoProfile -File scripts/verification/verify-package-layout.Tests.ps1
#>

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$gate = Join-Path $PSScriptRoot 'verify-package-layout.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) ('atomui-layout-tests-' + [Guid]::NewGuid().ToString('N'))
$script:failures = @()
$script:passed = 0

function New-TestPackage {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Id,
        [Parameter(Mandatory = $true)][string]$Version,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$Entries
    )
    $zip = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $nuspec = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata>
    <id>$Id</id>
    <version>$Version</version>
  </metadata>
</package>
"@
        $entry = $zip.CreateEntry("$Id.nuspec")
        $writer = [IO.StreamWriter]::new($entry.Open())
        try { $writer.Write($nuspec) } finally { $writer.Dispose() }

        foreach ($name in $Entries) {
            $entry = $zip.CreateEntry($name)
            $stream = $entry.Open()
            $writer = [IO.StreamWriter]::new($stream)
            try { $writer.Write('placeholder') } finally { $writer.Dispose() }
        }
    }
    finally { $zip.Dispose() }
}

function New-Scenario {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$BaselineEntries,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][string[]]$CurrentEntries,
        [string]$Id = 'Scenarios'
    )
    $directory = Join-Path $root $Name
    New-Item -ItemType Directory -Path $directory -Force | Out-Null
    $baseline = Join-Path $directory 'baseline.nupkg'
    $current = Join-Path $directory 'current.nupkg'
    New-TestPackage -Path $baseline -Id $Id -Version '6.1.8' -Entries $BaselineEntries
    New-TestPackage -Path $current -Id $Id -Version '6.1.9' -Entries $CurrentEntries
    return [PSCustomObject]@{ Baseline = $baseline; Current = $current }
}

function Invoke-Gate {
    param(
        [Parameter(Mandatory = $true)][string]$Current,
        [Parameter(Mandatory = $true)][string]$Baseline,
        [string]$AllowListPath,
        [string]$EnvironmentAllowList
    )
    $arguments = @(
        '-NoLogo', '-NoProfile', '-File', $gate,
        '-PackagePath', $Current,
        '-BaselinePackage', $Baseline
    )
    if ($AllowListPath) { $arguments += @('-AllowListPath', $AllowListPath) }
    $previous = $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST
    try {
        # Keep the tests hermetic: never let them read the repository allow list implicitly.
        $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST = if ($EnvironmentAllowList) { $EnvironmentAllowList } else { $script:emptyAllowList }
        $output = & pwsh @arguments 2>&1 | Out-String
    }
    finally { $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST = $previous }
    return [PSCustomObject]@{ ExitCode = $LASTEXITCODE; Output = $output }
}

function Assert-Equal {
    param([Parameter(Mandatory = $true)]$Actual, [Parameter(Mandatory = $true)]$Expected, [Parameter(Mandatory = $true)][string]$Because)
    if ($Actual -ne $Expected) { $script:failures += "$Because (expected '$Expected', got '$Actual')" }
    else { $script:passed++ }
}

function Assert-Match {
    param([Parameter(Mandatory = $true)][string]$Text, [Parameter(Mandatory = $true)][string]$Pattern, [Parameter(Mandatory = $true)][string]$Because)
    if ($Text -notmatch $Pattern) { $script:failures += "$Because (output did not match /$Pattern/)" }
    else { $script:passed++ }
}

if (-not (Test-Path -LiteralPath $gate)) {
    Write-Error "verify-package-layout.ps1 not found at $gate"
}

New-Item -ItemType Directory -Path $root -Force | Out-Null

$script:emptyAllowList = Join-Path $root 'empty-allowlist.json'
'{}' | Set-Content -LiteralPath $script:emptyAllowList -Encoding utf8

try {
    $layout = @(
        'lib/net8.0/AtomUI.Generator.dll',
        'lib/net10.0/AtomUI.Generator.dll',
        'tools/netstandard2.0/AtomUI.Build.Tasks.dll',
        'buildTransitive/AtomUI.Generator.targets'
    )

    # Scenario 1: identical layouts must pass.
    $s = New-Scenario -Name 'identical' -BaselineEntries $layout -CurrentEntries $layout
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline
    Assert-Equal -Actual $result.ExitCode -Expected 0 -Because 'identical layouts must pass'

    # Scenario 2: the 6.1.9 regression - a tools assembly moves to a new target framework.
    $movedEntries = @(
        'lib/net8.0/AtomUI.Generator.dll',
        'lib/net10.0/AtomUI.Generator.dll',
        'tools/net10.0/AtomUI.Build.Tasks.dll',
        'buildTransitive/AtomUI.Generator.targets'
    )
    $s = New-Scenario -Name 'tools-moved' -BaselineEntries $layout -CurrentEntries $movedEntries
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'a tools target-framework move must fail the gate'
    Assert-Match -Text $result.Output -Pattern 'tools/net10\.0/AtomUI\.Build\.Tasks\.dll' -Because 'the added tools path must be reported'
    Assert-Match -Text $result.Output -Pattern 'tools/netstandard2\.0/AtomUI\.Build\.Tasks\.dll' -Because 'the removed tools path must be reported'

    # Scenario 3: an added buildTransitive asset must fail.
    $s = New-Scenario -Name 'asset-added' -BaselineEntries $layout -CurrentEntries ($layout + 'buildTransitive/AtomUI.Build.Tasks.Process.cs')
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'an added buildTransitive asset must fail the gate'
    Assert-Match -Text $result.Output -Pattern 'buildTransitive/AtomUI\.Build\.Tasks\.Process\.cs' -Because 'the added asset must be reported'

    # Scenario 4: dropping a target framework from lib must fail.
    $s = New-Scenario -Name 'tfm-dropped' -BaselineEntries $layout -CurrentEntries @(
        'lib/net10.0/AtomUI.Generator.dll',
        'tools/netstandard2.0/AtomUI.Generator.dll',
        'buildTransitive/AtomUI.Generator.targets'
    )
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'removing a lib target framework must fail the gate'
    Assert-Match -Text $result.Output -Pattern 'lib/net8\.0/' -Because 'the removed target framework must be reported'

    # Scenario 5: an acknowledged change passes when the allow list covers it.
    $s = New-Scenario -Name 'acknowledged' -BaselineEntries $layout -CurrentEntries $movedEntries
    $allow = Join-Path $root 'allow.json'
    @'
{
  "6.1.8": {
    "Scenarios": {
      "allowed": [
        "tools/netstandard2.0/AtomUI.Build.Tasks.dll",
        "tools/net10.0/AtomUI.Build.Tasks.dll"
      ],
      "reason": "test acknowledgement"
    }
  }
}
'@ | Set-Content -LiteralPath $allow -Encoding utf8
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline -AllowListPath $allow
    Assert-Equal -Actual $result.ExitCode -Expected 0 -Because 'an acknowledged layout change must pass'

    # Scenario 6: an unused allow list entry must fail, keeping the allow list honest.
    $stale = Join-Path $root 'stale.json'
    @'
{
  "6.1.8": {
    "Scenarios": {
      "allowed": [
        "tools/netstandard2.0/AtomUI.Build.Tasks.dll",
        "tools/net10.0/AtomUI.Build.Tasks.dll",
        "tools/never/seen.dll"
      ],
      "reason": "stale entry"
    }
  }
}
'@ | Set-Content -LiteralPath $stale -Encoding utf8
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline -AllowListPath $stale
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'an unused allow list entry must fail'

    # Scenario 7: an acknowledgement scoped to a different baseline must not apply.
    $otherVersion = Join-Path $root 'other-version.json'
    @'
{
  "6.1.7": {
    "Scenarios": {
      "allowed": [
        "tools/netstandard2.0/AtomUI.Build.Tasks.dll",
        "tools/net10.0/AtomUI.Build.Tasks.dll"
      ],
      "reason": "scoped to an older baseline"
    }
  }
}
'@ | Set-Content -LiteralPath $otherVersion -Encoding utf8
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline -AllowListPath $otherVersion
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'an acknowledgement for another baseline version must not apply'

    # Scenario 8: a missing baseline is a usage error, not a silent pass.
    $result = Invoke-Gate -Current $s.Current -Baseline (Join-Path $root 'missing.nupkg')
    Assert-Equal -Actual $result.ExitCode -Expected 2 -Because 'a missing baseline must be reported as a usage error'

    # Scenario 9: an empty package directory must not pass silently.
    $empty = Join-Path $root 'empty'
    New-Item -ItemType Directory -Path $empty -Force | Out-Null
    $previousEnv = $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST
    try {
        $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST = $script:emptyAllowList
        $output = & pwsh -NoLogo -NoProfile -File $gate -PackageDirectory $empty -BaselineVersion '6.1.8' 2>&1 | Out-String
    }
    finally { $env:ATOMUI_PACKAGE_LAYOUT_ALLOWLIST = $previousEnv }
    Assert-Equal -Actual $LASTEXITCODE -Expected 2 -Because 'an empty package directory must be a usage error'

    # Scenario 10: the environment variable supplies the allow list when no path is passed.
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline -EnvironmentAllowList $allow
    Assert-Equal -Actual $result.ExitCode -Expected 0 -Because 'the environment allow list must be honoured'

    # Scenario 11: an empty allow list file must fail with a clear error, not a property error.
    $emptyFile = Join-Path $root 'empty.json'
    Set-Content -LiteralPath $emptyFile -Value '' -Encoding utf8
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline -AllowListPath $emptyFile
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'an empty allow list file must fail'
    Assert-Match -Text $result.Output -Pattern 'is empty' -Because 'an empty allow list must report a clear reason'

    # Scenario 12: an empty allow list object leaves real changes unacknowledged.
    $noAcks = Join-Path $root 'no-acks.json'
    '{}' | Set-Content -LiteralPath $noAcks -Encoding utf8
    $result = Invoke-Gate -Current $s.Current -Baseline $s.Baseline -AllowListPath $noAcks
    Assert-Equal -Actual $result.ExitCode -Expected 1 -Because 'changes without acknowledgements must fail'
}
finally {
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
}

if ($script:failures.Count -gt 0) {
    Write-Host "FAILED ($($script:failures.Count) assertion(s), $script:passed passed)"
    foreach ($failure in $script:failures) { Write-Host "  - $failure" }
    exit 1
}

Write-Host "PASSED ($script:passed assertions)"
exit 0
