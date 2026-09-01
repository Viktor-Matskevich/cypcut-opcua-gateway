#requires -Version 5.1
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'Common.ps1')
Assert-GatewayFiles

$env:DOTNET_ROOT = $script:RuntimeDirectory
$env:CYPCUT_GATEWAY_ROOT = $script:GatewayRoot
$env:DOTNET_EnableDiagnostics = '0'

Write-Host 'Starting standalone CypCut -> OPC UA gateway.' -ForegroundColor Cyan
Write-Host 'Press Ctrl+C to stop.'
& $script:DotnetExe $script:GatewayDll
exit $LASTEXITCODE
