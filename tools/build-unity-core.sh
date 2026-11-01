#!/usr/bin/env bash
# Forgeborne — build the Unity core DLLs (Integration Step 4, 0.111.0).
#
# Run from anywhere; the script finds the repo root from its own location.
# It publishes src/Forgeborne.Core for netstandard2.1 (Release) to a
# temporary staging folder, then copies ONLY the 9 shipped DLLs into
# ForgeborneUnity/Assets/Plugins/. It never copies .cs files, never
# copies the publish deps.json or pdb, and never deletes the Plugins
# README. Publish (not plain build) is used on purpose: a plain build
# leaves System.Text.Json and its transitive DLLs in the NuGet cache,
# while publish collects exactly the 9 DLLs Unity needs in one folder.
#
# Usage (repo root, or anywhere):
#   bash tools/build-unity-core.sh
# Windows equivalent: tools/build-unity-core.ps1 (same 9 DLLs, same target).
# After a core change, run this script, then upload the changed DLLs to
# GitHub (the snapshot push is UTF-8 text only and cannot carry DLLs).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if command -v dotnet >/dev/null 2>&1; then
    DOTNET="dotnet"
elif [ -x "$HOME/.dotnet/dotnet" ]; then
    DOTNET="$HOME/.dotnet/dotnet"
else
    echo "dotnet not found on PATH or at \$HOME/.dotnet/dotnet" >&2
    exit 1
fi

DLLS=(
    "Forgeborne.Core.dll"
    "Microsoft.Bcl.AsyncInterfaces.dll"
    "System.Buffers.dll"
    "System.Memory.dll"
    "System.Numerics.Vectors.dll"
    "System.Runtime.CompilerServices.Unsafe.dll"
    "System.Text.Encodings.Web.dll"
    "System.Text.Json.dll"
    "System.Threading.Tasks.Extensions.dll"
)

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT

"$DOTNET" publish src/Forgeborne.Core/Forgeborne.Core.csproj \
    -f netstandard2.1 -c Release -o "$STAGE" --nologo

for dll in "${DLLS[@]}"; do
    if [ ! -f "$STAGE/$dll" ]; then
        echo "publish output is missing $dll; Plugins left unchanged" >&2
        exit 1
    fi
    cp "$STAGE/$dll" "ForgeborneUnity/Assets/Plugins/$dll"
done

echo "Copied ${#DLLS[@]} DLLs to ForgeborneUnity/Assets/Plugins/"
ls -1 ForgeborneUnity/Assets/Plugins/*.dll
echo "Note: DLLs are build outputs, not source. The no-drift guard in"
echo "tests/Forgeborne.Core.Tests/Program.cs fails if a core .cs file"
echo "appears under ForgeborneUnity/Assets/."
