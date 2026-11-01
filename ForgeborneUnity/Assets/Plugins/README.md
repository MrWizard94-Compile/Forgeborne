# Forgeborne core DLLs — read this before touching this folder

This folder holds **compiled DLLs only, never core source files**.
Copying `Forgeborne.Core` `.cs` files here was rejected by Rob
(2026-10-31, "NO QUICK FIXES") and would drift from the one
source of truth in `src/Forgeborne.Core/`.

## What is here

- `Forgeborne.Core.dll` — the core, built for **netstandard2.1**,
  so Unity 6000.3.27f2 with API Compatibility Level set to
  **.NET Standard 2.1** can reference it. The net8.0 build of the
  same source is what the headless test suite runs.
- `System.Text.Json.dll` and its transitive dependencies
  (`System.Text.Encodings.Web`, `System.Memory`, `System.Buffers`,
  `System.Numerics.Vectors`, `System.Runtime.CompilerServices.Unsafe`,
  `Microsoft.Bcl.AsyncInterfaces`, `System.Threading.Tasks.Extensions`)
  — System.Text.Json is in-box for net8.0 but not for
  netstandard2.1, so the package (8.0.5) and its dependencies
  ship with the core DLL. System.Text.Json is not an official
  Unity package; in Unity it is a manual Plugins DLL, which is
  exactly what this is.

## How these DLLs are built (repo root, Integration Step 4, 0.111.0)

Do not rebuild by hand from the NuGet cache. Run one documented script,
which publishes the core for netstandard2.1 and copies only the 9 DLLs
below into this folder (never `.cs` files, never the publish deps.json
or pdb):

```
# Linux / macOS / WSL:
bash tools/build-unity-core.sh
# Windows PowerShell:
.\tools\build-unity-core.ps1
```

A plain `dotnet build` is not enough: it leaves System.Text.Json and
its transitive DLLs in the NuGet cache, while `dotnet publish` collects
exactly the 9 DLLs Unity needs in one staging folder. After a core
change, run the script, then upload the changed DLLs to GitHub — the
snapshot push is UTF-8 text only and cannot carry DLLs, so a DLL
change needs the website-upload route used for 0.107.0, not a text push.

A no-drift guard runs with the headless test suite
(`tests/Forgeborne.Core.Tests/Program.cs`, Integration Step 4): it
fails if any core `.cs` filename, or any core file's exact content,
appears under `ForgeborneUnity/Assets/`, and it proves itself by
catching a deliberately planted copy in a temp tree.

## What is still owed on Windows

A clean Unity compile with these DLLs in place, with API
Compatibility Level set to .NET Standard 2.1, observed on
Windows in Unity 6000.3.27f2. If Unity reports a conflict with
a DLL it already ships, remove only the conflicting shipped
copy and record that change — do not fall back to copying
core source. If Unity rejects the System.Text.Json DLLs
altogether, the fallback recorded in the integration scope is
to abstract save serialisation behind an interface, not to
copy source.
