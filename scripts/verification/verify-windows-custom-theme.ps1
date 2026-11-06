#requires -Version 7.0
param(
    [Parameter(Mandatory)]
    [ValidateSet('platform-interaction', 'native-aot', 'package-layout', 'conditional-registration-engines')]
    [string]$Check
)
$ErrorActionPreference = 'Stop'
$python = $env:AtomUIRegistrationPython
if (-not $python) {
    $python = (Get-Command python -ErrorAction Stop).Source
}
& $python -B (Join-Path $PSScriptRoot 'verify-windows-custom-theme.py') --check $Check
if ($LASTEXITCODE -ne 0) { throw "Windows $Check verification failed ($LASTEXITCODE)" }
