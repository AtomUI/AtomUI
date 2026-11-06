#requires -Version 7.0
param([Parameter(Mandatory)][string]$PublishRoot, [Parameter(Mandatory)][string]$Receipt)
$ErrorActionPreference = 'Stop'
$candidates = @(Get-ChildItem -LiteralPath $PublishRoot -Recurse -File -Filter AtomUIGallery.Desktop.exe)
if ($candidates.Count -ne 1) { throw 'Expected exactly one official Gallery executable' }
$exe = [IO.Path]::GetFullPath($candidates[0].FullName)
$process = Start-Process -FilePath $exe -PassThru -WindowStyle Hidden
try {
    $ready = $false
    for ($attempt = 0; $attempt -lt 150; $attempt++) {
        Start-Sleep -Milliseconds 100
        $process.Refresh()
        if ($process.HasExited) { throw "Gallery exited before startup ($($process.ExitCode))" }
        if ($process.MainWindowHandle -ne 0) { $ready = $true; break }
    }
    if (-not $ready) { throw 'Gallery did not create a native HWND' }
    if ([IO.Path]::GetFullPath($process.Path) -ne $exe) { throw 'Launched process path mismatch' }
    $modules = @($process.Modules | ForEach-Object ModuleName)
    if ($modules -contains 'coreclr.dll' -or $modules -contains 'clr.dll') { throw 'Gallery loaded a CLR' }
    @{
        executable = $exe
        sha256 = (Get-FileHash -LiteralPath $exe -Algorithm SHA256).Hash
        processId = $process.Id
        hwnd = $process.MainWindowHandle.ToInt64()
        coreClr = $false
    } | ConvertTo-Json | Set-Content -LiteralPath $Receipt -Encoding utf8
    Write-Output 'OFFICIAL_GALLERY_STARTUP_PASS'
}
finally {
    if (-not $process.HasExited) {
        $null = $process.CloseMainWindow()
        if (-not $process.WaitForExit(10000)) {
            # Only the exact process created and verified above belongs to this run.
            if ([IO.Path]::GetFullPath($process.Path) -ne $exe) { throw 'Owned process identity changed' }
            $process.Kill()
            $process.WaitForExit()
        }
    }
    $process.Dispose()
}
