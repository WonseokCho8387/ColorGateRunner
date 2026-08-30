Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$validationRepoPath = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$qualityPreflightPath = Join-Path `
    $validationRepoPath `
    'Tools\QualityGraph\Invoke-QualityPreflight.ps1'

if (-not (Test-Path -LiteralPath $qualityPreflightPath -PathType Leaf)) {
    throw "Quality Graph preflight was not found at '$qualityPreflightPath'."
}

& $qualityPreflightPath -Mode 'batch-validate' -WriteManifest

$validationProjectPath = (Resolve-Path -LiteralPath (Join-Path $validationRepoPath 'ColorGateRunner')).Path
$validationAssetsPath = Join-Path $validationProjectPath 'Assets'
$validationArtifactsPath = Join-Path $validationRepoPath 'Artifacts\Validation'

if (-not (Test-Path -LiteralPath (Join-Path $validationProjectPath 'ProjectSettings\ProjectVersion.txt'))) {
    throw "Unity project was not found at '$validationProjectPath'."
}

$validationUnityPath = $env:UNITY_EDITOR_PATH
if ([string]::IsNullOrWhiteSpace($validationUnityPath)) {
    $validationUnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.1f1\Editor\Unity.exe'
}

if (-not (Test-Path -LiteralPath $validationUnityPath -PathType Leaf)) {
    throw "Unity Editor was not found at '$validationUnityPath'. Set UNITY_EDITOR_PATH to the Unity 6000.5.1f1 executable."
}

New-Item -ItemType Directory -Force -Path $validationArtifactsPath | Out-Null

function Test-AssemblyDefinitionAvailable {
    param(
        [Parameter(Mandatory)]
        [string] $AssemblyName
    )

    $assemblyDefinitionFiles = Get-ChildItem -LiteralPath $validationAssetsPath -Recurse -File -Filter '*.asmdef'
    foreach ($assemblyDefinitionFile in $assemblyDefinitionFiles) {
        $assemblyDefinition = Get-Content -Raw -LiteralPath $assemblyDefinitionFile.FullName | ConvertFrom-Json
        if ($assemblyDefinition.name -eq $AssemblyName) {
            return $true
        }
    }

    return $false
}

function Invoke-UnityTestAssembly {
    param(
        [Parameter(Mandatory)]
        [ValidateSet('EditMode', 'PlayMode')]
        [string] $Mode,

        [Parameter(Mandatory)]
        [string] $AssemblyName
    )

    if (-not (Test-AssemblyDefinitionAvailable -AssemblyName $AssemblyName)) {
        Write-Host "[$Mode] $AssemblyName is not yet available."
        return $false
    }

    $resultFile = Join-Path $validationArtifactsPath "$Mode-results.xml"
    $logFile = Join-Path $validationArtifactsPath "$Mode.log"

    if (Test-Path -LiteralPath $resultFile) {
        Remove-Item -LiteralPath $resultFile -Force
    }

    # -nographics keeps automated runs headless and reproducible.
    # Remove it later if a PlayMode test must validate actual rendered pixels,
    # GPU behavior, or other visual output that is unavailable in headless mode.
    $unityArguments = @(
        '-batchmode',
        '-nographics',
        '-projectPath', "`"$validationProjectPath`"",
        '-runTests',
        '-testPlatform', $Mode,
        '-assemblyNames', $AssemblyName,
        '-randomOrderSeed', '12345',
        '-testResults', "`"$resultFile`"",
        '-logFile', "`"$logFile`""
    )

    $unityProcess = Start-Process `
        -FilePath $validationUnityPath `
        -ArgumentList $unityArguments `
        -WindowStyle Hidden `
        -Wait `
        -PassThru

    $unityExitCode = $unityProcess.ExitCode
    if ($unityExitCode -ne 0) {
        throw "$Mode tests failed with exit code $unityExitCode. See '$logFile'."
    }

    if (-not (Test-Path -LiteralPath $resultFile -PathType Leaf)) {
        throw "$Mode did not produce a test-results file. See '$logFile'."
    }

    [xml] $testResults = Get-Content -Raw -LiteralPath $resultFile
    $testRun = $testResults.'test-run'
    if ($null -eq $testRun) {
        throw "$Mode produced an invalid test-results file: '$resultFile'."
    }

    $executedTestCount = [int] $testRun.total
    if ($executedTestCount -lt 1) {
        throw "$Mode completed without executing any tests."
    }

    $failedTestCount = [int] $testRun.failed
    if ($failedTestCount -gt 0) {
        throw "$Mode reported $failedTestCount failed tests. See '$resultFile'."
    }

    Write-Host "[$Mode] $executedTestCount tests executed; all available tests passed."
    return $true
}

$availableTestRunCount = 0

if (Invoke-UnityTestAssembly -Mode 'EditMode' -AssemblyName 'ColorGateRunner.EditModeTests') {
    $availableTestRunCount++
}

if (Invoke-UnityTestAssembly -Mode 'PlayMode' -AssemblyName 'ColorGateRunner.PlayModeTests') {
    $availableTestRunCount++
}

if ($availableTestRunCount -eq 0) {
    Write-Host 'Baseline validation complete. Test assemblies are not yet available; no tests were executed.'
} else {
    Write-Host "$availableTestRunCount available test suite(s) executed successfully."
}
