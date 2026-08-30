[CmdletBinding()]
param(
    [string] $DestinationRoot = (Join-Path $env:USERPROFILE '.codex\skills'),
    [switch] $Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..\..')).Path
$source = (Resolve-Path -LiteralPath (
    Join-Path $repositoryRoot 'Tools\AgentSkills\quality-graph')).Path
$destinationRootFull = [System.IO.Path]::GetFullPath($DestinationRoot)
$destination = [System.IO.Path]::GetFullPath(
    (Join-Path $destinationRootFull 'quality-graph'))

if (-not $destination.StartsWith(
    $destinationRootFull + [System.IO.Path]::DirectorySeparatorChar,
    [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Destination '$destination' is outside '$destinationRootFull'."
}

if (Test-Path -LiteralPath $destination) {
    if (-not $Force) {
        throw "Skill already exists at '$destination'. Use -Force to update it."
    }

    $sourceRelative = Get-ChildItem -LiteralPath $source -Recurse -File |
        ForEach-Object { $_.FullName.Substring($source.Length + 1) }
    $destinationRelative = Get-ChildItem -LiteralPath $destination -Recurse -File |
        ForEach-Object { $_.FullName.Substring($destination.Length + 1) }
    $extraFiles = $destinationRelative | Where-Object { $_ -notin $sourceRelative }
    if ($extraFiles) {
        throw "Installed Skill contains files absent from the source: $($extraFiles -join ', ')."
    }
} else {
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
}

Copy-Item -Path (Join-Path $source '*') -Destination $destination -Recurse -Force
Write-Host "Installed Quality Graph Skill at '$destination'."
