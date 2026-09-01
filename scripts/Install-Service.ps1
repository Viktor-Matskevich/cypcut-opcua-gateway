#requires -Version 5.1
[CmdletBinding()]
param(
    [switch]$StartService
)

. (Join-Path $PSScriptRoot 'Common.ps1')
Assert-Administrator
Assert-GatewayFiles

if (Get-Service -Name $script:ServiceName -ErrorAction SilentlyContinue) {
    throw "Service $script:ServiceName already exists. Uninstall it first."
}

& (Join-Path $PSScriptRoot 'Validate-Configuration.ps1')

$binaryPath = '"{0}" "{1}"' -f $script:DotnetExe, $script:GatewayDll
New-Service -Name $script:ServiceName -BinaryPathName $binaryPath `
    -DisplayName 'CypCut Standalone OPC UA Gateway' `
    -Description 'Independent CypCut HTTP to OPC UA gateway.' `
    -StartupType Automatic

$registryPath = "HKLM:\SYSTEM\CurrentControlSet\Services\$script:ServiceName"
New-ItemProperty -Path $registryPath -Name Environment -PropertyType MultiString -Force -Value @(
    "DOTNET_ROOT=$script:RuntimeDirectory",
    "CYPCUT_GATEWAY_ROOT=$script:GatewayRoot",
    'DOTNET_EnableDiagnostics=0'
) | Out-Null

$gateway = Get-GatewayConfig
foreach ($machine in Get-EnabledMachines) {
    $ruleName = "CypCut OPC UA - $($machine.Id)"
    if (Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue) {
        Remove-NetFirewallRule -DisplayName $ruleName
    }
    New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Action Allow -Protocol TCP `
        -LocalAddress $gateway.publishedIp -LocalPort ([int]$machine.OpcUaPort) | Out-Null
}

& sc.exe failure $script:ServiceName reset= 86400 actions= restart/5000/restart/15000/none/0 | Out-Null
Write-Host "Service installed: $script:ServiceName" -ForegroundColor Green
if ($StartService) {
    Start-Service $script:ServiceName
    Get-Service $script:ServiceName | Format-Table Status, Name, DisplayName
}
