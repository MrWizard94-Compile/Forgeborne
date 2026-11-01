# Forgeborne — build the Unity core DLLs (Integration Step 4, 0.111.0).
#
# Windows counterpart of tools/build-unity-core.sh: same target, same 9 DLLs.
# Run from anywhere; the script finds the repo root from its own location.
# It publishes src/Forgeborne.Core for netstandard2.1 (Release) to a
# temporary staging folder, then copies ONLY the 9 shipped DLLs into
# ForgeborneUnity/Assets/Plugins/. It never copies .cs files, never
# copies the publish deps.json or pdb, and never deletes the Plugins
# README. Publish (not plain build) is used on purpose: a plain build
# leaves System.Text.Json and its transitive DLLs in the NuGet cache,
# while publish collects exactly the 9 DLLs Unity needs in one folder.
#
# Usage (from PowerShell, any directory):
#   .\tools\build-unity-core.ps1
# After a core change, run this script, then upload the changed DLLs to
# GitHub (the snapshot push is UTF-8 text only and cannot carry DLLs).
$ErrorActionPreference = 'Stop'
$Root = Split-Path $PSScriptRoot -Parent
Set-Location $Root

if (Get-Command dotnet -ErrorAction SilentlyContinue) {
    $Dotnet = 'dotnet'
} elseif (Test-Path "$env:USERPROFILE\.dotnet\dotnet.exe") {
    $Dotnet = "$env:USERPROFILE\.dotnet\dotnet.exe"
} else {
    throw 'dotnet not found on PATH or at $env:USERPROFILE\.dotnet\dotnet.exe'
}

$Dlls = @(
    'Forgeborne.Core.dll',
    'Microsoft.Bcl.AsyncInterfaces.dll',
    'System.Buffers.dll',
    'System.Memory.dll',
    'System.Numerics.Vectors.dll',
    'System.Runtime.CompilerServices.Unsafe.dll',
    'System.Text.Encodings.Web.dll',
    'System.Text.Json.dll',
    'System.Threading.Tasks.Extensions.dll'
)

$Stage = Join-Path ([System.IO.Path]::GetTempPath()) ('forgeborne-publish-' + [Guid]::NewGuid().ToString('N'))
try {
    & $Dotnet publish src/Forgeborne.Core/Forgeborne.Core.csproj -f netstandard2.1 -c Release -o $Stage --nologo
    foreach ($Dll in $Dlls) {
        $Src = Join-Path $Stage $Dll
        if (-not (Test-Path $Src)) {
            throw "publish output is missing $Dll; Plugins left unchanged"
        }
        Copy-Item $Src "ForgeborneUnity/Assets/Plugins/$Dll" -Force
    }
    Write-Host "Copied $($Dlls.Count) DLLs to ForgeborneUnity/Assets/Plugins/"
    Get-ChildItem "ForgeborneUnity/Assets/Plugins/*.dll" | ForEach-Object { Write-Host $_.Name }
    Write-Host 'Note: DLLs are build outputs, not source. The no-drift guard in'
    Write-Host 'tests/Forgeborne.Core.Tests/Program.cs fails if a core .cs file'
    Write-Host 'appears under ForgeborneUnity/Assets/.'
} finally {
    if (Test-Path $Stage) {
        Remove-Item $Stage -Recurse -Force
    }
}
