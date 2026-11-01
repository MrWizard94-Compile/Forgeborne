param([string]$VcVars)
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
if (-not $VcVars) {
    $VcVars = @(
        'C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvars64.bat',
        'C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat'
    ) | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if (-not $VcVars -or -not (Test-Path -LiteralPath $VcVars)) { throw 'MSVC x64 build tools required. Supply -VcVars.' }
$build = Join-Path $repo 'Builds\WindowsBridge'
$destination = Join-Path $repo 'ForgeborneUnity\Assets\Plugins\x86_64'
New-Item -ItemType Directory -Path $build,$destination -Force | Out-Null
$source = Join-Path $repo 'native\Forgeborne.Windows\SaveDialog.cpp'
$commandFile = Join-Path $build 'compile.cmd'
# Paths originate in this checkout/toolchain, never in imported save content.
@"
@echo off
call "$VcVars" >nul
if errorlevel 1 exit /b %errorlevel%
cl.exe /nologo /LD /MT /O2 /EHsc /W4 /WX /DUNICODE /D_UNICODE "$source" /link /OUT:"$build\Forgeborne.Windows.dll" ole32.lib shell32.lib user32.lib
exit /b %errorlevel%
"@ | Set-Content -LiteralPath $commandFile -Encoding ascii
Push-Location $build
try {
    & $env:ComSpec /d /c $commandFile
    if ($LASTEXITCODE -ne 0) { throw "Native bridge build failed ($LASTEXITCODE)." }
    Copy-Item -LiteralPath (Join-Path $build 'Forgeborne.Windows.dll') -Destination $destination -Force
} finally { Pop-Location }
Write-Output 'Built and installed original x64 Windows dialog bridge (static CRT).'
