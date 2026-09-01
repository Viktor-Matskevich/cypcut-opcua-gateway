#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$script:GatewayRoot = Split-Path -Parent $PSScriptRoot
$script:RuntimeDirectory = Join-Path $script:GatewayRoot 'runtime'
$script:BinDirectory = Join-Path $script:GatewayRoot 'bin'
$script:ConfigDirectory = Join-Path $script:GatewayRoot 'config'
$script:DotnetExe = Join-Path $script:RuntimeDirectory 'dotnet.exe'
$script:GatewayDll = Join-Path $script:BinDirectory 'CypCutOpcUaGateway.dll'
$script:ServiceName = 'CypCutStandaloneOpcUaGateway'

function Assert-GatewayFiles {
    foreach ($path in @(
        $script:DotnetExe,
        $script:GatewayDll,
        (Join-Path $script:ConfigDirectory 'gateway.json'),
        (Join-Path $script:ConfigDirectory 'machines.csv')
    )) {
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required file is missing: $path" }
    }
}

function Assert-Administrator {
    $identity = [Security.Principal.WindowsIdentity]::GetCurrent()
    $principal = [Security.Principal.WindowsPrincipal]::new($identity)
    if (-not $principal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Run PowerShell as Administrator.'
    }
}

function Get-GatewayConfig {
    return Get-Content -LiteralPath (Join-Path $script:ConfigDirectory 'gateway.json') -Raw | ConvertFrom-Json
}

function Get-EnabledMachines {
    return @(Import-Csv -LiteralPath (Join-Path $script:ConfigDirectory 'machines.csv') |
        Where-Object { $_.Enabled -match '^(?i:true|1|yes)$' })
}
