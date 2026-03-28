[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [string]$InstallerUrl,

    [Parameter(Mandatory = $true)]
    [string]$ZipPath,

    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\artifacts\winget")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($Version -notmatch '^[0-9]+\.[0-9]+\.[0-9]+([-.][0-9A-Za-z.-]+)?$') {
    throw "Version '$Version' is not a valid SemVer-style release version."
}

$repoRoot = Split-Path $PSScriptRoot -Parent
$templateDirectory = Join-Path $repoRoot "packaging\winget\templates"
$resolvedZipPath = (Resolve-Path $ZipPath).Path
$manifestDirectory = Join-Path $OutputDirectory "JekyllNet.JekyllNet\$Version"

if (-not (Test-Path $templateDirectory)) {
    throw "Winget template directory not found: $templateDirectory"
}

New-Item -ItemType Directory -Path $manifestDirectory -Force | Out-Null

$zipSha256 = (Get-FileHash -Path $resolvedZipPath -Algorithm SHA256).Hash.ToUpperInvariant()
$replacements = @{
    "__VERSION__"                 = $Version
    "__WINDOWS_X64_ZIP_URL__"     = $InstallerUrl
    "__WINDOWS_X64_ZIP_SHA256__"  = $zipSha256
}

foreach ($template in Get-ChildItem $templateDirectory -Filter "*.yaml") {
    $content = Get-Content $template.FullName -Raw -Encoding UTF8

    foreach ($key in $replacements.Keys) {
        $content = $content.Replace($key, $replacements[$key])
    }

    $targetPath = Join-Path $manifestDirectory $template.Name
    Set-Content -Path $targetPath -Value $content -Encoding utf8
}

$resolvedManifestDirectory = (Resolve-Path $manifestDirectory).Path
Write-Host "Generated manifests: $resolvedManifestDirectory"
Write-Host "Installer URL: $InstallerUrl"
Write-Host "Installer SHA256: $zipSha256"
