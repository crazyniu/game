param([string]$Command = 'play', [int]$Level = 0)
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot
$creator = $env:COCOS_CREATOR
if (-not $creator) { $creator = 'C:\ProgramData\Cocos\editors\Creator\3.8.8\CocosCreator.exe' }
if ($Command -eq 'editor') { & $creator --project "$PSScriptRoot\PerchSort"; exit }
if ($Command -eq 'test') { npm --prefix PerchSort test; exit $LASTEXITCODE }
if ($Command -ne 'serve') {
  if (-not (Test-Path PerchSort/node_modules)) { npm --prefix PerchSort install --no-audit --no-fund }
  python tools/cocos-meta.py
  & $creator --project "$PSScriptRoot\PerchSort" --build "configPath=$PSScriptRoot\PerchSort\build-config\web-mobile.json"
  if ($LASTEXITCODE -ne 36 -and $LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
if ($Command -eq 'build') { exit }
$port = if ($env:WEB_PORT) { $env:WEB_PORT } else { '8765' }
Start-Process python -ArgumentList @('scripts/serve.py','--port',$port) -WorkingDirectory $PSScriptRoot
$url = "http://127.0.0.1:$port/"
if ($Level -gt 0) { $url += "?level=$Level" }
Start-Process $url
