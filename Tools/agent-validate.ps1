param(
    [string]$UnityPath = "",
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$LocalConfig = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    $UnityPath = $env:UNITY_EXE
}

if ([string]::IsNullOrWhiteSpace($UnityPath)) {
    throw "请通过 -UnityPath 或 UNITY_EXE 指定 Unity/Tuanjie 编辑器路径"
}

if (![string]::IsNullOrWhiteSpace($LocalConfig)) {
    $env:ASSETLIB233_LOCAL_CONFIG = $LocalConfig
}

& $UnityPath `
  -batchmode `
  -quit `
  -projectPath $ProjectRoot `
  -executeMethod AssetLib233.Editor.AssetLib233EditorAgentValidationPipeline.RunAgentFirstValidationBatchMode

if ($LASTEXITCODE -ne 0) {
    throw "AssetLib233 agent validation failed. exitCode=$LASTEXITCODE"
}

Write-Host "AssetLib233 agent validation done"
