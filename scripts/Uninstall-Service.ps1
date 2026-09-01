#requires -Version 5.1
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'Common.ps1')
Assert-Administrator

$service = Get-Service -Name $script:ServiceName -ErrorAction SilentlyContinue
if ($service) {
    if ($service.Status -ne 'Stopped') { Stop-Service $script:ServiceName -Force }
    & sc.exe delete $script:ServiceName | Out-Null
    Write-Host "Service removed: $script:ServiceName" -ForegroundColor Green
}
else {
    Write-Host 'Service is not installed.' -ForegroundColor Yellow
}

Get-NetFirewallRule -DisplayName 'CypCut OPC UA - *' -ErrorAction SilentlyContinue | Remove-NetFirewallRule
