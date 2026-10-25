# First publication has no historical API or package-layout baseline. An explicit manifest opt-in
# and a NuGet package-index 404 are both required; a missing version of an existing package still fails.
function Get-AtomUIPackageValidationBaselineVersion {
    param(
        [Parameter(Mandatory = $true)]$Package,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$BaselineVersion
    )

    if ([string]::IsNullOrWhiteSpace($BaselineVersion)) {
        return ""
    }
    $allowUnpublishedBaseline = $Package.PSObject.Properties["AllowUnpublishedBaseline"]
    if ($null -eq $allowUnpublishedBaseline -or $allowUnpublishedBaseline.Value -ne $true) {
        return $BaselineVersion
    }

    $indexUrl = "https://api.nuget.org/v3-flatcontainer/$($Package.PackageId.ToLowerInvariant())/index.json"
    try {
        $null = Invoke-WebRequest -Uri $indexUrl -Method Head -TimeoutSec 30 -ErrorAction Stop
    }
    catch {
        if ($_.Exception -is [Microsoft.PowerShell.Commands.HttpResponseException] -and
            [int]$_.Exception.Response.StatusCode -eq 404) {
            Write-Host "$($Package.PackageId): no published versions; historical baseline comparison is not applicable."
            return ""
        }
        throw
    }

    return $BaselineVersion
}
