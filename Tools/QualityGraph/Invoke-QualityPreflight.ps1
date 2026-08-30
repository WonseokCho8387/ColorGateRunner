[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateSet('inspect', 'source-edit', 'batch-validate', 'visual-qc', 'finalize')]
    [string] $Mode,

    [string[]] $Path = @(),

    [ValidateSet('text', 'json')]
    [string] $Format = 'text',

    [ValidateSet('not-applicable', 'headless', 'graphics-editor', 'player-artifact')]
    [string] $Backend,

    [string] $EvidencePath,

    [string] $GraphicsApi,

    [string] $Resolution,

    [switch] $WriteManifest,

    [switch] $SelfTest
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$qualityGraphRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $qualityGraphRoot '..')).Path
$preflightScript = Join-Path `
    $repositoryRoot `
    'Tools\AgentSkills\quality-graph\scripts\quality_preflight.py'
$configPath = Join-Path $PSScriptRoot 'quality-graph.json'

function Resolve-QualityGraphPython {
    $candidates = [System.Collections.Generic.List[string]]::new()

    if (-not [string]::IsNullOrWhiteSpace($env:QUALITY_GRAPH_PYTHON)) {
        $candidates.Add($env:QUALITY_GRAPH_PYTHON)
    }

    $codexPython = Join-Path `
        $env:USERPROFILE `
        '.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
    $candidates.Add($codexPython)

    $pythonCommand = Get-Command python -ErrorAction SilentlyContinue
    if ($null -ne $pythonCommand) {
        $candidates.Add($pythonCommand.Source)
    }

    foreach ($candidate in $candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate) -or
            -not (Test-Path -LiteralPath $candidate -PathType Leaf)) {
            continue
        }

        $version = & $candidate --version 2>&1
        if ($LASTEXITCODE -eq 0 -and "$version" -match '^Python 3\.') {
            return (Resolve-Path -LiteralPath $candidate).Path
        }
    }

    throw 'Python 3 was not found. Set QUALITY_GRAPH_PYTHON to a Python 3 executable.'
}

if (-not (Test-Path -LiteralPath $preflightScript -PathType Leaf)) {
    throw "Quality Graph preflight script was not found at '$preflightScript'."
}
if (-not (Test-Path -LiteralPath $configPath -PathType Leaf)) {
    throw "Quality Graph configuration was not found at '$configPath'."
}

$pythonPath = Resolve-QualityGraphPython
$arguments = [System.Collections.Generic.List[string]]::new()
$arguments.Add($preflightScript)

if ($SelfTest) {
    $arguments.Add('--self-test')
} else {
    $arguments.Add('--config')
    $arguments.Add($configPath)
    $arguments.Add('--mode')
    $arguments.Add($Mode)
    $arguments.Add('--format')
    $arguments.Add($Format)

    foreach ($requestedPath in $Path) {
        $arguments.Add('--path')
        $arguments.Add($requestedPath)
    }

    if (-not [string]::IsNullOrWhiteSpace($Backend)) {
        $arguments.Add('--backend')
        $arguments.Add($Backend)
    }

    if (-not [string]::IsNullOrWhiteSpace($EvidencePath)) {
        $arguments.Add('--evidence-path')
        $arguments.Add($EvidencePath)
    }

    if (-not [string]::IsNullOrWhiteSpace($GraphicsApi)) {
        $arguments.Add('--graphics-api')
        $arguments.Add($GraphicsApi)
    }

    if (-not [string]::IsNullOrWhiteSpace($Resolution)) {
        $arguments.Add('--resolution')
        $arguments.Add($Resolution)
    }

    if ($WriteManifest) {
        $arguments.Add('--write-manifest')
    }
}

& $pythonPath $arguments
if ($LASTEXITCODE -ne 0) {
    throw "Quality Graph preflight rejected mode '$Mode'."
}
