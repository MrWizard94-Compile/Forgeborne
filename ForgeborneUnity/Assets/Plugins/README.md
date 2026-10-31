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

## How these DLLs were built (repo root)

```
dotnet build src/Forgeborne.Core/Forgeborne.Core.csproj -c Release
# then copy src/Forgeborne.Core/bin/Release/netstandard2.1/Forgeborne.Core.dll
# plus the System.Text.Json transitive DLLs from the NuGet cache
# (see the core csproj for the package version) into this folder.
```

A documented, runnable build script and a no-drift guard that
fails if any core `.cs` file appears under `ForgeborneUnity/Assets/`
are Integration Step 4, not this step — they are not claimed here.

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
