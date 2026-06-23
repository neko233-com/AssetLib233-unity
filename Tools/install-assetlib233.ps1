param(
    [string]$ProjectRoot = (Get-Location).Path,
    [string]$Ref = "main"
)

$ErrorActionPreference = "Stop"
$target = Join-Path $ProjectRoot "Assets/neko233/AssetLib233"
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("AssetLib233Install_" + [System.Guid]::NewGuid().ToString("N"))
$zipPath = Join-Path $tempRoot "AssetLib233-unity.zip"
$extractRoot = Join-Path $tempRoot "extract"
$url = "https://github.com/neko233-com/AssetLib233-unity/archive/refs/heads/$Ref.zip"

New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
New-Item -ItemType Directory -Force -Path $extractRoot | Out-Null
Invoke-WebRequest -Uri $url -OutFile $zipPath
Expand-Archive -LiteralPath $zipPath -DestinationPath $extractRoot -Force

$source = Get-ChildItem -LiteralPath $extractRoot -Directory | Select-Object -First 1
if ($null -eq $source) {
    throw "AssetLib233 zip 解压失败"
}

New-Item -ItemType Directory -Force -Path (Split-Path -Parent $target) | Out-Null
if (Test-Path -LiteralPath $target) {
    $backup = $target + ".backup_" + (Get-Date -Format "yyyyMMdd_HHmmss")
    Move-Item -LiteralPath $target -Destination $backup
}

Copy-Item -LiteralPath $source.FullName -Destination $target -Recurse
Remove-Item -LiteralPath $tempRoot -Recurse -Force
Write-Host "AssetLib233 installed to $target"
