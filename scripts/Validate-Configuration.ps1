#requires -Version 5.1
[CmdletBinding()]
param()

. (Join-Path $PSScriptRoot 'Common.ps1')
Assert-GatewayFiles

$env:DOTNET_ROOT = $script:RuntimeDirectory
$env:CYPCUT_GATEWAY_ROOT = $script:GatewayRoot

& $script:DotnetExe $script:GatewayDll --self-test
if ($LASTEXITCODE -ne 0) { throw "Gateway self-test failed: $LASTEXITCODE" }
& $script:DotnetExe $script:GatewayDll --validate-config
if ($LASTEXITCODE -ne 0) { throw "Configuration validation failed: $LASTEXITCODE" }

$gateway = Get-GatewayConfig
$localAddresses = @(Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty IPAddress)
if ($gateway.publishedIp -notin $localAddresses -and $gateway.publishedIp -ne '127.0.0.1') {
    Write-Host "Warning: publishedIp $($gateway.publishedIp) is not assigned to this server." -ForegroundColor Yellow
    Write-Host "Local IPv4: $($localAddresses -join ', ')"
}

Write-Host 'Source connection test:' -ForegroundColor Cyan
$results = foreach ($machine in Get-EnabledMachines) {
    $result = Test-NetConnection $machine.CypCutIp -Port ([int]$machine.CypCutPort) -WarningAction SilentlyContinue
    [pscustomobject]@{
        Id = $machine.Id
        Source = "$($machine.CypCutIp):$($machine.CypCutPort)"
        Reachable = $result.TcpTestSucceeded
        OpcUa = "$($gateway.publishedIp):$($machine.OpcUaPort)"
    }
}
$results | Format-Table -AutoSize
