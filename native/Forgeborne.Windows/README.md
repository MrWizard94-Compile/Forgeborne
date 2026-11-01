# Original Windows platform bridge

`SaveDialog.cpp` wraps Windows' Common Item Dialog for the Windows x64 player
and Windows Editor. It is platform integration only; gameplay and persistence
remain in C# and the existing Core DLL. It returns an owned-buffer Unicode file
path, cancellation (`S_FALSE`) or a failing HRESULT. Every COM allocation and
apartment initialization is released before returning.

Build from the repository root with `tools/build-windows-bridge.ps1`. It requires
MSVC x64 plus the Windows SDK and accepts `-VcVars <vcvars64.bat>` for another
installation. `/MT /O2 /W4 /WX` builds a static-CRT DLL in ignored `Builds`, then
installs it in `ForgeborneUnity/Assets/Plugins/x86_64`. Stop the Editor/player
before replacing a native DLL already loaded by those processes. Unity's importer
must retain the Windows x64/Windows Editor restriction.

Rebuild the managed Core separately with `tools/build-unity-core.ps1`. Open saved
Main in Unity 6000.3.27f2 and use **Forgeborne > Build Windows QA Player** with Play
stopped and authored scenes saved. The utility creates a fresh timestamped build
folder, records `build-report.json`, and writes the last output folder to
`Library/forgeborne-last-build.txt`. No build is uploaded or published.
It uses ordinary player options and does not enable Unity's development
connection. A development build prompted Windows Firewall during initial QA; no
security settings were changed.

Core checks, native compilation, Editor UI, built-player behavior, performance
and director fun/quality acceptance are separate evidence gates.
